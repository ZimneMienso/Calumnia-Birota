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
    [Space]
    [SerializeField] float leanSpeed = 1f;
    [SerializeField] float leanSpring = 1f;
    [SerializeField] float leanDamping = 1f;
    [Space]
    [SerializeField] float rampCorrectiveTorque = 1f;
    [Space]
    [SerializeField] float airControllSpeed = 1f;
    [SerializeField] float airControllAcceleration = 0.2f;
    [Space]
    [SerializeField] float breakForce = 1f;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool groundedFront, groundedBack;
    private float leanAngle;
    private Vector3 normalFront = Vector3.zero;
    private Vector3 normalBack = Vector3.zero;
    private Vector3 groundNormal;
    private Vector3 lastFrontNormal = Vector3.up;



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
        RedirectMomentum();
        HandleAcceleration();
        HandleBreaking();
        HandleSteering();
        PreventSlip();
        HandleLean();
        HandleGravity();
        AirControll();

        lastFrontNormal = normalFront;
    }


    private void HandleWheels()
    {
        RaycastHit hit;
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
            force = Vector3.Dot(force, normalBack) * normalBack;
            rb.AddForceAtPosition(force, wheelFront.position, ForceMode.VelocityChange);
        }
        else
        {
            normalFront = Vector3.zero;
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
            force = Vector3.Dot(force, normalFront) * normalFront;
            rb.AddForceAtPosition(force, wheelBack.position, ForceMode.VelocityChange);
        }
        else
        {
            normalBack = Vector3.zero;
        }

        groundNormal = ((normalFront + normalBack) / 2).normalized;
    }


    private void RedirectMomentum()
    {
        if(groundedFront && groundedBack)
        {
            // Make going up ramps smoother, by applying some torque and changing the velocity direction

            Vector3 torque = Vector3.Cross(lastFrontNormal, normalFront) * rampCorrectiveTorque;
            rb.AddTorque(torque, ForceMode.VelocityChange);

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, groundNormal);
            rb.linearVelocity = projectedVelocity;
        }
    }


    private void HandleAcceleration()
    {
        if(!groundedBack) return;

        float speed = Mathf.Abs(rb.linearVelocity.magnitude);
        float force = accelerationCurve.Evaluate(speed / maxSpeed) * acceleration;
        //Debug.Log(speed);
        rb.AddForce(transform.forward * moveInput.z * force, ForceMode.VelocityChange);
    }


    private void HandleBreaking()
    {
        if(moveInput.z == 0) return;
        float forwardVelocity = Vector3.Dot(transform.forward, rb.linearVelocity);
        if(Mathf.Sign(moveInput.z) == Mathf.Sign(forwardVelocity)) return;

        // Break
        if(groundedFront)
        {
            float zVel = Vector3.Dot(wheelFront.forward, rb.linearVelocity);
            Vector3 force = -wheelFront.forward * zVel * breakForce;
            rb.AddForceAtPosition(force, wheelFront.position);
        }
        
        if(groundedBack)
        {
            float zVel = Vector3.Dot(wheelBack.forward, rb.linearVelocity);
            Vector3 force = -wheelBack.forward * zVel * breakForce;
            rb.AddForceAtPosition(force, wheelBack.position);
        }
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
        if(groundedBack || groundedFront)
        {
            leanAngle = Mathf.Lerp(leanAngle, -moveInput.x * 30, leanSpeed);
            Vector3 targetNormal = Quaternion.AngleAxis(leanAngle, transform.forward) * groundNormal;
            float angleDif = Vector3.SignedAngle(transform.up, targetNormal, transform.forward);

            float springForce = angleDif * leanSpring;
            float dampenForce = -Vector3.Dot(rb.angularVelocity, transform.forward) * leanDamping;
            float leanForce = springForce + dampenForce;

            rb.AddTorque(transform.forward * leanForce, ForceMode.VelocityChange);
        }
    }


    private void HandleGravity()
    {
        if(groundedFront || groundedBack)
        {
            rb.AddForce(-groundNormal * 0.5f*gravityForce, ForceMode.VelocityChange);
            rb.AddForce(Vector3.down * 0.5f*gravityForce, ForceMode.VelocityChange);
        }
        else
        {
            rb.AddForce(Vector3.down * gravityForce, ForceMode.VelocityChange);
        }
    }


    private void AirControll()
    {
        if(groundedBack || groundedFront) return;
        
        Vector3 xTarget = airControllSpeed * moveInput.z * transform.right;
        Vector3 yTarget = airControllSpeed * moveInput.x * transform.up;
        Vector3 targetAngularVelocity = xTarget + yTarget;
        
        rb.angularVelocity = Vector3.Slerp(rb.angularVelocity, targetAngularVelocity, airControllAcceleration);
    }
}
