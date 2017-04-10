using AssemblyCSharp;
using UnityEngine;
using System;
using System.Collections;
using PathologicalGames;

public class LifeTime : EngineStrategy.MonsterState {
	public float life_time;
	public string pool_name;
	public float despawn_time;
	public bool automatically_start;
	public GameObject explosion;

#if BOLT
	public override void Attached () {
#else
	void Start () {
#endif		
	}

#if BOLT
	public override void SimulateOwner () {
#else
	void Update () {
#endif
	}

	void OnEnable () {
		if (automatically_start) {
			StartCoroutine(Despawn());
		}
		if (!BoltEntityExtensions.IsAttached(gameObject.GetComponent<BoltEntity>())) {
			//BoltNetwork.Attach(gameObject);
		}
    }

	void OnDisable () {
		StopAllCoroutines();
    }

    public void StartLifeTime () {
		StartCoroutine(Despawn());
    }

	IEnumerator Despawn () {
		yield return new WaitForSeconds(life_time);
		if (pool_name == "Current Level") {
			if (despawn_time > 0) {
				foreach(Collider collider in GetComponents<Collider>())
					collider.enabled = false;

				Transform[] allChildren = GetComponentsInChildren<Transform>();
				foreach (Transform child in allChildren) {
					if (child.GetComponent<ParticleSystem>() != null) {
						child.GetComponent<ParticleSystem>().Stop();
					}
				}

				yield return new WaitForSeconds(despawn_time);
				foreach(Collider collider in GetComponents<Collider>())
					collider.enabled = true;
				SpawnPool pool = PoolManager.Pools[CurrentLevel.GetCurrentLevelPoolName()];
				Despawner.Despawn(pool, gameObject);
			}
			else {
				SpawnPool pool = PoolManager.Pools[CurrentLevel.GetCurrentLevelPoolName()];
				Despawner.Despawn(pool, gameObject);
				if (explosion != null) {
					pool.Spawn(explosion, transform.position, transform.rotation);
				}
			}
		}
		else {
			SpawnPool pool = PoolManager.Pools[pool_name];
			Despawner.Despawn(pool, gameObject);
		}
    }
}


