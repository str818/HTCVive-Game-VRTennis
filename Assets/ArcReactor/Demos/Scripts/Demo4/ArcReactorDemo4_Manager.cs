using UnityEngine;
using System.Collections;
using UnityEngine.UI;
	
public class ArcReactorDemo4_Manager : MonoBehaviour {

	public GameObject targetPrefab;
	public int points;
	public GameObject currentTarget;
	public Text pointsText;
	public Text levelText;


	public ArcReactorDemo3GenerateMirrors mirrorGen;

	private int startMirrorCount;

	public void TargetHit()
	{
		points += mirrorGen.mirrorCount;
		mirrorGen.mirrorCount++;
		mirrorGen.GenerateMirrors(mirrorGen.mirrorCount);
	}

	// Use this for initialization
	void Start () {
		mirrorGen = GetComponent<ArcReactorDemo3GenerateMirrors>();
		startMirrorCount = mirrorGen.mirrorCount;
	}
	
	// Update is called once per frame
	void Update () 
	{
		pointsText.text = "Points: " + points.ToString();
		levelText.text = "Level: " + (mirrorGen.mirrorCount-startMirrorCount + 1).ToString();
		if (currentTarget == null)
		{
			GameObject obj = (GameObject)Object.Instantiate(targetPrefab);
			obj.GetComponent<ArcReactorDemo4_Target>().manager = this;
			mirrorGen.PlaceObject2D(obj,new Vector2(transform.position.x,transform.position.y));
			currentTarget = obj;
		}
	}
}
