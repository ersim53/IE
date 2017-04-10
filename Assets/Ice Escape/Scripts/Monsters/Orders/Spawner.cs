using AssemblyCSharp;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;

public class Spawner : EngineStrategy.MonsterState {
	public GameObject unit;
	public float time_elapsed;
	public float time_interval;
	public MoveType move_type;
	public float movement_speed;
	public float rotation_speed;
	public float wait_time_between_moves;
	public bool no_rotation;
	public bool no_movements;
	public float life_time;
	public float spawn_animation_time;
	public float despawn_animation_time;

	private List<Vector3> coordinates_list = new List<Vector3>();
	private float current_time_interval;

#if BOLT
	public override void Attached () {
#else
	public virtual void Start () {
#endif
		coordinates_list.Add(transform.position);
		foreach (Transform child in transform) {
			if (child.tag == "Target") {
				coordinates_list.Add(child.position);
			}
		}
		current_time_interval = 0;
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
		else if (current_time_interval > 0) {
			current_time_interval -= EngineStrategy.GetDeltaTime();
		}
		else {
			current_time_interval = time_interval;
			InstantiatePrefab();
		}
	}

	private void InstantiatePrefab () {
		SpawnPool level_pool = PoolManager.Pools[CurrentLevel.GetCurrentLevelPoolName()];
		Transform unit_entity = level_pool.Spawn(unit, transform.position, transform.rotation);
		if (!Equals(unit_entity.gameObject.GetComponent<BoltEntity>(), null)) {
			BoltNetwork.Attach(unit_entity.gameObject);
		}

		if (!no_movements) {
			MoveTo move_to = unit_entity.GetComponent<MoveTo>();
			move_to.SetCoordinates(coordinates_list);
			move_to.move_type = move_type;
			move_to.movement_speed = movement_speed;
			move_to.rotation_speed = rotation_speed;
			move_to.wait_time_between_moves = wait_time_between_moves;
			move_to.no_rotation = no_rotation;
			move_to.despawn_animation_time = despawn_animation_time;
		}
		if (life_time > 0) {
			LifeTime life = unit_entity.GetComponent<LifeTime>();
			life.life_time = life_time;
			life.despawn_time = despawn_animation_time;
			if (!life.automatically_start) {
				life.StartLifeTime();
			}
		}
		Transform[] allChildren = unit_entity.GetComponentsInChildren<Transform>();
		foreach (Transform child in allChildren) {
			if (child.GetComponent<ParticleSystem>() != null) {
				child.GetComponent<ParticleSystem>().Play();
			}
		}
		if (spawn_animation_time > 0) {
			foreach(Collider collider in GetComponents<Collider>())
				collider.enabled = false;
			StartCoroutine(SpawnAnimation());
		}
	}

	IEnumerator SpawnAnimation () {
		yield return new WaitForSeconds(spawn_animation_time);
		foreach(Collider collider in GetComponents<Collider>())
			collider.enabled = true;
    }
}
