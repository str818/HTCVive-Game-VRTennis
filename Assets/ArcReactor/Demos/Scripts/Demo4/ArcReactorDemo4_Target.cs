using UnityEngine;
using System.Collections;

public class ArcReactorDemo4_Target : MonoBehaviour {

	public ArcReactorDemo4_Manager manager;

	public void ArcReactorHit(ArcReactorHitInfo2D hit)
	{
		manager.TargetHit();
		Destroy(gameObject);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
