using UnityEngine;
using System.Collections;

public class ReactToAudioTest : MonoBehaviour {

	float thresholdForResponse = 90;

	bool rotating = false;

	// Update is called once per frame
	void Update () {
		if (AudioFilterTest.currentValue >= thresholdForResponse && !rotating){
		}
	}

}
