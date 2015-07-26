using UnityEngine;
using System.Collections;

public class GUIController : MonoBehaviour {

	[SerializeField] protected TurboMeter turboMeter;
	[SerializeField] protected Material eqGridMaterial;

	static GUIController instance;

	Vector2 eqUV,
	 		eqOffset;

	float eqUVScrollRate = .75f,
		  eqOffsetX = 0,
		  eqOffsetXBoost = 1f,
		  eqOffsetXDecrement = .025f,
		  eqOffsetXIncrement = .09f;

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
			eqUV = Vector2.zero;
			eqOffset = Vector2.one;
			eqOffsetX = 1;
			this.StartSafeCoroutine(scrollEQUVs());
		}
	}

	void Update(){
		HandleEQOffset();
	}

	void HandleEQOffset(){
		if (eqOffset.x > eqOffsetX){
			eqOffset.x -= eqOffsetXDecrement;
		}
		else if (eqOffset.x < eqOffsetX){
			eqOffset.x += eqOffsetXIncrement;
		}

		eqOffsetX -= eqOffsetXDecrement;

		if (eqOffsetX < 1){
			eqOffsetX = 1;
			eqOffset.x = 1;
		}

		eqGridMaterial.SetTextureScale("_MainTex", eqOffset);
	}

	public void BoostEQOffsetX(){
		eqOffsetX += eqOffsetXBoost;

		Debug.Log(eqOffsetX);
	}

	IEnumerator scrollEQUVs(){
		while (true){
			eqUV.y += eqUVScrollRate * Time.deltaTime;
			eqGridMaterial.SetTextureOffset("_MainTex", eqUV);
			yield return null;
		}
	}

}
