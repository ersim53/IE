using AssemblyCSharp;
using UnityEngine;
using System;
using System.Collections;

public class PlayerMovements : Player {
	private PlayerCollision collision;
	private bool player_already_moving;
	private bool movements_locked;
	private bool stunned;
	private float stun_timer;

	private const float MINIMUM_MOVE_DISTANCE = 0.2f;
	private const float FORWARD_VECTOR_DISTANCE = 1f;

#if BOLT
	public override void Attached () {
		base.Attached();
#else
	public override void Start () {
		base.Start();
#endif		
		collision = new PlayerCollision(respawner, transform, rotation_speed);
		player_already_moving = false;
		movements_locked = false;
		stunned = false;
		stun_timer = 0;
	}

#if BOLT
	public override void SimulateOwner () {
		base.SimulateOwner();
#else
	public override void Update () {
		base.Update();
#endif
		if (!movements_locked) {
			platforming.DetectPlatform();
			DeterminePlayerMovements();
			CheckIfPlayerIsGettingCrushed();
			if (!stunned) {
				move.Move();
				rotate.Rotate();
			}
			else {
				if (stun_timer >  0) {
					stun_timer -= EngineStrategy.GetDeltaTime();
				}
				else {
					stunned = false;
				}
			}
			fall.Fall();
		}
	}

	void OnTriggerEnter(Collider collider) {
		if (!movements_locked) {
			collision.OnCollision(gameObject, collider, rotate);
        }
    }

	private void DeterminePlayerMovements () {
		if (platforming.GetCurrentMovementType() == PlayerMovementType.Snow) {
			SetPlayerMovements(false, false);
        }
		else if (platforming.GetCurrentMovementType() == PlayerMovementType.Ice) {
			platforming.MoveForwardOnIce();
			SetPlayerMovements(true, false);
        }
		else if (platforming.GetCurrentMovementType() == PlayerMovementType.Reverse) {
			platforming.MoveForwardOnIce();
			SetPlayerMovements(false, true);
        }
		else if (platforming.GetCurrentMovementType() == PlayerMovementType.Straight) {
			collision.RotateTorwardArrow();
			platforming.MoveForwardOnIce();
        }
	}

	private void SetPlayerMovements (bool on_ice, bool on_reverse) {
		if ((Input.GetMouseButton(1)) && (!player_already_moving)) {
			player_already_moving = true;
			collision.IsRotatingWithArrow(false);
			Vector3 mouse_position = Input.mousePosition;
			Ray path = Camera.main.ScreenPointToRay(mouse_position);
			RaycastHit mouse_hit;
			int layerMask = 1 << 8;
			layerMask = ~layerMask;
			if (Physics.Raycast(path, out mouse_hit, 10000, layerMask)) {
				Vector3 move_point = mouse_hit.point;
				move_point.y = transform.position.y;
				if ((!platforming.PlayerOnIce()) && ((transform.position - move_point).magnitude > MINIMUM_MOVE_DISTANCE)) {
					move.SetMovePoint(mouse_hit);
				}
				rotate.SetRotationPoint(mouse_hit);
				rotate.SetOnIce(on_ice);
				rotate.SetOnReverseIce(on_reverse);
			}
		}
		else if ((!Input.GetMouseButton(1)) && (player_already_moving)) {
			player_already_moving = false;
		}
	}

	private void CheckIfPlayerIsGettingCrushed () {
		RaycastHit cliff_hit;
		Vector3 up_direction_ray = new Vector3(0, 1, 0);

		if (Physics.Raycast(transform.position, up_direction_ray, out cliff_hit, FORWARD_VECTOR_DISTANCE)) {
			if ((cliff_hit.collider.gameObject.tag == "Snow") || (cliff_hit.collider.gameObject.tag == "Ice") ||
				(cliff_hit.collider.gameObject.tag == "Straight") || (cliff_hit.collider.gameObject.tag == "Reverse")
				|| (cliff_hit.collider.gameObject.tag == "Door")) {
				respawner.KillPlayer(gameObject, PlayerAnimations.Death, true);
			}
		}
	}

	public void LockMovements(bool locked) {
		movements_locked = locked;
		move.StopMoving();
		rotate.StopRotating();
	}

	public void StunPlayer (float duration) {
		stun_timer = duration;
		stunned = true;
	}

	public bool CanMove () {
		return !movements_locked;
	}
}
