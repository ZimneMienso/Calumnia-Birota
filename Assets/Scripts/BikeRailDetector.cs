using System;
using UnityEngine;

public class BikeRailDetector : MonoBehaviour
{
    [SerializeField] LayerMask railLayer;

    public event Action<Transform> OnEnterRail;
    public event Action<Transform> OnExitRail;



    void OnTriggerEnter(Collider other)
    {
        Debug.Log((railLayer.value & (1 << other.gameObject.layer)) != 0);
        if((railLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            OnEnterRail?.Invoke(other.transform);
        }
    }


    void OnTriggerExit(Collider other)
    {
        if((railLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            OnExitRail?.Invoke(other.transform);
        }
    }
}
