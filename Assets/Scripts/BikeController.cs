using System.Collections.Generic;
using UnityEngine;

public class BikeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform wheelFront;
    [SerializeField] Transform wheelBack;
    [SerializeField] BikeRailDetector railDetector;
    [SerializeField] LayerMask groundLayer;
    [Header("General")]
    [SerializeField] float acceleration = 5;
    [SerializeField] AnimationCurve accelerationCurve;
    [SerializeField] float maxSpeed = 30f;
    [SerializeField] float breakForce = 1f;
    [SerializeField] float gravityForce = 1f;
    [SerializeField] float jumpForce = 3f;
    [SerializeField] float groundingTorque = 3f;
    [Header("Steering")]
    [SerializeField] float maxSteer = 30f;
    [SerializeField] float minSteer = 10f;
    [SerializeField] float steerAcceleration = .2f;
    [Header("Wheels")]
    [SerializeField] float wheelRadius = 0.5f;
    [SerializeField] float wheelOverreach = 0.05f;
    [SerializeField] float wheelSpring = 10f;
    [SerializeField] float wheelDamping = 5f;
    [Space]
    [SerializeField] bool useDynamicSideFriction = false;
    [SerializeField] float antiSlipForce = 1f;
    [SerializeField] float maxSideVelocity = 4f;
    [SerializeField] AnimationCurve dynamicSideFriction;
    [Header("Lean")]
    [SerializeField] float maxLean = 30f;
    [SerializeField] float minLean = 10f;
    [SerializeField] float leanSpeed = 1f;
    [SerializeField] float leanSpring = 1f;
    [SerializeField] float leanDamping = 1f;
    [Header("Air Control")]
    [SerializeField] float airRollStabilisation = 1f;
    [SerializeField] float airControllSpeed = 1f;
    [SerializeField] float airControllAcceleration = 0.2f;
    [Header("Grinding")]
    [SerializeField] float minRailSpeed = 5f;
    [SerializeField] float railRotLerp = .1f;

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
    private float grindingGoingDir;
    private float grindingSideDir;
    private Transform grindingRail;

    public bool IsFrontGrounded() { return groundedFront; }
    public bool IsBackGrounded() { return groundedBack; }

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
            HandleGrinding();
        }
        else
        {
            HandleWheels();
            HandleAcceleration();
            HandleBreaking();
            GroundBothWheels();
            PreventSlip();
            HandleLean();
            HandleGravity();
            AirControll();
        }
        Jump();
    }


    private void HandleGrinding()
    {
        // Redirect velocity
        rb.linearVelocity = grindingGoingDir * grindingRail.forward * rb.linearVelocity.magnitude;

        Vector3 positionDif = grindingRail.position - railDetector.transform.position;
        positionDif = Vector3.ProjectOnPlane(positionDif, grindingRail.forward);
        rb.MovePosition(transform.position + positionDif);
        
        // Stabilise rotation
        Quaternion targetRot = Quaternion.LookRotation(grindingSideDir * grindingRail.right, grindingRail.up);
        Quaternion rotation = Quaternion.Slerp(transform.rotation, targetRot, railRotLerp);
        rb.MoveRotation(rotation);
    }


    private void HandleWheels()
    {
        groundedFront = groundedBack = false;
        RaycastHit closestHit = new RaycastHit();
        Vector3 closestHitDir = Vector3.zero;
        RaycastHit hit;
        bool didHit;

        // Front wheel
        didHit = false;
        closestHit.distance = 10*wheelRadius;
        for (int i = 0; i < 16; i++)
        {
            // Raycast down to find ground
            float angle = -70 + 140/16 * i;
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

            // Apply spring force to push away from ground
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
        
        // Back wheel
        didHit = false;
        closestHit.distance = 10*wheelRadius;
        for (int i = 0; i < 16; i++)
        {
            // Raycast down to find ground
            float angle = -70 + 140/16 * i;
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
            // Apply spring force to push away from ground
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


    private void GroundBothWheels()
    {
        // Apply a torque to force other wheel to the ground, if only one is grounded

        if(groundedFront && !groundedBack)
        {
            float effect = 1 - Vector3.Dot(transform.up, normalFront);
            rb.AddTorque(-transform.right * groundingTorque, ForceMode.VelocityChange);
        }
        if(!groundedFront && groundedBack)
        {
            float effect = 1 - Vector3.Dot(transform.up, normalBack);
            rb.AddTorque(transform.right * groundingTorque, ForceMode.VelocityChange);
        }
    }


    private void PreventSlip()
    {
        if(groundedFront)
        {
            float rightVelocity = Vector3.Dot(rb.GetPointVelocity(wheelFront.position), wheelFront.right);
            float frictionForce;
            if(useDynamicSideFriction)
            {
                frictionForce = -antiSlipForce * rightVelocity * dynamicSideFriction.Evaluate(Mathf.Abs(rightVelocity) / maxSideVelocity);
            }
            else
            {
                frictionForce = -antiSlipForce * rightVelocity;
            }
            rb.AddForceAtPosition(wheelFront.right * frictionForce, wheelFront.position, ForceMode.VelocityChange);
        }
        if(groundedBack)
        {
            float rightVelocity = Vector3.Dot(rb.GetPointVelocity(wheelBack.position), wheelBack.right);
            float frictionForce;
            if(useDynamicSideFriction)
            {
                frictionForce = -antiSlipForce * rightVelocity * dynamicSideFriction.Evaluate(Mathf.Abs(rightVelocity) / maxSideVelocity);
            }
            else
            {
                frictionForce = -antiSlipForce * rightVelocity;
            }
            rb.AddForceAtPosition(wheelBack.right * frictionForce, wheelBack.position, ForceMode.VelocityChange);
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
            rb.AddForce(Vector3.down * gravityForce - 0.4f * groundNormal * gravityForce, ForceMode.VelocityChange);
        }
        else
        {
            rb.AddForce(Vector3.down * gravityForce, ForceMode.VelocityChange);
        }
    }


    private void AirControll()
    {
        if(groundedBack || groundedFront) return;

        // Predict landing
        RaycastHit landingHit = new();
        landingHit.normal = Vector3.up;
        Vector3 predictLastPos = transform.position;
        Vector3 predictPos = transform.position;
        Vector3 predictVel = rb.linearVelocity;
        float timeStep = 1f;
        float magicNumber = 0.02f; // To convert unity velocity to actual velocity (may need tweeking)
        for (int i = 0; i < 200; i++)
        {
            predictPos += magicNumber * timeStep * predictVel;
            predictVel += timeStep * Vector3.down * gravityForce;
            Debug.DrawLine(predictLastPos, predictPos, Color.magenta, 0.5f);
            if(Physics.Raycast(predictLastPos, predictPos-predictLastPos, out landingHit, (predictPos-predictLastPos).magnitude, groundLayer))
            {
                Debug.DrawLine(predictLastPos, landingHit.point, Color.magenta, 0.5f);
                Debug.DrawRay(landingHit.point, landingHit.normal, Color.green);
                break;
            }

            predictLastPos = predictPos;
        }

        // Stabilise Roll
        Vector3 correctionAxis = Vector3.Cross(transform.up, landingHit.normal);
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
        grindingGoingDir = Mathf.Sign(railSpeed);
        grindingSideDir = Mathf.Sign(Vector3.Dot(rail.right, transform.forward));
    }


    private void ExitRail(Transform rail)
    {
        if(rail != grindingRail) return;

        grinding = false;
        grindingRail = null;
        grindingGoingDir = 0f;
        grindingSideDir = 0f;
    }
}
