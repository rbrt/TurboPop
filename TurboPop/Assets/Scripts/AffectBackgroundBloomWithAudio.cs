using UnityEngine;
using System.Collections;

public class AffectBackgroundBloomWithAudio : MonoBehaviour {

	UnityStandardAssets.ImageEffects.Bloom bloom;

	void Awake(){
		bloom = GetComponent<UnityStandardAssets.ImageEffects.Bloom>();
	}

	void Update(){
		bloom.flareRotation += .1f;
		bloom.lensflareIntensity = (AudioFilterTest.currentValue / 100) * 3.14f;

	}

}
