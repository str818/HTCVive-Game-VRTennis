using UnityEngine;
using System.Collections;

public class ArcReactorDemoTeleportZone : MonoBehaviour {

	public Vector3 teleportTo;

	void OnTriggerEnter(Collider other) {
		other.transform.position = teleportTo;
	}

}
