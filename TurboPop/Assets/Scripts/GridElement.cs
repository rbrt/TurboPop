using UnityEngine;
using System.Collections;

public enum CubeColours {Green, Yellow, Purple, Blue}

public class GridElement : MonoBehaviour {

	CubeColours cubeColour;

	public void Init(CubeColours cubeColour){
		this.cubeColour = cubeColour;
		GetComponent<MeshRenderer>().sharedMaterial = GridInstantiator.Instance.GetGridColourForType(cubeColour);
	}

	public void WasClicked(){
		this.StartSafeCoroutine(Die());
	}

	IEnumerator Die(){
		var particles = GridInstantiator.Instance.GetParticlesForExplosion(this.transform, cubeColour);

		for (float i = 0; i <= 1; i += Time.deltaTime / .2f){
			transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, i);
			yield return null;
		}

		while (particles.isPlaying){
			yield return null;
		}

		Destroy(particles);
		Destroy(this.gameObject);
	}
}
