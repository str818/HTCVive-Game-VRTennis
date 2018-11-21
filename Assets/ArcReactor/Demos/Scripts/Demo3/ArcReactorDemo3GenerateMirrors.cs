using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArcReactorDemo3GenerateMirrors : MonoBehaviour {

	public GameObject mirrorPrefab;
	public GameObject spherePrefab;
	public int mirrorCount;
	public float mirrorSphereRatio;
	public Vector3 center;
	public Vector3 size;
	public Vector3[] forbiddenZones;

	private List<Transform> mirrors = new List<Transform>();

	public float minDistance = 5;
	public LayerMask placementRaycastMask;

	public void GenerateMirrors(int count)
	{
		foreach(Transform tr in mirrors)
			Object.Destroy(tr.gameObject);		
		mirrors.Clear();

		GameObject obj;
		Vector3 position;
		float dist = float.MaxValue;
		for (int i = 0; i < count; i++)
		{
			if (UnityEngine.Random.value > mirrorSphereRatio)
				obj = (GameObject)(GameObject.Instantiate(mirrorPrefab));
			else
				obj = (GameObject)(GameObject.Instantiate(spherePrefab));
			obj.transform.parent = transform;
			dist = float.MaxValue;
			int iteration = 0;
			do
			{
				iteration++;
				dist = float.MaxValue;
				position = new Vector3(UnityEngine.Random.Range(center.x-size.x,center.x+size.x),
				                       UnityEngine.Random.Range(center.y-size.y,center.y+size.y),
				                       UnityEngine.Random.Range(center.z-size.z,center.z+size.z));

				foreach(Vector3 vect in forbiddenZones)
				{
					if (Vector3.Distance(vect,position) < dist)
						dist = Vector3.Distance(vect,position);
				}

				foreach (Transform tr in mirrors)
				{
					if (Vector3.Distance(tr.position,position) < dist)
						dist = Vector3.Distance(tr.position,position);
				}
				//Debug.Log(position.ToString() + ":" + dist);
			} while (dist < minDistance && iteration < 10000);
			if (iteration >= 10000)
				Debug.LogWarning("Couldn't place all objects within 10000 iterations");
			obj.transform.position = position;
			mirrors.Add (obj.transform);
		}
	}

	public void PlaceObject2D(GameObject obj,Vector2 rayStart)
	{
		float dist = float.MaxValue;
		Vector3 position;
		obj.transform.parent = transform;
		dist = float.MaxValue;
		bool flag;
		int iteration = 0;
		do
		{
			iteration++;
			dist = float.MaxValue;
			position = new Vector3(UnityEngine.Random.Range(center.x-size.x,center.x+size.x),
			                       UnityEngine.Random.Range(center.y-size.y,center.y+size.y),
			                       UnityEngine.Random.Range(center.z-size.z,center.z+size.z));
			
			foreach(Vector3 vect in forbiddenZones)
			{
				if (Vector3.Distance(vect,position) < dist)
					dist = Vector3.Distance(vect,position);
			}
			
			foreach (Transform tr in mirrors)
			{
				if (Vector3.Distance(tr.position,position) < dist)
					dist = Vector3.Distance(tr.position,position);
			}
			flag = Physics2D.Raycast(rayStart,new Vector2(position.x,position.y)-rayStart,Vector2.Distance(rayStart,new Vector2(position.x,position.y)),placementRaycastMask);
			//Debug.Log(flag);
		} while ((dist < minDistance || !flag) && iteration < 1000);
		if (iteration >= 1000)
			Debug.LogWarning("Couldn't place all objects within 10000 iterations");
		obj.transform.position = position;
	}



	// Use this for initialization
	void Start () 
	{
		GenerateMirrors(mirrorCount);
	}


	void Update ()
	{
		if (Input.GetKeyUp(KeyCode.Q))
		{					
			GenerateMirrors(mirrorCount);
		}
	}

}
