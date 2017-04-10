using AssemblyCSharp;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DetectCollision : EngineStrategy.MonsterState {
	private List<Collider> collider_dead_list = new List<Collider>();

#if BOLT
	public override void Attached () {
#else
	public virtual void Start () {
#endif
	}

#if BOLT
	public override void SimulateOwner () {
#else
	public virtual void Update () {
#endif
	}

	void OnTriggerEnter(Collider collider) {
		if ((collider.gameObject.tag == "Player") && (collider.gameObject.GetComponent<PlayerMovements>().CanMove())) {
			transform.parent.GetComponent<MoveTo>().PlayerCollisionEnter();
        }
    }

	void OnTriggerExit(Collider collider) {
		if ((collider.gameObject.tag == "Player") && (collider.gameObject.GetComponent<PlayerMovements>().CanMove())) {
			transform.parent.GetComponent<MoveTo>().PlayerCollisionExit();
        }
		if ((collider.gameObject.tag == "Player") && (collider_dead_list.Contains(collider))) {
			collider_dead_list.Remove(collider);
		}
    }

	void OnTriggerStay (Collider collider) {
		if (collider.gameObject.tag == "Player") {
			if ((!collider.gameObject.GetComponent<PlayerMovements>().CanMove()) && (!collider_dead_list.Contains(collider))) {
				transform.parent.GetComponent<MoveTo>().PlayerCollisionExit();
				collider_dead_list.Add(collider);
			} // When player is ressurected by another player.
			else if ((collider.gameObject.GetComponent<PlayerMovements>().CanMove()) && (collider_dead_list.Contains(collider))) {
				transform.parent.GetComponent<MoveTo>().PlayerCollisionEnter();
				collider_dead_list.Remove(collider);
			}
		}
	}
}

