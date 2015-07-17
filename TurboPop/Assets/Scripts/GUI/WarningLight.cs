using UnityEngine;
using System.Collections;

public class WarningLight : MonoBehaviour {

	[SerializeField] protected Animator warningLightAnimator;

	float timeElapsed;

	string timeRemainingString = "TimeRemaining";

	void Start () {
		UpdateWarningLight();
	}

	void Update () {
		UpdateWarningLight();
	}

	void UpdateWarningLight(){
		timeElapsed = GridController.Instance.GetTimeRemainingUntilSegmentsAdvance();
		warningLightAnimator.SetFloat(timeRemainingString, timeElapsed);
	}

}
