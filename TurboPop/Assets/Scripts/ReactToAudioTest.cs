using UnityEngine;
using System.Collections;

public class ReactToAudioTest : MonoBehaviour {

	float thresholdForResponse = 90;

	bool rotating = false;

	Vector3 rotation;

	void Awake(){
		rotation = transform.localRotation.eulerAngles;
	}

	// Update is called once per frame
	void Update () {
		if (AudioFilterTest.currentValue >= thresholdForResponse && !rotating){
			//transform.Rotate(0,2,0);
		}
	}

}
