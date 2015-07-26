using UnityEngine;
using System.Collections;

public class GameStateController : MonoBehaviour {

	public delegate void ClearStaticFunctionDelegate();
	event ClearStaticFunctionDelegate clearStaticFunctions;

	static GameStateController instance;

	private const string mainSceneName = "Prototyping";

	public static GameStateController Instance {
		get {
			return instance;
		}
	}

	public static void AddStaticCleanupFunction(ClearStaticFunctionDelegate cleanupFunction){
		instance.clearStaticFunctions += cleanupFunction;
	}

	void Awake(){
		if (instance == null){
			instance = this;
		}
	}

	public void LoseGame(){
		Application.LoadLevel(mainSceneName);
		if (clearStaticFunctions != null){
			clearStaticFunctions();
		}
	}

}
