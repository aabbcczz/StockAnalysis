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
        private Dictionary<string, List<Equity>> _equities = new Dictionary<string, List<Equity>>();

        public double InitialCapital { get; private set; }

        public double CurrentCapital { get; private set; }

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

                Equity equity = new Equity(transaction);

                if (!_equities.ContainsKey(equity.Code))
                {
                    _equities.Add(equity.Code, new List<Equity>());
                }

                _equities[equity.Code].Add(equity);

                // charge money
                CurrentCapital -= charge;

                return true;
            }
            else if (transaction.Action == TradingAction.CloseLong)
            {
                string code = transaction.Code;
                double earn = transaction.Price * transaction.Volume - transaction.Commission;

                if (!_equities.ContainsKey(code))
                {
                    error = string.Format("Transaction object {0} does not exists", code);
                    return false;
                }

                Equity[] equities = _equities[code].ToArray();

                int totalVolume = equities.Sum(e => e.Volume);
                if (totalVolume < transaction.Volume)
                {
                    error = "There is no enough volume for selling";
                    return false;
                }
                else
                {
                    double buyCost = 0.0;
                    double buyCommission = 0.0;

                    if (totalVolume == transaction.Volume)
                    {
                        buyCost = equities.Sum(t => t.Price * t.Volume);
                        buyCommission = equities.Sum(t => t.Commission);

                        // update equities for given code
                        _equities.Remove(code);
                    }
                    else
                    {
                        int remainingVolume = transaction.Volume;

                        for (int i = 0; i < equities.Length && remainingVolume == 0; ++i)
                        {
                            if (equities[i].Volume <= remainingVolume)
                            {
                                buyCost += equities[i].Price * equities[i].Volume;
                                buyCommission += equities[i].Commission;

                                remainingVolume -= equities[i].Volume;
                                equities[i] = null;
                            }
                            else
                            {
                                double commissionPerUnit = equities[i].Commission / equities[i].Volume;
                                buyCost += equities[i].Price * remainingVolume;
                                buyCommission += commissionPerUnit * remainingVolume;

                                equities[i].Volume -= remainingVolume;
                                equities[i].Commission = equities[i].Volume * commissionPerUnit;
                                remainingVolume = 0;
                            }
                        }

                        if (remainingVolume != 0)
                        {
                            throw new InvalidProgramException("Logic error");
                        }

                        // update equities for given code
                        var remainingEquities = equities.Where(e => e != null).ToList();

                        if (remainingEquities == null || remainingEquities.Count == 0)
                        {
                            throw new InvalidProgramException("Logic error");
                        }

                        _equities[code] = remainingEquities;
                    }

                    // update current capital
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
            }
            else
            {
                throw new InvalidOperationException(
                    string.Format("unsupported action {0}", transaction.Action));
            }
        }

        public IEnumerable<Equity> GetEquityDetails(string code)
        {
            return _equities[code];
        }

        public IEnumerable<string> GetAllEquityCodes()
        {
            return _equities.Keys.ToArray();
        }

        public bool ExistsEquity(string code)
        {
            return _equities.ContainsKey(code);
        }

        public double GetTotalEquityMarketValue(ITradingDataProvider provider, DateTime time)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            double totalEquity = CurrentCapital;

            foreach (var kvp in _equities)
            {
                string code = kvp.Key;
                int volume = kvp.Value.Sum(e => e.Volume);

                Bar bar;
                
                if (!provider.GetLastEffectiveBar(code, time, out bar))
                {
                    throw new InvalidOperationException(
                        string.Format("Can't get data from data provider for code {0}, time {1}", code, time));
                }

                totalEquity += volume * bar.ClosePrice;
            }

            return totalEquity;
        }

        public double GetEquityMarketValue(ITradingDataProvider provider, string code, DateTime time)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            double equity = 0;

            if (_equities.ContainsKey(code))
            { 
                int volume = _equities[code].Sum(e => e.Volume);

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
