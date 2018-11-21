namespace VRTK.Examples {
	
	using UnityEngine;
	using System.Collections;
    using UnityEngine.UI;

    public class UIControl : MonoBehaviour {
		
		public GameObject leftController;													//左手手柄
		public GameObject wanjia;															//玩家对象
		private GameObject qiu;																//球

		public GameObject canvas;															//画布对象
		public GameObject mainMenu_Panel;													//主菜单panel
		public GameObject fallNet_Panel;													//落网panel
		public GameObject outSide_Panel;													//出界panel
        public GameObject difficulty_Panel;													//难度panel
		public GameObject gamemusic;														//游戏音频储存对象
		public GameObject mainscenemusic;													//主菜单音频存储对象
        public GameObject scorePanel;                                                       //分数面板
        public GameObject worldKeyboard;                                                    //键盘
        public GameObject rankPanel;                                                        //排名面板
        public Rank rankScript;                                                             //排名脚本

		private Vector3 CANVAS_START_POSITION = new Vector3 (-9.67f, 1.6f, -6.15f);			//画布开始位置
		private Vector3 CANVAS_HIDE_POSITION = new Vector3 (-9f, -2f, -6f);					//画布隐藏位置
		private Vector3 renPos;															
		private Vector3 panel_Position;													

		void Start () {
			mainscenemusic.GetComponent<AudioSource> ().Play ();							//播放主菜单音频
            scorePanel.SetActive(false);                                                    //隐藏计分板
            
        }

        void Update() {
            if (Constant.SCORE > 10) {
                scorePanel.GetComponent<TextMesh>().text = Constant.SCORE / 10 + " " + Constant.SCORE % 10;
            }else {
                scorePanel.GetComponent<TextMesh>().text = Constant.SCORE+"";
            }
        }
			
		//主菜单的开始游戏按钮
		public void start_ButtonClick(){
            Constant.gameOver = false;
            mainMenu_Panel.SetActive(false);												//隐藏主菜单
			canvas.transform.position = CANVAS_HIDE_POSITION;								//此时画布的位置为隐藏位置
			leftController.gameObject.GetComponent<TestThrow> ().enabled = true;	//左手控制器投掷脚本开启
			mainscenemusic.GetComponent<AudioSource> ().Stop();								//暂停播放主菜单音频
			gamemusic.GetComponent<AudioSource> ().Play();									//播放游戏音频
            Constant.SCORE = 0;                                                             //初始分数为0
            scorePanel.SetActive(true);                                                     //显示计分板
        }

		//主菜单的难度按钮
        public void difficulty_ButtonClick(){
			mainMenu_Panel.SetActive(false);												//隐藏主菜单
            difficulty_Panel.SetActive(true);												//打开难度Panel
        }

		//主菜单的退出按钮
        public void exit() {
            Application.Quit();																//退出应用
        }

		//落网界面，重新开始按钮
		public void fallnet_restart_ButtonClick(){
            Constant.gameOver = false;
            qiu = GameObject.Find ("qiu(Clone)");											//获取球
			DestroyImmediate (qiu);															//立即销毁球对象
			fallNet_Panel.SetActive (false);												//隐藏落网Panel
            rankPanel.SetActive(false);
            wanjia.transform.position = Constant.WANJIA_START_POSITION;						//重置玩家位置
			wanjia.transform.forward = new Vector3 (-1f,0,0);								//设置玩家朝向
			canvas.transform.position = CANVAS_HIDE_POSITION;								//此时画布的位置为隐藏位置
			canvas.transform.forward = new Vector3 (-1f, 0, 0);								//设置画布朝向
			Time.timeScale = Constant.currentDifficulty;									//设置当前模式的时间缩放率
            Constant.SCORE = 0;                                                             //初始分数为0
            scorePanel.SetActive(true);                                                     //显示计分板
            Constant.isFirstHit = true;                                                     //第一次击打
            scorePanel.transform.forward = new Vector3(-1, 0, 0);                           //重置计分板
        }

		//落网结束 返回按钮
		public void fallnet_return_ButtonClick(){
			qiu = GameObject.Find ("qiu(Clone)");											//获取球
			DestroyImmediate (qiu);															//立即销毁球对象
			fallNet_Panel.SetActive (false);												//隐藏落网Panel
            rankPanel.SetActive(false);
            wanjia.transform.position = Constant.WANJIA_START_POSITION;						//重置玩家位置
			wanjia.transform.forward = new Vector3 (-1f,0,0);								//设置玩家朝向
			canvas.transform.position = CANVAS_START_POSITION;								//此时画布的位置为隐藏位置
			canvas.transform.forward = new Vector3 (-1f, 0, 0);								//设置画布朝向
			mainMenu_Panel.SetActive (true);												//打开主菜单
			leftController.gameObject.GetComponent<TestThrow> ().enabled = false;	//关闭投掷脚本
			Time.timeScale = Constant.currentDifficulty;									//设置当前模式的时间缩放率
			mainscenemusic.GetComponent<AudioSource> ().Play();								//播放主菜单音频
			gamemusic.GetComponent<AudioSource> ().Stop();									//暂停游戏音频
            scorePanel.SetActive(false);                                                    //隐藏计分板
        }

		//出界界面重新开始按钮
		public void outside_restart_ButtonClick(){
            Constant.gameOver = false;
			qiu = GameObject.Find("qiu(Clone)");											//获取球
			DestroyImmediate(qiu);															//立即销毁球对象
			outSide_Panel.SetActive(false);													//隐藏出界Panel
            rankPanel.SetActive(false);
            wanjia.transform.position = Constant.WANJIA_START_POSITION;						//重置玩家位置
			wanjia.transform.forward = new Vector3(-1f, 0, 0);								//设置玩家朝向
            scorePanel.transform.forward = new Vector3(-1, 0, 0);                           //重置计分板
			canvas.transform.position = CANVAS_HIDE_POSITION;								//此时画布的位置为隐藏位置
			canvas.transform.forward = new Vector3(-1f, 0, 0);								//设置画布朝向
			Time.timeScale = Constant.currentDifficulty;									//设置当前模式的时间缩放率
            Constant.SCORE = 0;                                                             //初始分数为0
            Constant.gameOver = false;                                                      //球未出界
            scorePanel.SetActive(true);                                                     //显示计分板
            Constant.isFirstHit = true;                                                     //第一次击打
        }

		//球出界 返回按钮
		public void outside_return_ButtonClick(){
			qiu = GameObject.Find ("qiu(Clone)");											//获取球
			DestroyImmediate (qiu);															//立即销毁球对象
			outSide_Panel.SetActive (false);												//隐藏出界Panel
            rankPanel.SetActive(false);
            wanjia.transform.position = Constant.WANJIA_START_POSITION;						//重置玩家位置
			wanjia.transform.forward = new Vector3 (-1f,0,0);								//设置玩家朝向
			canvas.transform.position = CANVAS_START_POSITION;								//此时画布的位置为隐藏位置
			canvas.transform.forward = new Vector3 (-1f, 0, 0);								//设置画布朝向
			mainMenu_Panel.SetActive (true);												//打开主菜单
			leftController.gameObject.GetComponent<TestThrow> ().enabled = false;	//关闭投掷脚本
			Time.timeScale = Constant.currentDifficulty;									//设置当前模式的时间缩放率
			mainscenemusic.GetComponent<AudioSource> ().Play();								//播放主菜单音频
			gamemusic.GetComponent<AudioSource> ().Stop();									//暂停游戏音频
            scorePanel.SetActive(false);                                                    //隐藏计分板
        }

		//简单难度按钮
        public void set_esay_ButtonClick() {
			Time.timeScale = Constant.EASY_TIMESCALE;										//设置当前模式的时间缩放率
            Constant.currentDifficulty = Constant.EASY_TIMESCALE;							//设置当前难度
			Constant.currentGL = Constant.EASY_GL;											//设置当前模式的出界概率
            difficulty_Panel.SetActive(false);												//关闭难度界面
            mainMenu_Panel.SetActive(true);													//显示主菜单
        }

		//一般难度按钮
        public void set_simple_ButtonClick() {
			Time.timeScale = Constant.SIMPLE_TIMESCALE;										//设置当前模式的时间缩放率
			Constant.currentDifficulty = Constant.SIMPLE_TIMESCALE;							//设置当前难度
			Constant.currentGL = Constant.SIMPLE_GL;										//设置当前模式的出界概率
			difficulty_Panel.SetActive(false);												//关闭难度界面
			mainMenu_Panel.SetActive(true);													//显示主菜单
        }

		//困难难度按钮
        public void set_hard_ButtonClick() {
			Time.timeScale = Constant.HARD_TIMESCALE;										//设置当前模式的时间缩放率
			Constant.currentDifficulty = Constant.HARD_TIMESCALE;							//设置当前难度
			Constant.currentGL = Constant.HARD_GL;											//设置当前模式的出界概率
			difficulty_Panel.SetActive(false);												//关闭难度界面
			mainMenu_Panel.SetActive(true);													//显示主菜单
        }

		//落网UI控制
		public void gameover_FallNet(){
			renPos = wanjia.transform.position;												//记录玩家位置
			if (renPos.x > -12.5f) {														//如果玩家位置的x大于-12.5
				panel_Position = new Vector3 (renPos.x - 3f, renPos.y + 1.6f, renPos.z);	//定义一个临时变量
				canvas.transform.position = panel_Position;									//把值赋给画布
			} else {
				panel_Position = new Vector3(renPos.x + 3f, renPos.y + 1.6f, renPos.z);		//定义一个临时变量
				canvas.transform.position = panel_Position;									//把值赋给画布
                canvas.transform.LookAt(new Vector3(-renPos.x, renPos.y + 1.6f, renPos.z));	//设置画布朝向
			}
			fallNet_Panel.SetActive (true);													//打开落网界面
            if (rankScript.isInRank(Constant.SCORE)) {                                      //若挤进前五名
                StartCoroutine(ShowKeyBoard(fallNet_Panel));                                //显示键盘
            }
            else {
                StartCoroutine(Show_Rank_Directly(fallNet_Panel));                          //显示排名
            }
        }

		//出界UI控制
		public void gameover_OutSide(){
			renPos = wanjia.transform.position;												//记录玩家位置
			if (renPos.x > -12.5f) {														//如果玩家位置的x大于-12.5
				panel_Position = new Vector3(renPos.x - 3f, renPos.y + 1.6f, renPos.z);		//定义一个临时变量
				canvas.transform.position = panel_Position;									//把值赋给画布
			} else {
				panel_Position = new Vector3(renPos.x + 3f, renPos.y + 1.6f, renPos.z);		//定义一个临时变量
				canvas.transform.position = panel_Position;									//把值赋给画布
				canvas.transform.LookAt(new Vector3(-renPos.x, renPos.y + 1.6f, renPos.z));	//设置画布朝向
			}
			outSide_Panel.SetActive (true);													//打开出界界面
            if (rankScript.isInRank(Constant.SCORE)) {                                      //若挤进前五名
                StartCoroutine(ShowKeyBoard(outSide_Panel));                                //显示键盘
            }else {
                StartCoroutine(Show_Rank_Directly(outSide_Panel));                          //显示排名
            }
		}

        //显示键盘
        IEnumerator ShowKeyBoard(GameObject obj) {
            yield return new WaitForSecondsRealtime(2.0f);                                  //等待2s
            obj.SetActive(false);                                                           //隐藏提示界面
            worldKeyboard.SetActive(true);                                                  //显示键盘
        }

        //从键盘输入回车后显示排名
        public void showRankAfterInput(string name,int score) {
            rankScript.updateRank(name, score);                                             //更新排名
            worldKeyboard.SetActive(false);                                                 //隐藏键盘
            showRank();                                                                     //展示排名
        }


        //直接显示排名协程
        IEnumerator Show_Rank_Directly(GameObject obj) {
            yield return new WaitForSecondsRealtime(2.0f);                                  //等待2s
            obj.SetActive(false);                                                           //隐藏提示界面
            showRank();                                                                     //直接显示排名
        }

        //根据排名字典对排名UI进行调整与显示
        public void showRank() {
            rankPanel.SetActive(true);
            int index = 1;                                                                  //序号
            foreach(var temp in rankScript.dic) {
                GameObject obj = rankPanel.transform.Find("NO_" + index).gameObject;   //获取对应排名对象
                obj.transform.Find("Name").GetComponent<Text>().text = temp.Key;       //设置名称
                obj.transform.Find("Score").GetComponent<Text>().text = temp.Value+""; //设置分数
                index++;                                                                    //序号加一
            }
            for(int i = index; i <= 5; i++) {
                GameObject obj = rankPanel.transform.Find("NO_" + index).gameObject;   //获取对应排名对象
                obj.transform.Find("Name").GetComponent<Text>().text = "";             //设置名称
                obj.transform.Find("Score").GetComponent<Text>().text = "";            //设置分数
            }
        }
    }
}