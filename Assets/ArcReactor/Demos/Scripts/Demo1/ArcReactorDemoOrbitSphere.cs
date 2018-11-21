using UnityEngine;
using System.Collections;

public class ArcReactorDemoOrbitSphere : MonoBehaviour {

	public Vector3 center;
	public float radius;
	public float angularSpeed;


	private float currentAngle = 0;

	// Update is called once per frame
	void Update () 
	{
		currentAngle += angularSpeed * Time.deltaTime;
		if (currentAngle > 360)
			currentAngle -= 360;
		if (currentAngle < 0)
			currentAngle += 360;
		transform.position = new Vector3(center.x + Mathf.Sin(currentAngle*Mathf.Deg2Rad)*radius,
		                                 transform.position.y,
		                                 center.z + Mathf.Cos(currentAngle*Mathf.Deg2Rad)*radius);
		transform.LookAt(center);
	}
}
