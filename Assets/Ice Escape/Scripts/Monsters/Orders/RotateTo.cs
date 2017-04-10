using AssemblyCSharp;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class RotateTo : EngineStrategy.MonsterState {
	public float[] angles;
	public MoveType rotation_type;
	public float rotation_speed;
	public float time_elapsed;
	public float wait_time_between_rotations;

	private List<float> angles_list = new List<float>();
	private float rotation_angle;
	private Quaternion rotation;
	private float current_wait_time;
	private int current_angle_index;

	private const float ROTATION_ANGLE_THRESHOLD = 0.1f;
	private const float ARROW_MODEL_X_ROTATION_ANGLE = 90f;

#if BOLT
	public override void Attached () {
#else
	public virtual void Start () {
#endif
		current_wait_time = wait_time_between_rotations;
		angles_list.Add(GetStartingRotation());
		foreach (float angle in angles) {
			angles_list.Add(angle);
		}
		current_angle_index = 1;
		rotation_speed = rotation_speed * 10;

		CalculateRotationAngle();
#if BOLT
		state.SetTransforms(state.Transform, transform);
#endif
	}

#if BOLT
	public override void SimulateOwner () {
#else
	public virtual void Update () {
#endif
		if (time_elapsed > 0) {
			time_elapsed -= EngineStrategy.GetDeltaTime();
		}
		else {
			Rotate();
		}
	}

	private void Rotate () {
		if (rotation_angle > ROTATION_ANGLE_THRESHOLD) {
			CalculateRotationAngle();
			transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, EngineStrategy.GetDeltaTime()
				* rotation_speed);
		}
		else {
			if (current_wait_time <= 0) {
				current_wait_time = wait_time_between_rotations;
				NextRotationAngle();
				CalculateRotationAngle();
			}
			else {
				current_wait_time -= EngineStrategy.GetDeltaTime();
			}
		}
	}

	private void NextRotationAngle () {
		if (current_angle_index < angles_list.Count - 1) {
			current_angle_index++;
		}
		else {
			if (rotation_type == MoveType.stop) {
				Destroy(gameObject);
			}
			else if (rotation_type == MoveType.continuous) {
				current_angle_index = 0;
			}
			else if (rotation_type == MoveType.patrol) {
				angles_list.Reverse();
				current_angle_index = 1;
			}
		}
	}

	private void CalculateRotationAngle () {
		float x = (float)Math.Cos(angles_list[current_angle_index] * (Math.PI / 180.0));
		float z = (float)Math.Sin(angles_list[current_angle_index] * (Math.PI / 180.0));
		Vector3 current_move_point = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);
		Vector3 direction = (current_move_point - transform.position).normalized;
		rotation = Quaternion.LookRotation(direction);
		Vector3 angle = rotation.eulerAngles;
		angle.x = transform.rotation.eulerAngles.x;
		rotation = Quaternion.Euler(angle);
		rotation_angle = Quaternion.Angle(transform.rotation, rotation);
	}

	private float GetStartingRotation () {
		Quaternion starting_rotation = transform.rotation;
		starting_rotation.x = 0;
		starting_rotation.y = 0;
		return starting_rotation.eulerAngles.z;
	}
}


