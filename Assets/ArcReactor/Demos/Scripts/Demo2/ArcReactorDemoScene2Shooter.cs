using UnityEngine;
using System.Collections;

public class ArcReactorDemoScene2Shooter : MonoBehaviour {

	public float period = 1;
	public float timer;
	public ArcReactor_Arc aLetter;
	public ArcReactor_Launcher aLauncher;
	
	// Use this for initialization
	void Start () 
	{
		//timer = 0;	
	}
	
	// Update is called once per frame
	void Update () 
	{
		//Debug.Log(aLetter.GetArcEndPosition(0));
		aLauncher.transform.LookAt(aLetter.GetArcEndPosition(0));
		timer -= Time.deltaTime;
		if (timer <= 0)
		{
			timer = timer + period;
			aLauncher.LaunchRay();
		}
	
	}
}
