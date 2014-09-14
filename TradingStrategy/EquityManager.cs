using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace TradingStrategy
{
    public sealed class EquityManager
    {
        private struct PositionToBeSold
        {
            public int Index;
            public int Volume;

            public PositionToBeSold(int index, int volume)
            {
                Index = index;
                Volume = volume;
            }
        }

        private Dictionary<string, List<Position>> _activePositions = new Dictionary<string, List<Position>>();

        private List<Position> _closedPositions = new List<Position>();

        public double InitialCapital { get; private set; }

        public double CurrentCapital { get; private set; }

        public IEnumerable<Position> ClosedPositions { get { return _closedPositions; } }

        public EquityManager(double initialCapital)
        {
            InitialCapital = initialCapital;
            CurrentCapital = initialCapital;
        }

        public bool ExecuteTransaction(
            Transaction transaction,
            bool allowNegativeCapital,
            out string error)
        {
            CompletedTransaction completed;
            return ExecuteTransaction(transaction, allowNegativeCapital, out completed, out error);
        }

        public bool ExecuteTransaction(
            Transaction transaction, 
            bool allowNegativeCapital,
            out CompletedTransaction completedTransaction, 
            out string error)
        {
            error = string.Empty;
            completedTransaction = null;

            if (transaction.Action == TradingAction.OpenLong)
            {
                double charge = transaction.Price * transaction.Volume + transaction.Commission;

                if (CurrentCapital < charge && !allowNegativeCapital)
                {
                    error = "No enough capital for the transaction";
                    return false;
                }

                Position position = new Position(transaction);

                if (!_activePositions.ContainsKey(position.Code))
                {
                    _activePositions.Add(position.Code, new List<Position>());
                }

                _activePositions[position.Code].Add(position);

                // charge money
                CurrentCapital -= charge;

                return true;
            }
            else if (transaction.Action == TradingAction.CloseLong)
            {
                string code = transaction.Code;

                if (!_activePositions.ContainsKey(code))
                {
                    error = string.Format("Transaction object {0} does not exists", code);
                    return false;
                }

                Position[] positions = _activePositions[code].ToArray();

                PositionToBeSold[] PositionsToBeSold = IdentifyPositionToBeSold(positions, transaction).ToArray();

                if (PositionsToBeSold == null || PositionsToBeSold.Count() == 0)
                {
                    return true;
                }

                // note: the position could be sold partially and we need to consider the situation
                // everywhere in the code

                double buyCost = 0.0;
                double buyCommission = 0.0;

                foreach (var ptbs in PositionsToBeSold)
                {
                    buyCost += positions[ptbs.Index].BuyPrice * ptbs.Volume;
                    buyCommission += positions[ptbs.Index].BuyCommission 
                        * (double)ptbs.Volume / positions[ptbs.Index].Volume;
                }

                foreach (var ptbs in PositionsToBeSold)
                {
                    // for partial selling, we need to split position firstly.
                    Position newPosition = null;
                    if (ptbs.Volume < positions[ptbs.Index].Volume)
                    {
                        newPosition = positions[ptbs.Index].Split(ptbs.Volume);
                    }

                    positions[ptbs.Index].Close(
                        new Transaction()
                        {
                            Action = transaction.Action,
                            Code = transaction.Code,
                            Comments = transaction.Comments,
                            Commission = transaction.Commission / transaction.Volume * positions[ptbs.Index].Volume,
                            Error = transaction.Error,
                            ExecutionTime = transaction.ExecutionTime,
                            InstructionId = transaction.InstructionId,
                            Price = transaction.Price,
                            SubmissionTime = transaction.SubmissionTime,
                            Succeeded = transaction.Succeeded,
                            Volume = positions[ptbs.Index].Volume
                        });

                    // move closed position to history
                    _closedPositions.Add(positions[ptbs.Index]);

                    // use new position to replace old position.
                    positions[ptbs.Index] = newPosition;
                }

                // update positions for given code
                var remainingPositions = positions.Where(e => e != null).ToList();

                if (remainingPositions == null || remainingPositions.Count == 0)
                {
                    _activePositions.Remove(code);
                }
                else
                {
                    _activePositions[code] = remainingPositions;
                }

                // update current capital
                double earn = transaction.Price * transaction.Volume - transaction.Commission;
                CurrentCapital += earn;

                // create completed transaction object
                completedTransaction = new CompletedTransaction()
                {
                    Code = code,
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
            else
            {
                throw new InvalidOperationException(
                    string.Format("unsupported action {0}", transaction.Action));
            }
        }

        /// <summary>
        /// Identify all positions that to be sold
        /// </summary>
        /// <param name="positions">existing positions to be examined</param>
        /// <param name="transaction">transaction to be executed</param>
        /// <returns>Tuples that identify the position and volume to be sold</returns>
        private IEnumerable<PositionToBeSold> IdentifyPositionToBeSold(Position[] positions, Transaction transaction)
        {
            System.Diagnostics.Debug.Assert(positions != null);
            System.Diagnostics.Debug.Assert(transaction != null);
            System.Diagnostics.Debug.Assert(positions.Length > 0);
            System.Diagnostics.Debug.Assert(transaction.Action == TradingAction.CloseLong);

            int remainingVolume = transaction.Volume;
            switch (transaction.SellingType)
            {
                case SellingType.ByPositionId:
                    for (int i = 0; i < positions.Length; ++i)
                    {
                        if (positions[i].ID == transaction.PositionIdForSell)
                        {
                            remainingVolume -= positions[i].Volume;
                            yield return new PositionToBeSold(i, positions[i].Volume);
                            yield break;
                        }
                    }
                    break;
                case SellingType.ByStopLossPrice:
                    for (int i = 0; i < positions.Length; ++i)
                    {
                        if (positions[i].StopLossPrice > transaction.StopLossPriceForSell)
                        {
                            remainingVolume -= positions[i].Volume;

                            yield return new PositionToBeSold(i, positions[i].Volume);
                        }
                    }
                    break;
                case SellingType.ByVolume:
                    int totalVolume = positions.Sum(e => e.Volume);
                    if (totalVolume < transaction.Volume)
                    {
                        throw new InvalidOperationException("There is no enough volume for selling");
                    }

                    for (int i = 0; i < positions.Length && remainingVolume > 0; ++i)
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

        public IEnumerable<Position> GetPositionDetails(string code)
        {
            return _activePositions[code];
        }

        public IEnumerable<string> GetAllPositionCodes()
        {
            return _activePositions.Keys.ToArray();
        }

        public bool ExistsPosition(string code)
        {
            return _activePositions.ContainsKey(code);
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

            double totalEquity = CurrentCapital;

            // cash is the core equity
            if (method == EquityEvaluationMethod.CoreEquity)
            {
                // do nothing
            }
            else
            {
                foreach (var kvp in _activePositions)
                {
                    string code = kvp.Key;

                    Bar bar;

                    if (!provider.GetLastEffectiveBar(code, period, out bar))
                    {
                        throw new InvalidOperationException(
                            string.Format("Can't get data from data provider for code {0}, time {1}", code, period));
                    }

                    if (method == EquityEvaluationMethod.TotalEquity)
                    {
                        int volume = kvp.Value.Sum(e => e.Volume);
                        totalEquity += volume * bar.ClosePrice;
                    }
                    else if (method == EquityEvaluationMethod.ReducedTotalEquity)
                    {
                        foreach (var position in kvp.Value)
                        {
                            totalEquity += position.Volume * Math.Min(bar.ClosePrice, position.StopLossPrice);
                        }
                    }
                }
            }

            return totalEquity;
        }

        public double GetPositionMarketValue(ITradingDataProvider provider, string code, DateTime time)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            double equity = 0;

            if (_activePositions.ContainsKey(code))
            { 
                int volume = _activePositions[code].Sum(e => e.Volume);

                Bar bar;

                if (!provider.GetLastEffectiveBar(code, time, out bar))
                {
                    throw new InvalidOperationException(
                        string.Format("Can't get data from data provider for code {0}, time {1}", code, time));
                }

                equity += volume * bar.ClosePrice;
            }

            return equity;
        }
    }
}
