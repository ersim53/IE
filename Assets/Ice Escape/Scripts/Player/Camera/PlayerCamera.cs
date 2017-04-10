using UnityEngine;
using System.Collections;

public class PlayerCamera : BoltSingletonPrefab<PlayerCamera> {
	private BoltEntity camera_target;
	private Vector3 camera_position;
	private float current_zoom_level;
	private float unlocked_current_zoom_level;
	private bool camera_locked;
	private float camera_bound_bottom;
	private float camera_bound_top;
	private float camera_bound_left;
	private float camera_bound_right;

	private const float ZOOM_DEFAULT_Y = 20f;
	private const float ZOOM_DEFAULT_Z = 15f;
	private const float ZOOM_SENSITIVITY = 10f;
	private const float ZOOM_LEVEL_MINIMUM = -10f;
	private const float ZOOM_LEVEL_MAXIMUM = 10f;
	private const float ZOOM_Y_TO_Z_CONVERSION = 0.7f;
	private const float CAMERA_SPEED = 0.5f;
	private const int MOUSE_DETECTION_RECT_SIZE = 15;

	void Awake () {
		DontDestroyOnLoad(gameObject);
	}

    void Start () {
		current_zoom_level = 0;
		camera_locked = true;
    }

    void LateUpdate () {
		current_zoom_level += -(ZOOM_SENSITIVITY * Input.GetAxis("Mouse ScrollWheel"));
		current_zoom_level = Mathf.Clamp(current_zoom_level, ZOOM_LEVEL_MINIMUM, ZOOM_LEVEL_MAXIMUM);
		if (camera_target != null) {
			if (camera_target.isOwner) {
				if (Input.GetKeyDown(KeyCode.C)) {
					if (camera_locked) {
						camera_locked = false;
					} else {
						camera_locked = true;
					}
				}
				SetCameraPosition();
				transform.position = camera_position;
				if (!camera_locked) {
					CameraMovements();
				}
			}
		}
    }

	public void SetTarget (BoltEntity entity, string map_name) {
		camera_target = entity;
		if (map_name == "World 1") {
			SetCameraBounds(-50, 200, -200, 200);
		}
		else if (map_name == "Level 1") {
			SetCameraBounds(-50, 200, -200, 200);
		}
		else if (map_name == "Level 2") {
			SetCameraBounds(-50, 200, -200, 200);
		}
		else if (map_name == "Level 3") {
			SetCameraBounds(-50, 200, -200, 200);
		}
		else if (map_name == "Level 4") {
			SetCameraBounds(-50, 200, -200, 200);
		}
    }

    private void SetCameraPosition () {
		if (camera_locked) {
			camera_position = camera_target.transform.position;
			camera_position.y = camera_position.y + ZOOM_DEFAULT_Y + current_zoom_level;
			camera_position.z = camera_position.z - ZOOM_DEFAULT_Z - (current_zoom_level * ZOOM_Y_TO_Z_CONVERSION);
		}
		else {
			camera_position = transform.position;
			if ((current_zoom_level < ZOOM_LEVEL_MAXIMUM) && (current_zoom_level > ZOOM_LEVEL_MINIMUM)) {
				unlocked_current_zoom_level = -(ZOOM_SENSITIVITY * Input.GetAxis("Mouse ScrollWheel"));
				camera_position.y = camera_position.y + unlocked_current_zoom_level;
				camera_position.z = camera_position.z - (unlocked_current_zoom_level * ZOOM_Y_TO_Z_CONVERSION);
			}
		}
    }

    private void CameraMovements () {
		// Bottom part of the screen
		if (new Rect(0, 0, Screen.width, MOUSE_DETECTION_RECT_SIZE).Contains(Input.mousePosition)) {
			if (transform.position.z > camera_bound_bottom) {
				transform.Translate(0, 0, -CAMERA_SPEED, Space.World);
        	}
    	}

		// Top part of the screen
		if (new Rect(0, Screen.height - MOUSE_DETECTION_RECT_SIZE, Screen.width,
			MOUSE_DETECTION_RECT_SIZE).Contains(Input.mousePosition)) {
			if (transform.position.z < camera_bound_top) {
				transform.Translate(0, 0, CAMERA_SPEED, Space.World);
	        }
    	}

		// Left part of the screen
		if (new Rect(0, 0, MOUSE_DETECTION_RECT_SIZE, Screen.height).Contains(Input.mousePosition)) {
			if (transform.position.x > camera_bound_left) {
				transform.Translate(-CAMERA_SPEED, 0, 0, Space.World);
       		}
   		}

		// Right part of the screen
		if (new Rect(Screen.width - MOUSE_DETECTION_RECT_SIZE, 0,
			MOUSE_DETECTION_RECT_SIZE, Screen.height).Contains(Input.mousePosition)) {
			if (transform.position.x < camera_bound_right) {
				transform.Translate(CAMERA_SPEED, 0, 0, Space.World);
        	}
    	}
    }

    public void SetCameraLock (bool locked) {
		camera_locked = locked;
    }

    private void SetCameraBounds (float bottom, float top, float left, float right) {
		camera_bound_bottom = bottom;
		camera_bound_top = top;
		camera_bound_left = left;
		camera_bound_right = right;
    }
}