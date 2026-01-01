using UnityEngine;

namespace StrangeSpace
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Strange Space/GameSettings", order = 1)]
    public class GameSettings : ScriptableObject
    {
        [Header("Game Settings")]
        [SerializeField]
        public float TapMaxTime = 0.1f;
    }
}