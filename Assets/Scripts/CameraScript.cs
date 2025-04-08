using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private Transform camTransform;
    private Transform bikeTransform;
    private Vector3 desiredOffset;
    private Quaternion desiredRotation;
    [SerializeField] float sensitivity = 20;

    void Awake()
    {
        camTransform = GetComponent<Transform>();
        bikeTransform = GameObject.FindWithTag("Bike").GetComponent<Rigidbody>().transform;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        desiredOffset = camTransform.position - bikeTransform.position;
        desiredRotation = camTransform.rotation;
    }

    void Update()
    {
        camTransform.position = bikeTransform.position + desiredOffset;

        camTransform.eulerAngles = new Vector3(desiredRotation.eulerAngles.x, desiredRotation.eulerAngles.y, 0f);

        camTransform.RotateAround(bikeTransform.position, Vector3.up, Input.mousePositionDelta.x * Time.deltaTime * sensitivity);
        camTransform.RotateAround(bikeTransform.position, camTransform.right, -Input.mousePositionDelta.y * Time.deltaTime * sensitivity);

        desiredOffset = camTransform.position - bikeTransform.position;
        desiredRotation = camTransform.rotation;
    }
}
