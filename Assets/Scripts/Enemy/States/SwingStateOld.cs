﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingStateOld : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		
		///// Transition /////
		
		animator.SetTrigger("Continue");
	}
}
