using System.Collections.Generic;
using UnityEngine;

public class BikeController : MonoBehaviour
{
    [SerializeField] Transform wheelFront;
    [SerializeField] Transform wheelBack;
    [SerializeField] BikeRailDetector railDetector;
    [SerializeField] LayerMask groundLayer;
    [Space]
    [SerializeField] float acceleration = 5;
    [SerializeField] float maxSpeed = 30f;
    [SerializeField] AnimationCurve accelerationCurve;
    [SerializeField] float gravityForce = 1f;
    [Space]
    [SerializeField] float maxSteer = 30f;
    [SerializeField] float minSteer = 10f;
    [SerializeField] float steerAcceleration = .2f;
    [Space]
    [SerializeField] float antiSlipForce = 1f;
    [Space]
    [SerializeField] float wheelRadius = 0.5f;
    [SerializeField] float wheelOverreach = 0.05f;
    [SerializeField] float wheelSpring = 10f;
    [SerializeField] float wheelDamping = 5f;
    [Space]
    [SerializeField] float maxLean = 30f;
    [SerializeField] float minLean = 10f;
    [SerializeField] float leanSpeed = 1f;
    [SerializeField] float leanSpring = 1f;
    [SerializeField] float leanDamping = 1f;
    [Space]
    [SerializeField] float airRollStabilisation = 1f;
    [SerializeField] float airControllSpeed = 1f;
    [SerializeField] float airControllAcceleration = 0.2f;
    [Space]
    [SerializeField] float breakForce = 1f;
    [Space]
    [SerializeField] float jumpForce = 3f;
    [Space]
    [SerializeField] float minRailSpeed = 5f;
    [SerializeField] float railCorrectionSpring = 5f;
    [SerializeField] float railCorrectionDampen = 5f;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool wantsJump;
    private bool groundedFront, groundedBack;
    private float steerAngle;
    private float leanAngle;
    private Vector3 normalFront = Vector3.zero;
    private Vector3 normalBack = Vector3.zero;
    private Vector3 groundNormal;
    private float maxSpeedPercent;
    private bool grinding = false;
    private float grindingDirForward;
    private float grindingDirSide;
    private Transform grindingRail;



    private void Awake() 
    {
        rb = GetComponent<Rigidbody>();
    }


    private void OnEnable()
    {
        railDetector.OnEnterRail += EnterRail;
        railDetector.OnExitRail += ExitRail;
    }


    private void OnDisable()
    {
        railDetector.OnEnterRail -= EnterRail;
        railDetector.OnExitRail -= ExitRail;
    }


    private void Update()
    {
        moveInput = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0,
            Input.GetAxisRaw("Vertical")
        );
        if(Input.GetKeyUp(KeyCode.Space))
        {
            wantsJump = true;
        }
    }


    private void FixedUpdate() 
    {
        maxSpeedPercent = Mathf.InverseLerp(0, maxSpeed, rb.linearVelocity.magnitude);

        HandleSteering();
        if(grinding)
        {
            //TODO clean up into seperate function
            // redirect velocity
            rb.linearVelocity = grindingDirForward * grindingRail.forward * rb.linearVelocity.magnitude;

            Vector3 positionDif = grindingRail.position - railDetector.transform.position;
            positionDif = Vector3.ProjectOnPlane(positionDif, grindingRail.forward);
            rb.MovePosition(transform.position + positionDif);
            
            // stabilise rotation
            Quaternion targetRot = Quaternion.LookRotation(grindingDirSide * grindingRail.right, grindingRail.up);
            Quaternion deltaRotation = targetRot * Quaternion.Inverse(transform.rotation);
            deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);
            rotationAxis.Normalize();
            Vector3 springForce = rotationAxis * angleInDegrees * railCorrectionSpring;
            Vector3 dampenForce = -rb.angularVelocity * railCorrectionDampen;
            rb.AddTorque(springForce + dampenForce, ForceMode.VelocityChange);

            // allow to jump
        }
        else
        {
            HandleWheels();
            HandleAcceleration();
            HandleBreaking();
            PreventSlip();
            HandleLean();
            HandleGravity();
            AirControll();
        }
        Jump();
    }


    private void HandleWheels()
    {
        groundedFront = groundedBack = false;
        RaycastHit closestHit = new RaycastHit();
        Vector3 closestHitDir = Vector3.zero;
        RaycastHit hit;
        bool didHit;

        didHit = false;
        closestHit.distance = 10*wheelRadius;
        for (int i = 0; i < 16; i++)
        {
            float angle = 360/16 * i;
            Vector3 dir = Quaternion.AngleAxis(angle, wheelFront.right) * -wheelFront.up;
            if(Physics.Raycast(wheelFront.position, dir, out hit, wheelRadius+wheelOverreach, groundLayer))
            {
                didHit = true;
                if(hit.distance >= closestHit.distance) continue;
                closestHit = hit;
                closestHitDir = dir;
            }
        }
        if (didHit)
        {
            normalFront = closestHit.normal;
            groundedFront = true;

            float distance = Vector3.Distance(wheelFront.position, closestHit.point);
            float wheelVelocityUp = Vector3.Dot(rb.GetPointVelocity(wheelFront.position), -closestHitDir);
            float displacement = wheelRadius - distance;
            
            Vector3 springForce = -closestHitDir * displacement * wheelSpring;
            Vector3 dampingForce = -wheelVelocityUp * wheelDamping * -closestHitDir;

            Vector3 force = springForce + dampingForce;
            force = Vector3.Dot(force, normalFront) * normalFront;
            rb.AddForceAtPosition(force, wheelFront.position, ForceMode.VelocityChange);
        }
        else
        {
            normalFront = Vector3.zero;
        }
        
        didHit = false;
        closestHit.distance = 10*wheelRadius;
        for (int i = 0; i < 16; i++)
        {
            float angle = 360/16 * i;
            Vector3 dir = Quaternion.AngleAxis(angle, wheelBack.right) * -wheelBack.up;
            if(Physics.Raycast(wheelBack.position, dir, out hit, wheelRadius+wheelOverreach, groundLayer))
            {
                didHit = true;
                if(hit.distance >= closestHit.distance) continue;
                closestHit = hit;
                closestHitDir = dir;
            }
        }
        if (didHit)
        {
            normalBack = closestHit.normal;
            groundedBack = true;

            float distance = Vector3.Distance(wheelBack.position, closestHit.point);
            float wheelVelocityUp = Vector3.Dot(rb.GetPointVelocity(wheelBack.position), -closestHitDir);
            float displacement = wheelRadius - distance;
            
            Vector3 springForce = -closestHitDir * displacement * wheelSpring;
            Vector3 dampingForce = -wheelVelocityUp * wheelDamping * -closestHitDir;

            Vector3 force = springForce + dampingForce;
            force = Vector3.Dot(force, normalBack) * normalBack;
            rb.AddForceAtPosition(force, wheelBack.position, ForceMode.VelocityChange);
        }
        else
        {
            normalBack = Vector3.zero;
        }

        groundNormal = ((normalFront + normalBack) / 2).normalized;
    }


    private void HandleAcceleration()
    {
        if(!groundedBack) return;

        float speed = Mathf.Abs(rb.linearVelocity.magnitude);
        float force = accelerationCurve.Evaluate(speed / maxSpeed) * acceleration;
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
        float desiredAngle = moveInput.x * Mathf.Lerp(maxSteer, minSteer, maxSpeedPercent);
        steerAngle = Mathf.Lerp(steerAngle, desiredAngle, steerAcceleration);
        
        wheelFront.localEulerAngles = new Vector3(0, steerAngle, 0);
    }


    private void HandleLean()
    {
        if(groundedBack || groundedFront)
        {
            float desiredLean = -moveInput.x * Mathf.Lerp(minLean, maxLean, maxSpeedPercent);
            leanAngle = Mathf.Lerp(leanAngle, desiredLean, leanSpeed);
            Vector3 targetNormal = Vector3.Slerp(Vector3.up, groundNormal, maxSpeedPercent);
            targetNormal = Quaternion.AngleAxis(leanAngle, transform.forward) * targetNormal;
            targetNormal = Vector3.ProjectOnPlane(targetNormal, transform.forward);
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
            rb.AddForce(Vector3.down * gravityForce, ForceMode.VelocityChange);
        }
        else
        {
            rb.AddForce(Vector3.down * gravityForce, ForceMode.VelocityChange);
        }
    }


    private void AirControll()
    {
        if(groundedBack && groundedFront) return;


        if(groundedBack || groundedFront) return;

        //TODO stabilise to last ground normal instead of just up?
        // Stabilise Roll
        Vector3 correctionAxis = Vector3.Cross(transform.up, Vector3.up);
        Vector3 rollTorque = Vector3.Project(correctionAxis, transform.forward);
        rb.AddTorque(rollTorque * airRollStabilisation, ForceMode.VelocityChange);
        
        Vector3 xTarget = airControllSpeed * moveInput.z * transform.right;
        Vector3 yTarget = airControllSpeed * moveInput.x * transform.up;
        Vector3 targetAngularVelocity = xTarget + yTarget;
        
        rb.angularVelocity = Vector3.Slerp(rb.angularVelocity, targetAngularVelocity, airControllAcceleration);
    }


    private void Jump()
    {
        if(groundedBack || groundedFront || grinding)
        {
            if(wantsJump)
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
                ExitRail(grindingRail);
            }
        }
        wantsJump = false;
    }


    private void EnterRail(Transform rail)
    {
        float railSpeed = Vector3.Dot(rail.forward, rb.linearVelocity);
        
        if(Mathf.Abs(railSpeed) < minRailSpeed) return;

        grinding = true;
        grindingRail = rail;
        grindingDirForward = Mathf.Sign(railSpeed);
        grindingDirSide = Mathf.Sign(Vector3.Dot(rail.right, transform.forward));
    }


    private void ExitRail(Transform rail)
    {
        if(rail != grindingRail) return;

        grinding = false;
        grindingRail = null;
        grindingDirForward = 0f;
        grindingDirSide = 0f;
    }
}
