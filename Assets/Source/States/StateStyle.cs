using UnityEngine;

namespace NoScope.States
{
    public class StateStyle : IState
    {
        public static StateStyle Instance { get; private set; }

        static StateStyle()
        {
            Instance = new StateStyle();
        }

        private StateStyle() { }

        public void Enter()
        {
            Debug.Log("Entering Style State - QTE Time!");
            // Arrêt complet du temps pour la QTE
            Time.timeScale = 0f;
        }

        public IState? Execute()
        {
            // Logique pendant la QTE (gérée par QTEManager)
            // Le temps est arrêté, on attend la fin de la QTE
            return null;
        }

        public void Exit()
        {
            Debug.Log("Exiting Style State - Resuming Game");
            // Reprise du temps normal
            Time.timeScale = 1f;
        }
    }
}
