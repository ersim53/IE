using UnityEngine;
using System;
using PathologicalGames;

namespace AssemblyCSharp {
	public class PlayerCollision {
		private PlayerRespawn respawner;
		private Transform transform;
		private float arrow_rotation_angle;
		private bool rotating_with_arrow;
		private float rotation_angle;
		private Quaternion rotation;
		private float rotation_speed;

		private const float ROTATION_ANGLE_THRESHOLD = 0.1f;
		private const float STRAIGHT_ICE_ROTATION_SPEED_AMPLIFIER = 1.5f;
		private const int DESTINATION_INDEX = 0;

		public PlayerCollision (PlayerRespawn respawner, Transform transform, float rotation_speed) {
			this.respawner = respawner;
			this.transform = transform;
			rotating_with_arrow = false;
			this.rotation_speed = rotation_speed;
		}

		public void OnCollision (GameObject player_object, Collider collider, PlayerRotate rotate) {
			if (collider.gameObject.tag == "Monster") {
					respawner.KillPlayer(player_object, PlayerAnimations.Death, true);
	        }
			else if ((collider.gameObject.tag == "Arrow") && (rotating_with_arrow == false)) {
				Quaternion collider_rotation = collider.transform.rotation;
				collider_rotation.x = 0;
				collider_rotation.y = 0;
				arrow_rotation_angle = collider_rotation.eulerAngles.z;
				CalculateRotationAngle();
				rotating_with_arrow = true;
	        }
			else if (collider.gameObject.tag == "Checkpoint") {
				Vector3 respawn_position;
				respawn_position = collider.transform.position;
				if (collider.transform.parent != null) {
					if (collider.transform.parent.tag == "Checkpoint") {
						respawn_position = collider.transform.parent.position;
					}
				}
				respawn_position.y -= 1;
				respawner.SetRespawnPosition(respawn_position);
	        }
			else if (collider.gameObject.tag == "Teleporter") {
				player_object.transform.position = collider.gameObject.transform.GetChild(DESTINATION_INDEX).position;
				rotate.StopRotating();
				//SpawnPool general_pool = PoolManager.Pools[CurrentLevel.GetGeneralPoolName()];
				//general_pool.Spawn(checkpoint_effect, collider.transform.position, new Quaternion(90, 0, 0, 0));
	        }
			else if (collider.gameObject.tag == "Lever") {
				collider.gameObject.tag = "LeverTriggered";
				bool open_door = true;
				foreach (Transform children in collider.gameObject.transform.parent) {
					if (children.tag == "Lever") {
						open_door = false;
					}
				}
				if (open_door) {
					collider.gameObject.transform.parent.tag = "DoorOpen";
				}
	        }
			else if (collider.gameObject.tag == "SpeedUp") {
				player_object.GetComponent<Player>().ActivateSpeedBoost(true);
	        }
			else if (collider.gameObject.tag == "SpeedDown") {
				player_object.GetComponent<Player>().ActivateSpeedBoost(false);
	        }
		}

		public void IsRotatingWithArrow (bool rotating) {
			rotating_with_arrow = rotating;
		}

		public void RotateTorwardArrow () {
			if (rotating_with_arrow == true) {
				if (rotation_angle > ROTATION_ANGLE_THRESHOLD) {
					CalculateRotationAngle();
					transform.rotation = Quaternion.Slerp(transform.rotation, rotation, EngineStrategy.GetDeltaTime() 
						* rotation_speed * STRAIGHT_ICE_ROTATION_SPEED_AMPLIFIER);
				}
				else {
					rotating_with_arrow = false;
				}
			}
		}

		private void CalculateRotationAngle () {
			float x = (float)Math.Cos(arrow_rotation_angle * (Math.PI / 180.0));
			float z = (float)Math.Sin(arrow_rotation_angle * (Math.PI / 180.0));
			Vector3 current_move_point = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);
			Vector3 direction = (current_move_point - transform.position).normalized;
			rotation = Quaternion.LookRotation(direction);
			Vector3 angle = rotation.eulerAngles;
			angle.x = 0;
			rotation = Quaternion.Euler(angle);
			rotation_angle = Quaternion.Angle(transform.rotation, rotation);
		}
	}
}


