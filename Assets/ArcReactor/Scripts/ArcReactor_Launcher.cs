using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[AddComponentMenu("Arc Reactor Rays/Ray Launcher")]
public class ArcReactor_Launcher : MonoBehaviour {

	public GameObject arcPrefab;
	public GameObject helperPrefab;
	public LaunchMethod launchMethod = LaunchMethod.forward_raycast;
	public float Distance = 100;
	public float PropagationSpeed = 10000;
	public LayerMask layers;
	public RayTransformBehaivour startBehaviour = RayTransformBehaivour.stick;
	public RayTransformBehaivour endBehaviour = RayTransformBehaivour.follow_raycast;
	public bool SendMessageToHitObjects;
	public bool SendMessageToTouchedObjects;
	public LayerMask touchLayers;
	public ReflectionSettings reflectionSettings;
	public InertialSettings rayInertiaSettings;
	public Transform globalSpaceTransform;


	const int maxReflections = 100;
	const float reflectGap = 0.01f;

	public List<RayInfo> rays;

	protected Vector3[] posArray = new Vector3[maxReflections * 2];
	protected int posArrayLen;
	protected Vector3[] positions = new Vector3[maxReflections];
	protected List<RayInfo> destrArr;
	protected RaycastHit hit;

	public List<RayInfo> Rays
	{
		get 
		{
			return rays;
		}
	}

	public class RayInfo
	{
		public ArcReactor_Arc arc;
		public Transform[] shape;
		public GameObject startObject;
		public GameObject endObject;
		public float distance;
	}

	[System.Serializable]
	public class ReflectionSettings
	{
		public ReflectSettings reflections;
		public Transform[] reflectors;
		public LayerMask reflectLayers;
		public float thickness = 0.05f;
		public bool sendMessageToReflectors;
	}


	[System.Serializable]
	public class InertialSettings
	{
		public InertiaMethod type = InertiaMethod.none;
		public float speed;
		public float detalization = 10;
		public bool localDetalization = true;
		public AnimationCurve snapbackForceCurve;
		public float maxSnapBackDistance = 100;
	}


	public enum InertiaMethod
	{
		none = 0,
		linespeed = 1
	}

	public enum LaunchMethod
	{
		forward_raycast = 0,
		double_raycast = 1
	}

	public enum RayTransformBehaivour
	{
		immobile = 0,
		stick = 1,
		follow_raycast = 2
	}

	public enum ReflectSettings
	{
		no_reflections = 0,
		reflect_specified_objects = 1,
		reflect_by_layer = 2
	}

	void Awake ()
	{
		rays = new List<RayInfo>();
		destrArr = new List<RayInfo>();
		hit = new RaycastHit();
	}


	protected bool CheckReflectObject(Transform checkTr)
	{
		foreach (Transform refl in reflectionSettings.reflectors)
		{
			if (refl == checkTr)
				return true;
		}
		return false;
	}

	protected void FillPosArray(Vector3 position, Vector3 direction, float maxDistance,RayInfo rayInfo)
	{
		RaycastHit hit = new RaycastHit();
		if (Physics.Raycast(position,direction,out hit,maxDistance,layers.value | reflectionSettings.reflectLayers.value))
		{
			if (SendMessageToHitObjects)
			{
				ArcReactorHitInfo arcHit = new ArcReactorHitInfo();
				arcHit.launcher = this;
				arcHit.rayInfo = rayInfo;
				arcHit.raycastHit = hit;
				hit.transform.gameObject.SendMessage("ArcReactorHit",arcHit,SendMessageOptions.DontRequireReceiver);
			}

			posArray[posArrayLen] = hit.point;
			posArrayLen++;

			if (SendMessageToTouchedObjects)
			{
				RaycastHit[] hits;
				hits = Physics.RaycastAll(position, direction, Vector3.Distance(position,hit.point), touchLayers);			
				foreach (RaycastHit touchHit in hits)
				{
					ArcReactorHitInfo arcHit = new ArcReactorHitInfo();
					arcHit.launcher = this;
					arcHit.rayInfo = rayInfo;
					arcHit.raycastHit = touchHit;
					touchHit.transform.gameObject.SendMessage("ArcReactorTouch",arcHit,SendMessageOptions.DontRequireReceiver);
				}
			}

			if ((reflectionSettings.reflections == ReflectSettings.reflect_by_layer || CheckReflectObject(hit.transform))
			    && (reflectionSettings.reflectLayers.value & 1 << hit.transform.gameObject.layer) > 0)
			{
				if (reflectionSettings.sendMessageToReflectors)
				{
					ArcReactorHitInfo arcHit = new ArcReactorHitInfo();
					arcHit.launcher = this;
					arcHit.rayInfo = rayInfo;
					arcHit.raycastHit = hit;
					hit.transform.gameObject.SendMessage("ArcReactorReflection",arcHit,SendMessageOptions.DontRequireReceiver);
				}
				FillPosArray(hit.point + hit.normal * reflectionSettings.thickness,Vector3.Reflect(direction, hit.normal), maxDistance - Vector3.Distance(position,hit.point),rayInfo );
			}
		}
		else
		{
			if (SendMessageToTouchedObjects)
			{
				RaycastHit[] hits;
				hits = Physics.RaycastAll(position, direction, maxDistance, touchLayers);
				foreach (RaycastHit touchHit in hits)
				{
					ArcReactorHitInfo arcHit = new ArcReactorHitInfo();
					arcHit.launcher = this;
					arcHit.rayInfo = rayInfo;
					arcHit.raycastHit = touchHit;
					touchHit.transform.gameObject.SendMessage("ArcReactorTouch",arcHit,SendMessageOptions.DontRequireReceiver);
				}
			}

			posArray[posArrayLen] = position + direction.normalized * maxDistance;
			posArrayLen++;
		}
	}

	[ContextMenu ("Launch Ray")]
	public void LaunchRay()
	{
		if (launchMethod == LaunchMethod.forward_raycast && startBehaviour == RayTransformBehaivour.follow_raycast)
		{
			Debug.LogError("Launch method 'forward_raycast' and start behaviour 'follow_raycast' are incompatible. Change one of the settings.");
			return;
		}

		if (arcPrefab == null)
		{
			Debug.LogError("No arc prefab set.");
			return;
		}

		Transform start = transform;
		Transform end;
		//GameObject startObj;
		//GameObject endObj;
		GameObject tmpobj = new GameObject("rayEndPoint");
		RaycastHit hit = new RaycastHit();

		//End position will be raycasted in any case
		end = tmpobj.transform;
		if (Physics.Raycast(transform.position,transform.forward,out hit,Distance,layers.value))		
		{
			end.position = hit.point;
			//endObj = hit.transform.gameObject;
		}
		else		
			end.position = transform.position + transform.forward * Distance;
		if (endBehaviour == RayTransformBehaivour.stick && hit.transform != null)
		{
			end.parent = hit.transform;
		}
		else
		{
			end.parent = globalSpaceTransform;
		}


		//Start position will depend on launch method
		switch (launchMethod)
		{
		case LaunchMethod.double_raycast:
			tmpobj = new GameObject("rayStartPoint");
			start = tmpobj.transform;
			if (Physics.Raycast(transform.position,-transform.forward,out hit,Distance,layers.value))
			{
				start.position = hit.point;
				//startObj = hit.transform.gameObject;
			}
			else
				start.position = transform.position - transform.forward * Distance;
			if (startBehaviour == RayTransformBehaivour.stick && hit.transform != null)
			{
				start.parent = hit.transform;
			}
			break;
		case LaunchMethod.forward_raycast:
			tmpobj = new GameObject("rayStartPoint");
			start = tmpobj.transform;
			start.position = transform.position;
			if (startBehaviour == RayTransformBehaivour.stick)
			{
				start.parent = transform;
				start.rotation = transform.rotation;
				if (helperPrefab != null)
				{
					tmpobj = (GameObject)Instantiate(helperPrefab);
					tmpobj.transform.parent = start;
					tmpobj.transform.position = start.transform.position;
					tmpobj.transform.rotation = start.transform.rotation;
				}
			}
			else
			{
				start.parent = globalSpaceTransform;
			}
			break;
		}

		RayInfo rinfo = new RayInfo();
		tmpobj = (GameObject)Instantiate(arcPrefab);
		tmpobj.transform.parent = globalSpaceTransform;
		rinfo.arc = tmpobj.GetComponent<ArcReactor_Arc>();
		//bool[] destrFlags = new bool[0];

		switch (rayInertiaSettings.type)
		{
		case InertiaMethod.none:
			rinfo.shape = new Transform[2];
			rinfo.shape[0] = start;
			rinfo.shape[1] = end;
			rinfo.arc.shapeTransforms = rinfo.shape;
			//destrFlags = new bool[2];
			break;
		case InertiaMethod.linespeed:
			int transformCount = 0;
			if (rayInertiaSettings.localDetalization)
			{
				transformCount = Mathf.CeilToInt(rayInertiaSettings.detalization) + 2;
			}
			else
			{
				transformCount = Mathf.CeilToInt(Vector3.Distance(start.position,end.position)/rayInertiaSettings.detalization) + 2;
			}
			rinfo.shape = new Transform[transformCount];
			//destrFlags = new bool[transformCount];
			rinfo.shape[0] = start;
			rinfo.shape[transformCount-1] = end;
			for (int i = 1; i < transformCount-1; i++)
			{
				tmpobj = new GameObject("rayInertiaPoint");
				tmpobj.transform.position = Vector3.Lerp(start.position,end.position,(float)i/(transformCount-1));
				tmpobj.transform.parent = globalSpaceTransform;
				rinfo.shape[i] = tmpobj.transform;
			}
			break;
		}


		rinfo.arc.shapeTransforms = rinfo.shape;

		/*for(int i = 0; i <= destrFlags.Length-1; i++)
			destrFlags[i] = true;
		rinfo.arc.transformsDestructionFlags = destrFlags;*/


		rays.Add(rinfo);
	}

		
	// Update is called once per frame
	void LateUpdate () 
	{
		for (int x = 0; x < rays.Count; x++)
		{
			if (rays[x].arc == null)
			{
				destrArr.Add(rays[x]);			
			}
			else
			{
				rays[x].distance = Mathf.Clamp(rays[x].distance + PropagationSpeed * Time.deltaTime,0,Distance);
				Vector3 endPos = Vector3.zero;
				switch (reflectionSettings.reflections)
				{
				case ReflectSettings.no_reflections:
					if (startBehaviour == RayTransformBehaivour.follow_raycast)
					{
						if (Physics.Raycast(transform.position,-transform.forward,out hit,rays[x].distance,layers.value))
						{
							if (SendMessageToHitObjects)
							{
								ArcReactorHitInfo arcHit = new ArcReactorHitInfo();
								arcHit.launcher = this;
								arcHit.rayInfo = rays[x];
								arcHit.raycastHit = hit;
								hit.transform.gameObject.SendMessage("ArcReactorHit",arcHit,SendMessageOptions.DontRequireReceiver);
							}
							if (SendMessageToTouchedObjects)
							{
								RaycastHit[] hits;
								hits = Physics.RaycastAll(transform.position,-transform.forward, Vector3.Distance(transform.position,hit.point), touchLayers);			
								foreach (RaycastHit touchHit in hits)
								{
									ArcReactorHitInfo arcHit = new ArcReactorHitInfo();
									arcHit.launcher = this;
									arcHit.rayInfo = rays[x];
									arcHit.raycastHit = touchHit;
									touchHit.transform.gameObject.SendMessage("ArcReactorTouch",arcHit,SendMessageOptions.DontRequireReceiver);
								}
							}
							rays[x].startObject = hit.transform.gameObject;
							rays[x].shape[0].position = transform.position + (transform.position - hit.point).normalized * (float)((transform.position - hit.point).magnitude - 0.05);
						}
						else
						{
							if (SendMessageToTouchedObjects)
							{
								RaycastHit[] hits;
								hits = Physics.RaycastAll(transform.position,-transform.forward, rays[x].distance, touchLayers);			
								foreach (RaycastHit touchHit in hits)
								{
									ArcReactorHitInfo arcHit = new ArcReactorHitInfo();
									arcHit.launcher = this;
									arcHit.rayInfo = rays[x];
									arcHit.raycastHit = touchHit;
									touchHit.transform.gameObject.SendMessage("ArcReactorTouch",arcHit,SendMessageOptions.DontRequireReceiver);
								}
							}
							rays[x].startObject = null;
							rays[x].shape[0].position = transform.position - transform.forward * rays[x].distance;
						}
					}
					if (endBehaviour == RayTransformBehaivour.follow_raycast)
					{
						if (Physics.Raycast(transform.position,transform.forward,out hit,rays[x].distance,layers.value))
						{
							if (SendMessageToHitObjects)
							{
								ArcReactorHitInfo arcHit = new ArcReactorHitInfo();
								arcHit.launcher = this;
								arcHit.rayInfo = rays[x];
								arcHit.raycastHit = hit;
								hit.transform.gameObject.SendMessage("ArcReactorHit",arcHit,SendMessageOptions.DontRequireReceiver);
							}

							if (SendMessageToTouchedObjects)
							{
								RaycastHit[] hits;
								hits = Physics.RaycastAll(transform.position,transform.forward, Vector3.Distance(transform.position,hit.point), touchLayers);			
								foreach (RaycastHit touchHit in hits)
								{
									ArcReactorHitInfo arcHit = new ArcReactorHitInfo();
									arcHit.launcher = this;
									arcHit.rayInfo = rays[x];
									arcHit.raycastHit = touchHit;
									touchHit.transform.gameObject.SendMessage("ArcReactorTouch",arcHit,SendMessageOptions.DontRequireReceiver);
								}
							}

							rays[x].endObject = hit.transform.gameObject;
							endPos = transform.position + (hit.point - transform.position).normalized * (float)((hit.point - transform.position).magnitude - 0.05);
							//endPos = hit.point;
						}
						else
						{
							if (SendMessageToTouchedObjects)
							{
								RaycastHit[] hits;
								hits = Physics.RaycastAll(transform.position,transform.forward, rays[x].distance, touchLayers);			
								foreach (RaycastHit touchHit in hits)
								{
									ArcReactorHitInfo arcHit = new ArcReactorHitInfo();
									arcHit.launcher = this;
									arcHit.rayInfo = rays[x];
									arcHit.raycastHit = touchHit;
									touchHit.transform.gameObject.SendMessage("ArcReactorTouch",arcHit,SendMessageOptions.DontRequireReceiver);
								}
							}
							rays[x].endObject = null;
							endPos = transform.position + transform.forward * rays[x].distance;
						}
					}
					else
					{
						endPos = rays[x].shape[rays[x].shape.Length-1].position;
					}
					switch (rayInertiaSettings.type)
					{
					case InertiaMethod.none:
						rays[x].shape[rays[x].shape.Length-1].position = endPos;
						break;
					case InertiaMethod.linespeed:
						int transformCount = rays[x].shape.Length;
						Vector3 targetPos;
						for (int i = 1; i < transformCount; i++)
						{
							targetPos = Vector3.Lerp(rays[x].shape[0].position,endPos,(float)i/(transformCount-1));
							rays[x].shape[i].position = Vector3.MoveTowards(rays[x].shape[i].position,targetPos,
							                                              rayInertiaSettings.speed *  rayInertiaSettings.snapbackForceCurve.Evaluate(Vector3.Distance(rays[x].shape[i].position,targetPos) / rayInertiaSettings.maxSnapBackDistance) *  Time.deltaTime);
						}
						break;
					}
					break;
				case ReflectSettings.reflect_by_layer:
				case ReflectSettings.reflect_specified_objects:
					int posNum;
					GameObject obj;

					if (startBehaviour == RayTransformBehaivour.follow_raycast)
					{
						posArrayLen = 0;
						FillPosArray(transform.position,-transform.forward,rays[x].distance,rays[x]);
						for (int i = 0; i < posArrayLen; i++)
						{
							positions[i] = posArray[i];
						}
						posNum = posArrayLen;
					}
					else
					{
						posNum = 1;
						positions[0] = rays[x].shape[0].position;
					}

					if (endBehaviour == RayTransformBehaivour.follow_raycast)
					{
						posArrayLen = 0;
						FillPosArray(transform.position,transform.forward,rays[x].distance,rays[x]);
						for (int i = 0; i < posArrayLen; i++)
						{
							positions[posNum + i] = posArray[i];
						}
						posNum += posArrayLen;
					}
					else
					{
						positions[posNum] = rays[x].shape[rays[x].shape.Length-1].position;
						posNum++;
					}
					if (rays[x].shape.Length == posNum)
					{
					}
					if (rays[x].shape.Length > posNum)
					{
						for (int i = posNum-1; i < rays[x].shape.Length-1; i++)
							GameObject.Destroy(rays[x].shape[i].gameObject);
						rays[x].shape[posNum-1] = rays[x].shape[rays[x].shape.Length-1];
						Array.Resize(ref rays[x].shape,posNum);

					}
					if (rays[x].shape.Length < posNum)
					{
						int oldSize = rays[x].shape.Length;
						Array.Resize(ref rays[x].shape, posNum);
						rays[x].shape[rays[x].shape.Length-1] = rays[x].shape[oldSize-1];
						for (int i = oldSize-1; i < posNum-1; i++)						
						{
							obj = new GameObject("RayPoint" + (i+1).ToString());
							obj.transform.parent = globalSpaceTransform;
							rays[x].shape[i] = obj.transform;
						}
					}
					for (int i = 0; i < posNum; i++)
						rays[x].shape[i].position = positions[i];
					rays[x].arc.shapeTransforms = rays[x].shape;
					break;
				}
			}
		}
		for (int x = 0; x < destrArr.Count; x++)
		{
			foreach (Transform tr in destrArr[x].shape)
				GameObject.Destroy(tr.gameObject);
			rays.RemoveAt(x);
		}
		if (destrArr.Count > 0)
			destrArr.Clear();
	}



}
