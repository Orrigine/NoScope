using UnityEngine;

namespace NoScope
{
    /// <summary>
    /// Configuration centralisée pour tous les paramètres du jeu
    /// ScriptableObject pour faciliter les ajustements sans recompiler
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "NoScope/Game Configuration")]
    public class GameConfig : ScriptableObject
    {
        [Header("Player Settings")]
        public float playerBaseSpeed = 10f;
        public float playerMaxSpeed = 30f;
        public float speedIncreasePerQTE = 2f;
        public float speedDecayRate = 0.5f;
        public float playerJumpForce = 10f;

        [Header("Shooting Settings")]
        public float baseFireRate = 0.5f;
        public float maxFireRate = 0.1f;
        public float fireRateIncreasePerQTE = 0.05f;
        public float bulletSpeed = 20f;
        public float bulletLifetime = 5f;
        public float bulletDamage = 10f;

        [Header("Pipe Generation")]
        public int initialPipeCount = 5;
        public int maxActivePipes = 10;
        public float pipeSpacing = 10f;
        public float pipeHeightVariation = 5f;
        public float pipeHorizontalVariation = 3f;

        [Header("QTE Settings")]
        public int minSequenceLength = 3;
        public int maxSequenceLength = 6;
        public float baseDifficulty = 2f;
        public float difficultyIncrease = 0.1f;
        public float minDifficulty = 1f;

        [Header("Enemy Mass Settings")]
        public float massMaxHealth = 1000f;
        public float massMoveSpeed = 8f;
        public float massCatchUpSpeed = 12f;
        public float massDistanceThreshold = 20f;
        public float massSpawnInterval = 5f;
        public int maxSmallEnemies = 5;
        public float smallEnemySpawnRadius = 5f;

        [Header("Small Enemy Settings")]
        public float smallEnemyHealth = 50f;
        public float smallEnemySpeed = 7f;
        public float smallEnemyLifetime = 10f;

        [Header("Game Balance")]
        public int pointsPerSecond = 10;
        public int pointsPerQTESuccess = 100;
        public int pointsPerEnemyKill = 50;

        [Header("Visual Effects")]
        public float styleSlowMotionScale = 0.3f;
        public float styleDuration = 2f;
    }
}
