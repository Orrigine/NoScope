using System.Collections.Generic;
using UnityEngine;
using NoScope.States;
using System;

namespace NoScope
{
    public class StateMachine : MonoBehaviour
    {
        public static StateMachine Instance { get; private set; }
        private IState _currentState;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            // Initialisation par défaut sur StatePlay
            _currentState = StatePlay.Instance;
            _currentState?.Enter();
        }

        void Update()
        {
            if (_currentState != null)
            {
                IState nextState = _currentState.Execute();
                if (nextState != null && nextState != _currentState)
                {
                    ChangeState(nextState);
                }
            }
        }

        public void ChangeState(IState newState)
        {
            if (_currentState != null)
            {
                _currentState.Exit();
            }

            _currentState = newState;

            if (_currentState != null)
            {
                _currentState.Enter();
            }
        }

        public IState GetCurrentState()
        {
            return _currentState;
        }

        public IState GetState<T>() where T : IState
        {
            // Retourne l'instance singleton du state demandé
            return typeof(T).Name switch
            {
                nameof(StatePlay) => StatePlay.Instance,
                nameof(StatePaused) => StatePaused.Instance,
                nameof(StateStyle) => StateStyle.Instance,
                _ => null
            };

        }
    }
}
