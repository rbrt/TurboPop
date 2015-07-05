using UnityEngine;
using System.Collections;

public class AudioFilterTest : MonoBehaviour {

	public float currentValue;

	void OnAudioFilterRead(float[] data, int channels){
		float myValue = 0;
		float myValue2 = 0;
		for (int i = 0; i < data.Length * .3; i += channels){
			myValue += Mathf.Abs(data[i]);
		}

		for (int i = 1; i < data.Length * .3 - 1; i += channels){
			myValue2 += Mathf.Abs(data[i]);
		}
		currentValue = Mathf.Max(myValue, myValue2);
	}
}
