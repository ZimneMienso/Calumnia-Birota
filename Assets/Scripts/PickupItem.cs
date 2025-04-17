using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;

public class PickupItem : MonoBehaviour
{
    private GameObject player;
    private GameObject pickupUI;
    public Vector3 offset;
    private bool isInZone=false;
    private InputAction pickAction;

    public GameObject pickedUpItem;
    public float UI_orbitRadius=2f;
    private InventoryManager inventoryManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventoryManager = GameObject.FindWithTag("InventoryManager").GetComponent<InventoryManager>();
        player = GameObject.FindWithTag("Bike");
        pickupUI = GameObject.FindWithTag("PickupUI").transform.GetChild(0).gameObject;
        pickAction=InputSystem.actions.FindAction("PickupItem");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag=="Bike")
        {
            player = other.gameObject;
            isInZone = true;
            pickupUI.SetActive(true);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Bike")
        {
            pickupUI.SetActive(false);
            isInZone = false;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (isInZone)
        {
            if (pickAction.WasPerformedThisFrame())
            {
                inventoryManager.EquipWeapon(pickedUpItem);
                pickupUI.SetActive(false);
                transform.parent.gameObject.SetActive(false);
            }
            // Calculate direction from item to player
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;

            // Position the pickupUI at a fixed radius in the direction the player came from
            pickupUI.transform.position = transform.position + directionToPlayer * UI_orbitRadius + offset;

            // Make the UI face the player
            pickupUI.transform.LookAt(Camera.main.transform);
            // Optional: lock Y rotation to keep it upright
            Vector3 euler = pickupUI.transform.eulerAngles;
            pickupUI.transform.eulerAngles = new Vector3(0f, euler.y, 0f);
        }
    }
}
