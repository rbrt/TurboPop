using UnityEngine;
using System.Collections;

public class TurboMeter : MonoBehaviour {

	[SerializeField] protected Material turboMeterMaterial;

	const int maxTurbo = 100,
			  minTurbo = 0,
			  turboAmount = 15,
 		 	  multiplier = 7;

	int turbo = 0,
		displayedTurbo = 0;

	float lastTime = 0,
		  turboDecrementFrequency = .04f;

	string turboMeterPercentage = "_Percentage";

	public int Turbo {
		get {
			return turbo;
		}
		set {
			turbo = (int)Mathf.Clamp(value, minTurbo, maxTurbo);
		}
	}

	public void IncreaseTurbo(int elementCount){
		Turbo += turboAmount + (elementCount * multiplier);
	}

	void Update(){
		if (Time.time - lastTime > turboDecrementFrequency){
			lastTime = Time.time;
			Turbo--;
		}

		if (displayedTurbo < Turbo){
			displayedTurbo += (Turbo - displayedTurbo) / 4;
		}
		else if (displayedTurbo > Turbo){
			displayedTurbo -= 1;
		}

		turboMeterMaterial.SetFloat(turboMeterPercentage, displayedTurbo / 100f);

		if (Turbo >= maxTurbo * .95f){
			HandleTurbo();
			Turbo = 0;
		}

	}

	void HandleTurbo(){
		GridElementDestroyer.Instance.DestroyFrontmostSegment();
	}
}
