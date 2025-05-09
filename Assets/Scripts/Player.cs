using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private WeaponItem defaultWeapon;
    [SerializeField] private Transform weaponHolder;

    private WeaponItem heldItem = null;
    private WeaponItem heldPrefabItem = null;

    private void Start()
    {
        Pickup(defaultWeapon);
    }


    public bool Pickup(WeaponItem itemPrefab)
    {
        if (heldPrefabItem && itemPrefab == heldPrefabItem) return false;

        if (heldItem && heldPrefabItem != defaultWeapon) heldItem.Drop(transform.position);
        else if (heldPrefabItem == defaultWeapon) Destroy(heldItem.gameObject);

        heldPrefabItem = itemPrefab;
        heldItem = Instantiate(itemPrefab);
        heldItem.transform.parent = weaponHolder;
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;
        return true;
    }


    public void Drop()
    {
        if (heldItem && heldPrefabItem != defaultWeapon) heldItem.Drop(transform.position);
        else if (heldPrefabItem == defaultWeapon) Destroy(heldItem.gameObject);
        heldItem = null;
        heldPrefabItem = null;
        Pickup(defaultWeapon);
    }

    public void DropUsedItem()
    {
        if (heldItem) heldItem.DropUsedItem();
        heldItem = null;
        heldPrefabItem = null;
        Pickup(defaultWeapon);
    }
}
