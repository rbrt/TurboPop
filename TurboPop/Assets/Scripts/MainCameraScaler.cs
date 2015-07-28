using UnityEngine;
using System.Collections;

public class MainCameraScaler : MonoBehaviour {

	int targetX = 1534,
		targetY = 2048;

	// Scale the camera's FOV to match the intended resolution of an iPad Air 2
	void Start () {
		float currentRatio = Screen.width / (float) Screen.height;
		float targetRatio = targetX / (float) targetY;

		GetComponent<Camera>().fieldOfView = GetComponent<Camera>().fieldOfView * targetRatio / currentRatio;
	}

	void Update () {

	}
}
