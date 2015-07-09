using UnityEngine;
using System.Collections;

public class AffectBackgroundBloomWithAudio : MonoBehaviour {

	UnityStandardAssets.ImageEffects.Bloom bloom;

	float rotationSpeed = .01f,
		  rotationBoost = .25f;

	void Awake(){
		bloom = GetComponent<UnityStandardAssets.ImageEffects.Bloom>();
	}

	void Update(){
		bloom.flareRotation += rotationSpeed + (AudioFilterTest.currentValue / 300) * rotationBoost;
		bloom.lensflareIntensity = (AudioFilterTest.currentValue / 300) * 3.14f + 1;

	}

}
