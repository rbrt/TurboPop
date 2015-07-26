using UnityEngine;
using System.Collections;

public class AffectBloom : MonoBehaviour {

	UnityStandardAssets.ImageEffects.Bloom bloom;

	float initialIntensity,
		  initialThreshold,
		  currentIntensity,
		  currentThreshold,
		  intensityDecrement = .05f,
		  thresholdDecrement = .05f,
		  intensityIncrement = .3f,
		  thresholdIncrement = .3f;

	void Awake(){
		bloom = GetComponent<UnityStandardAssets.ImageEffects.Bloom>();

		initialIntensity = bloom.bloomIntensity;
		currentIntensity = initialIntensity;
		initialThreshold = bloom.bloomThreshold;
		currentThreshold = initialThreshold;
	}

	void Update(){
		currentThreshold = Mathf.Min(currentThreshold + thresholdDecrement, initialThreshold);
		currentIntensity = Mathf.Max(currentIntensity - intensityDecrement, initialIntensity);

		bloom.bloomIntensity = currentIntensity;
		bloom.bloomThreshold = currentThreshold;
	}

	public void PopBlockEffect(){
		currentIntensity += intensityIncrement;
		currentThreshold -= thresholdIncrement;
	}

}
