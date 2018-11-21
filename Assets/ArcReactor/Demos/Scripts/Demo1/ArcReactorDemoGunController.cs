using UnityEngine;
using System.Collections;

public class ArcReactorDemoGunController : MonoBehaviour {

	public ArcReactor_Launcher[] launchers;
	public float rechargeRate = 1;
	public int selectedLauncher;

	private float recharge;
	

	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKey(KeyCode.Alpha1) && launchers.Length > 0 && launchers[0] != null)
		    selectedLauncher = 0;
		if (Input.GetKey(KeyCode.Alpha2) && launchers.Length > 1 && launchers[1] != null)
			selectedLauncher = 1;
		if (Input.GetKey(KeyCode.Alpha3) && launchers.Length > 2 && launchers[2] != null)
			selectedLauncher = 2;
		if (Input.GetKey(KeyCode.Alpha4) && launchers.Length > 3 && launchers[3] != null)
			selectedLauncher = 3;
		if (Input.GetKey(KeyCode.Alpha5) && launchers.Length > 4 && launchers[4] != null)
			selectedLauncher = 4;
		if (Input.GetKey(KeyCode.Alpha6) && launchers.Length > 5 && launchers[5] != null)
			selectedLauncher = 5;
		if (Input.GetKey(KeyCode.Alpha7) && launchers.Length > 6 && launchers[6] != null)
			selectedLauncher = 6;
		if (Input.GetKey(KeyCode.Alpha8) && launchers.Length > 7 && launchers[7] != null)
			selectedLauncher = 7;
		if (Input.GetKey(KeyCode.Alpha9) && launchers.Length > 8 && launchers[8] != null)
			selectedLauncher = 8;
		if (Input.GetKey(KeyCode.Alpha0) && launchers.Length > 9 && launchers[9] != null)
			selectedLauncher = 9;
		recharge = Mathf.Clamp(recharge - Time.deltaTime,0,1000);
		//Screen.lockCursor = true;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		if (Input.GetMouseButton(0) && recharge == 0)
		{
			launchers[selectedLauncher].LaunchRay();
			recharge = rechargeRate;
		}
	
	}
}
