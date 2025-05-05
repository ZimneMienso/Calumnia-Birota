using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;

public class Lance : WeaponItem
{
    [SerializeField] float xOffset;
    [SerializeField] Transform spike;
    [SerializeField] SphereCollider deathRange;
    [SerializeField] Vector3 maxRotations1;
    [SerializeField] Vector3 maxRotations2;
    [SerializeField] float maxAngle;
    [SerializeField] BoxCollider range;
    [SerializeField] float rotationSpeed;

    private bool canDoDamage = false;
    private bool spikedSth = false;

    void PutOnLeft()
    {
        Vector3 pos = new Vector3(-xOffset, transform.localPosition.y, transform.localPosition.z);
        transform.localPosition = pos;
    }

    void PutOnRight()
    {
        Vector3 pos = new Vector3(xOffset, transform.localPosition.y, transform.localPosition.z);
        transform.localPosition = pos;
    }

    void Start()
    {
        Vector3 pos = new Vector3(xOffset, transform.localPosition.y, transform.localPosition.z);
        transform.localPosition = pos;
    }

    private void SpikeTarget(SphereCollider hitBox)
    {
        Collider[] colliders = Physics.OverlapSphere(hitBox.bounds.center, hitBox.radius);
        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out ITarget target))
            {
                target.GetSpiked(spike);
                spikedSth = true;
            }
        }
    }

    private ITarget GetClosestTarget(BoxCollider radius)
    {
        ITarget closestTarget = null;
        float closestDistance = Mathf.Infinity;

        Collider[] colliders = Physics.OverlapBox(range.bounds.center, range.bounds.extents);
        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out ITarget target))
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                Vector3 directionToTarget = (target.Transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.parent.forward, directionToTarget);
                Debug.Log(angle);
                if (distance < closestDistance && angle < maxAngle)
                {
                    closestDistance = distance;
                    closestTarget = target;
                }
            }
        }
        return closestTarget;
    }

    void UpdateLanceState()
    {
        if (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1))
        {
            //Opuszczaj do pewnego poziomu
        }
        else
        {
            // podnos
        }

        if (/*canDoDamage &&*/ true)
        {
            ITarget target = GetClosestTarget(range);
            if (target != null && !spikedSth)
            {
                Quaternion targetRotation = Quaternion.LookRotation(target.Transform.position - transform.position);
                Quaternion rot = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                //Vector3 euler = rot.eulerAngles;
                //euler.x = NormalizeAngle(euler.x);
                //euler.y = NormalizeAngle(euler.y);
                //euler.z = NormalizeAngle(euler.z);
                //euler.x = Mathf.Clamp(euler.x, maxRotations1.x, maxRotations2.x);
                //euler.y = Mathf.Clamp(euler.y, maxRotations1.y, maxRotations2.y);
                //euler.z = Mathf.Clamp(euler.z, maxRotations1.z, maxRotations2.z);
                transform.rotation = rot; //Quaternion.Euler(euler);
            }
            else
            {
                Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        canDoDamage = transform.localEulerAngles.x > 344 || transform.localEulerAngles.x < 8;
    }

    float NormalizeAngle(float angle)
    {
        return (angle > 180f) ? angle - 360f : angle;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !canDoDamage)
        {
            PutOnLeft();
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1) && !canDoDamage)
        {
            PutOnRight();
        }
        UpdateLanceState();
        if (canDoDamage)
        {
            // Check player velocity?
            SpikeTarget(deathRange);
            if (spikedSth)
            {
                // drop lance
            }
        }
    }
}
