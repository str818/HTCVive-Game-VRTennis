using UnityEngine;
using System.Collections;

public class ArcReactorDemo5_EffectChanger : MonoBehaviour {

	public ArcReactor_Trail trail;
	public GameObject[] rayPrefabs;


	void ChangePrefab(int ind)
	{
		ArcReactor_Arc arc = trail.DetachRay(false);
		if (arc != null)
		{
			arc.playbackType = ArcReactor_Arc.ArcsPlaybackType.once;
			arc.elapsedTime = arc.lifetime;
			arc.playBackward = false;
		}
		trail.arcPrefab = rayPrefabs[ind];
	}

	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
			ChangePrefab(0);
		if (Input.GetKeyDown(KeyCode.Alpha2))
			ChangePrefab(1);
		if (Input.GetKeyDown(KeyCode.Alpha3))
			ChangePrefab(2);
		if (Input.GetKeyDown(KeyCode.Alpha4))
			ChangePrefab(3);
		if (Input.GetKeyDown(KeyCode.Alpha5))
			ChangePrefab(4);
	
	}
}
