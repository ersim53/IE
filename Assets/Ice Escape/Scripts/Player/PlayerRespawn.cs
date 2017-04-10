using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;

namespace AssemblyCSharp {
	public class PlayerRespawn {
		private Vector3 respawn_position;
		private GameObject player_unit;
		private PlayerMovements player_movements;
		private PlayerAnimator animator;
		private GameObject death_effect;
		private bool player_death;

		private const float RESPAWN_TIMER = 10f;
		private const float DEATH_ANIMATION_TIME = 3f;

		public PlayerRespawn (PlayerAnimator animator, GameObject death_effect) {
			this.animator = animator;
			this.death_effect = death_effect;
			player_death = false;
		}

		public void KillPlayer (GameObject unit, PlayerAnimations animation, bool play_death_effect) {
			if (!player_death) {
				player_death = true;
				player_unit = unit;
				player_movements = player_unit.GetComponent<PlayerMovements>();
				player_movements.LockMovements(true);
				animator.SetAnimation(animation);
				player_movements.HealthBarVisibility(false);
				player_movements.SetPlayerHP(0);
				player_movements.ActivateSpeedBoost(false);
				if (play_death_effect) {
					SpawnPool general_pool = PoolManager.Pools[CurrentLevel.GetGeneralPoolName()];
					general_pool.Spawn(death_effect, unit.transform.position, new Quaternion(0, 0, 0, 0));
				}

				if (animation == PlayerAnimations.Death) {
					player_movements.StartCoroutine(DeathAnimation());
				}
				else {
					player_movements.StartCoroutine(ResurrectPlayer(RESPAWN_TIMER));
				}
			}
		}

		public void SetRespawnPosition (Vector3 position) {
			respawn_position = position;
		}

		public Vector3 GetRespawnPosition () {
			return respawn_position;
		}

		IEnumerator DeathAnimation () {
			yield return new WaitForSeconds(DEATH_ANIMATION_TIME);
			player_movements.StartCoroutine(ResurrectPlayer(RESPAWN_TIMER - DEATH_ANIMATION_TIME));
	    }

		IEnumerator ResurrectPlayer (float wait_time) {
			ToggleUnitVisibility();
			yield return new WaitForSeconds(wait_time);
			player_movements.LockMovements(false);
			player_unit.transform.position = respawn_position;
			ToggleUnitVisibility();
			PlayerCamera.instance.SetCameraLock(true);
			player_movements.FullHealPlayer();
			player_death = false;
			animator.SetAnimation(PlayerAnimations.Stand);  // Not working
	    }

		private void ToggleUnitVisibility () {
			Renderer[] renderers = player_unit.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in renderers) {
				renderer.enabled = !renderer.enabled;
			}
		}
	}
}
