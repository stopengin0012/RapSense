using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	[SerializeField]
	private GameObject Panel1, Panel2;
	[SerializeField]
	private int nowPlayer = 0;
	[SerializeField]
	private Text textCountdown, textCountdown2;
	[SerializeField]
	private GameObject nowText1,nowText2;
	[SerializeField]
	private Image[] nowStage;
	[SerializeField]
	private int nowStageNum=-1;
	[SerializeField]
	private bool endStage = false;

	void Start(){
		ChangeTurn();
	}

	void Update(){
	}

	void TurnOnNowStageBar(){
		iTween.ValueTo(gameObject, iTween.Hash(
			"from", 0f
			, "to", 1f
			, "time", 1f
			, "onupdate", "SetAlpha"  // 毎フレーム SetAlpha() を呼びます。
		));
		nowStageNum ++;
	}
	void SetAlpha(float alpha){
		nowStage[nowStageNum].color = new Color (255,255,255,alpha);
	}


	void ChangeTurn(){
		if(nowPlayer == 1){
			Panel1.SetActive(true);
			Panel2.SetActive(false);
			nowPlayer = 2;
			StartCoroutine(CountdownCoroutine(textCountdown2));
		}else{
			TurnOnNowStageBar();
			Panel1.SetActive(false);
			Panel2.SetActive(true);
			nowPlayer = 1;

			StartCoroutine(CountdownCoroutine(textCountdown));
		}
	}

	IEnumerator ChangeTurnSpan(float sec){
		yield return new WaitForSeconds(sec);
		ChangeTurn();
	}


	IEnumerator CountdownCoroutine(Text countdown)
	{
		yield return new WaitForSeconds(2f);
		countdown.gameObject.SetActive(true);
		countdown.text = "3";
		yield return new WaitForSeconds(1.0f);
		countdown.text = "2";
		yield return new WaitForSeconds(1.0f);
		countdown.text = "1";
		yield return new WaitForSeconds(1.0f);
		countdown.text = "GO!";
		yield return new WaitForSeconds(1.0f);
		countdown.text = "";
		countdown.gameObject.SetActive(false);
		if(nowPlayer == 1){
			StartCoroutine(ChangeTurnSpan(nowText1.GetComponent<TextSpan>().spanTime));
		}else{
			StartCoroutine(ChangeTurnSpan(nowText2.GetComponent<TextSpan>().spanTime));
		}
	}
}
