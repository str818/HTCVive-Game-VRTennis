using UnityEngine;
using System.Collections;

public class ArcReactorDemo_touchTest : MonoBehaviour {

	void ArcReactorTouch (ArcReactorHitInfo hitInfo)
	{
		Debug.Log(hitInfo.raycastHit.point);
	}
}
