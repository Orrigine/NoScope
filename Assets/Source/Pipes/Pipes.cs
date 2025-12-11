using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoScope
{
    public class Pipes : MonoBehaviour
    {
        [Header("Pipe Settings")]
        public Transform startPoint;
        public Transform endPoint;

        [HideInInspector]
        public Pipes nextPipe;

        private bool _isActivated = false;

        public void Activate()
        {
            _isActivated = true;
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            _isActivated = false;
            gameObject.SetActive(false);
        }

        public bool IsPlayerPast(Vector3 playerPosition)
        {
            // Vérifie si le joueur a dépassé l'endPoint de cette pipe sur l'axe Z
            return playerPosition.z > endPoint.position.z;
        }
    }
}
