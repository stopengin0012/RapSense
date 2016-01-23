using UnityEngine;
using System.Collections;

/// <summary>
/// 定常性を判別するための自己相関値を算出するクラス
/// @stopengin0012
/// </summary>
public class CalcACF : MonoBehaviour {

    double[] acf_x, acf_y, acf_z;

    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update () {
	
	}

    public double[] autocorr_vec3(Vector3[] input) {

        double[] ret;
        ret = new double[3];
        for (int i=0; i<3; i++) {
            ret[i] = 0.0d;
        }

        double[] in_x = new double[input.Length];
        double[] in_y = new double[input.Length];
        double[] in_z = new double[input.Length];
        for (int i = 0; i < input.Length; i++) {
            in_x[i] = (double)input[i].x;
            in_y[i] = (double)input[i].y;
            in_z[i] = (double)input[i].z;
        }

        ret[0] = calcFirstPeak(autocorr(in_x));
        ret[1] = calcFirstPeak(autocorr(in_y));
        ret[2] = calcFirstPeak(autocorr(in_z));

        return ret;

    }

    public double[] autocorr(double[] input) {
        double[] autocorrfunc = {};
        double[] ret_autocorrfunc = { };
        double s;    //Σ用

        int N, n;
        n = input.Length;   //入力するデータの長さ
        N = n / 2;          // N = 2n

        autocorrfunc = new double[n];
        ret_autocorrfunc = new double[n];
        //Debug.Log("N:" + N + ", n:"+n);

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
            //Debug.Log("autocorrfunc[" + j + "]:" + autocorrfunc[j]+","+ "autocorrfunc[0]:"+ autocorrfunc[0]);
            ret_autocorrfunc[j] = autocorrfunc[j] / autocorrfunc[0];
            //autocorrfunc[j] = autocorrfunc[j] / autocorrfunc[0];
        }

        return ret_autocorrfunc;
        //return autocorrfunc;

    }

    public double calcFirstPeak(double[] autocorr) {
        double ret = 0.0d;

        int i = 0;

        while (autocorr[i] > autocorr[i + 1]) {
            i += 1;
        }

        while (autocorr[i] < autocorr[i + 1]) {
            i += 1;
        }

        ret = autocorr[i]; 

        return ret;
        
    }

}
