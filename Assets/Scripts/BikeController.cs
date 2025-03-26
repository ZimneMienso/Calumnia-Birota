using System.Collections.Generic;
using UnityEngine;

public class BikeController : MonoBehaviour
{
    [SerializeField] Transform wheelFront;
    [SerializeField] Transform wheelBack;
    [Space]
    [SerializeField] float acceleration = 5;
    [SerializeField] float maxSpeed = 30f;
    [SerializeField] AnimationCurve accelerationCurve;
    [SerializeField] float gravityForce = 1f;
    [Space]
    [SerializeField] float antiSlipForce = 1f;
    [Space]
    [SerializeField] float wheelRadius = 0.5f;
    [SerializeField] float wheelOverreach = 0.05f;
    [SerializeField] float wheelSpring = 10f;
    [SerializeField] float wheelDamping = 5f;
    // [Space]
    // [SerializeField] float supportWidth = 0.5f;
    // [SerializeField] float supportRadius = 0.5f;
    // [SerializeField] float supportSpring = 10f;
    // [SerializeField] float supportDamping = 5f;
    [Space]
    [SerializeField] float leanSpeed = 1f;
    [SerializeField] float leanSpring = 1f;
    [SerializeField] float leanDamping = 1f;

    private Rigidbody rb;
    private Vector3 moveInput;
    private Vector3 groundNormal;
    private bool groundedFront, groundedBack;
    private float leanAngle;



    private void Update()
    {
        moveInput = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0,
            Input.GetAxisRaw("Vertical")
        );
    }


    private void Awake() 
    {
        rb = GetComponent<Rigidbody>();
    }


    private void FixedUpdate() 
    {
        HandleWheels();

        HandleAcceleration();
        //TODO braking;
        HandleSteering();
        PreventSlip();
        HandleLean();
        HandleGravity();

    }


    private void HandleWheels()
    {
        RaycastHit hit;
        Vector3 normalFront = Vector3.zero;
        Vector3 normalBack = Vector3.zero;
        groundedFront = groundedBack = false;

        Debug.DrawRay(wheelFront.position, -wheelFront.up * (wheelRadius+wheelOverreach), Color.red);
        if (Physics.Raycast(wheelFront.position, -wheelFront.up, out hit, wheelRadius+wheelOverreach))
        {
            normalFront = hit.normal;
            groundedFront = true;

            float distance = Vector3.Distance(wheelFront.position, hit.point);
            float wheelVelocityUp = Vector3.Dot(rb.GetPointVelocity(wheelFront.position), wheelFront.up);
            float displacement = wheelRadius - distance;
            
            Vector3 springForce = wheelFront.up * displacement * wheelSpring;
            Vector3 dampingForce = -wheelVelocityUp * wheelDamping * wheelFront.up;
            Debug.DrawRay(wheelFront.position, springForce, Color.green);
            Debug.DrawRay(wheelFront.position, dampingForce, Color.blue);

            Vector3 force = springForce + dampingForce;
            rb.AddForceAtPosition(force, wheelFront.position, ForceMode.VelocityChange);
        }
        
        Debug.DrawRay(wheelBack.position, -wheelBack.up * (wheelRadius+wheelOverreach), Color.red);
        if (Physics.Raycast(wheelBack.position, -wheelBack.up, out hit, wheelRadius+wheelOverreach))
        {
            normalBack = hit.normal;
            groundedBack = true;

            float distance = Vector3.Distance(wheelBack.position, hit.point);
            float wheelVelocityUp = Vector3.Dot(rb.GetPointVelocity(wheelBack.position), wheelBack.up);
            float displacement = wheelRadius - distance;
            
            Vector3 springForce = wheelBack.up * displacement * wheelSpring;
            Vector3 dampingForce = -wheelVelocityUp * wheelDamping * wheelBack.up;
            Debug.DrawRay(wheelBack.position, springForce, Color.green);
            Debug.DrawRay(wheelBack.position, dampingForce, Color.blue);

            Vector3 force = springForce + dampingForce;
            rb.AddForceAtPosition(force, wheelBack.position, ForceMode.VelocityChange);
        }

        groundNormal = ((normalFront + normalBack) / 2).normalized;
    }


    private void RedirectForce()
    {
        
    }


    private void HandleAcceleration()
    {
        if(!groundedBack) return;

        float speed = Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.forward));
        float force = accelerationCurve.Evaluate(speed / maxSpeed) * acceleration;
        Debug.Log(speed);
        rb.AddForce(transform.forward * moveInput.z * force, ForceMode.VelocityChange);
    }


    private void PreventSlip()
    {
        if(groundedFront)
        {
            Vector3 rightVelocity = Vector3.Dot(rb.GetPointVelocity(wheelFront.position), wheelFront.right) * wheelFront.right;
            rb.AddForceAtPosition(-rightVelocity * antiSlipForce, wheelFront.position, ForceMode.VelocityChange);
        }
        if(groundedBack)
        {
            Vector3 rightVelocity = Vector3.Dot(rb.GetPointVelocity(wheelBack.position), wheelBack.right) * wheelBack.right;
            rb.AddForceAtPosition(-rightVelocity * antiSlipForce, wheelBack.position, ForceMode.VelocityChange);
        }
    }


    private void HandleSteering()
    {
        float angle = moveInput.x * 30;
        wheelFront.localEulerAngles = new Vector3(0, angle, 0);
    }


    private void HandleLean()
    {
        leanAngle = Mathf.Lerp(leanAngle, -moveInput.x * 30, leanSpeed);
        Vector3 targetNormal = Quaternion.AngleAxis(leanAngle, transform.forward) * groundNormal;
        float angleDif = Vector3.SignedAngle(transform.up, targetNormal, transform.forward);

        float springForce = angleDif * leanSpring;
        float dampenForce = -Vector3.Dot(rb.angularVelocity, transform.forward) * leanDamping;
        float leanForce = springForce + dampenForce;

        rb.AddTorque(transform.forward * leanForce, ForceMode.VelocityChange);
    }


    private void HandleGravity()
    {
        if(groundedFront || groundedBack)
        {
            rb.AddForce(-groundNormal * gravityForce, ForceMode.VelocityChange);
        }
        else
        {
            rb.AddForce(Vector3.down * gravityForce, ForceMode.VelocityChange);
        }
    }
}
