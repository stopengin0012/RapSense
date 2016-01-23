using UnityEngine;
using System.Collections;

public class ScoreManager : SingletonMonoBehaviour<ScoreManager> {

	public UIManager uiManager;

	public int score1p=0, score2p=0;

	public void Awake()
	{
	 if(this != Instance)
	 {
		Destroy(this);
		return;
	 }
 }

	void AddScore(int score){
		if(uiManager.nowPlayer == 1){
			score1p += score;
		}else if(uiManager.nowPlayer == 2){
			score2p += score;
		}
	}

}
