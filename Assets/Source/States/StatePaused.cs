using UnityEngine;

namespace NoScope.States
{
    public class StatePaused : IState
    {
        public static StatePaused Instance { get; private set; }

        static StatePaused()
        {
            Instance = new StatePaused();
        }

        private StatePaused() { }

        public void Enter()
        {
            Debug.Log("Entering Paused State");
            Time.timeScale = 0f;
        }

        public IState? Execute()
        {
            // Logique pendant la pause
            // Attendre que le joueur reprenne
            return null;
        }

        public void Exit()
        {
            Debug.Log("Exiting Paused State");
            Time.timeScale = 1f;
        }
    }
}
