using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using AudioModule;

public class BikeAudioController : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private BikeController bikeController;
    [SerializeField] private EventReference rollingSound;
    
    [Tooltip("Минимальная скорость для начала воспроизведения звука")]
    [SerializeField] private float minSpeedThreshold = 0.5f;
    
    [Tooltip("Параметр громкости звука в зависимости от скорости")]
    [SerializeField] private float volumeMultiplier = 1f;
    
    [Tooltip("Максимальная скорость для масштабирования звука")]
    [SerializeField] private float maxSpeedForScale = 20f;

    private EventInstance rollingSoundInstance;
    private bool isSoundPlaying = false;
    private Rigidbody bikeRigidbody;

    private void Start()
    {
        // Получаем Rigidbody велосипеда, если он не был назначен через инспектор
        if (bikeController != null && bikeRigidbody == null)
        {
            bikeRigidbody = bikeController.GetComponent<Rigidbody>();
        }
        
        // Инициализация пула звуков
        if (audioManager != null)
        {
            audioManager.InitializeSoundPool(rollingSound, 1);
        }
        else
        {
            Debug.LogError("AudioManager не назначен в BikeAudioController!");
        }
    }

    private void Update()
    {
        if (bikeRigidbody == null) return;

        float currentSpeed = bikeRigidbody.linearVelocity.magnitude;
        
        // Если скорость выше порога и звук не воспроизводится - запускаем звук
        if (currentSpeed > minSpeedThreshold && !isSoundPlaying)
        {
            StartRollingSound();
        }
        // Если скорость ниже порога и звук воспроизводится - останавливаем звук
        else if (currentSpeed <= minSpeedThreshold && isSoundPlaying)
        {
            StopRollingSound();
        }
        // Если звук воспроизводится - обновляем параметры звука в зависимости от скорости
        else if (isSoundPlaying)
        {
            UpdateRollingSoundParameters(currentSpeed);
        }
    }

    private void StartRollingSound()
    {
        if (audioManager == null) return;
        
        // Воспроизводим звук с использованием инстанса для возможности управления
        rollingSoundInstance = audioManager.PlaySoundWithInstance(rollingSound, true, transform.position);
        
        if (rollingSoundInstance.isValid())
        {
            isSoundPlaying = true;
            
            // Устанавливаем начальные параметры звука
            float currentSpeed = bikeRigidbody.linearVelocity.magnitude;
            UpdateRollingSoundParameters(currentSpeed);
        }
    }

    private void StopRollingSound()
    {
        if (rollingSoundInstance.isValid())
        {
            // Останавливаем звук
            rollingSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            isSoundPlaying = false;
        }
    }

    private void UpdateRollingSoundParameters(float currentSpeed)
    {
        if (!rollingSoundInstance.isValid()) return;
        
        // Пример изменения параметра "Speed" звука в зависимости от скорости велосипеда
        // Нормализуем скорость от 0 до 1 в пределах maxSpeedForScale
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / maxSpeedForScale);
        
        // Устанавливаем параметр "Speed" для звука
        rollingSoundInstance.setParameterByName("Speed", normalizedSpeed);
        
        // Также можно управлять громкостью звука
        float volume = normalizedSpeed * volumeMultiplier;
        rollingSoundInstance.setVolume(volume);
        
        // Обновляем позицию звука при движении
        rollingSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
    }

    private void OnDestroy()
    {
        // Останавливаем звук при уничтожении объекта
        StopRollingSound();
    }
}