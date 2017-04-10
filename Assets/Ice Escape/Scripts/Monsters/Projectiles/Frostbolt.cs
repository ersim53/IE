using AssemblyCSharp;
using UnityEngine;
using System;
using PathologicalGames;
using System.Collections.Generic;

public class Frostbolt : EngineStrategy.MonsterState {
	public GameObject target;
	public float movement_speed;
	public GameObject explosion;
	public float rotation_speed;

	private ProjectileExplosion projectile_explosion;
	private Quaternion rotation;
	private float rotation_angle;

	private const float ROTATION_ANGLE_THRESHOLD = 0.1f;

#if BOLT
	public override void Attached () {
		state.SetTransforms(state.Transform, transform);
#else
	public virtual void Start () {
#endif
		projectile_explosion = new ProjectileExplosion();
	}

	public void SetProjectile (float area_of_effect, float base_damage, float damage_distance_factor) {
		List<Debuffs> debuffs = new List<Debuffs>();
		debuffs.Add(Debuffs.freeze);
		projectile_explosion.SetExplosion(area_of_effect, base_damage, damage_distance_factor, debuffs);
	}

#if BOLT
	public override void SimulateOwner () {
#else
	public virtual void Update () {
#endif
		if (gameObject.activeSelf) {
			Move();
			CalculateRotationAngle();
			Rotate();
		}
	}

	void OnEnable () {
		if (!BoltEntityExtensions.IsAttached(gameObject.GetComponent<BoltEntity>())) {
			BoltNetwork.Attach(gameObject);
		}
    }

	public void OnTriggerEnter (Collider collider) {
		if (collider.gameObject.tag == "Player") {
			projectile_explosion.Explode(gameObject, explosion, transform.position, transform.rotation);
        }
	}

	private void Move () {
		transform.position += transform.forward * EngineStrategy.GetDeltaTime() * movement_speed;
	}

	private void Rotate () {
		if (rotation_angle > ROTATION_ANGLE_THRESHOLD) {
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation, EngineStrategy.GetDeltaTime()
				* rotation_speed);
		}
	}

	public void CalculateRotationAngle () {
		Vector3 direction;
		direction = (target.transform.position - transform.position).normalized;
		rotation = Quaternion.LookRotation(direction);
		Vector3 angle = rotation.eulerAngles;
		angle.x = 0;
		rotation = Quaternion.Euler(angle);
		rotation_angle = Quaternion.Angle(transform.rotation, rotation);
	}
}

