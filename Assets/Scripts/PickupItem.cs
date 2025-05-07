using UnityEngine;
using UnityEngine.InputSystem;

public class PickupItem : MonoBehaviour
{
    [SerializeField] private WeaponItem itemPrefab;
    // [SerializeField] private Vector3 offset;
    // [SerializeField] private float UI_orbitRadius = 2f;

    // private GameObject pickupUI;
    private bool isInZone=false;
    private InputAction pickAction;
    private Player player;



    void Start()
    {
        // pickupUI = GameObject.FindWithTag("PickupUI").transform.GetChild(0).gameObject;
        pickAction=InputSystem.actions.FindAction("PickupItem");
        pickAction.started += PickUp;
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isInZone = true;
            // pickupUI.SetActive(true);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isInZone = false;
            // pickupUI.SetActive(false);
        }
    }


    private void PickUp(InputAction.CallbackContext context)
    {
        if (!isInZone) return;

        // pickupUI.SetActive(false);
        bool isPicked = player.Pickup(itemPrefab);
        if (!isPicked) return;
        pickAction.started -= PickUp;
        Destroy(transform.parent.gameObject);

        // // Calculate direction from item to player
        // Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;

        // // Position the pickupUI at a fixed radius in the direction the player came from
        // pickupUI.transform.position = transform.position + directionToPlayer * UI_orbitRadius + offset;

        // // Make the UI face the player
        // pickupUI.transform.LookAt(Camera.main.transform);
        // // Optional: lock Y rotation to keep it upright
        // Vector3 euler = pickupUI.transform.eulerAngles;
        // pickupUI.transform.eulerAngles = new Vector3(0f, euler.y, 0f);
    }
}
