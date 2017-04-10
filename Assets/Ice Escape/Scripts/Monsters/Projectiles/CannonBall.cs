using AssemblyCSharp;
using UnityEngine;
using System;
using PathologicalGames;

public class CannonBall : EngineStrategy.MonsterState {
	public Vector3 coordinate;
	public float movement_speed;
	public Vector3 catapult_position;
	public GameObject explosion;

	private Rigidbody body;
	private ProjectileExplosion projectile_explosion;

	private const float COLLISION_DISTANCE = 0.25f;
	private const float PROJECTILE_LOB_MINIMUM_TIME = 5f;

#if BOLT
	public override void Attached () {
		state.SetTransforms(state.Transform, transform);
#else
	public virtual void Start () {
#endif
		body = GetComponent<Rigidbody>();
		projectile_explosion = new ProjectileExplosion();
	}

	public void SetProjectile (float area_of_effect, float base_damage, float damage_distance_factor) {
		projectile_explosion.SetExplosion(area_of_effect, base_damage, damage_distance_factor, null);
		LobProjectile();
	}

#if BOLT
	public override void SimulateOwner () {
#else
	public virtual void Update () {
#endif
		if (gameObject.activeSelf) {
			Move();
		}
	}

	void OnEnable () {
		if (!BoltEntityExtensions.IsAttached(gameObject.GetComponent<BoltEntity>())) {
			//BoltNetwork.Attach(gameObject);
		}
		body.velocity = Vector3.zero;
		body.angularVelocity = Vector3.zero;
    }

	private void Move () {
		if ((transform.position - coordinate).magnitude > COLLISION_DISTANCE) {
			transform.position = Vector3.MoveTowards(transform.position, coordinate, movement_speed *
				EngineStrategy.GetDeltaTime());
		}
		else {
			projectile_explosion.Explode(gameObject, explosion, transform.position, transform.rotation);
		}
	}

	private void LobProjectile () {
		float distance = (transform.position - coordinate).magnitude;
		float time = distance * movement_speed * EngineStrategy.GetDeltaTime();
		if (time < PROJECTILE_LOB_MINIMUM_TIME)
			time = PROJECTILE_LOB_MINIMUM_TIME;
		Vector3 throwSpeed = calculateBestThrowSpeed(catapult_position, coordinate, time);
		body.AddForce(throwSpeed, ForceMode.VelocityChange);
	}

	private static Vector3 calculateBestThrowSpeed(Vector3 origin, Vector3 target, float time_to_target) {
		Vector3 to_target = target - origin;
		Vector3 to_target_xz = to_target;
		to_target_xz.y = 0f;

		float y = to_target.y;
		float xz = to_target_xz.magnitude;
		float t = time_to_target;
		float v0y = y / t + 0.5f * Physics.gravity.magnitude * t;
		float v0xz = xz / t;

		Vector3 result = to_target_xz.normalized;
		result *= v0xz;
		result.y = v0y;

		return result;
	}
}

