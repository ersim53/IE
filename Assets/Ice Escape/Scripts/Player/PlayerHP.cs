using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace AssemblyCSharp {
	public class PlayerHP {
		private int current_health;
		private int max_health;
		private Image health_bar;
		private Image health_bar_background;
		private bool currently_visible;
		private Player player;
		private bool coroutine_canceled;
		private PlayerRespawn respawner;
		private PlayerAnimator animations;
		private Transform transform;

		private const float SHOW_HEALTH_BAR_TIMER = 2f;

		public PlayerHP (int health, Transform transform, PlayerRespawn respawner) {
			max_health = health;
			current_health = max_health;
			currently_visible = false;
			coroutine_canceled = false;
			health_bar = transform.FindChild("HealthBarCanvas").FindChild("HealthBarBG").FindChild("HealthBar").GetComponent<Image>();
			health_bar_background = transform.FindChild("HealthBarCanvas").FindChild("HealthBarBG").GetComponent<Image>();
			player = transform.GetComponent<Player>();
			this.respawner = respawner;
			this.transform = transform;
		}

		public void DamagePlayer (int damage) {
			current_health -= damage;
			if (current_health > 0) {
				health_bar.fillAmount = (float)current_health / (float)max_health;
				if (!currently_visible) {
					player.StartCoroutine(PeriodicallyShowHealthBar(SHOW_HEALTH_BAR_TIMER));
				}
			}
			else {
				respawner.KillPlayer(transform.gameObject, PlayerAnimations.Stand, true);
			}
		}

		public void SetHealthBarVisibile () {
			if (current_health > 0) {
				currently_visible = true;
				coroutine_canceled = true;
				health_bar.enabled = true;
				health_bar_background.enabled = true;
			}
		}

		public void SetHealthBarInVisibile () {
			currently_visible = false;
			coroutine_canceled = true;
			health_bar.enabled = false;
			health_bar_background.enabled = false;
		}

		IEnumerator PeriodicallyShowHealthBar (float time) {
			SetHealthBarVisibile();
			coroutine_canceled = false;
			yield return new WaitForSeconds(time);
			if (!coroutine_canceled) {
				SetHealthBarInVisibile();
			}
			coroutine_canceled = false;
	    }

		public void FullHealPlayer () {
			current_health = max_health;
			health_bar.fillAmount = (float)current_health / (float)max_health;
	    }

		public void SetHP (int health) {
			current_health = health;
			health_bar.fillAmount = (float)current_health / (float)max_health;
	    }
	}
}
