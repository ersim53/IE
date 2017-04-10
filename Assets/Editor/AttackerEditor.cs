using UnityEditor;
using UnityEngine;
 
 [CustomEditor(typeof(Attacker)), CanEditMultipleObjects]
 public class AttackerEditor : Editor {
     
	public SerializedProperty 
		random_property,
		projectil_property,
		throw_type_property,
		time_elapsed_property,
		time_interval_property,
		time_between_throws_property,
		rotation_speed_property,
		no_rotation_property,
		movement_speed_property,
		area_of_effect_property,
		base_damage_property,
		damage_distance_factor_property,
		platform_property,
		range_property,
		throwing_amount_property,
		trigger_with_platform_property,
		attack_player_property,
		auto_aim_property;
     
	void OnEnable () {
		random_property = serializedObject.FindProperty ("random");
		projectil_property = serializedObject.FindProperty ("projectil");
		throw_type_property = serializedObject.FindProperty ("throw_type");
		time_elapsed_property = serializedObject.FindProperty ("time_elapsed");
		time_interval_property = serializedObject.FindProperty ("time_interval");
		time_between_throws_property = serializedObject.FindProperty ("time_between_throws");
		rotation_speed_property = serializedObject.FindProperty ("rotation_speed");
		no_rotation_property = serializedObject.FindProperty ("no_rotation");
		movement_speed_property = serializedObject.FindProperty ("movement_speed");
		area_of_effect_property = serializedObject.FindProperty ("area_of_effect");
		base_damage_property = serializedObject.FindProperty ("base_damage");
		damage_distance_factor_property = serializedObject.FindProperty ("damage_distance_factor");
		platform_property = serializedObject.FindProperty ("platform");
		range_property = serializedObject.FindProperty ("range");
		throwing_amount_property = serializedObject.FindProperty ("throwing_amount");
		trigger_with_platform_property = serializedObject.FindProperty ("trigger_with_platform");
		attack_player_property = serializedObject.FindProperty ("attack_player");
		auto_aim_property = serializedObject.FindProperty ("auto_aim");
	}
     
	public override void OnInspectorGUI() {
		serializedObject.Update ();
		var attacker = target as Attacker;

		EditorGUILayout.PropertyField(projectil_property, new GUIContent("Projectile type"));
		EditorGUILayout.PropertyField(throw_type_property, new GUIContent("Throw type"));
		EditorGUILayout.PropertyField(time_elapsed_property, new GUIContent("Time elapsed"));
		EditorGUILayout.PropertyField(time_between_throws_property, new GUIContent("Time between throws"));
		EditorGUILayout.PropertyField(movement_speed_property, new GUIContent("Projectile speed"));
		EditorGUILayout.PropertyField(area_of_effect_property, new GUIContent("Area of effect"));
		EditorGUILayout.PropertyField(base_damage_property, new GUIContent("Base damage"));
		EditorGUILayout.PropertyField(damage_distance_factor_property, new GUIContent("Area damage factor"));
		EditorGUILayout.PropertyField(range_property, new GUIContent("Range"));

		attacker.no_rotation = GUILayout.Toggle(attacker.no_rotation, "Disable rotation");
		if(!attacker.no_rotation) {
			EditorGUILayout.PropertyField(rotation_speed_property, new GUIContent("Rotation Speed"));    
		}

		attacker.random = GUILayout.Toggle(attacker.random, "Random attacks");
		if(attacker.random) {
			EditorGUILayout.PropertyField(platform_property, new GUIContent("Platform"));
			EditorGUILayout.PropertyField(throwing_amount_property, new GUIContent("Throwing Amount"));  
			EditorGUILayout.PropertyField(trigger_with_platform_property, new GUIContent("Trigger with platform"));  
		}
		else {
			EditorGUILayout.PropertyField(time_interval_property, new GUIContent("Time interval"));
			attacker.attack_player = GUILayout.Toggle(attacker.attack_player, "Attack player");
			if(attacker.attack_player) {
				EditorGUILayout.PropertyField(auto_aim_property, new GUIContent("Auto aim player"));
			}
		}
		serializedObject.ApplyModifiedProperties ();
	}
 }
