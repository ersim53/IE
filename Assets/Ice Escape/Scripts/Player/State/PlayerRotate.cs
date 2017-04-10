using UnityEngine;
using System;

namespace AssemblyCSharp {
	public class PlayerRotate {
		private Transform transform;

		private bool is_rotating;
		private bool on_ice;
		private bool on_reverse_ice;

		private Vector3 rotation_point;
		private Vector3 direction;
		private Quaternion rotation;

		private float rotation_speed;
		private float ice_rotation_speed;
		private float rotation_angle;
		private float total_degrees_rotated;
		private float degrees_to_rotate;
		private float previous_angle;

		private const float ROTATION_ANGLE_THRESHOLD = 1.0f;

		public PlayerRotate (Transform transform) {
			this.transform = transform;

			is_rotating = false;
			on_ice = false;
			on_reverse_ice = false;
		}

		public void Rotate () {
			if (is_rotating == true && rotation_angle > ROTATION_ANGLE_THRESHOLD) {
				CalculateRotationAngle();
				if (!on_ice && !on_reverse_ice) {
					transform.rotation = Quaternion.Slerp(transform.rotation, rotation, EngineStrategy.GetDeltaTime()
						* rotation_speed);
				}
				else {
					if (total_degrees_rotated < degrees_to_rotate) {
						transform.rotation = Quaternion.Slerp(transform.rotation, rotation, EngineStrategy.GetDeltaTime()
							* ice_rotation_speed);
					}
					else {
						StopRotating();
					}
				}
				total_degrees_rotated += Math.Abs(previous_angle - rotation_angle);
				previous_angle = rotation_angle;
			}
			else if (is_rotating == true && rotation_angle <= ROTATION_ANGLE_THRESHOLD) {
				StopRotating();
			}
		}

		public void SetRotationPoint (RaycastHit hit) {
			rotation_point = hit.point;
			CalculateRotationAngle();
			is_rotating = true;
			degrees_to_rotate = rotation_angle;
			total_degrees_rotated = 0;
			previous_angle = rotation_angle;
		}

		private void CalculateRotationAngle () {
			direction = (rotation_point - transform.position).normalized;
			if (on_reverse_ice) {
				direction = -direction;
			}
			rotation = Quaternion.LookRotation(direction);
			Vector3 angle = rotation.eulerAngles;
			angle.x = 0;
			rotation = Quaternion.Euler(angle);
			rotation_angle = Quaternion.Angle(transform.rotation, rotation);
		}

		public void SetRotationSpeed (float rotation_speed) {
			this.rotation_speed = rotation_speed;
		}

		public void SetIceRotationSpeed (float ice_rotation_speed) {
			this.ice_rotation_speed = ice_rotation_speed;
		}

		public void StopRotating () {
			is_rotating = false;
		}

		public void SetOnIce(bool on_ice) {
			this.on_ice = on_ice;
		}

		public void SetOnReverseIce(bool on_reverse_ice) {
			this.on_reverse_ice = on_reverse_ice;
		}

		public bool GetOnIce () {
		 return on_ice;
		}
	}
}

