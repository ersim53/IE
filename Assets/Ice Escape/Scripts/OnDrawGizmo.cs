using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class OnDrawGizmo : MonoBehaviour {
	public Color color;

    void OnDrawGizmos() {
		Gizmos.color = color;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}