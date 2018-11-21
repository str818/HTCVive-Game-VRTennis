using UnityEngine;
using System.Collections;

public class ArcReactorDemoAutoShooter : MonoBehaviour {

	public ArcReactor_Launcher[] launchers;
	

	// Update is called once per frame
	void Update () {
		if (UnityEngine.Random.value  < Time.deltaTime)
		{
			launchers[UnityEngine.Random.Range(0,launchers.Length)].LaunchRay();
		}
	}
}
