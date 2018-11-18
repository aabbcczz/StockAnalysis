namespace StockAnalysis.TradingStrategy.MetricBooleanExpression
{
    using System;

    public abstract class LogicOperation : IMetricBooleanExpression
    {
        private IMetricBooleanExpression _expression1;
        private IMetricBooleanExpression _expression2;

        public LogicOperation(IMetricBooleanExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException();
            }

            _expression1 = expression;
            _expression2 = null;
        }

        public LogicOperation(IMetricBooleanExpression expression1, IMetricBooleanExpression expression2)
        {
            if (expression1 == null || expression2 == null)
            {
                throw new ArgumentNullException();
            }

            _expression1 = expression1;
            _expression2 = expression2;
        }

        public void Initialize(IRuntimeMetricManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException();
            }

            _expression1.Initialize(manager);

            if (_expression2 != null)
            {
                _expression2.Initialize(manager);
            }
        }
        
        public bool IsTrue(ITradingObject tradingObject)
        {
            if (_expression2 == null)
            {
                return Operate(_expression1.IsTrue(tradingObject));
            }
            else
            {
                return Operate(_expression1.IsTrue(tradingObject), _expression2.IsTrue(tradingObject));
            }
        }

        public string GetInstantializedExpression(ITradingObject tradingObject)
        {
            return string.Format("({0}) {1} ({2})", 
                _expression1.GetInstantializedExpression(tradingObject), 
                GetOperationString(),
                _expression2.GetInstantializedExpression(tradingObject));
        }

        public abstract bool Operate(bool v1, bool v2);

        public abstract bool Operate(bool v);

        public abstract string GetOperationString();
    }
}
