using UnityEngine;
using System.Collections;

public class AudioFilterTest : MonoBehaviour {

	public static float currentValue;

	public float bassValue,
				 trebleValue,
				 midValue,
				 totalValue;

	float rawValue1 = 0,
		  rawValue2 = 0,
		  bassValue1 = 0,
		  bassValue2 = 0,
		  trebleValue1 = 0,
		  trebleValue2 = 0,
		  midValue1 = 0,
		  midValue2 = 0;

	void OnAudioFilterRead(float[] data, int channels){
		rawValue1 = 0;
		rawValue2 = 0;
		bassValue1 = 0;
		bassValue2 = 0;
		trebleValue1 = 0;
		trebleValue2 = 0;
		midValue1 = 0;
		midValue2 = 0;

		for (int i = 0; i < data.Length; i += channels){
			if (i < data.Length * .3){
				bassValue1 += Mathf.Abs(data[i]);
			}
			else if (i < data.Length * .7){
				midValue1 += Mathf.Abs(data[i]);
			}
			else{
				trebleValue1 += Mathf.Abs(data[i]);
			}
			rawValue1 += Mathf.Abs(data[i]);
		}

		for (int i = 1; i < data.Length - 1; i += channels){
			if (i < data.Length * .3){
				bassValue2 += Mathf.Abs(data[i]);
			}
			else if (i < data.Length * .7){
				midValue2 += Mathf.Abs(data[i]);
			}
			else{
				trebleValue2 += Mathf.Abs(data[i]);
			}
			rawValue2 += Mathf.Abs(data[i]);
		}

		bassValue = Mathf.Max(bassValue1, bassValue2);
		midValue = Mathf.Max(midValue1, midValue2);
		trebleValue = Mathf.Max(trebleValue1, trebleValue2);
		currentValue = Mathf.Max(rawValue1, rawValue2);

		totalValue = (int)currentValue;
	}


}
