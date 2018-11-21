using UnityEngine;
using System.Collections;

public class ArcReactor_EmitterDestructor : MonoBehaviour {

	public ParticleSystem partSystem;

	// Update is called once per frame
	void Update () 
	{
		if (!partSystem.IsAlive())
			Destroy(gameObject);
	}
}
