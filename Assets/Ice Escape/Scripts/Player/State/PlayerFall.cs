using UnityEngine;
using System;

namespace AssemblyCSharp {
	public class PlayerFall {
		private Transform transform;
		private PlayerAnimator animations;
		private float fall_speed;
		private bool is_falling;
		private float movement_speed_while_falling;

		private const float FORWARD_VECTOR_DISTANCE = 1f;

		public PlayerFall (Transform transform, PlayerAnimator animations) {
			this.transform = transform;
			this.animations = animations;
		}

		public void StartFalling (float movement_speed_while_falling) {
			this.movement_speed_while_falling = movement_speed_while_falling;
			is_falling = true;
			animations.SetAnimation(PlayerAnimations.Fall);
		}

		public void Fall() {
			if (is_falling == true) {
				CheckObstacle();
				Vector3 fall_position = transform.position;
				fall_position.y -= 1;
				transform.position = Vector3.MoveTowards(transform.position, fall_position, fall_speed *
					EngineStrategy.GetDeltaTime());
				transform.position += transform.forward * EngineStrategy.GetDeltaTime()  * movement_speed_while_falling;
			}
		}

		private void CheckObstacle () {
			RaycastHit obstacle_hit;
			Vector3 start_position = transform.position;
			start_position.y += (transform.lossyScale.y / 2);
			if (Physics.Raycast(start_position, transform.forward, out obstacle_hit, FORWARD_VECTOR_DISTANCE)) {
				if ((obstacle_hit.collider.gameObject.tag == "Snow") || (obstacle_hit.collider.gameObject.tag == "Ice") ||
				(obstacle_hit.collider.gameObject.tag == "Straight") || (obstacle_hit.collider.gameObject.tag == "Reverse")
				|| (obstacle_hit.collider.gameObject.tag == "Door") || (obstacle_hit.collider.gameObject.tag == "Obstacle")) {
					transform.forward = Vector3.Reflect(transform.forward, obstacle_hit.normal);
				}
			}
		}

		public void StopFalling () {
			is_falling = false;
			animations.SetAnimation(PlayerAnimations.Stand);
		}

		public void SetFallSpeed (float fall_speed) {
			this.fall_speed = fall_speed;
		}
	}
}

