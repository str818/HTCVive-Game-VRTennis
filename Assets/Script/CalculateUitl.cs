using UnityEngine;
using System.Collections;

public class CalculateUtil : MonoBehaviour {

	//根据玩家打出球的位置和速度计算对面落点的位置和反弹速度
	public static Vector3[] getPosAndSpeB(Vector3 position, Vector3 speed){
		Vector3[] result = new Vector3[2];
		Vector3 positionB = new Vector3 (0f, 0f, 0f);
		Vector3 speedB = new Vector3 (0f, 0f, 0f);
		//y方向上就是竖直上抛 起始速度是speed.y，其实高度是球场高度+球的半径
		float t1 = Mathf.Abs (speed.y / Constant.A.y);										//球到最高点的时间
		float s1 = Mathf.Abs (speed.y * speed.y / (2 * Constant.A.y));						//到最高点的距离
		float s2 = s1 + position.y - (Constant.COURT_Y + Constant.QIU_R);					//最高点到地面的距离
		float t2 = (float)Mathf.Sqrt (Mathf.Abs (2 * s2 / Constant.A.y));					//最高点到地面的时间（自由落体）
		float t = t1 + t2;																	//球到对面场地（b点）的时间
		//b点的坐标
		float px = position.x + speed.x * t;
		float pz = position.z + speed.z * t;
		positionB = new Vector3 (px, Constant.COURT_Y + Constant.QIU_R, pz);				//计算AI端发球位置B

		//计算反弹后一瞬间的速度, x z上的加速度暂时为0
		float vx = speed.x + Constant.A.x * t;
		float vy = Mathf.Abs ((speed.y + Constant.A.y * t) * Constant.QIU_ENERGY_LOST);
		float vz = speed.z + Constant.A.z * t;
		speedB = new Vector3 (vx, vy, vz);													//计算AI端发球速度B

		result [0] = positionB;																//保存位置
		result [1] = speedB;																//保存速度
		return result;																		//返回计算结果
	}	
}