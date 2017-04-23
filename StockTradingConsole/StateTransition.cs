using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockTrading.Utility;
using StockAnalysis.Share;

namespace StockTradingConsole
{
    class StateTransition<StateType, InputType>
    {
        public StateType FromState { get; private set; }
        public StateType ToState { get; private set; }

        private Func<InputType, bool> _transitionFunction;

        public StateTransition(StateType fromState, StateType toState, Func<InputType, bool> transitionFunction)
        {
            if (transitionFunction == null)
            {
                throw new ArgumentNullException("transitionFunction");
            }

            FromState = fromState;
            ToState = ToState;
            _transitionFunction = transitionFunction;
        }

        public bool Transfer(InputType input)
        {
            if (input == null)
            {
                throw new ArgumentNullException();
            }

            return _transitionFunction(input);
        }
    }

    static class StateTransition
    {
        public static StateTransition<StateType, InputType> Create<StateType, InputType>(StateType fromState, StateType toState, Func<InputType, bool> transitionFunction)
        {
            return new StateTransition<StateType, InputType>(fromState, toState, transitionFunction);
        }
    }
}
