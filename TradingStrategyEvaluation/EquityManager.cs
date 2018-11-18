using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.Common.Data;

using StockAnalysis.TradingStrategy;

namespace StockAnalysis.TradingStrategy.Evaluation
{
    public sealed class EquityManager
    {
        private struct PositionToBeSold
        {
            public int Index;
            public long Volume;

            public PositionToBeSold(int index, long volume)
            {
                Index = index;
                Volume = volume;
            }
        }

        private readonly Dictionary<string, List<Position>> _activePositions = new Dictionary<string, List<Position>>();

        private readonly List<Position> _closedPositions = new List<Position>();

        private readonly ICapitalManager _capitalManager;

        private readonly int _positionFrozenDays;

        public double InitialCapital 
        {
            get { return _capitalManager.InitialCapital; }
        }

        public double CurrentCapital 
        {
            get { return _capitalManager.CurrentCapital; }
        }

        public IEnumerable<Position> ClosedPositions { get { return _closedPositions; } }

        public EquityManager(ICapitalManager capitalManager, int positionFrozenDays)
        {
            if (capitalManager == null)
            {
                throw new ArgumentNullException();
            }

            _capitalManager = capitalManager;

            _positionFrozenDays = positionFrozenDays;
        }

        private void AddPosition(Position position)
        {
            if (!_activePositions.ContainsKey(position.Symbol))
            {
                _activePositions.Add(position.Symbol, new List<Position>());
            }

            _activePositions[position.Symbol].Add(position);
        }

        public void ManualAddPosition(Position position)
        {
            AddPosition(position);
        }

        public bool ExecuteTransaction(
            Transaction transaction, 
            bool allowNegativeCapital,
            out CompletedTransaction completedTransaction, 
            out string error,
            bool forcibly = false)
        {
            error = string.Empty;
            completedTransaction = null;

            if (transaction.Action == TradingAction.OpenLong)
            {
                var charge = transaction.Price * transaction.Volume + transaction.Commission;

                // try to allocate capital
                bool isFirstPosition = !ExistsPosition(transaction.Symbol);
                if (!_capitalManager.AllocateCapital(charge, isFirstPosition, allowNegativeCapital))
                {
                    error = "No enough capital for the transaction";
                    return false;
                }

                var position = new Position(transaction);

                AddPosition(position);

                return true;
            }

            if (transaction.Action == TradingAction.CloseLong)
            {
                var symbol = transaction.Symbol;

                if (!ExistsPosition(symbol))
                {
                    error = string.Format("There is no position for trading object {0}", symbol);
                    return false;
                }

                var positions = _activePositions[symbol].ToArray();

                var positionsToBeSold = IdentifyPositionToBeSold(positions, transaction).ToArray();

                if (positionsToBeSold.Count() == 0)
                {
                    return true;
                }

                // check if position is still frozen if transaction is not executed forcibly
                if (!forcibly)
                {
                    foreach (var ptbs in positionsToBeSold)
                    {
                        if (_positionFrozenDays > 0 
                            && positions[ptbs.Index].LastedPeriodCount < _positionFrozenDays)
                        {
                            error = string.Format("position is still frozen");
                            return false;
                        }
                    }
                }

                // note: the position could be sold partially and we need to consider the situation
                // everywhere in the code

                var buyCost = 0.0;
                var buyCommission = 0.0;

                foreach (var ptbs in positionsToBeSold)
                {
                    buyCost += positions[ptbs.Index].BuyPrice * ptbs.Volume;
                    buyCommission += positions[ptbs.Index].BuyCommission 
                                     * ptbs.Volume / positions[ptbs.Index].Volume;
                }

                foreach (var ptbs in positionsToBeSold)
                {
                    var position = positions[ptbs.Index];
                    if (position == null)
                    {
                        throw new InvalidOperationException();
                    }

                    // for partial selling, we need to split position firstly.
                    Position newPosition = null;
                    if (ptbs.Volume < position.Volume)
                    {
                        newPosition = position.Split(ptbs.Volume);
                    }

                    // close the position
                    var newTransaction = new Transaction
                        {
                            Action = transaction.Action,
                            Symbol = transaction.Symbol,
                            Name = transaction.Name,
                            Comments = transaction.Comments,
                            Commission = transaction.Commission / transaction.Volume * position.Volume,
                            Error = transaction.Error,
                            ExecutionTime = transaction.ExecutionTime,
                            InstructionId = transaction.InstructionId,
                            Price = transaction.Price,
                            RelatedObjects = transaction.RelatedObjects,
                            SubmissionTime = transaction.SubmissionTime,
                            Succeeded = transaction.Succeeded,
                            Volume = position.Volume
                        };

                    position.Close(newTransaction);

                    // move closed position to history
                    _closedPositions.Add(position);

                    // use new position to replace old position.
                    positions[ptbs.Index] = newPosition;

                    // free capital
                    var earn = newTransaction.Price * newTransaction.Volume - newTransaction.Commission;
                    _capitalManager.FreeCapital(earn, ptbs.Index == 0);
                }

                // update positions for given symbol
                var remainingPositions = positions.Where(e => e != null).ToList();

                if (remainingPositions == null || remainingPositions.Count == 0)
                {
                    _activePositions.Remove(symbol);
                }
                else
                {
                    _activePositions[symbol] = remainingPositions;
                }

                // create completed transaction object
                completedTransaction = new CompletedTransaction
                {
                    Symbol = symbol,
                    Name = transaction.Name,
                    ExecutionTime = transaction.ExecutionTime,
                    Volume = transaction.Volume,
                    BuyCost = buyCost,
                    AverageBuyPrice = buyCost / transaction.Volume,
                    SoldPrice = transaction.Price,
                    SoldGain = transaction.Price * transaction.Volume,
                    Commission = transaction.Commission + buyCommission,
                };

                return true;
            }
            
            throw new InvalidOperationException(
                string.Format("unsupported action {0}", transaction.Action));
        }

        /// <summary>
        /// Identify all positions that to be sold
        /// </summary>
        /// <param name="positions">existing positions to be examined</param>
        /// <param name="transaction">transaction to be executed</param>
        /// <returns>Tuples that identify the position and volume to be sold</returns>
        private IEnumerable<PositionToBeSold> IdentifyPositionToBeSold(Position[] positions, Transaction transaction)
        {
            if (positions == null || transaction == null)
            {
                throw new ArgumentNullException();
            }

            if (positions.Length == 0 || transaction.Action != TradingAction.CloseLong)
            {
                throw new ArgumentException();
            }

            var remainingVolume = transaction.Volume;
            switch (transaction.SellingType)
            {
                case SellingType.ByPositionId:
                    for (var i = 0; i < positions.Length; ++i)
                    {
                        if (positions[i].Id == transaction.PositionIdForSell)
                        {
                            yield return new PositionToBeSold(i, positions[i].Volume);
                            yield break;
                        }
                    }
                    break;
                case SellingType.ByStopLossPrice:
                    for (var i = 0; i < positions.Length; ++i)
                    {
                        if (positions[i].StopLossPrice >= transaction.StopLossPriceForSelling
                            && positions[i].Id == transaction.PositionIdForSell)
                        {
                            remainingVolume -= positions[i].Volume;

                            yield return new PositionToBeSold(i, positions[i].Volume);
                            yield break;
                        }
                    }
                    break;
                case SellingType.ByVolume:
                    var totalVolume = positions.Sum(e => e.Volume);
                    if (totalVolume < transaction.Volume)
                    {
                        throw new InvalidOperationException("There is no enough volume for selling");
                    }

                    for (var i = 0; i < positions.Length && remainingVolume > 0; ++i)
                    {
                        if (positions[i].Volume <= remainingVolume)
                        {
                            remainingVolume -= positions[i].Volume;
                            yield return new PositionToBeSold(i, positions[i].Volume);
                        }
                        else
                        {
                            yield return new PositionToBeSold(i, remainingVolume);

                            remainingVolume = 0;
                        }
                    }

                    break;
            }

            if (remainingVolume != 0)
            {
                throw new InvalidOperationException("The volume specified in transaction does not match the positions affected by the transaction");
            }
        }

        public int PositionCount { get { return _activePositions.Count; } }

        public IEnumerable<Position> GetPositionDetails(string symbol)
        {
            return _activePositions[symbol];
        }

        public IEnumerable<string> GetAllPositionSymbols()
        {
            return _activePositions.Keys.ToArray();
        }

        public bool ExistsPosition(string symbol)
        {
            return _activePositions.ContainsKey(symbol);
        }

        public double GetTotalEquity(
            ITradingDataProvider provider, 
            DateTime period, 
            EquityEvaluationMethod method)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }


            if (method == EquityEvaluationMethod.InitialEquity)
            {
                return InitialCapital;
            }

            double equity = CurrentCapital;

            // cash is the core equity
            if (method == EquityEvaluationMethod.CoreEquity)
            {
                return equity;
            }

            foreach (var kvp in _activePositions)
            {
                var symbol = kvp.Key;

                Bar bar;

                var index = provider.GetIndexOfTradingObject(symbol);
                if (index < 0)
                {
                    throw new InvalidOperationException(string.Format("Can't get index for symbol {0}", symbol));
                }

                if (!provider.GetLastEffectiveBar(index, period, out bar))
                {
                    throw new InvalidOperationException(
                        string.Format("Can't get data from data provider for symbol {0}, time {1}", symbol, period));
                }

                if (method == EquityEvaluationMethod.TotalEquity
                    || method == EquityEvaluationMethod.LossControlTotalEquity
                    || method == EquityEvaluationMethod.LossControlInitialEquity)
                {
                    var volume = kvp.Value.Sum(e => e.Volume);
                    equity += volume * bar.ClosePrice;
                }
                else if (method == EquityEvaluationMethod.ReducedTotalEquity 
                        || method == EquityEvaluationMethod.LossControlReducedTotalEquity)
                {
                    equity += kvp.Value.Sum(position => position.Volume * Math.Min(bar.ClosePrice, position.StopLossPrice));
                }
            }

            if (method == EquityEvaluationMethod.TotalEquity
                || method == EquityEvaluationMethod.ReducedTotalEquity)
            {
                return equity;
            }
            else if (method == EquityEvaluationMethod.LossControlInitialEquity)
            {
                return equity > InitialCapital
                    ? InitialCapital
                    : 2 * equity - InitialCapital;
            }
            else if (method == EquityEvaluationMethod.LossControlTotalEquity)
            {
                return equity > InitialCapital
                    ? equity
                    : 2 * equity - InitialCapital;
            }
            else if (method == EquityEvaluationMethod.LossControlReducedTotalEquity)
            {
                return equity > InitialCapital
                    ? equity
                    : 2 * equity - InitialCapital;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public double GetPositionMarketValue(ITradingDataProvider provider, string symbol, DateTime time)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            double equity = 0;

            if (_activePositions.ContainsKey(symbol))
            { 
                var volume = _activePositions[symbol].Sum(e => e.Volume);

                Bar bar;

                var index = provider.GetIndexOfTradingObject(symbol);
                if (!provider.GetLastEffectiveBar(index, time, out bar))
                {
                    throw new InvalidOperationException(
                        string.Format("Can't get data from data provider for symbol {0}, time {1}", symbol, time));
                }

                equity += volume * bar.ClosePrice;
            }

            return equity;
        }
    }
}
