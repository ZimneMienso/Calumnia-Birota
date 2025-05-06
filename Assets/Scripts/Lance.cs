using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;

public class Lance : WeaponItem
{
    [SerializeField] float xOffset;
    [SerializeField] Transform spike;
    [SerializeField] SphereCollider deathRange;
    [SerializeField] Vector3 raisedPos;
    [SerializeField] float maxAngle;
    [SerializeField] BoxCollider range;
    [SerializeField] float rotationSpeed;

    private bool canDoDamage = false;
    private bool spikedSth = false;
    private bool isOnRight;

    void Start()
    {
        Vector3 pos = new Vector3(xOffset, transform.localPosition.y, transform.localPosition.z);
        transform.localPosition = pos;
        transform.localRotation = Quaternion.Euler(raisedPos);
        isOnRight = true;
    }

    void PutOnLeft()
    {
        Vector3 pos = new Vector3(-xOffset, transform.localPosition.y, transform.localPosition.z);
        transform.localPosition = pos;
        isOnRight = false;
    }

    void PutOnRight()
    {
        Vector3 pos = new Vector3(xOffset, transform.localPosition.y, transform.localPosition.z);
        transform.localPosition = pos;
        isOnRight = true;
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
        if (((Input.GetKey(KeyCode.Mouse0) && !isOnRight) || (Input.GetKey(KeyCode.Mouse1) && isOnRight)) && !spikedSth)
        {
            ITarget target = GetClosestTarget(range);
            if(canDoDamage && target != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(target.Transform.position - transform.position);
                Quaternion rot = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                transform.rotation = rot;
            }
            else
            {
                Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            Quaternion targetRotation = Quaternion.Euler(raisedPos);
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        canDoDamage = transform.localEulerAngles.x > 344 || transform.localEulerAngles.x < 8;
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
                //drop lance
            }
        }
    }
}
