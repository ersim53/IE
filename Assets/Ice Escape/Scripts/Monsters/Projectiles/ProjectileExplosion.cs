using UnityEngine;
using System;
using PathologicalGames;
using System.Collections.Generic;

namespace AssemblyCSharp {
	public class ProjectileExplosion {
		private float area_of_effect;
		private float base_damage;
		private float damage_distance_factor;
		private List<Debuffs> debuffs = new List<Debuffs>();

		private const float FREEZE_DURATION = 2f;
		private const float STUN_DURATION = 1f;

		public ProjectileExplosion () {
		}

		public void SetExplosion (float area_of_effect, float base_damage, float damage_distance_factor, List<Debuffs> debuffs) {
			this.area_of_effect = area_of_effect;
			this.base_damage = base_damage;
			this.damage_distance_factor = damage_distance_factor;
			if (debuffs != null) {
				this.debuffs = new List<Debuffs>(debuffs);
			}
		}

		public void Explode (GameObject projectile, GameObject explosion, Vector3 position, Quaternion rotation) {
			SpawnPool level_pool = PoolManager.Pools[CurrentLevel.GetCurrentLevelPoolName()];
			Despawner.Despawn(level_pool, projectile);
			level_pool.Spawn(explosion, position, rotation);
			DealDamage(projectile.transform, area_of_effect);
		}

		private void DealDamage (Transform explosion_transform, float area_of_effect) {
			Collider[] hit_colliders = Physics.OverlapSphere(explosion_transform.position, area_of_effect);
			foreach (Collider collider in hit_colliders) {
				if (collider.transform.tag == "Player") {
					float distance_to_explosion = (collider.transform.position - explosion_transform.position).magnitude;
					int damage = (int)(base_damage - (distance_to_explosion * damage_distance_factor));
					if (damage > 0) {
						Player player = collider.transform.GetComponent<Player>();
						player.DamagePlayer(damage);
					}
					if (debuffs.Count > 0) {
						DealDebuffs(collider);
					}
				}
			}
		}

		private void DealDebuffs (Collider collider) {
			if (debuffs.Contains(Debuffs.freeze)) {
				collider.transform.GetComponent<PlayerMovements>().StunPlayer(FREEZE_DURATION);
			}
			if (debuffs.Contains(Debuffs.stun)) {
				collider.transform.GetComponent<PlayerMovements>().StunPlayer(STUN_DURATION);
			}
		}
	}
}

