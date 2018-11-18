namespace StockTradingConsole
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using StockAnalysis.Common.Utility;

    class StateMachine<StateType, InputType> where InputType : class
    {
        private Dictionary<StateType, List<StateTransition<StateType, InputType>>> _outgoingTransitions;
        private Dictionary<StateType, List<StateTransition<StateType, InputType>>> _ingoingTransitions;

        private StateType _initialState;
        private ILookup<StateType, StateType> _finalStates;

        public StateType CurrentState { get; private set; }

        public StateMachine(IEnumerable<StateTransition<StateType, InputType>> transitions, StateType initialState)
        {
            if (transitions == null || transitions.Count() == 0)
            {
                throw new ArgumentNullException();
            }

            _outgoingTransitions = transitions.GroupBy(t => t.FromState).ToDictionary(g => g.Key, g => g.ToList());
            _ingoingTransitions = transitions.GroupBy(t => t.ToState).ToDictionary(g => g.Key, g => g.ToList());

            // final states are the states that have ingoing transition and has no outgoing transition.
            var finalStates = _ingoingTransitions.Keys.Except(_outgoingTransitions.Keys);
            if (finalStates.Count() < 1)
            {
                throw new ArgumentException("no final state found");
            }

            _finalStates = finalStates.ToLookup(t => t);

            _initialState = initialState;
            if (_finalStates.Contains(_initialState))
            {
                throw new ArgumentException("inital state can't be final state");
            }

            // set current state
            CurrentState = _initialState;
        }

        public bool IsFinalState()
        {
            return _finalStates.Contains(CurrentState);
        }

        public void ProcessInput(InputType input)
        {
            if (!IsFinalState())
            {
                var transitions = _outgoingTransitions[CurrentState];
                foreach (var transition in transitions)
                {
                    if (transition.Transfer(input))
                    {
                        AppLogger.Default.DebugFormat("Transfer state from {0} to {1}", CurrentState, transition.ToState);

                        CurrentState = transition.ToState;
                        break;
                    }
                }
            }
        }
    }
}
