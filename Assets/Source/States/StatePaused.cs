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
            Time.timeScale = 1f;
        }
    }
}
