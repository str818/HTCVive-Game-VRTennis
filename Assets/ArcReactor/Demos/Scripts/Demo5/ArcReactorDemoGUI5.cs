using UnityEngine;
using System.Collections;

public class ArcReactorDemoGUI5 : MonoBehaviour {

	public float updateInterval = 1;
	
	private float accum = 0; // FPS accumulated over the interval
	private int frames = 0; // Frames drawn over the interval
	private float timeleft; // Left time for current interval
	private float fps;
	
	
	void Start ()
	{
		timeleft = updateInterval;  
	}
	
	void OnGUI () 
	{
		GUI.Box (new Rect (Screen.width-250,Screen.height-150,240,140), "");
		GUI.Label (new Rect (Screen.width-240,Screen.height-140,230,20), "Press 1,2,3,4 to change trail effect.");
		GUI.Label (new Rect (Screen.width-240,Screen.height-110,230,20), "WASD to rotate camera.");
		GUI.Label (new Rect (Screen.width-240,Screen.height-80,230,20), "Click and drag to move shpere.");
		GUI.Label (new Rect (Screen.width-240,Screen.height-50,230,20), "FPS:"+System.String.Format("{0:F2} FPS",fps));
	}
	
	
	// Update is called once per frame
	void Update () 
	{
		timeleft -= Time.deltaTime;
		accum += Time.timeScale/Time.deltaTime;
		++frames;
		
		// Interval ended - update GUI text and start new interval
		if( timeleft <= 0.0 )
		{
			// display two fractional digits (f2 format)
			fps = accum/frames;
			timeleft = updateInterval;
			accum = 0.0F;
			frames = 0;
		}
	}
}
