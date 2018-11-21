using UnityEngine;
using System.Collections;

public class ArcReactorDemo5_mousemove : MonoBehaviour {

	public bool grabbed = false;
	public LayerMask raycastMask;
	public LayerMask objectMask;

	// Update is called once per frame
	void Update () 
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Input.GetMouseButtonDown(0))
		{
			grabbed = (Physics.Raycast(ray, out hit,1000,objectMask));
		}

		if (grabbed && Input.GetMouseButton(0) && Physics.Raycast(ray, out hit,1000,raycastMask))
		{
			transform.position = hit.point;
		}

		if (Input.GetMouseButtonUp(0))
		{
			grabbed = false;
		}
	
	}
}
