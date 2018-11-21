using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMove : MonoBehaviour {


    private float posY;         //初始气球位置
    private int forward = 1;    //移动方向
    public float span = 0.3f;  //上下浮动范围
    public float v = 0.005f;
    private float initialDis;   //初始浮动距离
    
    void Start() {
        initialDis = Random.Range(-1*span, span);
        posY = transform.position.y;
        transform.position = new Vector3(transform.position.x, transform.position.y + initialDis, transform.position.z);
    }

	void Update () {
        if (transform.position.y > posY + span) {
            forward = -1;
        }else if(transform.position.y < posY - span) {
            forward = 1;
        }
        transform.position = new Vector3(transform.position.x, transform.position.y + v * forward, transform.position.z);
	}
}
