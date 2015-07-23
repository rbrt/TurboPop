using UnityEngine;
using System.Collections;

public enum CubeColours {Green, Yellow, Purple, Blue, TEMP}

public class GridSegmentElement : MonoBehaviour {

	CubeColours cubeColour;

	bool destroyed;

	public bool Destroyed {
		get {
			return destroyed;
		}
	}

	public CubeColours CubeColour {
		get {
			return cubeColour;
		}
	}

	public void InitializeElement(CubeColours cubeColour){
		this.cubeColour = cubeColour;
		GetComponent<MeshRenderer>().enabled = true;
		GetComponent<Collider>().enabled = true;
		transform.localScale = Vector3.one;
		GetComponent<MeshRenderer>().sharedMaterial = GridInstantiator.Instance.GetGridColourForType(cubeColour);
		destroyed = false;
	}

	public void DestroyElement(){
		if (this.destroyed){
			return;
		}
		
		this.destroyed = true;
		this.StartSafeCoroutine(Die());
	}

	public IEnumerator Die(){
		var particles = GridInstantiator.Instance.GetParticlesForExplosion(this.transform, cubeColour);

		for (float i = 0; i <= 1; i += Time.deltaTime / .2f){
			transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, i);
			yield return null;
		}

		while (particles.isPlaying){
			yield return null;
		}

		Destroy(particles);

		GetComponent<MeshRenderer>().enabled = false;
		GetComponent<Collider>().enabled = false;
	}
}
