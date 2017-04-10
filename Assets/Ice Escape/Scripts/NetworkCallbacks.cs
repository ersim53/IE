using AssemblyCSharp;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[BoltGlobalBehaviour]
public class NetworkCallbacks : Bolt.GlobalEventListener {
	private List<string> logMessages = new List<string>();
	private Vector3 spawn_position;
	private string current_map;

	public override void SceneLoadLocalDone (string map) {
		current_map = map;
		PlayerCamera.Instantiate();
		SetSpawnPosition(current_map);
		BoltEntity player_entity = BoltNetwork.Instantiate(BoltPrefabs.FireDragon, spawn_position, Quaternion.identity);
		PlayerCamera.instance.SetTarget(player_entity, current_map);
		player_entity.GetComponent<Player>().SetRespawnPosition(spawn_position);
		CurrentLevel.InitializeLevel(true);
	}

	public override void ControlOfEntityGained (BoltEntity arg) {
		PlayerCamera.instance.SetTarget(arg, current_map);
	}

	public override void OnEvent (LogEvent evnt) {
		logMessages.Insert(0, evnt.message);
	}

	void OnGUI () {
		int maxMessages = Mathf.Min(5, logMessages.Count);

		GUILayout.BeginArea(new Rect(Screen.width / 2 - 200, Screen.height - 100, 400, 100), GUI.skin.box);

		for (int i = 0; i < maxMessages; ++i) {
			GUILayout.Label(logMessages[i]);
		}

		GUILayout.EndArea();
	}

	private void SetSpawnPosition (string map_name) {
		if (map_name == "World 1") {
			spawn_position = new Vector3(0, 5, 30);
		}
		else if (map_name == "Level 1") {
			spawn_position = new Vector3(0, 5, -69);
		}
		else if (map_name == "Level 2") {
			spawn_position = new Vector3(0, 5, 0);
			CurrentLevel.SetCurrentLevelPoolName("W1L2 Pool");
		}
		else if (map_name == "Level 3") {
			spawn_position = new Vector3(0, 5, 0);
			CurrentLevel.SetCurrentLevelPoolName("W1L3 Pool");
		}
		else if (map_name == "Level 4") {
			spawn_position = new Vector3(0, 5, 0);
			CurrentLevel.SetCurrentLevelPoolName("W1L4 Pool");
		}
	}
}