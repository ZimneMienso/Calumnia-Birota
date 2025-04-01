using FMOD.Studio;
using FMODUnity;
using UnityEngine;
namespace AudioModule.AudioPlayrs
{
    public class RollingSound : MonoBehaviour
    {
        [SerializeField] private AudioTracksBase audioTracksBase; // Reference to rolling sound event
        [SerializeField] private Rigidbody2D bikeRigidbody; // Reference to the bike's Rigidbody2D
        [SerializeField] private float speedMultiplier = 1.0f; // Multiplier to control playback speed

        private EventInstance rollingSoundInstance;

        private void Start()
        {
            if (!audioTracksBase.bicycleRolling.IsNull)
            {
                rollingSoundInstance = RuntimeManager.CreateInstance(audioTracksBase.bicycleRolling);
                rollingSoundInstance.start();
            }
            else
            {
                Debug.LogWarning("Rolling sound event reference is empty!");
            }
        }

        private void Update()
        {
            if (!rollingSoundInstance.isValid()) return;

            float speed = bikeRigidbody.linearVelocity.magnitude;

            if (speed > 0.1f) // Play sound only if the bike is moving
            {
                rollingSoundInstance.setParameterByName("Speed", speed * speedMultiplier);
            }
            else
            {
                rollingSoundInstance.setParameterByName("Speed", 0f);
            }
        }

        private void OnDestroy()
        {
            if (rollingSoundInstance.isValid())
            {
                rollingSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                rollingSoundInstance.release();
            }
        }
    }
}
