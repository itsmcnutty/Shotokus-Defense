using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchArrow : StateMachineBehaviour
{
    // Normalized time values in shoot animation that correspond grab and release events of the arrow
    private float ARROW_GRAB = 0.18f;
    private float ARROW_RELEASE = 0.625f;
    
    // Flags to check if events have been reached already
    private bool arrowGrabbed = false;
    private bool arrowReleased = false;
    
    // Reset flags
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        arrowGrabbed = false;
        arrowReleased = false;
    }

    // Check for specific times in animation to perform arrow logic
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!arrowGrabbed && stateInfo.normalizedTime >= ARROW_GRAB)
        {
            // Create the arrow in enemy's hand
            animator.gameObject.GetComponentInChildren<ShootingAbility>().CreateArrow();
            arrowGrabbed = true;
        }
        if (!arrowReleased && stateInfo.normalizedTime >= ARROW_RELEASE)
        {
            // Launch arrow from bow
            animator.gameObject.GetComponentInChildren<ShootingAbility>().LaunchArrow();
            arrowReleased = true;
        }
    }
}
