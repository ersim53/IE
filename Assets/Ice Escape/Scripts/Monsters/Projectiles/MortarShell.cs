using AssemblyCSharp;
using UnityEngine;
using System;
using PathologicalGames;

public class MortarShell : EngineStrategy.MonsterState {
	public float movement_speed;
	public GameObject explosion;
	public GameObject platform;
	public Vector3 coordinate_on_platform;
	public Vector3 shadow_scale;

	private bool rising;
	private Vector3 coordinate;
	private ProjectileExplosion projectile_explosion;

	private const float COLLISION_DISTANCE = 0.25f;
	private const float MAXIMUM_HEIGHT = 35f;
	private const float SHADOW_HEIGHT_POSITION = 3.5f;

#if BOLT
	public override void Attached () {
		state.SetTransforms(state.Transform, transform);
#else
	public virtual void Start () {
#endif
		rising = true;
		projectile_explosion = new ProjectileExplosion();
	}

	public void SetProjectile (float area_of_effect, float base_damage, float damage_distance_factor) {
		projectile_explosion.SetExplosion(area_of_effect, base_damage, damage_distance_factor, null);
		coordinate.x = (platform.transform.position.x + coordinate_on_platform.x);
		coordinate.y = platform.transform.position.y + platform.transform.lossyScale.y;
		coordinate.z = (platform.transform.position.z + coordinate_on_platform.z);
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
			BoltNetwork.Attach(gameObject);
		}
		foreach (Transform child in transform) {
			child.localScale = shadow_scale;
		}
		rising = true;
    }

	private void Move () {
		if (rising) {
			if (transform.position.y < MAXIMUM_HEIGHT) {
				Vector3 rise_position = transform.position;
				rise_position.y = MAXIMUM_HEIGHT;
				transform.position = Vector3.MoveTowards(transform.position, rise_position, movement_speed *
					EngineStrategy.GetDeltaTime());
			}
			else {
				rising = false;
				Vector3 target_position = coordinate;
				target_position.y = MAXIMUM_HEIGHT;
				transform.position = target_position;
			}
		}
		else {
			UpdatePosition();
			UpdateCoordinates();
			UpdateShadow();
			if ((transform.position - coordinate).magnitude > COLLISION_DISTANCE) {
				transform.position = Vector3.MoveTowards(transform.position, coordinate, movement_speed *
					EngineStrategy.GetDeltaTime());
			}
			else {
				projectile_explosion.Explode(gameObject, explosion, transform.position, transform.rotation);
			}
		}
	}

	private void UpdatePosition () {
		Vector3 target_position = transform.position;
		target_position.x = (platform.transform.position.x + coordinate_on_platform.x);
		target_position.z = (platform.transform.position.z + coordinate_on_platform.z);
		transform.position = target_position;
	}

	private void UpdateCoordinates () {
		coordinate.x = (platform.transform.position.x + coordinate_on_platform.x);
		coordinate.z = (platform.transform.position.z + coordinate_on_platform.z);
	}

	private void UpdateShadow () {
		foreach (Transform child in transform) {
			Vector3 shadow = child.position;
			shadow.y = SHADOW_HEIGHT_POSITION;
			child.position = shadow;

			Vector3 shadow_scale = child.localScale;
			shadow_scale.x += 0.5f * EngineStrategy.GetDeltaTime();
			shadow_scale.z += 0.5f * EngineStrategy.GetDeltaTime();
			child.localScale = shadow_scale;
		}
	}
}


