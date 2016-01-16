using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	[SerializeField]
	private GameObject Panel1, Panel2;
	[SerializeField]
	private int nowPlayer = 0;
	[SerializeField]
	private Text textCountdown;

	void Start(){
		Panel1.SetActive(false);
		Panel2.SetActive(true);
		nowPlayer = 1;
		StartCoroutine(CountdownCoroutine());
	}

	void ChangeTurn(){
		if(nowPlayer == 1){
			Panel1.SetActive(true);
			Panel2.SetActive(false);
			nowPlayer = 2;
		}else if(nowPlayer == 2){
			Panel1.SetActive(false);
			Panel2.SetActive(true);
			nowPlayer = 1;
		}
	}

	IEnumerator ChangeTurnSpan(float sec){
		yield return new WaitForSeconds(sec);
		ChangeTurn();
	}
	public void OnClickButtonStart()
	{
		StartCoroutine(CountdownCoroutine());
	}

IEnumerator CountdownCoroutine()
	{
		textCountdown.gameObject.SetActive(true);

		textCountdown.text = "3";
		yield return new WaitForSeconds(1.0f);

		textCountdown.text = "2";
		yield return new WaitForSeconds(1.0f);

		textCountdown.text = "1";
		yield return new WaitForSeconds(1.0f);

		textCountdown.text = "GO!";
		yield return new WaitForSeconds(1.0f);

		textCountdown.text = "";
		textCountdown.gameObject.SetActive(false);
	}
}
