using UnityEngine;

public class WeaponItem : MonoBehaviour
{
    [SerializeField] private GameObject pickupPrefab;


    public void Drop(Vector3 pos)
    {
        Instantiate(pickupPrefab).transform.position = pos;
        Destroy(gameObject);
    }
}
