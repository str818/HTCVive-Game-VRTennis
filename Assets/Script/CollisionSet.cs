namespace VRTK.Examples{
	
	using UnityEngine;
	using System.Collections;

	public class CollisionSet : MonoBehaviour {
		
		public void OnCollisionEnter(Collision collision){
			if (collision.gameObject.name == "qiu(Clone)") {
				Time.timeScale = 0;										//静止
                Constant.isShock = false;								//没有震动
				GameObject.Find ("UI_Interactions").GetComponent<UIControl> ().gameover_FallNet ();		//调用游戏结束-落网
            }
		}
	}

}
