using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class AttackSword : WeaponItem
{
    [SerializeField] BoxCollider leftHitBox;
    [SerializeField] BoxCollider rightHitBox;
    [SerializeField] int multiHitScore = 100;
    GameManager gameManager;
    Animator animator;
    bool preparing = false;

    void Start()
    {
        gameManager = FindObjectsByType<GameManager>(FindObjectsSortMode.None)[0];
        animator = FindObjectsByType<Animator>(FindObjectsSortMode.None)[0];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            animator.SetTrigger("PrepareLeft");
            preparing = true;
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            animator.SetTrigger("PrepareRight");
            preparing = true;
        }
        if (Input.GetKeyUp(KeyCode.Mouse0) && preparing)
        {
            animator.SetTrigger("SwingLeft");
            AttackOnLeft();
            preparing = false;
        }
        if (Input.GetKeyUp(KeyCode.Mouse1) && preparing)
        {
            animator.SetTrigger("SwingRight");
            AttackOnRight();
            preparing = false;
        }
    }

    private IEnumerator BlockPedalingForABit()
    {
        animator.SetBool("Pedal", false);
        yield return new WaitForSeconds(2f);
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
