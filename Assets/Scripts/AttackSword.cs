using UnityEngine;

public class AttackSword : MonoBehaviour
{
    [SerializeField] BoxCollider leftHitBox;
    [SerializeField] BoxCollider rightHitBox;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            AttackOnLeft();
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            AttackOnRight();
        }
    }

    private void AttackOnLeft()
    {
        CheckIfInsideCollider(leftHitBox);
    }

    private void AttackOnRight()
    {
        CheckIfInsideCollider(rightHitBox);
    }

    private void CheckIfInsideCollider(BoxCollider hitBox)
    {
        Collider[] colliders = Physics.OverlapBox(hitBox.bounds.center, hitBox.bounds.extents);
        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out ITarget target))
            {
                target.GetHit();
            }
        }
    }
}
