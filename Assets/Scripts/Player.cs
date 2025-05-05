using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private WeaponItem startingSword;
    [SerializeField] private Transform weaponHolder;

    private WeaponItem heldItem;

    private void Start()
    {
        Pickup(startingSword);
    }


    public void Pickup(WeaponItem itemPrefab)
    {
        if(heldItem) heldItem.Drop(transform.position);

        heldItem = Instantiate(itemPrefab);
        heldItem.transform.parent = weaponHolder;
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;
    }


    public void Drop()
    {
        if(heldItem) heldItem.Drop(transform.position);
        heldItem = null;
    }
}
