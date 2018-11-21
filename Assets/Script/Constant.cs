using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Constant : MonoBehaviour {
	
	public static  Vector3 WANJIA_START_POSITION = new Vector3 (-6.8f, 0, -6f);//玩家的初始位置

	public const float UNIT_SIZE = 2.0f;
	//球场的长宽高 
	public const float COURT_LENGTH = 17.48f;
	public const float COURT_WIDTH = 6.547f;
	public const float COURT_HEIGHT = 0f;

	//球的半径
	public const float QIU_R = 0.030f;

	//网的宽和高
	public const float NET_WIDTH = 6.547f;
	public const float NET_HEIGHT = 1.773f;

	//网高度的y值
	public const float NET_Y = NET_HEIGHT + COURT_HEIGHT / 2.0f - 0.1f;

	//重力加速度
	public static  Vector3 A = new Vector3(0.0f,-9.8f,0.0f); 
	//球台面位置  高度y坐标  
	public static float COURT_Y = COURT_HEIGHT;

	//球场位置x最大值
	public const float COURT_X_MAX = COURT_WIDTH / 2.0f;
	//球场位置x最小值
	public const float COURT_X_MIN = -COURT_WIDTH / 2.0f;
	//球场位置z最大值
	public const float COURT_Z_MAX = COURT_LENGTH / 2.0f;
	//球场位置z最小值
	public const float COURT_Z_MIN=-COURT_LENGTH/2.0f;

	//球碰撞反弹后的能量损失系数
	public const float QIU_ENERGY_LOST = 0.866f;
	//是否是玩家端发球
	public static bool IS_SHOOT_MAN=true;
	//游戏结束标示位
	public static bool gameOver=false;
	//落网标志位
	public static bool isFallNet=false;
	//手柄震动标志位
	public static bool isShock=false;
	//是否要出界
    public static bool isOutside = false;

	//简单模式时间的缩放率
    public const float EASY_TIMESCALE = 0.8f;
	//一般模式时间的缩放率
    public const float SIMPLE_TIMESCALE = 0.9f;
	//困难模式时间的缩放率
    public const float HARD_TIMESCALE = 1.0f;
	//暂停时间
    public const float PAUSE = 0f;
	//当前模式的时间缩放率
    public static float currentDifficulty=SIMPLE_TIMESCALE;

	//简单模式出界概率阈值
    public const int EASY_GL = 97;
	//一般模式出界概率阈值
    public const int SIMPLE_GL = 95;
	//困难模式出界概率阈值
    public const int HARD_GL = 90;
	//当前模式的出界概率
    public static int currentGL = SIMPLE_GL;

	//对面机器人的发球位置，此处已经固定位置
	public const float AI_Z = -(COURT_Z_MAX + 1f);

    //当前分数
    public static int SCORE = 0;
    public static bool isFirstHit = true;
}
