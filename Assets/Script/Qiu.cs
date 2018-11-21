namespace VRTK.Examples{
	
	using UnityEngine;
	using System.Collections;
	using VRTK;

	public class Qiu : MonoBehaviour {
		
		private GameObject qiu; 																//网球对象
		public GameObject wanjia;																//玩家对象

		private Vector3 speed_start;															//球初始的速度
		private Vector3 position_start;															//球初始的位置

		private Vector3 speed;																	//当前球的速度
		private Vector3 position;																//当前球的位置
		private Vector3 speA;																	//球被打中后的速度

		private Vector3[] resultB;
		private Vector3 posB;																	//落点B的位置
		private Vector3 speB;																	//落点B的速度

        private Vector3 posC;																	//人接球的位置

		private Vector3 bingV;																	//手柄的速度
		private Vector3 renPos;																	//人的位置
		private bool isTouch = false;
		public bool isFallnet = false;
        public int gl;

        public bool isMoved = false;                                                            //判断是否位移过去
        public GameObject countPanel;                                                           //计分板

		private GameObject hitmusic;															//球拍击打网球音频源

		void Start(){
            Time.timeScale = Constant.currentDifficulty;										//设置难度
			wanjia = GameObject.Find ("[CameraRig]");											//获取[CameraRig]组件
			hitmusic = GameObject.Find ("hit");													//获取球拍击打网球音频源
			qiu = this.gameObject;																//获取球
            countPanel = GameObject.Find("SocrePanel");                                         //获取计分板引用
			//qiu.transform.position = position_start;											//设置球的初始位置
			//qiu.GetComponent<Rigidbody> ().velocity=speed_start;								//给球初始速度
		}

		void Update (){
            if (Constant.gameOver) {
                return;
            }
           	renPos = wanjia.transform.position;													//获取玩家的位置
			position = qiu.transform.position;													//获取网球的位置
            outSide();																			//判断出界
			canPlay();																			//判断网球是否进入玩家的击打范围
			play();																				//击打
		}

		//判断网球是否进入玩家的击打范围
        public void canPlay() {
			//玩家击打范围判断
            if (renPos.x - 1.2f < position.x && position.x < renPos.x + 1.2f) {
				//如果网球进入玩家的击打范围内
                if (renPos.y - 1.5f < position.y && position.y < renPos.y + 1.5f) {
                    if (renPos.z - 1.2f < position.z && position.z < renPos.z + 1.2f) {
                        isTouch = true;															//可击打
                    } else {
                        isTouch = false;
                    }
                } else {
                    isTouch = false;    
                }
            } else {
                isTouch = false;
            }
        }

		//玩家击打动作
		public void play(){
            if (isTouch) {
				//获取手柄速度
                bingV = VRTK_DeviceFinder.GetControllerRightHand().GetComponent<VRTK_ControllerEvents>().GetVelocity();
				if (bingV.magnitude > 0.8f && !isMoved) {										//如果玩家挥拍了
                    isMoved = true;                                                             //移动标志位置true
                    gailv();																	//判断概率
                    gestureJudge();																//判断手势
                    position = qiu.transform.position;											//获取球的位置
                    qiu.GetComponent<Rigidbody>().velocity = speA;								//给球一个回去的速度
                    resultB = CalculateUtil.getPosAndSpeB(position, speA);						//计算球落点的位置
                    posB = resultB[0];															//计算落球点B的位置
                    shock();																	//判断游戏是否结束
                    Constant.isOutside = false;													//顺利运行到这里表示未出界
                    isTouch = false;															//球未到玩家击球范围
                }
            }
		}

		//位移方法
		public void weiyi(){                                                                              //移动标志位置false
            if (posB.x > -8.5f || posB.x < -16.8f) {
                wanjia.transform.position = posB;												//把B位置赋给玩家
                wanjia.transform.LookAt(new Vector3(-12.5f, wanjia.transform.position.y, wanjia.transform.position.z));	//设置玩家朝向
                Constant.isShock = false;														//关闭震动
            } else {
                if (posB.x < -8.5f && posB.x > -12.5f) {
                    posC = new Vector3(posB.x + 2.5f, posB.y, posB.z);
                } else if (posB.x < -12.5f && posB.x > -16.8f) {
                    posC = new Vector3(posB.x - 2.5f, posB.y, posB.z);
                }
                wanjia.transform.position = posC;
                wanjia.transform.LookAt(new Vector3(-12.5f, wanjia.transform.position.y, wanjia.transform.position.z));
                Constant.isShock = false;
            }
            if (Constant.isFirstHit) {                                                                  //若是第一次击打
                Constant.isFirstHit = false;
            }else {
                Constant.SCORE += 3;
            }
            countPanel.transform.forward *= -1;                                                 //计分板翻转
            isMoved = false;
        }

		//判断游戏结束
		public void shock(){
			//如果出界
			if (position.x < -21f || position.x > -4f || position.z > -3.5f || position.z < -11.4f) {
                Constant.isShock = false;																//不允许震动
				Constant.gameOver = true;																//游戏结束
				Time.timeScale = Constant.PAUSE;                                                        //暂停
                GameObject.Find ("UI_Interactions").GetComponent<UIControl> ().gameover_OutSide();		//调用出界方法
                //Constant.SCORE -= 3;                                                                  //分数减3
			} else {
				hitmusic.GetComponent<AudioSource>().Play();											//播放打球音频
				Constant.isShock = true;																//允许震动
                Invoke("weiyi", 0.3f);                                                                  //0.3s后调用weiyi方法
            }
            
        }

		//判断球是否出界
        public void outSide() {
            if (position.x < -21f || position.x > -4f || position.z > -3.5f || position.z < -11.4f) {	//诊断当前球位置是否在界外
                Constant.isShock = false;																//关闭震动
                Constant.gameOver = true;																//设置游戏结束
                Time.timeScale = 0;                                                                     //暂停游戏
                GameObject.Find("UI_Interactions").GetComponent<UIControl>().gameover_OutSide();		//打开游戏结束出界界面
            }
        }

        public void gailv() {
            gl = Random.Range(0,100);																	//获取概率
            if (gl > Constant.currentGL) {																//如果当前获得的盖度大于难度概率
                Constant.isOutside = true;																//出界
            }
        }

		//手势判定
		public void gestureJudge(){
			//获取手柄速度
			bingV = VRTK_DeviceFinder.GetControllerRightHand().GetComponent<VRTK_ControllerEvents> ().GetVelocity ();
			if (renPos.x > -12.5f) {
				//玩家向z轴正方向挥拍
				if (bingV.z > 0) {
                    if (Constant.isOutside) {
                        speA.x = Random.Range(-10, -7);
                        speA.y = Random.Range(5, 9);
                        speA.z = Random.Range(0, 2);
                    } else {
                        speA.x = Random.Range(-6, -5);
                        speA.y = Random.Range(5, 9);
                        speA.z = Random.Range(0, 2);
                    }
				}else {																					//玩家向z轴负方向挥拍
                    if (Constant.isOutside) {
                        speA.x = Random.Range(-10, -7);
                        speA.y = Random.Range(5, 9);
                        speA.z = Random.Range(-2, 0);
                    } else {
                        speA.x = Random.Range(-6, -5);
                        speA.y = Random.Range(5, 9);
                        speA.z = Random.Range(-2, 0);
                    }
				}	
			} else {																					//玩家在AI端
                if (bingV.z > 0){																		//玩家向z轴正方向挥拍
                    if (Constant.isOutside) {
                        speA.x = Random.Range(8, 11);
                        speA.y = Random.Range(5, 9);
                        speA.z = Random.Range(0, 2);
                    } else {
                        speA.x = Random.Range(5, 7);
                        speA.y = Random.Range(5, 9);
                        speA.z = Random.Range(0, 2);
                    }
                } else {																				//玩家向z轴负方向挥拍
                    if (Constant.isOutside) {
                        speA.x = Random.Range(8, 11);
                        speA.y = Random.Range(5, 9);
                        speA.z = Random.Range(-2, 0);
                    } else {
                        speA.x = Random.Range(5, 7);
                        speA.y = Random.Range(5, 9);
                        speA.z = Random.Range(-2, 0);
                    }
                }	
			}
		}

	}

}
