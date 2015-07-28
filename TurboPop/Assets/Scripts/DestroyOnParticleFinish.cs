using UnityEngine;
using System.Collections;

public class DestroyOnParticleFinish : MonoBehaviour {

	ParticleSystem particleSystem;

	// Use this for initialization
	void Start () {
		particleSystem = GetComponent<ParticleSystem>();
	}

	// Update is called once per frame
	void Update () {
		if (!particleSystem.isPlaying){
			Destroy(this.gameObject);
		}
	}
}
