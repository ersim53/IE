using UnityEngine;
using System;
using PathologicalGames;

namespace AssemblyCSharp {
	public class SpawnProjectile {
		private Transform transform;
		private bool random;
		private GameObject projectile;
		private float movement_speed;
		private float area_of_effect;
		private float base_damage;
		private float damage_distance_factor;
		private GameObject platform;

		public SpawnProjectile (Transform transform, bool random, GameObject projectile, float movement_speed, float area_of_effect,
			float base_damage, float damage_distance_factor, GameObject platform) {
			this.transform = transform;
			this.random = random;
			this.projectile = projectile;
			this.movement_speed = movement_speed;
			this.area_of_effect = area_of_effect;
			this.base_damage = base_damage;
			this.damage_distance_factor = damage_distance_factor;
			this.platform = platform;
		}

		public void InstantiatePrefab (Vector3 throw_point) {
			SpawnPool level_pool = PoolManager.Pools[CurrentLevel.GetCurrentLevelPoolName()];
			Transform unit_entity = level_pool.Spawn(projectile, transform.position, transform.rotation);
			if (!random) {
				CannonBall cannon_ball = unit_entity.GetComponent<CannonBall>();
				cannon_ball.coordinate = throw_point;
				cannon_ball.movement_speed = movement_speed;
				cannon_ball.catapult_position = transform.position;
				cannon_ball.SetProjectile(area_of_effect, base_damage, damage_distance_factor);
			}
			else {
				MortarShell mortar_shell = unit_entity.GetComponent<MortarShell>();
				mortar_shell.coordinate_on_platform = throw_point;
				mortar_shell.movement_speed = movement_speed;
				mortar_shell.platform = platform;
				mortar_shell.SetProjectile(area_of_effect, base_damage, damage_distance_factor);
			}
		}

		public void InstantiatePrefab (GameObject unit) {
			SpawnPool level_pool = PoolManager.Pools[CurrentLevel.GetCurrentLevelPoolName()];
			Transform unit_entity = level_pool.Spawn(projectile, transform.position, transform.rotation);

			Frostbolt frostbolt = unit_entity.GetComponent<Frostbolt>();
			frostbolt.target = unit;
			frostbolt.movement_speed = movement_speed;
			frostbolt.SetProjectile(area_of_effect, base_damage, damage_distance_factor);
		}
	}
}

