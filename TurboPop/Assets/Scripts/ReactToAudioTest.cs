using UnityEngine;
using System.Collections;

public class ReactToAudioTest : MonoBehaviour {

	static AudioFilterTest audioFilterTest;

	float thresholdForResponse = 90;

	bool rotating = false;

	Vector3 rotation;

	void Awake(){
		rotation = transform.localRotation.eulerAngles;
		if (audioFilterTest == null){
			audioFilterTest = GameObject.FindObjectOfType<AudioFilterTest>();
		}
	}

	// Update is called once per frame
	void Update () {
		if (audioFilterTest.currentValue >= thresholdForResponse && !rotating){
			transform.Rotate(0,2,0);
		}
	}

}
