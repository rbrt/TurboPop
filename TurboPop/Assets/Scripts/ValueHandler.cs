using UnityEngine;
using System.Collections;

public class ValueHandler : MonoBehaviour {

	static ValueHandler instance;

	const int maxTurbo = 100,
			  minTurbo = 0;

	int turbo;

	public static ValueHandler Instance {
		get {
			return instance;
		}
	}

	public int Turbo {
		get {
			return turbo;
		}
		set {
			turbo = (int)Mathf.Clamp(value, minTurbo, maxTurbo);
		}
	}

	void Awake(){
		if (instance == null){
			instance = this;
		}
	}
}
