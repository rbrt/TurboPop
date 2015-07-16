using UnityEngine;
using System.Collections;

public class GUIController : MonoBehaviour {

	[SerializeField] protected Material turboMeterMaterial;

	string turboMeterPercentage = "_Percentage";

	static GUIController instance;

	public static GUIController Instance{
		get {
			return instance;
		}
	}

	void Awake(){
		if (instance == null){
			instance = this;
		}
	}

	int currentTurbo = 0;
	float lastTime = 0,
		  turboDelay = .1f;
	void Update(){
		if (Time.time - lastTime > turboDelay){
			lastTime = Time.time;
			ValueHandler.Instance.Turbo--;
		}

		if (currentTurbo < ValueHandler.Instance.Turbo){
			currentTurbo++;
		}
		else if (currentTurbo > ValueHandler.Instance.Turbo){
			currentTurbo--;
		}

		turboMeterMaterial.SetFloat(turboMeterPercentage, currentTurbo / 100f);


	}
}
