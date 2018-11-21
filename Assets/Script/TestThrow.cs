using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class TestThrow : MonoBehaviour {
	
	public GameObject prefab;
	public Rigidbody attachPoint;
	GameObject qiu;

	SteamVR_TrackedObject trackedObj;
	FixedJoint joint;

	void Awake() {
		trackedObj = GetComponent<SteamVR_TrackedObject>();
	}

	void FixedUpdate(){
		var device = SteamVR_Controller.Input((int)trackedObj.index);
		qiu = GameObject.Find ("qiu(Clone)");
		if (joint == null && device.GetTouchDown (SteamVR_Controller.ButtonMask.Trigger)) {
			if (qiu == null) {
                Debug.Log("位置为:" + attachPoint.transform.position);
                var go = GameObject.Instantiate (prefab, attachPoint.transform.position, new Quaternion(0,0,0,0));
				//go.transform.position = attachPoint.transform.position;
                Debug.Log("球的位置为:" + go.transform.position);
				joint = go.AddComponent<FixedJoint> ();
				joint.connectedBody = attachPoint;
			}
		} else if (joint != null && device.GetTouchUp (SteamVR_Controller.ButtonMask.Trigger)) {
			var go = joint.gameObject;
			var rigidbody = go.GetComponent<Rigidbody> ();
			Object.DestroyImmediate (joint);
			joint = null;
			Object.Destroy (go, 100.0f);

			var origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;
			if (origin != null) {
				rigidbody.velocity = origin.TransformVector (device.velocity);
				rigidbody.angularVelocity = origin.TransformVector (device.angularVelocity);
			} else {
				rigidbody.velocity = device.velocity;
				rigidbody.angularVelocity = device.angularVelocity;
			}
			rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;
		}

	}

}