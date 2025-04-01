using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace AudioModule
{
    /// <summary>
    /// Manages a pool of FMOD event instances to optimize sound playback.
    /// </summary>
    public class SoundPool
    {
        private Queue<EventInstance> pool = new Queue<EventInstance>(); // Queue of reusable sound instances
        private List<EventInstance> activeInstances = new List<EventInstance>(); // Currently active instances
        private EventReference eventReference; // FMOD event reference

        /// <summary>
        /// Initializes the sound pool with a given number of instances.
        /// </summary>
        /// <param name="eventRef">FMOD event reference</param>
        /// <param name="initialCount">Number of instances to pre-create</param>
        public SoundPool(EventReference eventRef, int initialCount = 5)
        {
            if (eventRef.Guid.IsNull)
            {
                Debug.Log("Sound Reference is empty");
                return;
            }

            this.eventReference = eventRef;

            for (int i = 0; i < initialCount; i++)
            {
                var instance = RuntimeManager.CreateInstance(eventRef);
                Set3DAttributes(instance, Vector3.zero); // Set default 3D attributes
                pool.Enqueue(instance);
            }
        }

        /// <summary>
        /// Retrieves an instance from the pool or creates a new one if necessary.
        /// </summary>
        /// <param name="position">3D position of the sound</param>
        /// <returns>FMOD event instance</returns>
        public EventInstance GetInstance(Vector3 position = default)
        {
            EventInstance instance;

            if (pool.Count > 0)
            {
                instance = pool.Dequeue();
            }
            else
            {
                instance = RuntimeManager.CreateInstance(eventReference);
            }

            Set3DAttributes(instance, position); // Apply 3D attributes
            activeInstances.Add(instance); // Add to active list
            return instance;
        }

        /// <summary>
        /// Returns an instance to the pool after playback.
        /// </summary>
        /// <param name="instance">The FMOD event instance to return</param>
        public void ReturnInstance(EventInstance instance)
        {
            if (instance.isValid())
            {
                instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                Set3DAttributes(instance, Vector3.zero); // Reset 3D attributes
                activeInstances.Remove(instance); // Remove from active list
                pool.Enqueue(instance); // Return to pool
            }
        }

        /// <summary>
        /// Stops all active sounds and returns them to the pool.
        /// </summary>
        public void StopAll()
        {
            // Stop all active instances
            foreach (var instance in activeInstances)
            {
                if (instance.isValid())
                {
                    instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                }
            }

            // Return all active instances to the pool
            List<EventInstance> instancesToReturn = new List<EventInstance>(activeInstances);
            foreach (var instance in instancesToReturn)
            {
                ReturnInstance(instance);
            }
        }

        /// <summary>
        /// Clears the sound pool, stopping all sounds and releasing memory.
        /// </summary>
        public void ClearPool()
        {
            // Stop all active sounds
            StopAll();

            // Release all pooled instances
            while (pool.Count > 0)
            {
                var instance = pool.Dequeue();
                if (instance.isValid())
                {
                    instance.release();
                }
            }
        }

        /// <summary>
        /// Sets the 3D attributes of an FMOD event instance.
        /// </summary>
        /// <param name="instance">FMOD event instance</param>
        /// <param name="position">3D position of the sound</param>
        private void Set3DAttributes(EventInstance instance, Vector3 position)
        {
            if (instance.isValid())
            {
                var attributes = RuntimeUtils.To3DAttributes(position);
                instance.set3DAttributes(attributes);
            }
        }
    }
}
