using UnityEngine;

public class Target : MonoBehaviour, ITarget
{
    [SerializeField] GameObject explosionPrefab;
    public void GetHit() 
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public void GetSpiked(Transform spike) {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
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
