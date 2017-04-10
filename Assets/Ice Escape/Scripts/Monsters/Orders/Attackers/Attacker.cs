using AssemblyCSharp;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;

public class Attacker : EngineStrategy.MonsterState {
	public GameObject projectil;
	public MoveType throw_type;
	public float time_elapsed;
	public float time_between_throws;
	public float movement_speed;
	public float area_of_effect;
	public float base_damage;
	public float damage_distance_factor;
	public bool attack_player;
	public bool auto_aim;

	private SpawnProjectile spawner;
	private AttackerRotation rotation;
	private float current_time_between_throws;
	private GameObject[] players;
	private List<GameObject> players_list = new List<GameObject>();
	private bool players_initialized;

	//Rotation
	public bool no_rotation;
	public float rotation_speed;

	//Catapults
	public float time_interval;

	protected List<Vector3> coordinates_list = new List<Vector3>();
	protected int throw_point_index;
	private float current_time_interval;

	//Mortars
	public bool random;
	public GameObject platform;
	public float range;
	public int throwing_amount;
	public bool trigger_with_platform;

	protected Vector3 throw_point = new Vector3();
	private bool triggered;
	private MoveTo move_to;
	private int current_throw_amount;
	private float current_throw_time;

	private const float ROTATION_ANGLE_THRESHOLD = 0.1f;
	private const float MORTAR_TIME_BETWEEN_THROWS = 0.1f;

#if BOLT
	public override void Attached () {
#else
	public virtual void Start () {
#endif
		current_time_between_throws = 0;
		players_initialized = false;
		spawner = new SpawnProjectile(transform, random, projectil, movement_speed, area_of_effect, base_damage,
			damage_distance_factor, platform);
		rotation = new AttackerRotation(transform, random);
		if (random) {
			throw_point.y = platform.transform.position.y;
			RandomCoordinate();
			triggered = false;
			if (trigger_with_platform)
				move_to = platform.GetComponent<MoveTo>();
			current_throw_amount = 0;
			current_throw_time = MORTAR_TIME_BETWEEN_THROWS;
		}
		else {
			if (!attack_player) {
				foreach (Transform child in transform) {
					if (child.tag == "Target") {
						coordinates_list.Add(child.position);
					}
				}
			}
			throw_point_index = 0;
			current_time_interval = 0;
		}
	}

#if BOLT
	public override void SimulateOwner () {
#else
	public virtual void Update () {
#endif
		if (((attack_player) && (players_initialized)) || (!attack_player)) {
			if (!random) {
				Attack();
			}
			else {
				CheckIfTriggered();
				if (((trigger_with_platform) && (triggered)) || (!trigger_with_platform)) {
					Attack();
				}
			}
		}
		else if ((attack_player) && (!players_initialized)) {
			if (GameObject.FindGameObjectsWithTag("Player").Length > 0) {
				InitPlayers();
			}
		}
	}

	private void InitPlayers () {
		players_list.Clear();
		coordinates_list.Clear();
		players = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject player in players) {
			players_list.Add(player);
		}
		foreach (GameObject player in players) {
			coordinates_list.Add(player.transform.position);
		}
		players_initialized = true;
	}

	private void Attack () {
		if (!WaitForTimers()) {
			if (VerifyRange()) {
				if (!attack_player) {
					ResetTimers();
				}
				if (!auto_aim) {
					spawner.InstantiatePrefab(GetThrowPoint());
				}
				else {
					spawner.InstantiatePrefab(players_list[throw_point_index]);
				}
			}
			NextThrowPoint();
		}
		if (!no_rotation) {
			rotation.CalculateRotationAngle(GetThrowPoint());
			Rotate();
		}
	}

	private bool WaitForTimers () {
		if (time_elapsed > 0) {
			time_elapsed -= EngineStrategy.GetDeltaTime();
		}
		else if ((!random) && (current_time_interval > 0)) {
			current_time_interval -= EngineStrategy.GetDeltaTime();
		}
		else if ((current_time_between_throws > 0) && ((current_throw_amount == throwing_amount) || (!random))) {
			current_time_between_throws -= EngineStrategy.GetDeltaTime();
		}
		else if ((random) && (current_throw_time > 0) && (current_throw_amount < throwing_amount)) {
			current_throw_time -= EngineStrategy.GetDeltaTime();
		}
		else {
			return false;
		}
		return true;
	}

	private void ResetTimers () {
		current_time_between_throws = time_between_throws;
		if (random) {
			if (current_throw_amount == throwing_amount) {
				current_throw_amount = 0;
			}
			current_throw_time = MORTAR_TIME_BETWEEN_THROWS;
			current_throw_amount++;
		}
	}

	private void Rotate () {
		if (rotation.GetRotationAngle() > ROTATION_ANGLE_THRESHOLD) {
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation.GetRotation(), EngineStrategy.GetDeltaTime()
				* rotation_speed);
		}
	}

	private void NextThrowPoint () {
		if (!random) {
			if (throw_point_index < coordinates_list.Count - 1) {
				throw_point_index++;
			}
			else {
				if (throw_type == MoveType.stop) {
					Destroy(gameObject);
				}
				else if (throw_type == MoveType.continuous) {
					throw_point_index = 0;
					if (!attack_player) {
						current_time_interval = time_interval;
					}
				}
				else if (throw_type == MoveType.patrol) {
					coordinates_list.Reverse();
					throw_point_index = 0;
					if (!attack_player) {
						current_time_interval = time_interval;
					}
				}
				if (attack_player) {
					InitPlayers();
				}
			}
		}
		else {
			RandomCoordinate();
		}
		if ((attack_player) && (VerifyRange())) {
			current_time_interval = time_interval;
		}
	}

	private void RandomCoordinate () {
		throw_point.x = UnityEngine.Random.Range(-platform.transform.GetComponent<Renderer>().bounds.extents.x + 0.5f, platform.transform.GetComponent<Renderer>().bounds.extents.x - 0.5f);
		throw_point.z = UnityEngine.Random.Range(-platform.transform.GetComponent<Renderer>().bounds.extents.z  + 0.5f, platform.transform.GetComponent<Renderer>().bounds.extents.z - 0.5f);
	}

	private void CheckIfTriggered () {
		if (trigger_with_platform)
			triggered = move_to.CheckIfTriggered();
	}

	private Vector3 GetThrowPoint () {
		if (!random) {
			if (attack_player) {
				return players_list[throw_point_index].transform.position;
			}
			else {
				return coordinates_list[throw_point_index];
			}
		}
		else {
			return throw_point;
		}
	}

	private bool VerifyRange () {
		if (attack_player) {
			if (!players_list[throw_point_index].GetComponent<PlayerMovements>().CanMove()) {
				return false;
			}
		}
		return (((transform.position - GetThrowPoint()).magnitude < range) || (range == 0));
	}
}




