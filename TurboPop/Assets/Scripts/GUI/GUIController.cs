using UnityEngine;
using System.Collections;

public class GUIController : MonoBehaviour {

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
}
