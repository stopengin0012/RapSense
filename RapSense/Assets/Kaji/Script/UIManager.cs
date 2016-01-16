using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	[SerializeField]
	private GameObject Panel1, Panel2;
	[SerializeField]
	private int nowPlayer = 1;

	void Start(){
	}

	void ChangeTurn(){
		if(nowPlayer == 1){
			Panel1.SetActive(false);
			Panel2.SetActive(true);
			nowPlayer = 2;
		}if(nowPlayer == 2){
			Panel2.SetActive(false);
			Panel1.SetActive(true);
			nowPlayer = 1;
		}
	}
}
