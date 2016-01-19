using UnityEngine;
using System.Collections;

/// <summary>
/// 定常性を判別するための自己相関値を算出するクラス
/// @stopengin0012
/// </summary>
public class CalcACF : MonoBehaviour {

    double[] in_acf_x, in_acf_y, in_acf_z;
    double[] acf_x, acf_y, acf_z;

    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update () {
	
	}

    double[] autocorr(double[] input) {
        double[] autocorrfunc = {};
        double s;    //Σ用

        int N, n;
        n = input.Length;   //入力するデータの長さ
        N = n / 2;          // N = 2n

        autocorrfunc = new double[n];
        Debug.Log("N:" + N + ", n:"+n);

        //単純な自己相関の算出
        //A(t) = input(t) * input(t * tau)
        for (int j = 0; j < N; j++)
        {
            s = 0;

            for (int i = 1; i < N; i++)
            {
                //Debug.Log("N:"+N+",i:"+i+ ",j:" +j);
                s = s + input[i] * input[i + j];
            }

            autocorrfunc[j] = s;
        }

        //-1 ~ +1 になるように正規化
        for (int j = 0; j < N; j++)
        {
            autocorrfunc[j] = autocorrfunc[j] / autocorrfunc[0];
        }

        return autocorrfunc;
        
    }
}
