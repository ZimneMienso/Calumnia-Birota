using UnityEngine;

public class Target : MonoBehaviour, ITarget
{
    public void GetHit() 
    { 
         Debug.Log(gameObject.name + " got hit"); 
    }

    public void GetSpiked(Transform spike) {
        transform.parent = spike;
        transform.localPosition = new Vector3(0, 0, 0);
        DisableAllColliders(gameObject);
    }

    public Transform Transform => transform;

    public void DisableAllColliders(GameObject obj)
    {
        Collider[] colliders = obj.GetComponents<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        foreach (Transform child in obj.transform)
        {
            DisableAllColliders(child.gameObject);
        }
    }
}
