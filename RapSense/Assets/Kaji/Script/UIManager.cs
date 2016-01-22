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
	[SerializeField]
	private GameObject endTextObj;
	[SerializeField]
	private GameObject[] stageNumSprite;
	[SerializeField]
	private GameObject StageSprite;
	private Vector3 stageSpriteInitPos;
	private bool end2p = false;

	private bool inEnd = false;

	void Start(){
		stageSpriteInitPos = StageSprite.GetComponent<RectTransform>().localPosition;
		StartCoroutine(NowStageUIIn());
		StartCoroutine(GameStartCor());
	}
	IEnumerator GameStartCor(){
		yield return new WaitForSeconds(2f);
		StartCoroutine(Change2to1());
	}

	IEnumerator NowStageUIIn(){
		if(nowStageNum != -1) SetPanel1();
		yield return new WaitForSeconds(2f);
		end2p = false;
		inEnd = true;

		iTween.ValueTo(gameObject, iTween.Hash(
			"from", -750f
			, "to", -32.5f
			, "time", 1f
			, "onupdate", "SetSpritePos"  // 毎フレーム SetAlpha() を呼びます。
		));
	}
	IEnumerator NowStageUIOut(){
		print("in");
		yield return new WaitForSeconds(1f);
		print("1");
		iTween.ValueTo(gameObject, iTween.Hash(
			"from", -32.5f
			, "to", 700f
			, "time", 1f
			, "onupdate", "SetSpritePos"  // 毎フレーム SetAlpha() を呼びます。
		));
	}
	void SetSpritePos(float pos){
		StageSprite.GetComponent<RectTransform>().localPosition = new Vector3(pos,40,0);
	}




	void Update(){
		if(inEnd && StageSprite.GetComponent<RectTransform>().localPosition.x == -32.5f){
			inEnd = false;
			StartCoroutine(NowStageUIOut());
		}
		if(StageSprite.GetComponent<RectTransform>().localPosition.x >=699){
			if(nowStageNum < 4){
				stageNumSprite[nowStageNum].SetActive(false);
				stageNumSprite[nowStageNum+1].SetActive(true);
			}
			StageSprite.GetComponent<RectTransform>().localPosition = stageSpriteInitPos;
			print(nowStageNum);
		}
		if(end2p){
			NowStageUIIn();
		}
	}

	void TurnOnNowStageBar(){
		nowStageNum ++;
		iTween.ValueTo(gameObject, iTween.Hash(
			"from", 0f
			, "to", 1f
			, "time", 1f
			, "onupdate", "SetAlpha"  // 毎フレーム SetAlpha() を呼びます。
		));
	}
	void SetAlpha(float alpha){
		nowStage[nowStageNum].color = new Color (255,255,255,alpha);
	}

	void SetPanelOut(){
		iTween.ValueTo(gameObject, iTween.Hash(
			"from", 0f
			, "to", 500f
			, "time", 1f
			, "onupdate", "SetPanelPos1"  // 毎フレーム SetAlpha() を呼びます。
		));
		iTween.ValueTo(gameObject, iTween.Hash(
			"from", 0f
			, "to", 500f
			, "time", 1f
			, "onupdate", "SetPanelPos2"  // 毎フレーム SetAlpha() を呼びます。
		));
	}
	void SetPanelAll(){
		iTween.ValueTo(gameObject, iTween.Hash(
			"from", 500f
			, "to", 0f
			, "time", 1f
			, "onupdate", "SetPanelPos1"  // 毎フレーム SetAlpha() を呼びます。
		));
		iTween.ValueTo(gameObject, iTween.Hash(
			"from", 500f
			, "to", 0f
			, "time", 1f
			, "onupdate", "SetPanelPos2"  // 毎フレーム SetAlpha() を呼びます。
		));
	}

	void SetPanel1(){
		iTween.ValueTo(gameObject, iTween.Hash(
			"from", 0f
			, "to", 500f
			, "time", 1f
			, "onupdate", "SetPanelPos1"  // 毎フレーム SetAlpha() を呼びます。
		));
	}
	void SetPanel2(){
		iTween.ValueTo(gameObject, iTween.Hash(
			"from", 0f
			, "to", 500f
			, "time", 1f
			, "onupdate", "SetPanelPos2"  // 毎フレーム SetAlpha() を呼びます。
		));
	}
	void OutPanel2(){
		iTween.ValueTo(gameObject, iTween.Hash(
			"from", 500f
			, "to", 0f
			, "time", 1f
			, "onupdate", "SetPanelPos2"  // 毎フレーム SetAlpha() を呼びます。
		));
	}
	void SetPanelCh1(){
		iTween.ValueTo(gameObject, iTween.Hash(
			"from", 0f
			, "to", 500f
			, "time", 1f
			, "onupdate", "SetPanelPos2"  // 毎フレーム SetAlpha() を呼びます。
		));
		iTween.ValueTo(gameObject, iTween.Hash(
			"from", 500f
			, "to", 0f
			, "time", 1f
			, "onupdate", "SetPanelPos1"  // 毎フレーム SetAlpha() を呼びます。
		));
	}
	void SetPanelCh2(){
		iTween.ValueTo(gameObject, iTween.Hash(
			"from", 0f
			, "to", 500f
			, "time", 1f
			, "onupdate", "SetPanelPos1"  // 毎フレーム SetAlpha() を呼びます。
		));
		iTween.ValueTo(gameObject, iTween.Hash(
			"from", 500f
			, "to", 0f
			, "time", 1f
			, "onupdate", "SetPanelPos2"  // 毎フレーム SetAlpha() を呼びます。
		));
	}
	void SetPanelPos1(float yPos){
		Vector3 temp = Panel1.GetComponent<RectTransform>().localPosition;
		temp.y = yPos;
		Panel1.GetComponent<RectTransform>().localPosition = temp;
	}
	void SetPanelPos2(float yPos){
		Vector3 temp = Panel2.GetComponent<RectTransform>().localPosition;
		temp.y = yPos;
		Panel2.GetComponent<RectTransform>().localPosition = temp;
	}

	IEnumerator Change2to1(){
		if(nowPlayer == 2) end2p = true;
		yield return new WaitForSeconds(3f);
		OutPanel2();
		TurnOnNowStageBar();

		nowPlayer = 1;
			StartCoroutine(CountdownCoroutine(textCountdown));
	}
	void Change1to2(){
		SetPanelCh1();
		nowPlayer = 2;
		StartCoroutine(CountdownCoroutine(textCountdown2));
	}

	void EndGame(){
		iTween.ValueTo(gameObject, iTween.Hash(
			"from", 0f
			, "to", 1f
			, "time", 0.5f
			, "onupdate", "SetEndPos"  // 毎フレーム SetAlpha() を呼びます。
		));
	}
	void SetEndPos(float scaleVal){
		endTextObj.GetComponent<RectTransform>().localScale = new Vector3(scaleVal,scaleVal,scaleVal);
	}


	IEnumerator ChangeTurnSpan(float sec){
		yield return new WaitForSeconds(sec);
		if(nowPlayer == 1){
			Change1to2();
		}else if(nowPlayer != 1 && nowStageNum < 4){
			StartCoroutine(Change2to1());
		}else{
			EndGame();
		}
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
