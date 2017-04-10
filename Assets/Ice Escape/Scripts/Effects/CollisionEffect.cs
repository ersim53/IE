using AssemblyCSharp;
using UnityEngine;
using System;
using PathologicalGames;

public class CollisionEffect : MonoBehaviour {
	public GameObject effect;
	public string pool_name;
	public float rotation_angle_x;
	public float rotation_angle_y;
	public float rotation_angle_z;

	public virtual void Start () {
	}

	void OnTriggerEnter(Collider collider) {
		if (CurrentLevel.LevelInitialized()) {
			if ((collider.tag == "Snow") || (collider.tag == "Ice") ||
				(collider.tag == "Reverse") || (collider.tag == "Straight")) {
				SpawnPool pool;
				if (pool_name == "Current Level") {
					pool = PoolManager.Pools[CurrentLevel.GetCurrentLevelPoolName()];
				}
				else {
					pool = PoolManager.Pools[CurrentLevel.GetGeneralPoolName()];
				}
				Quaternion rotation = collider.transform.rotation * Quaternion.Euler(rotation_angle_x, rotation_angle_y, rotation_angle_z);
				pool.Spawn(effect, transform.position, rotation);
	        }
        }
    }
}

