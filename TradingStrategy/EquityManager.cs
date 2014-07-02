using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class EquityManager
    {
        private Dictionary<string, List<Equity>> _equities = new Dictionary<string, List<Equity>>();

        public readonly double InitialCapital { get; private set; }

        public double CurrentCapital { get; private set; }

        public SellingSequence SequenceOfSelling { get; private set; }

        public EquityManager(double initialCapital, SellingSequence sequenceOfSelling)
        {
            InitialCapital = initialCapital;
            CurrentCapital = initialCapital;
            SequenceOfSelling = sequenceOfSelling;
        }

        public bool ExecuteTransaction(Transaction transaction, out string error)
        {
            error = string.Empty;

            if (transaction.Action == TradingAction.Noop)
            {
                error = "Can't execute No-op action";
                return false;
            }
            else if (transaction.Action == TradingAction.OpenLong)
            {
                double charge = transaction.Price * transaction.Volume + transaction.Commission;

                if (CurrentCapital < charge)
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
            }
            else if (transaction.Action == TradingAction.CloseLong)
            {
                string code = transaction.Object.Code;

                if (!_equities.ContainsKey(code))
                {
                    error = string.Format("Transaction object {0} does not exists", code);
                    return false;
                }

                Equity[] equities = _equities[code].ToArray();
                
                if (SequenceOfSelling == SellingSequence.LIFO)
                {
                    equities = equities.Reverse().ToArray();
                }

                int totalVolume = equities.Sum(e => e.Volume);
                if (totalVolume < transaction.Volume)
                {
                    error = "There is no enough volume for selling";
                    return false;
                }
                else if (totalVolume == transaction.Volume)
                {
                    _equities.Remove(code);
                    return true;
                }
                else
                {
                    int remainingVolume = transaction.Volume;
                    for (int i = 0; i < equities.Length && remainingVolume == 0; ++i)
                    {
                        if (equities[i].Volume <= remainingVolume)
                        {
                            remainingVolume -= equities[i].Volume;
                            equities[i] = null;
                        }
                        else
                        {
                            equities[i].Volume -= remainingVolume;
                            remainingVolume = 0;
                        }
                    }

                    if (remainingVolume != 0)
                    {
                        throw new InvalidProgramException("Logic error");
                    }

                    var remainingEquities = equities.Where(e => e != null).ToList();

                    if (remainingEquities == null || remainingEquities.Count == 0)
                    {
                        throw new InvalidProgramException("Logic error");
                    }

                    if (SequenceOfSelling == SellingSequence.LIFO)
                    {
                        remainingEquities.Reverse();
                    }

                    _equities[code] = remainingEquities;

                    return true;
                }
            }
            
            throw new InvalidProgramException();
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
    }
}
