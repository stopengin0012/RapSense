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

	void Start(){
		Panel1.SetActive(false);
		Panel2.SetActive(true);
		ChangeTurn();
	}

	void ChangeTurn(){
		if(nowPlayer == 1){
			Panel1.SetActive(true);
			Panel2.SetActive(false);
			nowPlayer = 2;
			StartCoroutine(CountdownCoroutine(textCountdown2));
		}else{
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
