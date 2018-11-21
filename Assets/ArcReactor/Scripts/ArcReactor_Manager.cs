using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ArcReactor_Manager : MonoBehaviour {

	public FPSInfo[] fpsPriorities;
	public float updateInterval = 1;
	public int defaultPriority;


	protected List<ArcReactor_Arc> arcSystems = new List<ArcReactor_Arc>();
	protected List<ArcReactor_Arc> arcSystemsForDeletion = new List<ArcReactor_Arc>();
	protected float accum = 0; // FPS accumulated over the interval
	protected int frames = 0; // Frames drawn over the interval
	protected float timeleft; // Left time for current interval
	protected int priority;
	protected FPSInfo[] fpsScales;
	protected float fps;	 

	

	[System.Serializable]
	public class FPSInfo
	{
		public float minFps;
		public int priority;
	}

	//Singleton
	public static ArcReactor_Manager Instance { get; private set;}


	public void AddArcSystem(ArcReactor_Arc arcSystem)
	{
		arcSystems.Add(arcSystem);
		arcSystem.SetPerformancePriority(priority);
	}


	public void DeleteArcSystem(ArcReactor_Arc arcSystem)
	{
		//arcSystems.Remove(arcSystem);
		arcSystemsForDeletion.Add(arcSystem);
	}



	protected void Awake()
	{
		if (ArcReactor_Manager.Instance == null)
			Instance = this;
		else
		{
			Debug.LogError("More than one instance of ArcReactor_Manager is active. Disabling additional instance");
			this.enabled = false;
		}
	}


	protected int GetPriority(float fps)
	{
		for (int i = 0; i < fpsScales.Length; i++)
			if (fps >= fpsScales[i].minFps)
				return fpsScales[i].priority;
		return defaultPriority;
	}


	// Use this for initialization
	protected void Start () 
	{
		priority = defaultPriority;
		fpsScales = fpsPriorities.OrderBy( fI => -fI.minFps).ToArray();			
	}
	
	// Update is called once per frame
	protected void Update () 
	{
		timeleft -= Time.deltaTime;
		accum += Time.timeScale/Time.deltaTime;
		++frames;
		
		// Interval ended - update priorities
		if( timeleft <= 0.0 )
		{
			foreach (ArcReactor_Arc arc in arcSystemsForDeletion)
				arcSystems.Remove(arc);
			arcSystemsForDeletion.Clear();
			fps = accum/frames;
			timeleft += updateInterval;
			accum = 0.0F;
			frames = 0;
			priority = GetPriority(fps);
			foreach(ArcReactor_Arc arc in arcSystems)
			{
				if (arc == null)
					DeleteArcSystem(arc);
				else				
					arc.SetPerformancePriority(priority);
			}
		}	
	}
}
