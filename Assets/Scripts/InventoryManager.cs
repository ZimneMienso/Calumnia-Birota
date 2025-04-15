using UnityEngine;
using System.Collections.Generic;
public class InventoryManager : MonoBehaviour
{
    private GameObject currentUsingObject;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void EquipWeapon(GameObject newWeapon)
    {
        if (currentUsingObject!=null)
        {

        currentUsingObject.SetActive(false);
        }
        currentUsingObject = newWeapon;
        currentUsingObject.SetActive(true);
    }
}
