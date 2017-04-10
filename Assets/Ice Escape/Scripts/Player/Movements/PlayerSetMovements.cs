using UnityEngine;
using System;
using System.Collections;
using PathologicalGames;

namespace AssemblyCSharp {
	public class PlayerSetMovements {
		private PlayerMovementType current_movement_type;
		protected Transform transform;
		private GameObject gameObject;
		protected PlayerMove move;
		private PlayerRotate rotate;
		private PlayerFall fall;
		private PlayerAnimator animations;
		private PlayerRespawn respawner;
		private GameObject water_effect;

		private float movement_speed;
		private float ice_movement_speed;
		private float last_movement_speed;
		private float extra_movement_speed;
		private RaycastHit previous_ray;
		private bool previous_ray_exist;

		private bool is_falling;
		private float speed_boost;

		private Vector3 PLAYER_RAY_DIRECTION = new Vector3(0,-1,0);
		private const float PLATFORM_DETECTION_RAY_DISTANCE = 0.5f;
		private const float EXTRA_MOVEMENT_SPEED_FACTOR = 1;
		private const float EXTRA_MOVEMENT_SPEED_INCREMENT = 0.5f;
		private const float SPEED_BOOST_FACTOR = 5f;
		private const float FORWARD_VECTOR_DISTANCE = 1f;
		private const float OBSTACLE_COLLISION_ON_ICE_STUN_DURATION = 0.25f;

		public PlayerSetMovements () {}

		public PlayerSetMovements (Transform transform, GameObject gameObject, PlayerMove move,
			PlayerRotate rotate, PlayerFall fall, PlayerAnimator animations, PlayerRespawn respawner, GameObject water_effect) {
			this.transform = transform;
			this.gameObject = gameObject;
			this.move = move;
			this.rotate = rotate;
			this.fall = fall;
			this.animations = animations;
			this.respawner = respawner;
			this.water_effect = water_effect;

			is_falling = false;
			extra_movement_speed = 0;
			previous_ray = new RaycastHit();
			previous_ray_exist = false;
			speed_boost = 1f;
		}

		public void DetectPlatform () {
			RaycastHit platform_hit;
			Vector3 start_position = transform.position;
			start_position.y += (transform.lossyScale.y / 2);
			if (Physics.Raycast(start_position, PLAYER_RAY_DIRECTION, out platform_hit,
			(transform.lossyScale.y / 2) + PLATFORM_DETECTION_RAY_DISTANCE)) {
				CheckPlatformType(platform_hit.transform);
				AdjustPlayerHeight(platform_hit.transform, platform_hit.point);
				AdjustExtraMovementSpeed(platform_hit);
			}
			else {
				StartFalling();
			}
		}

		private void CheckPlatformType (Transform platform) {
			if ((platform.tag == "Snow") && (current_movement_type != PlayerMovementType.Snow)) {
				current_movement_type = PlayerMovementType.Snow;
				last_movement_speed = movement_speed;
				extra_movement_speed = 0;
				ResetMovements(false, true, true);
				animations.SetAnimation(PlayerAnimations.Stand);
	        }
			else if ((platform.tag == "Ice") && (current_movement_type != PlayerMovementType.Ice)) {
				current_movement_type = PlayerMovementType.Ice;
				last_movement_speed = ice_movement_speed;
				ResetMovements(true, false, true);
				animations.SetAnimation(PlayerAnimations.Ice);
	        }
			else if ((platform.tag == "Reverse") && (current_movement_type != PlayerMovementType.Reverse)) {
				current_movement_type = PlayerMovementType.Reverse;
				last_movement_speed = ice_movement_speed;
				ResetMovements(true, false, true);
				animations.SetAnimation(PlayerAnimations.Ice);
	        }
			else if ((platform.tag == "Straight") && (current_movement_type != PlayerMovementType.Straight)) {
				current_movement_type = PlayerMovementType.Straight;
				last_movement_speed = ice_movement_speed;
				ResetMovements(true, false, true);
				animations.SetAnimation(PlayerAnimations.Ice);
	        }
			else if (platform.tag == "Water") {
				current_movement_type = PlayerMovementType.None;
				extra_movement_speed = 0;
				ResetMovements(false, true, true);
				respawner.KillPlayer(gameObject, PlayerAnimations.Stand, false);
				SpawnPool general_pool = PoolManager.Pools[CurrentLevel.GetGeneralPoolName()];
				general_pool.Spawn(water_effect, transform.position, new Quaternion(90, 0, 0, 0));
	        }
	    }

		private void AdjustPlayerHeight (Transform platform, Vector3 point) {
			if ((platform.tag == "Snow") || (platform.tag == "Ice") ||
			(platform.tag == "Reverse") || (platform.tag == "Straight")) {
				if (transform.position.y != point.y) {
					Vector3 platform_position_top = transform.position;
					platform_position_top.y = point.y;
					transform.position = platform_position_top;
				}
			}
		}

		private void AdjustExtraMovementSpeed (RaycastHit platform_hit) {
			if (PlayerOnIce() && previous_ray_exist) {
				if (platform_hit.point.y > previous_ray.point.y) {
					extra_movement_speed -= ((platform_hit.point.y - previous_ray.point.y)
					/ EXTRA_MOVEMENT_SPEED_FACTOR);
				}
				else if (platform_hit.point.y < previous_ray.point.y) {
					extra_movement_speed += ((previous_ray.point.y - platform_hit.point.y)
					/ EXTRA_MOVEMENT_SPEED_FACTOR);
				}
				else {
					if ((extra_movement_speed < EXTRA_MOVEMENT_SPEED_INCREMENT) && (extra_movement_speed >
					-EXTRA_MOVEMENT_SPEED_INCREMENT)) {
						extra_movement_speed = 0;
					}
				}
				previous_ray = platform_hit;
			}
			else if (PlayerOnIce() && !previous_ray_exist) {
				previous_ray_exist = true;
				previous_ray = platform_hit;
			}
		}

		private void StartFalling () {
			if (is_falling == false) {
				fall.StartFalling((last_movement_speed + extra_movement_speed) * speed_boost);
				ResetMovements(rotate.GetOnIce(), true, false);
				current_movement_type = PlayerMovementType.None;
				is_falling = true;
	    	}
	    }

		private void ResetMovements (bool set_on_ice, bool reset_previous_ray, bool reset_is_falling) {
			move.StopMoving();
			if (!set_on_ice) {
				rotate.StopRotating();
			}
			rotate.SetOnIce(set_on_ice);
			if (reset_previous_ray) {
				previous_ray = new RaycastHit();
				previous_ray_exist = false;
			}
			if (reset_is_falling) {
				if (is_falling == true) {
					fall.StopFalling();
					is_falling = false;
				}
			}
	    }

	    public bool PlayerOnIce () {
			return ((current_movement_type == PlayerMovementType.Ice) || (current_movement_type == PlayerMovementType.Reverse)
				|| (current_movement_type == PlayerMovementType.Straight));
	    }

		public PlayerMovementType GetCurrentMovementType () {
	    	return current_movement_type;
	    }

		public void MoveForwardOnIce () {
			CheckObstacleOnIce();
			transform.position += transform.forward * EngineStrategy.GetDeltaTime() 
				* (ice_movement_speed + extra_movement_speed) * speed_boost;

			if (extra_movement_speed > 0) {
				extra_movement_speed -= EngineStrategy.GetDeltaTime()  * EXTRA_MOVEMENT_SPEED_INCREMENT *
				Math.Abs(extra_movement_speed);
			}
			else if (extra_movement_speed < 0) {
				extra_movement_speed += EngineStrategy.GetDeltaTime()  * EXTRA_MOVEMENT_SPEED_INCREMENT *
				Math.Abs(extra_movement_speed);
			}
	    }

		private void CheckObstacleOnIce () {
			RaycastHit obstacle_hit;
			Vector3 start_position = transform.position;
			start_position.y += (transform.lossyScale.y / 2);
			if (Physics.Raycast(start_position, transform.forward, out obstacle_hit, FORWARD_VECTOR_DISTANCE)) {
				if ((obstacle_hit.collider.gameObject.tag == "Snow") || (obstacle_hit.collider.gameObject.tag == "Ice") ||
				(obstacle_hit.collider.gameObject.tag == "Straight") || (obstacle_hit.collider.gameObject.tag == "Reverse")
				|| (obstacle_hit.collider.gameObject.tag == "Door") || (obstacle_hit.collider.gameObject.tag == "Obstacle")) {
					transform.forward = Vector3.Reflect(transform.forward, obstacle_hit.normal);
					transform.GetComponent<PlayerMovements>().StunPlayer(OBSTACLE_COLLISION_ON_ICE_STUN_DURATION);
				}
			}
		}

		public void SetMovementSpeed (float movement_speed) {
	    	this.movement_speed = movement_speed;
	    }

		public void SetIceMovementSpeed (float ice_movement_speed) {
			this.ice_movement_speed = ice_movement_speed;
	    }

	    public void ActivateSpeedBoost (bool activate) {
	    	if (activate) {
				speed_boost = SPEED_BOOST_FACTOR;
			}
			else {
				speed_boost = 1f;
			}
	    }
	}
}

