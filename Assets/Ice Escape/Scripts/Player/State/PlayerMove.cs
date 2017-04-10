using UnityEngine;
using System;

namespace AssemblyCSharp {
	public class PlayerMove {
		private Transform transform;
		private PlayerAnimator animations;
		private float movement_speed;
		private bool is_moving;
		private Vector3 move_point;

		private const float COLLISION_DISTANCE = 0.25f;
		private const float FORWARD_VECTOR_DISTANCE = 1f;
		private const float CONE_LIMIT_ANGLE = 0.5f;

		public PlayerMove (Transform transform, PlayerAnimator animations) {
			this.transform = transform;
			this.animations = animations;
			is_moving = false;
		}

		public void Move () {
			if (is_moving == true && (transform.position - move_point).magnitude > COLLISION_DISTANCE) {
				if (CheckObstacleCollision()) {
					transform.position = Vector3.MoveTowards(transform.position, move_point, movement_speed *
						EngineStrategy.GetDeltaTime());
				}
				else {
					StopMoving();
				}
			}
			else if (is_moving == true && (transform.position - move_point).magnitude <= COLLISION_DISTANCE) {
				StopMoving();
			}
		}

		private bool CheckObstacleCollision () {
			RaycastHit cliff_hit;
			Vector3 start_position = transform.position;
			start_position.y += (transform.lossyScale.y / 2);

			// Get direction ray and cone limit rays.
			Vector3 ray_direction = GetDirectionVector();
			Vector3 second_ray_direction = ray_direction;
			Vector3 third_ray_direction = ray_direction;

			if ((ray_direction.x > 0) && (ray_direction.z > 0)) {
				second_ray_direction.x += CONE_LIMIT_ANGLE;
				second_ray_direction.z -= CONE_LIMIT_ANGLE;
				third_ray_direction.x -= CONE_LIMIT_ANGLE;
				third_ray_direction.z += CONE_LIMIT_ANGLE;
			}
			else if ((ray_direction.x < 0) && (ray_direction.z < 0)) {
				second_ray_direction.x -= CONE_LIMIT_ANGLE;
				second_ray_direction.z += CONE_LIMIT_ANGLE;
				third_ray_direction.x += CONE_LIMIT_ANGLE;
				third_ray_direction.z -= CONE_LIMIT_ANGLE;
			}
			else if ((ray_direction.x < 0) && (ray_direction.z > 0)) {
				second_ray_direction.x += CONE_LIMIT_ANGLE;
				second_ray_direction.z += CONE_LIMIT_ANGLE;
				third_ray_direction.x -= CONE_LIMIT_ANGLE;
				third_ray_direction.z -= CONE_LIMIT_ANGLE;
			}
			else {
				second_ray_direction.x -= CONE_LIMIT_ANGLE;
				second_ray_direction.z -= CONE_LIMIT_ANGLE;
				third_ray_direction.x += CONE_LIMIT_ANGLE;
				third_ray_direction.z += CONE_LIMIT_ANGLE;
			}

			// Check collision for all rays. (both cone limits and direction vector rays).
			if (Physics.Raycast(start_position, ray_direction, out cliff_hit, FORWARD_VECTOR_DISTANCE)) {
				if ((cliff_hit.collider.gameObject.tag == "Snow") || (cliff_hit.collider.gameObject.tag == "Ice") ||
					(cliff_hit.collider.gameObject.tag == "Straight") || (cliff_hit.collider.gameObject.tag == "Reverse")
					|| (cliff_hit.collider.gameObject.tag == "Door")) {
					return false;
				}
			}
			if (Physics.Raycast(start_position, second_ray_direction, out cliff_hit, FORWARD_VECTOR_DISTANCE)) {
				if ((cliff_hit.collider.gameObject.tag == "Snow") || (cliff_hit.collider.gameObject.tag == "Ice") ||
					(cliff_hit.collider.gameObject.tag == "Straight") || (cliff_hit.collider.gameObject.tag == "Reverse")
					|| (cliff_hit.collider.gameObject.tag == "Door")) {
					return false;
				}
			}
			if (Physics.Raycast(start_position, third_ray_direction, out cliff_hit, FORWARD_VECTOR_DISTANCE)) {
				if ((cliff_hit.collider.gameObject.tag == "Snow") || (cliff_hit.collider.gameObject.tag == "Ice") ||
					(cliff_hit.collider.gameObject.tag == "Straight") || (cliff_hit.collider.gameObject.tag == "Reverse")
					|| (cliff_hit.collider.gameObject.tag == "Door")) {
					return false;
				}
			}
			return true;
		}

		private Vector3 GetDirectionVector () {
			Vector3 start = transform.position;
			Vector3 end = move_point;
			end.y = start.y;
			Vector3 heading = end - start;
			float distance = heading.magnitude;
			return heading / distance;
		}

		public void SetMovePoint (RaycastHit hit) {
			move_point = hit.point;
			move_point.y = transform.position.y;
			is_moving = true;
			animations.SetAnimation(PlayerAnimations.Walk);
		}

		public void SetMovementSpeed (float movement_speed) {
			this.movement_speed = movement_speed;
		}

		public void StopMoving () {
			is_moving = false;
			animations.SetAnimation(PlayerAnimations.Stand);
		}
	}
}

