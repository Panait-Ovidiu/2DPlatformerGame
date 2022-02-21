using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    private string currentAnimation;
    private Animator animator;

    private bool isRaised;

    const string FLAG_RISE = "Flag_Rise";
    const string FLAG_IDLE = "Flag_Idle";

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isRaised)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                isRaised = true;
                ChangeAnimationState(FLAG_RISE);
                float delay = animator.GetCurrentAnimatorStateInfo(0).length;
                Invoke("FlagIdle", delay);
                FindObjectOfType<MenuSinglePlayer>().ShowWin();
            }
        }
    }

    private void FlagIdle()
    {
        ChangeAnimationState(FLAG_IDLE);
    }

    private void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimation == newAnimation)
        {
            return;
        }

        animator.Play(newAnimation);
        currentAnimation = newAnimation;
    }
}
