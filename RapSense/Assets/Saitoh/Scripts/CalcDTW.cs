using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 最も近いジェスチャを確認するためのDynamic Time Warping[1981, Myers]の値を計算するクラス
/// @stopengin0012
/// </summary>
public class CalcDTW : MonoBehaviour {


    #region Field
    double[] x;     //一つ目のデータ列
    double[] y;     //二つ目のデータ列
    double[,] distance;     //DTWの距離を算出する
    double[,] f;            //
    ArrayList pathX;        
    ArrayList pathY;
    ArrayList distanceList;
    double sum;
    #endregion 


    // Use this for initialization
    void Start () {
        
        //テスト用
        //double[] _x = { 9.0, 3.0, 1.0, 5.0, 1.0, 2.0, 0.0, 1.0, 0.0, 2.0, 2.0, 8.0, 1.0, 7.0, 0.0, 6.0, 4.0, 4.0, 5.0 };
        double[] _x = { 2.0, 8.0, 1.0, 0.0, 6.0, 4.0, 4.0, 5.0 };
        double[] _y = { 1.0, 0.0, 5.0, 5.0, 0.0, 1.0, 0.0, 1.0, 0.0, 3.0, 3.0, 2.0, 8.0, 1.0, 0.0, 6.0, 4.0, 4.0, 5.0 };

        setX(_x);
        setY(_y);
        Debug.Log("x.Length:" + x.Length + ", y.Length:" + y.Length);

        Initialize();
        computeDTW();

        Debug.Log("sum:"+getSum());

    }

    void Initialize() {
        distance = new double[x.Length, y.Length];
        f = new double[x.Length + 1, y.Length + 1];

        for (int i = 0; i < x.Length; ++i)
        {
            for (int j = 0; j < y.Length; ++j)
            {
                distance[i, j] = Math.Abs(x[i] - y[j]);
            }
        }

        for (int i = 0; i <= x.Length; ++i)
        {
            for (int j = 0; j <= y.Length; ++j)
            {
                f[i, j] = -1.0;
            }
        }

        for (int i = 1; i <= x.Length; ++i)
        {
            f[i, 0] = double.PositiveInfinity;
        }
        for (int j = 1; j <= y.Length; ++j)
        {
            f[0, j] = double.PositiveInfinity;
        }

        f[0, 0] = 0.0;
        sum = 0.0;

        pathX = new ArrayList();
        pathY = new ArrayList();
        distanceList = new ArrayList();

    }

    // Update is called once per frame
    void Update () {
	
	}

    #region GetterSetter

    public void setX(double[] _x) {
        this.x = _x;
    }

    public void setY(double[] _y)
    {
        this.y = _y;
    }

    public ArrayList getPathX()
    {
        return pathX;
    }

    public ArrayList getPathY()
    {
        return pathY;
    }

    public double getSum()
    {
        return sum;
    }

    public double[,] getFMatrix()
    {
        return f;
    }

    public ArrayList getDistanceList()
    {
        return distanceList;
    }
    #endregion

    public void computeDTW()
    {
        sum = computeFBackward(x.Length, y.Length);
        //sum = computeFForward();
    }

    //前向きにDTW計算
    public double computeFForward()
    {
        for (int i = 1; i <= x.Length; ++i)
        {
            for (int j = 1; j <= y.Length; ++j)
            {
                if (f[i - 1, j] <= f[i - 1, j - 1] && f[i - 1, j] <= f[i, j - 1])
                {
                    f[i, j] = distance[i - 1, j - 1] + f[i - 1, j];
                }
                else if (f[i, j - 1] <= f[i - 1, j - 1] && f[i, j - 1] <= f[i - 1, j])
                {
                    f[i, j] = distance[i - 1, j - 1] + f[i, j - 1];
                }
                else if (f[i - 1, j - 1] <= f[i, j - 1] && f[i - 1, j - 1] <= f[i - 1, j])
                {
                    f[i, j] = distance[i - 1, j - 1] + f[i - 1, j - 1];
                }
            }
        }
        return f[x.Length, y.Length];
    }

    //後ろ向きにDTW計算
    public double computeFBackward(int i, int j)
    {
        if (!(f[i, j] < 0.0))
        {
            return f[i, j];
        }
        else
        {
            if (computeFBackward(i - 1, j) <= computeFBackward(i, j - 1) && computeFBackward(i - 1, j) <= computeFBackward(i - 1, j - 1)
                && computeFBackward(i - 1, j) < double.PositiveInfinity)
            {
                f[i, j] = distance[i - 1, j - 1] + computeFBackward(i - 1, j);
            }
            else if (computeFBackward(i, j - 1) <= computeFBackward(i - 1, j) && computeFBackward(i, j - 1) <= computeFBackward(i - 1, j - 1)
                && computeFBackward(i, j - 1) < double.PositiveInfinity)
            {
                f[i, j] = distance[i - 1, j - 1] + computeFBackward(i, j - 1);
            }
            else if (computeFBackward(i - 1, j - 1) <= computeFBackward(i - 1, j) && computeFBackward(i - 1, j - 1) <= computeFBackward(i, j - 1)
                && computeFBackward(i - 1, j - 1) < double.PositiveInfinity)
            {
                f[i, j] = distance[i - 1, j - 1] + computeFBackward(i - 1, j - 1);
            }
        }
        return f[i, j];
    }
}
