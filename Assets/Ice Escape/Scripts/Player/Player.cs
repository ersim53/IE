using AssemblyCSharp;
using UnityEngine;
using System;
using System.Collections;

public class Player : EngineStrategy.DragonState {
	public int movement_speed;
	public int rotation_speed;
	public int ice_movement_speed;
	public int ice_rotation_speed;
	public int fall_speed;
	public int health;
	public GameObject[] player_effects;

	protected PlayerMove move;
	protected PlayerRotate rotate;
	protected PlayerFall fall;
	protected PlayerSetMovements platforming;
	protected PlayerAnimator animations;
	protected PlayerRespawn respawner;
	protected PlayerHP health_bar;

#if BOLT
	public override void Attached () {
		animations = new PlayerAnimator(state);
#else
	public virtual void Start () {
		animations = new PlayerAnimator();
#endif		
		move = new PlayerMove(transform, animations);
		rotate = new PlayerRotate(transform);
		fall = new PlayerFall(transform, animations);
		respawner = new PlayerRespawn(animations, player_effects[1]);
		platforming = new PlayerSetMovements(transform, gameObject, move, rotate, fall, animations, respawner, player_effects[0]);
		health_bar = new PlayerHP(health, transform, respawner);


		move.SetMovementSpeed((float)movement_speed);
		rotate.SetRotationSpeed((float)rotation_speed);
		rotate.SetIceRotationSpeed((float)ice_rotation_speed);
		fall.SetFallSpeed((float)fall_speed);
		platforming.SetMovementSpeed((float)movement_speed);
		platforming.SetIceMovementSpeed((float)ice_movement_speed);
		health_bar.SetHealthBarInVisibile();
#if BOLT
		state.SetTransforms(state.Transform, transform);

		state.SetAnimator(GetComponent<Animator>());
    	state.Animator.applyRootMotion = entity.isOwner;
		state.Animator.SetInteger("Type", 0); //Tiger = 0.
		//state.Stand = true;
		state.Animator.SetBool("Stand", true);

		//Player colors

		string player_network_id = entity.networkId.ToString();
		player_network_id = player_network_id.Substring(Math.Max(0, player_network_id.Length - 2));
		player_network_id = player_network_id[0].ToString();
//		if (entity.isOwner) {
//			if (player_network_id == "1") {
//				state.CubeColor = new Color(255, 0, 0);
//			}
//			else if (player_network_id == "2") {
//				state.CubeColor = new Color(0, 255, 0);
//			}
//				else {
//				state.CubeColor = new Color(255, 255, 255);
//			}
//		}
//		state.AddCallback("CubeColor", ColorChanged);
#endif
	}

#if BOLT
	public override void SimulateOwner () {
#else
	public virtual void Update () {
#endif
		if (Input.GetKeyDown(KeyCode.LeftAlt)) {
			foreach(GameObject player in GameObject.FindGameObjectsWithTag("Player")) {
				Player player_index = player.GetComponent<Player>();
				player_index.health_bar.SetHealthBarVisibile();
			}
		}
		else if (Input.GetKeyUp(KeyCode.LeftAlt)) {
			foreach(GameObject player in GameObject.FindGameObjectsWithTag("Player")) {
				Player player_index = player.GetComponent<Player>();
				player_index.health_bar.SetHealthBarInVisibile();
			}
		}
	}

	public void SetRespawnPosition (Vector3 position) {
		respawner.SetRespawnPosition(position);
	}

	public void DamagePlayer (int damage) {
		health_bar.DamagePlayer(damage);
	}

	public void SetPlayerHP (int health) {
		health_bar.SetHP(health);
	}

	public void FullHealPlayer () {
		health_bar.FullHealPlayer();
	}

	public void HealthBarVisibility (bool visible) {
		if (visible) {
			health_bar.SetHealthBarVisibile();
		}
		else {
			health_bar.SetHealthBarInVisibile();
		}
	}

	public void ActivateSpeedBoost (bool activate) {
		platforming.ActivateSpeedBoost(activate);
	}

#if BOLT
	void ColorChanged() {
	  //GetComponent<Renderer>().material.color = state.CubeColor;
	}
#endif

#if BOLT
	void OnGUI() {
//	if (entity.isOwner) {
//		GUI.color = state.CubeColor;
//		GUILayout.Label("@@@");
//		GUI.color = Color.white;
//		}
	}
#endif
}

