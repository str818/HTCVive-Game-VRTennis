using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[AddComponentMenu("Arc Reactor Rays/Ray Trail")]
public class ArcReactor_Trail : MonoBehaviour {

	public GameObject arcPrefab;
	public bool truncateByDistance;
	public float distanceThreshold = 10;
	public bool truncateByLifetime;
	public float lifetimeThreshold = 1;
	public float precision = 0.01f;
	public Transform globalSpaceTransform;

	public List<SegmentInfo> segments;
	[HideInInspector]
	public ArcReactor_Arc currentArc;

	
	public ArcReactor_Arc DetachRay (bool newshape = false)
	{
		ArcReactor_Arc tempArc = currentArc;
		currentArc = null;
		if (newshape)
			Initialize();
		return tempArc;
	}

	public class SegmentInfo
	{
		public Vector3 pos;
		public float birthtime;
		public SegmentInfo(Vector3 pos, float birthtime)
		{
			this.pos = pos;
			this.birthtime = birthtime;
		}
	}



	// Use this for initialization
	void Awake () 
	{
		segments = new List<SegmentInfo>();
	}

	void Start ()
	{
		Initialize();
	}

	void Initialize()
	{
		segments.Clear();
		segments.Add(new SegmentInfo(transform.position,Time.time));
	}

	// Update is called once per frame
	void LateUpdate () 
	{
		if (Vector3.SqrMagnitude(transform.position - segments[segments.Count - 1].pos) > precision * precision)
		{
			segments.Add(new SegmentInfo(transform.position,Time.time));
		}

		if (truncateByLifetime && segments.Count > 1)
		{
			if (Time.time - segments[segments.Count-1].birthtime > lifetimeThreshold)
			{
				Initialize();
			}
			else
			{
				for (int i = 0; i < segments.Count-1; i++)
				{
					if (Time.time - segments[segments.Count-1-i].birthtime > lifetimeThreshold)
					{
						segments.RemoveRange(0,segments.Count-2-i);
						break;
					}
				}
			}
		}

		if (truncateByDistance && segments.Count > 1)
		{
			float distance = Vector3.Distance(transform.position,segments[segments.Count-1].pos);
			if (distance > distanceThreshold)
			{
				Initialize();
			}
			else
			{
				for (int i = 0; i < segments.Count-1; i++)
				{
					distance += Vector3.Distance(segments[segments.Count-1-i].pos,segments[segments.Count-2-i].pos);
					if (distance > distanceThreshold)
					{
						segments.RemoveRange(0,segments.Count-2-i);
						break;
					}
				}
			}
		}


		if (currentArc == null && segments.Count > 1)
		{
			GameObject obj = (GameObject)Instantiate(arcPrefab);
			currentArc = obj.GetComponent<ArcReactor_Arc>();
			if (globalSpaceTransform != null)
				obj.transform.parent = globalSpaceTransform;
		}

		if (currentArc != null)
		{
			if (currentArc.shapePoints.Length != segments.Count)
			{
				Array.Resize(ref currentArc.shapePoints,Mathf.Max(segments.Count,2));
			}

			currentArc.shapePoints[0] = transform.position;
			for (int x = 0; x < segments.Count - 1; x++)
			{
				currentArc.shapePoints[segments.Count - x - 1] = segments[x].pos;
			}
		}

	}
}
