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
            // IMPORTANT : Garde le temps actif pour permettre la physique du saut
            Time.timeScale = 0.5f;
        }

        public IState? Execute()
        {
            // Logique pendant la QTE (gérée par QTEManager)
            // Le temps continue, permettant au joueur de voler pendant la QTE
            return null;
        }

        public void Exit()
        {
            // Assure que le temps est bien normal
            Time.timeScale = 1f;
        }
    }
}
