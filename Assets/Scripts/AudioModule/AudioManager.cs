using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace AudioModule
{
    public class AudioManager : MonoBehaviour
    {
        private Dictionary<FMOD.GUID, SoundPool> soundPools = new Dictionary<FMOD.GUID, SoundPool>();

        /// <summary>
        /// Initializes a sound pool for a given sound event.
        /// </summary>
        /// <param name="soundPath">The FMOD event reference.</param>
        /// <param name="initialCount">Initial pool size.</param>
        /// <returns>The initialized SoundPool instance.</returns>
        public SoundPool InitializeSoundPool(EventReference soundPath, int initialCount = 5)
        {
            if (soundPath.Guid.IsNull)
            {
                Debug.LogWarning("Sound reference is empty!");
                return null;
            }

            if (!soundPools.ContainsKey(soundPath.Guid))
            {
                soundPools[soundPath.Guid] = new SoundPool(soundPath, initialCount);
            }

            return soundPools[soundPath.Guid];
        }

        /// <summary>
        /// Plays a sound either as a one-shot or using a pooled instance.
        /// </summary>
        /// <param name="soundPath">The FMOD event reference.</param>
        /// <param name="useInstance">If true, uses a pooled instance.</param>
        /// <param name="position">Optional position for 3D sound.</param>
        public void PlaySound(EventReference soundPath, bool useInstance = false, Vector3? position = null)
        {
            if (soundPath.Guid.IsNull)
            {
                Debug.LogWarning("Sound reference is empty!");
                return;
            }

            if (!useInstance)
            {
                // Play as a one-shot sound
                if (position.HasValue)
                    RuntimeManager.PlayOneShot(soundPath, position.Value);
                else
                    RuntimeManager.PlayOneShot(soundPath);
            }
            else
            {
                // Use a sound pool
                if (!soundPools.ContainsKey(soundPath.Guid))
                {
                    Debug.LogWarning($"Sound pool for {soundPath.Guid} not found. Initializing new pool.");
                    InitializeSoundPool(soundPath);
                }

                var instance = soundPools[soundPath.Guid].GetInstance();

                // Set 3D attributes
                if (position.HasValue)
                    instance.set3DAttributes(RuntimeUtils.To3DAttributes(position.Value));
                else
                    instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));

                instance.start();

                // Return to pool after playback
                StartCoroutine(ReturnToPoolAfterPlay(instance, soundPath.Guid));
            }
        }

        /// <summary>
        /// Stops all instances of a given sound.
        /// </summary>
        /// <param name="soundPath">The FMOD event reference.</param>
        public void StopSound(EventReference soundPath)
        {
            if (soundPath.Guid.IsNull)
            {
                Debug.LogWarning("Sound reference is empty!");
                return;
            }

            if (soundPools.ContainsKey(soundPath.Guid))
            {
                soundPools[soundPath.Guid].StopAll();
            }
            else
            {
                Debug.LogWarning($"Sound pool for {soundPath.Guid} not found.");
            }
        }

        /// <summary>
        /// Stops all currently playing sounds.
        /// </summary>
        public void StopAllSounds()
        {
            foreach (var pool in soundPools.Values)
            {
                pool.StopAll();
            }
        }

        /// <summary>
        /// Returns a sound instance to the pool after it finishes playing.
        /// </summary>
        private IEnumerator ReturnToPoolAfterPlay(EventInstance instance, FMOD.GUID soundGuid)
        {
            if (!instance.isValid())
            {
                yield break;
            }

            instance.getDescription(out var eventDescription);

            if (eventDescription.isValid())
            {
                eventDescription.getLength(out int length);
                yield return new WaitForSeconds(length / 1000f);
            }

            if (soundPools.ContainsKey(soundGuid))
            {
                soundPools[soundGuid].ReturnInstance(instance);
            }
        }

        /// <summary>
        /// Plays a sound and returns an EventInstance.
        /// </summary>
        /// <param name="soundPath">The FMOD event reference.</param>
        /// <param name="useInstance">If true, uses a pooled instance.</param>
        /// <param name="position">Optional position for 3D sound.</param>
        /// <returns>The playing EventInstance.</returns>
        public EventInstance PlaySoundWithInstance(EventReference soundPath, bool useInstance = false,
            Vector3? position = null)
        {
            if (soundPath.Guid.IsNull)
            {
                Debug.LogWarning("Sound reference is empty!");
                return default;
            }

            if (!useInstance)
            {
                // Play as a one-shot sound
                if (position.HasValue)
                    RuntimeManager.PlayOneShot(soundPath, position.Value);
                else
                    RuntimeManager.PlayOneShot(soundPath);

                return default;
            }
            else
            {
                // Use a sound pool
                if (!soundPools.ContainsKey(soundPath.Guid))
                {
                    Debug.LogWarning($"Sound pool for {soundPath.Guid} not found. Initializing new pool.");
                    InitializeSoundPool(soundPath);
                }

                var instance = soundPools[soundPath.Guid].GetInstance();

                // Set 3D attributes
                if (position.HasValue)
                    instance.set3DAttributes(RuntimeUtils.To3DAttributes(position.Value));
                else
                    instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));

                instance.start();

                return instance; // Return the instance
            }
        }
    }
}
