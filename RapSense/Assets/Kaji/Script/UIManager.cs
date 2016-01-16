using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	[SerializeField]
	private GameObject Panel1, Panel2;
	[SerializeField]
	private int nowPlayer = 0;

	void Start(){
		Panel1.SetActive(false);
		Panel2.SetActive(true);
		nowPlayer = 1;
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
}
