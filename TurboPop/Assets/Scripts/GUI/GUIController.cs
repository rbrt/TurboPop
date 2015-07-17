using UnityEngine;
using System.Collections;

public class GUIController : MonoBehaviour {

	[SerializeField] protected TurboMeter turboMeter;

	static GUIController instance;

	public static GUIController Instance{
		get {
			return instance;
		}
	}

	public TurboMeter GetTurboMeter(){
		return turboMeter;
	}

	void Awake(){
		if (instance == null){
			instance = this;
		}
	}

}
