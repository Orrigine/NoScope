using UnityEngine;

namespace NoScope.States
{
    public class StatePlay : IState
    {
        public static StatePlay Instance { get; private set; }

        static StatePlay()
        {
            Instance = new StatePlay();
        }

        private StatePlay() { }

        public void Enter()
        {
            Debug.Log("Entering Play State");
            Time.timeScale = 1f;
        }

        public IState? Execute()
        {
            // Logique de jeu pendant le Play state
            // Gérée par les différents managers
            return null;
        }

        public void Exit()
        {
            Debug.Log("Exiting Play State");
        }
    }
}
