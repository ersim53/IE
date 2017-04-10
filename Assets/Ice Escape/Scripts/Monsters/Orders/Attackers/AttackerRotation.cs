using UnityEngine;
using System;

namespace AssemblyCSharp {
	public class AttackerRotation {
		private Transform transform;
		private bool random;
		private Quaternion rotation;
		private float rotation_angle;

		public AttackerRotation (Transform transform, bool random) {
			this.transform = transform;
			this.random = random;
		}

		public void CalculateRotationAngle (Vector3 throw_point) {
			Vector3 direction;
			if (!random) {
				direction = (throw_point - transform.position).normalized;
			}
			else {
				direction = (throw_point - transform.position).normalized;
			}
			rotation = Quaternion.LookRotation(direction);
			Vector3 angle = rotation.eulerAngles;
			angle.x = 0;
			rotation = Quaternion.Euler(angle);
			rotation_angle = Quaternion.Angle(transform.rotation, rotation);
		}

		public Quaternion GetRotation () {
			return rotation;
		}

		public float GetRotationAngle () {
			return rotation_angle;
		}
	}
}

