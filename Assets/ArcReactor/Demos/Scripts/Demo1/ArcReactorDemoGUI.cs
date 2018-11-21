using UnityEngine;
using System.Collections;

public class ArcReactorDemoGUI : MonoBehaviour {

	
	public GameObject showcase;
	public GameObject activeTarget;
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
		GUI.Label (new Rect (Screen.width-240,Screen.height-140,230,20), "Press 1,2,3,4,5,6,7,8,9,0");
		GUI.Label (new Rect (Screen.width-240,Screen.height-123,230,20), "to change weapon effect.");
		GUI.Label (new Rect (Screen.width-240,Screen.height-90,230,20), "Press Q to toggle showcase.");
		GUI.Label (new Rect (Screen.width-240,Screen.height-50,230,20), "FPS:"+System.String.Format("{0:F2} FPS",fps));
	}


	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown(KeyCode.Q) && showcase != null)
			showcase.SetActive(!showcase.activeSelf);
		if (Input.GetKeyDown(KeyCode.E) && activeTarget != null)
			activeTarget.SetActive(!activeTarget.activeSelf);	

		
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
