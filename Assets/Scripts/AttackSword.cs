using Unity.VisualScripting;
using UnityEngine;

public class AttackSword : WeaponItem
{
    [SerializeField] BoxCollider leftHitBox;
    [SerializeField] BoxCollider rightHitBox;
    [SerializeField] int multiHitScore = 100;
    GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectsByType<GameManager>(FindObjectsSortMode.None)[0];
    }

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
        int targetsKilled = 0;
        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out ITarget target))
            {
                int extraScore = multiHitScore * targetsKilled;
                if (extraScore > 0)
                {
                    gameManager.AddScore(extraScore, "Multi-hit");
                }
                target.GetHit();
                targetsKilled++;
            }
        }
    }
}
