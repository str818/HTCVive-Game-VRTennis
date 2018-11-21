using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

//排行榜脚本
public class Rank : MonoBehaviour {

    public Dictionary<string, int> dic = new Dictionary<string, int>();                   //排名字典 string - 玩家名称 int - 玩家分数

	void Start () {
        createFile(Application.persistentDataPath, "Rank_List.txt");                //创建文本文档记录排名
        localText(Application.persistentDataPath+ "/Rank_List.txt");                //从本地加载仅字典
    }
	
	void Update () {
		
	}

    //创建txt文本
    public void createFile(string spath, string name) {
        if (Directory.Exists(spath) && !File.Exists(spath + "/" + name)) {          //如果不存在该文件夹
            FileStream stream = File.Create(spath + "/" + name);                    //创建文件
            stream.Close();                                                         //如不及时关闭,短时间进行其他操作会有问题
        }
    }

    //读取txt中的信息
    public void localText(string path) {
        dic.Clear();                                                                 //清空列表                                                                              //初始化内嵌实验资源包 到本地实验包列表
        using (System.IO.StreamReader sr = new System.IO.StreamReader(
            path, Encoding.Default)) {
            string str;                                                             //定义局部变量保存每行字符串
            while ((str = sr.ReadLine()) != null) {                                 //如果此行不为空
                string[] ss = str.Split(new char[] { ' ' });                        //根据空格分隔字符串 
                dic.Add(ss[0], int.Parse(ss[1]));                                   //读取文本中的内容到数据字典中
            }
        }
    }

    //将字典中的内容根据分数排序
    public void dictionarySort() {
        if (dic.Count > 0) {                                                        //若字典不为空
            List<KeyValuePair<string, int>> lst = new List<KeyValuePair<string, int>>(dic);
            lst.Sort(delegate (KeyValuePair<string, int> s1, KeyValuePair<string, int> s2)
            {
                return s2.Value.CompareTo(s1.Value);
            });
            dic.Clear();

            foreach (KeyValuePair<string, int> kvp in lst)
                dic.Add(kvp.Key, kvp.Value);
        }
    }

    //向本地txt中写入数据
    public void writeFile() {
        //StreamWriter一个参数默认覆盖
        //StreamWriter第二个参数为false覆盖现有文件，为true则把文本追加到文件末尾
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(
            Application.persistentDataPath + "/Rank_List.txt", false)) {
            foreach (KeyValuePair<string, int> kv in dic) {
                file.WriteLine(kv.Key+" "+kv.Value);            //覆盖之前的内容，重新记录排名
            }
        }
    }

    //判断本局所得分数是否在前五的排名中
    public bool isInRank(int socre) {
        if (dic.Count < 5) return true;
        foreach (KeyValuePair<string, int> kv in dic) {
            if(socre > kv.Value) return true;                   //若当前的分数大于排名前五的某一个玩家
        }
        return false;
    }

    //更新排名
    public void updateRank(string playerName,int score) {
        if(dic.ContainsKey(playerName)) {                       //若包含此玩家名称
            dic[playerName] = score;                            //直接更改分数
        }else {
            dic.Add(playerName, score);                         //将新的得分添加到排名列表中
        }
        dictionarySort();                                       //根据得分进行排序
        if (dic.Count > 5) {
            string lastName = null;                             //排名最后的名称
            foreach (KeyValuePair<string, int> kv in dic) {
                lastName = kv.Key;
            }
            dic.Remove(lastName);                               //删除最后一名
        }
        writeFile();                                            //写入txt
    }
}
