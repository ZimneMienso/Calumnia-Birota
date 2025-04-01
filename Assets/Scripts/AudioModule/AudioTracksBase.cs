using FMODUnity;
using UnityEngine;

namespace AudioModule
{
    public class AudioTracksBase : MonoBehaviour
    {
        [SerializeField] private AudioManager audioManager;

        // New sounds
        public EventReference bicycleRolling;    // Sound of a bicycle rolling on a surface
        public EventReference bicycleFall;       // Sound of a bicycle falling from a height
        public EventReference bicycleChain;      // Sound of a bicycle chain
        public EventReference bicycleBrake;      // Sound of bicycle brakes
        public EventReference swordHitWood;      // Sound of a sword hitting wood
        public EventReference swordHitMetal;     // Sound of a sword hitting metal
        public EventReference menuButtonClick;   // Sound of pressing a button in the menu
        public EventReference mouseClick;        // Sound of a mouse click

        private void Start()
        {
            // Initialize sound pools for new sounds
            audioManager.InitializeSoundPool(bicycleRolling, 5);
            audioManager.InitializeSoundPool(bicycleFall, 5);
            audioManager.InitializeSoundPool(bicycleChain, 5);
            audioManager.InitializeSoundPool(bicycleBrake, 5);
            audioManager.InitializeSoundPool(swordHitWood, 5);
            audioManager.InitializeSoundPool(swordHitMetal, 5);
            audioManager.InitializeSoundPool(menuButtonClick, 5);
            audioManager.InitializeSoundPool(mouseClick, 5);
        }
    }
}
