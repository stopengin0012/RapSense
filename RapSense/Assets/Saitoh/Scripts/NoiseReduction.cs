using UnityEngine;
using System.Collections;

/// <summary>
/// 【CLASS】『NoiseReduction』【放物線で補間】
/// 作成者：neo-tech-lab, 上田さん http://www.neo-tech-lab.co.uk/
/// 変更者：sto_pengin @10/05/2015
/// </summary>
public class NoiseReduction : MonoBehaviour {

    //*************************************************************************
    //*************************************************************************
    //【CLASS】『NoiseReduction』【放物線で補間】
    //*************************************************************************
    //*************************************************************************
    // Oj = a*xj^2+b*xj+c で近似

    private int nSample;   // FIFO段数
    public double[] FIFO;   // FIFOデータ サンプル
    private int wPtr;      // FIFO書込みポインタ
    private int rPtr;      // FIFO読出しポインタ
    /*
    private float a, b, c; // 放物線O(t)=a*t^2+b*t+cの未知数
    private float d;       // 放物線の軸値d=-b/(2*a)  (加速度を示す)
    private float Ka;      // Ka=1.0f/Σtj^4
    private float Kb;      // Kb=1.0f/Σtj^2
    private float Kc;      // Kb=1.0f/n
    */

    private double a, b, c; // 放物線O(t)=a*t^2+b*t+cの未知数
    private double d;       // 放物線の軸値d=-b/(2*a)  (加速度を示す)
    private double Ka;      // Ka=1.0f/Σtj^4
    private double Kb;      // Kb=1.0f/Σtj^2
    private double Kc;      // Kb=1.0f/n

    private bool FIFOfull;  // FIFOfull flag
    private int repeat = 10; //放物線推測の回数

    public struct ParabolaData
    {
        public double A; //【放物線係数】
        public double B; //【放物線係数】
        public double C; //【放物線係数】
        public double D; //【放物線の軸】d=-b/(2*a)
    }

    private ParabolaData Parabola1;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //*************************************************************
    //【Initialize】
    //*************************************************************
    public void Initialize(int n)
    {
        double t, t2;
        nSample = n;
        FIFO = new double[nSample];
        wPtr = rPtr = 0;
        Ka = Kb = 0.0d;
        for (int i = 0; i < nSample; i++)
        {
            t = (double)(-i); // *0.033f; //【1フレームは33mS】
            FIFO[i] = 0.0d;  //【FIFOは0クリア】
            t2 = t * t;      //(-tj)^2
            Ka += t2 * t2;   //Σ(-tj)^4
            Kb += t2;        //Σ(-tj)^2
        }

        //if (Ka == 0.0d) Ka = 0.0d;
        //else Ka = 1.0d / Ka;
        Ka = 1.0d / Ka;

        //if (Kb == 0.0d) Kb = 0.0d;
        //else Kb = 1.0d / Kb;
        Kb = 1.0d / Kb;

        //Kb = 1.0d / Kb;
        Kc = 1.0d / (double)nSample;
        FIFOfull = false;
    }

    //*************************************************************
    //【Estimation】
    //*************************************************************
    public void Estimation(double data)
    {
        FIFO[wPtr] = data;
        rPtr = wPtr;
        //【書込みポインタの更新】
        wPtr++;
        if (wPtr == nSample)
        {
            wPtr = 0; FIFOfull = true;
        }

        //【Base Transition Ruleで放物線a*t^2+b*t+cを推定する】１０回実行
        if (FIFOfull)
        {
            for (int i=0;i<repeat;i++) {
                ParabolaEstimation();
            }

            //【推定した放物線a*t^2+b*t+cから軸値d=-b/(2*a)を計算】
            CalculateParabolaAxis();
        }
    }

    //*************************************************************
    //【Property】☆☆☆☆☆ Estimated Value ☆☆☆☆☆
    //*************************************************************
    public int Length { get { return nSample; } }
    public int Pointer { get { return wPtr; } }
    public bool Full { get { return FIFOfull; } }
    public double A { get { if (FIFOfull) { return a; } else { return 0.0f; } } }
    public double B { get { if (FIFOfull) { return b; } else { return 0.0f; } } }
    public double C { get { if (FIFOfull) { return c; } else { return 0.0f; } } }
    public double D { get { if (FIFOfull) { return d; } else { return -1000.0d; } } }

    public double getD() {
        return this.D;
    }

    public double getA()
    {
        return this.A;
    }

    public double getB()
    {
        return this.B;
    }

    public double getC()
    {
        return this.C;
    }

    public ParabolaData ParabolaParameters
    {
        get
        {
            if (FIFOfull)
            {
                Parabola1.A = a;
                Parabola1.B = b;
                Parabola1.C = c;
                Parabola1.D = d;
            }
            else
            {
                Parabola1.A = 0.0d;
                Parabola1.B = 0.0d;
                Parabola1.C = 0.0d;
                Parabola1.D = -1000.0d;
            }
            return Parabola1;
        }
    }

    public void ParabolaInit() {

        Parabola1.A = 0.0d;
        Parabola1.B = 0.0d;
        Parabola1.C = 0.0d;
        Parabola1.D = -1000.0d;
    }

    //---------------------------------------------------------------
    //【CircleEstimation】☆☆☆☆☆ Base Transition Rule ☆☆☆☆☆
    //---------------------------------------------------------------
    private void ParabolaEstimation()
    {
        int j;
        double t, t2, t4, e;
        double Sa, Sb, Sc; // (working parameter) Sa=Σt^4*(Sj-Oj), Sb=Σt^2*(Sj-Oj), Sc=Σ(Sj-Oj)
                          //【Estimate Xc】
        Sa = 0.0f;
        for (j = 0; j < nSample; j++)
        {
            t = (double)(-j); // *0.033f;
            t2 = t * t;
            t4 = t2 * t2;
            e = FIFO[rPtr] - (a * t2 + b * t + c);
            Sa += t2 * e;
            //【読出しポインタの更新】
            rPtr--; if (rPtr < 0) { rPtr = nSample - 1; }
        }
        a += Ka * Sa;

        //【Estimate Xc】
        Sb = 0.0f;
        for (j = 0; j < nSample; j++)
        {
            t = (double)(-j); // *0.033f;
            t2 = t * t;
            e = FIFO[rPtr] - (a * t2 + b * t + c);
            Sb += t * e;
            //【読出しポインタの更新】
            rPtr--; if (rPtr < 0) { rPtr = nSample - 1; }
        }
        b += Kb * Sb;
        
        //【Estimate Xc】
        Sc = 0.0f;
        for (j = 0; j < nSample; j++)
        {
            t = (double)(-j); // *0.033f;
            t2 = t * t;
            e = FIFO[rPtr] - (a * t2 + b * t + c);
            Sc += e;
            //【読出しポインタの更新】
            rPtr--; if (rPtr < 0) { rPtr = nSample - 1; }
        }
        c += Kc * Sc;
    }

    //---------------------------------------------------------------
    //【CalculateRadius】☆☆☆☆☆ Average ☆☆☆☆☆
    //---------------------------------------------------------------
    private void CalculateParabolaAxis()
    {
        /*
        if ((a>0 || a<0)
            || (b>0 || b<0))  //aやbが0のときはNanになるので、0を返す
        {
            d = -b / (2.0f * a);
        }
        else {
            d = 0.0f;
        }
        */

        if (a == 0.0d || b == 0.0d)
        {
            d = 0.0d;
        }
        else {
            d = -b / (2.0d * a);
        }
    }

}
