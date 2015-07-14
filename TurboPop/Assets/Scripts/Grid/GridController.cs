using UnityEngine;
using System.Collections;

public class GridController : MonoBehaviour {

	static GridController instance;

	public static GridController Instance{
		get {
			return instance;
		}
	}

	GridSegment[] gridSegments;

	void Awake(){
		if (instance == null){
			instance = this;
		}
	}

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}
}
