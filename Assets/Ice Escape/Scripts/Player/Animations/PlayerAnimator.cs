#define BOLT
using UnityEngine;
using System;

namespace AssemblyCSharp {
	public class PlayerAnimator {
#if BOLT
		private IDragonState state;
#endif
		private PlayerAnimations current_animation;

		public PlayerAnimator () {
			current_animation = PlayerAnimations.Stand;
		}

#if BOLT
		public PlayerAnimator (IDragonState state) {
			this.state = state;
			current_animation = PlayerAnimations.Stand;
		}
#endif

		public void SetAnimation (PlayerAnimations animation) {
			if (animation != current_animation) {
#if BOLT
				ResetAnimationValuesBolt();
				current_animation = animation;
				StartCorrectAnimationBolt();
#else
				ResetAnimationValues();
				current_animation = animation;
				StartCorrectAnimation();
#endif
			}
		}

#if BOLT
		private void StartCorrectAnimationBolt () {
			if (current_animation == PlayerAnimations.Stand) {
				//Dragon
				state.Animator.SetBool("Stand", true);
				//state.Stand = true;
			}
			else if (current_animation == PlayerAnimations.Walk) {
				//Dragon
				state.Animator.SetFloat("Vertical", 1.0f);
				//state.Vertical = 1.0f;
			}
			else if (current_animation == PlayerAnimations.Ice) {
				//Dragon
				state.Animator.SetBool("Swim", true);
				state.Animator.SetFloat("Vertical", 1.0f);
				//state.Swim = true;
				//state.Vertical = 1.0f;
			}
			else if (current_animation == PlayerAnimations.Fall) {
				//Dragon
				state.Animator.SetBool("Fall", true);
				state.Animator.SetFloat("Vertical", 1.0f);
				//state.Fall = true;
				//state.Vertical = 1.0f;
			}
			else if (current_animation == PlayerAnimations.Special1) {
				//Dragon
				state.Animator.SetBool("Fly", true);
				//state.Fly = true;
			}
			else if (current_animation == PlayerAnimations.Death) {
				//Dragon
				state.OnDeath();
			}
		}
#endif

		private void StartCorrectAnimation () {

		}


#if BOLT
		private  void ResetAnimationValuesBolt () {
			if (current_animation == PlayerAnimations.Stand) {
				//Dragon
				//state.Stand = false;
				state.Animator.SetBool("Stand", false);
			}
			else if (current_animation == PlayerAnimations.Walk) {
				//Dragon
				//state.Vertical = 0;
				state.Animator.SetFloat("Vertical", 0f);
			}
			else if (current_animation == PlayerAnimations.Ice) {
				//Dragon
				state.Animator.SetBool("Swim", false);
				state.Animator.SetFloat("Vertical", 0f);
				//state.Swim = false;
				//state.Vertical = 0;
			}
			else if (current_animation == PlayerAnimations.Fall) {
				//Dragon
				state.Animator.SetBool("Fall", false);
				state.Animator.SetFloat("Vertical", 0f);
				//state.Fall = false;
				//state.Vertical = 0;
			}
			else if (current_animation == PlayerAnimations.Special1) {
				//Dragon
				state.Animator.SetBool("fly", false);
				//state.Fly = false;
			}
		}
#else
		private void ResetAnimationValues () {

		}
#endif
	}
}


