using UnityEngine;
using System.Collections;

public class LyricManager : SingletonMonoBehaviour<LyricManager> {

	public UIManager uiManager;
	public GameObject[] lyric1p;
	public GameObject[] lyric2p;
	public Canvas canvas;
	public bool canCh=false;
	// Use this for initialization
	void Start () {
		uiManager.nowText1 = lyric1p[0];
		uiManager.nowText2 = lyric2p[0];
	}

	// Update is called once per frame
	void Update () {
		print(uiManager.end2p);
		if(uiManager.end2p){
			changeLyric();
		}
		print(uiManager.nowStageNum);
	}
	void changeLyric(){
		lyric1p[uiManager.nowStageNum].SetActive(false);
		lyric2p[uiManager.nowStageNum].SetActive(false);
		lyric1p[uiManager.nowStageNum+1].SetActive(true);
		lyric2p[uiManager.nowStageNum+1].SetActive(true);
	}
}
