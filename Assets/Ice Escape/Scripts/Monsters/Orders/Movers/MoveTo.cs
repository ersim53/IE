using AssemblyCSharp;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;

public class MoveTo : EngineStrategy.MonsterState {
	public MoveType move_type;
	public float movement_speed;
	public float time_elapsed;
	public float wait_time_between_moves;
	public float despawn_animation_time;

	public bool no_rotation;
	public float rotation_speed;

	public bool trigger_on_collision;
	public float teleport_wait_time;
	public float teleport_threshold_time;
	public float end_wait_time;

	private List<Vector3> coordinates_list = new List<Vector3>();
	private int move_point_index;
	private float current_wait_time;
	private float starting_wait_time;

	//Trigger variables
	private bool triggered;
	private bool stop_after_chain_of_commands;
	private int number_of_collider;
	private bool patrol_coming_back;
	private float current_teleport_wait_time;
	private bool teleporting;
	private float default_y;
	private float animation_y;
	private float total_time_elapsed;
	private List<Vector3> reached_coordinates_list = new List<Vector3>();
	private List<Vector3> coordinates_list_backup = new List<Vector3>();
	private bool teleport_is_moving_back;

	//Rotation variables
	private Vector3 rotation_point;
	private Quaternion rotation;
	private float rotation_angle;

	private const float COLLISION_DISTANCE = 0.25f;
	private const float ROTATION_ANGLE_THRESHOLD = 0.1f;
	private const float ROTATION_ANGLE_MOVE_THRESHOLD = 90.0f;
	private const float TRIGGER_TELEPORT_ANIMATION_SPEED = 2f;

#if BOLT
	public override void Attached () {
#else
	public virtual void Start () {
#endif
		current_wait_time = wait_time_between_moves;
		coordinates_list.Add(transform.position);

		foreach (Transform child in transform) {
			if (child.tag == "Target") {
				coordinates_list.Add(child.position);
			}
		}

		if ((trigger_on_collision) && (move_type == MoveType.continuous)) {
			coordinates_list.Add(transform.position);
		}
		coordinates_list_backup = coordinates_list;

		move_point_index = 1;
		triggered = false;
		stop_after_chain_of_commands = false;
		starting_wait_time = time_elapsed;
		number_of_collider = 0;
		patrol_coming_back = false;
		current_teleport_wait_time = 0;
		teleporting = false;
		default_y = transform.position.y;
		animation_y = 0;
		total_time_elapsed = 0;
		teleport_is_moving_back = false;

		//CalculateRotationAngle();
#if BOLT
		state.SetTransforms(state.Transform, transform);
#endif
	}

#if BOLT
	public override void SimulateOwner () {
#else
	public virtual void Update () {
#endif
		if (trigger_on_collision) {
			if (current_teleport_wait_time > 0) {
				current_teleport_wait_time -= EngineStrategy.GetDeltaTime();
			}
			else {
				if (teleporting) {
					if (transform.position.y > animation_y + COLLISION_DISTANCE) {
						Vector3 animation_position = transform.position;
						animation_position.y = animation_y;
						transform.position = Vector3.MoveTowards(transform.position, animation_position,
							TRIGGER_TELEPORT_ANIMATION_SPEED * EngineStrategy.GetDeltaTime());
					}
					else {
						teleporting = false;
						Vector3 animation_position = coordinates_list[0];
						animation_position.y = transform.position.y;
						transform.position = animation_position;
						move_point_index = 1;
						if (trigger_on_collision) {
							starting_wait_time = time_elapsed;
							triggered = false;
						}
					}
				}
				else {
					if (transform.position.y < default_y - COLLISION_DISTANCE) {
						Vector3 animation_position = transform.position;
						animation_position.y = default_y;
						transform.position = Vector3.MoveTowards(transform.position, animation_position,
							TRIGGER_TELEPORT_ANIMATION_SPEED * EngineStrategy.GetDeltaTime());
					}
					else {
						if (triggered) {
							if (starting_wait_time > 0) {
								starting_wait_time -= EngineStrategy.GetDeltaTime();
							}
							else {
								total_time_elapsed += EngineStrategy.GetDeltaTime();
								Move();
								if (!no_rotation) {
									Rotate();
								}
							}
						}
					}
				}
			}
		}
		else {
			if (starting_wait_time > 0) {
				starting_wait_time -= EngineStrategy.GetDeltaTime();
			}
			else {
				Move();
				if (!no_rotation) {
					Rotate();
				}
			}
		}
	}

	public void PlayerCollisionEnter() {
		number_of_collider++;
		if (move_type == MoveType.teleport) {
			if (teleporting) {
				teleporting = false;
				current_teleport_wait_time = 0;
			}
		}
		stop_after_chain_of_commands = false;
		if (!triggered) {
			triggered = true;
			if (move_type == MoveType.teleport) {
				reached_coordinates_list.Add(coordinates_list[0]);
			}
		}
    }

	public void PlayerCollisionExit() {
		number_of_collider--;
		if (number_of_collider == 0) {
			if (move_type == MoveType.teleport) {
				if (total_time_elapsed < teleport_threshold_time) {
					reached_coordinates_list.Reverse();
					coordinates_list = new List<Vector3>(reached_coordinates_list);
					teleport_is_moving_back = true;
					total_time_elapsed = 0;
					move_point_index = 0;
				}
				else {
					current_teleport_wait_time = teleport_wait_time;
					teleporting = true;
					total_time_elapsed = 0;
					reached_coordinates_list.Clear();
				}
			}
			stop_after_chain_of_commands = true;
		}
    }

	private void Move () {
		if (no_rotation) {
			if ((transform.position - coordinates_list[move_point_index]).magnitude > COLLISION_DISTANCE) {
				transform.position = Vector3.MoveTowards(transform.position, coordinates_list[move_point_index], movement_speed *
					EngineStrategy.GetDeltaTime());
			}
			else if (current_wait_time < 0) {
				current_wait_time = wait_time_between_moves;
				NextMovePoint();
			}
			else {
				current_wait_time -= EngineStrategy.GetDeltaTime();
			}
		}
		else {
			if (((transform.position - coordinates_list[move_point_index]).magnitude > COLLISION_DISTANCE)
			&& (rotation_angle <= ROTATION_ANGLE_MOVE_THRESHOLD)) {
				transform.position = Vector3.MoveTowards(transform.position, coordinates_list[move_point_index], movement_speed *
					EngineStrategy.GetDeltaTime());
			}
			else if (rotation_angle <= ROTATION_ANGLE_MOVE_THRESHOLD) {
				if (current_wait_time < 0) {
					current_wait_time = wait_time_between_moves;
					NextMovePoint();
					CalculateRotationAngle();
				}
				else {
					current_wait_time -= EngineStrategy.GetDeltaTime();
				}
			}
		}
	}

	private void Rotate () {
		if (rotation_angle > ROTATION_ANGLE_THRESHOLD) {
			CalculateRotationAngle();
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation, EngineStrategy.GetDeltaTime()
				* rotation_speed);
		}
	}

	private void NextMovePoint () {
		if (move_point_index < coordinates_list.Count - 1) {
			move_point_index++;
			if ((trigger_on_collision) && (move_point_index == coordinates_list.Count - 1)
			&& (move_type == MoveType.continuous)) {
				starting_wait_time = end_wait_time;
			}
			if ((trigger_on_collision) && (move_type == MoveType.teleport)) {
				reached_coordinates_list.Add(coordinates_list[move_point_index - 1]);
			}
		}
		else {
			if (move_type == MoveType.stop) {
				if (despawn_animation_time > 0) {
					foreach(Collider collider in GetComponents<Collider>())
						collider.enabled = false;

					Transform[] allChildren = GetComponentsInChildren<Transform>();
					foreach (Transform child in allChildren) {
						if (child.GetComponent<ParticleSystem>() != null) {
							child.GetComponent<ParticleSystem>().Stop();
						}
					}
					StartCoroutine(DespawnAnimation());
				}
				else {
					SpawnPool pool = PoolManager.Pools[CurrentLevel.GetCurrentLevelPoolName()];
					Despawner.Despawn(pool, gameObject);
				}
			}
			else if (move_type == MoveType.continuous) {
				move_point_index = 0;
				if (trigger_on_collision) {
					starting_wait_time = time_elapsed;
					if (stop_after_chain_of_commands) {
						triggered = false;
						stop_after_chain_of_commands = false;
					}
				}
			}
			else if (move_type == MoveType.patrol) {
				coordinates_list.Reverse();
				move_point_index = 1;
				if (trigger_on_collision) {
					if (patrol_coming_back) {
						starting_wait_time = time_elapsed;
						patrol_coming_back = false;
						if (stop_after_chain_of_commands) {
							triggered = false;
							stop_after_chain_of_commands = false;
						}
					}
					else {
						starting_wait_time = end_wait_time;
						patrol_coming_back = true;
					}
				}
			}
			if (move_type == MoveType.teleport) {
				if (!trigger_on_collision) {
					transform.position = coordinates_list[0];
					move_point_index = 1;
				}
				else if (teleport_is_moving_back) {
					coordinates_list = new List<Vector3>(coordinates_list_backup);
					reached_coordinates_list.Clear();
					teleport_is_moving_back = false;
					move_point_index = 0;
					starting_wait_time = time_elapsed;
					if (stop_after_chain_of_commands) {
						triggered = false;
						stop_after_chain_of_commands = false;
					}
				}
			}
		}
	}

	private void CalculateRotationAngle () {
		Vector3 direction = (coordinates_list[move_point_index] - transform.position).normalized;
		rotation = Quaternion.LookRotation(direction);
		Vector3 angle = rotation.eulerAngles;
		angle.x = 0;
		rotation = Quaternion.Euler(angle);
		rotation_angle = Quaternion.Angle(transform.rotation, rotation);
	}

	public void SetCoordinates (List<Vector3> coordinates_list) {
		this.coordinates_list = new List<Vector3>(coordinates_list);
		move_point_index = 1;
		CalculateRotationAngle();
	}

	public bool CheckIfTriggered () {
		return triggered;
	}

	IEnumerator DespawnAnimation () {
		yield return new WaitForSeconds(despawn_animation_time);
		foreach(Collider collider in GetComponents<Collider>())
			collider.enabled = true;
		SpawnPool pool = PoolManager.Pools[CurrentLevel.GetCurrentLevelPoolName()];
		Despawner.Despawn(pool, gameObject);
    }
}
