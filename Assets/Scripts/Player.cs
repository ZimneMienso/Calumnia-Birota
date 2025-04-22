using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private List<GameObject> itemPrefabs = new();

    
    public void Pickup(GameObject itemPrefab)
    {
        itemPrefabs.Add(itemPrefab);

        Debug.Log(itemPrefabs);
    }
}
