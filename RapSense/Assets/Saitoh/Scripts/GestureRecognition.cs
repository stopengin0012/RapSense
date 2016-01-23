using UnityEngine;
using System.Collections;
using RSUnityToolkit;


/// <summary>
/// 手の動きからジェスチャを認識するクラス
/// アルゴリズムとしては、村尾ら[2011]の手法を参考にする
/// 1. まず各jointの位置の加速度を算出
/// 2. 閾値による変動検出(要調整)
/// 3. 定常性判定による、繰り返し動作かどうかの判別
/// 4-1. [定常性が見られた場合]繰り返し動作と判断し、ムード・波をイメージしたエフェクトをかける
/// 4-2. [定常性が見られない場合]ジェスチャと判別し、星が流れたり、シュッとしたエフェクトをかける
/// </summary>
public class GestureRecognition : MonoBehaviour {

    #region Field
    private SystemAdmin systemAdmin;  //システム情報(singleton)

    //手元、親指(の先)、中指、小指(_つきは元情報を格納、_なしはwristからの相対位置を示す)
    Vector3 _left_wrist_pos, _left_index_pos, _left_thumb_pos, _left_middle_pos, _left_pinky_pos;
    Vector3 past_left_wrist_pos, past_left_index_pos, past_left_thumb_pos, past_left_middle_pos, past_left_pinky_pos;
    Vector3 left_index_pos, left_thumb_pos, left_middle_pos, left_pinky_pos;
    //GameObject _left_wrist, _left_thumb, _left_middle, _left_pinky; //元データ取得のためのGameObject
    GameObject[] _lefthand_gobj; //元データ取得のためのGameObject
    Vector3 left_wrist_acc, left_index_acc, left_thumb_acc, left_middle_acc, left_pinky_acc;

    //基底遷移アルゴリズムによる加速度計算用の関節位置の配列[4つの関節,3軸]
    // 0: wrist, 1:index,, 1:thumb, 2:middle, 3:pinky
    //(注)ただし、wristだけはカメラからの位置を用いる
    NoiseReduction[][] nr;
    private int nr_nFIFO = 5;  //何個前の位置情報から加速度を推定するか：10の場合2秒間の残像


    //自己相関関数の値
    double[][] ACFValue;             //4つの関節, 3軸
    Vector3[][] ACFFIFOStack;       //4つの関節, スタックする数
    int StackNum = 72;
    int[] StackCount;
    bool isStackFull = false;
    CalcACF calcACF;

    GameObject heart_particle_gobj, star_particle_gobj, Conf, groundLightsBase, RainbowBallBase; //パーティクル
    GameObject[] particle_finger;

    GameObject RSManager;
    SenseToolkitManager STKManager;

    Camera MainCam;
    //PXCMSenseManager


    #endregion


    // Use this for initialization
    void Start () {
        systemAdmin = SystemAdmin.Instance;   //システムインスタンス(シングルトン)の生成

        RSManager = GameObject.Find("SenseManager");
        STKManager = RSManager.GetComponent<SenseToolkitManager>();
        MainCam = GameObject.Find("ImageCam").GetComponent<Camera>();

        _lefthand_gobj = new GameObject[5];
        _lefthand_gobj[0] = GameObject.Find("first_left_wrist");
        _lefthand_gobj[1] = GameObject.Find("first_left_index-tip");
        _lefthand_gobj[2] = GameObject.Find("first_left_thumb-tip");
        _lefthand_gobj[3] = GameObject.Find("first_left_middle-tip");
        _lefthand_gobj[4] = GameObject.Find("first_left_pinky-tip");

        NRinitialize();     //基底遷移アルゴリズムによる加速度計算用配列の初期化
        ACFinitialize();    //自己相関関数計算用配列の初期化


        particle_finger = new GameObject[5];
        heart_particle_gobj = Resources.Load("Prehab/Hearts") as GameObject;
        groundLightsBase = Resources.Load("Prehab/groundLightsBase") as GameObject;
        star_particle_gobj = Resources.Load("Prehab/Stars") as GameObject;
        Conf = Resources.Load("Prehab/Confetti") as GameObject;
        RainbowBallBase = Resources.Load("Prehab/RainbowBallBase") as GameObject;


        generateParticle(heart_particle_gobj, 0, true);
    }

    //基底遷移アルゴリズムによる加速度計算用配列の初期化
    void NRinitialize()
    {
        nr = new NoiseReduction[5][];
        for (int i = 0; i < 5; i++)
        {
            nr[i] = new NoiseReduction[3];
            for (int j = 0; j < 3; j++)
            {
                nr[i][j] = new NoiseReduction();
                nr[i][j].Initialize(nr_nFIFO);
            }
        }
    }

    //自己相関関数計算用配列の初期化
    void ACFinitialize()
    {
        ACFValue = new double[5][];
        StackCount = new int[5];
        for (int i = 0; i < 5; i++)
        {
            ACFValue[i] = new double[3];
            StackCount[i] = 0;

            for (int j = 0; j < 3; j++) {
                ACFValue[i][j] = 0.0d;
            }
        }

        ACFFIFOStack = new Vector3[5][];
        for (int i = 0; i < 5; i++)
        {
            ACFFIFOStack[i] = new Vector3[StackNum];

            for (int j = 0; j < StackNum; j++) {
                ACFFIFOStack[i][j] = Vector3.zero;
            }
        }

        calcACF = new CalcACF();
    }

    // Update is called once per frame
    void Update () {
        /*
        PXCMHandData.IHand handData;
        STKManager.HandDataOutput.QueryHandData(PXCMHandData.AccessOrderType.ACCESS_ORDER_LEFT_HANDS, 0, out handData);
        PXCMPointF32 handPoint = handData.QueryMassCenterImage();

        Debug.Log("handPoint:"+handPoint.x +":" +handPoint.y);
        Vector3 handPoint3D = MainCam.ScreenToWorldPoint(new Vector3(-handPoint.x, -handPoint.y, MainCam.nearClipPlane));
        */


        //RealSenseからの直接のデータを取得
        _left_wrist_pos = _lefthand_gobj[0].transform.position;
        //_lefthand_gobj[0].transform.position = handPoint3D;
        //_left_wrist_pos = handPoint3D;
        _left_index_pos = _lefthand_gobj[1].transform.position;
        _left_thumb_pos = _lefthand_gobj[2].transform.position;
        _left_middle_pos = _lefthand_gobj[3].transform.position;
        _left_pinky_pos = _lefthand_gobj[4].transform.position;

        //wristを中心とした相対位置情報に変換
        left_index_pos = _left_index_pos - _left_wrist_pos;
        left_thumb_pos = _left_thumb_pos - _left_wrist_pos;
        left_middle_pos = _left_middle_pos - _left_wrist_pos;
        left_pinky_pos = _left_pinky_pos - _left_wrist_pos;

        #region EstimateAcc
        //基底遷移による加速度情報の推定
        //(注)wristのみ、カメラからの位置情報を用いる
        nr[0][0].Estimation((double) _left_wrist_pos.x);nr[0][1].Estimation((double)_left_wrist_pos.y);nr[0][2].Estimation((double)_left_wrist_pos.z);
        nr[1][0].Estimation((double)_left_index_pos.x); nr[1][1].Estimation((double)_left_index_pos.y); nr[1][2].Estimation((double)_left_index_pos.z);
        nr[2][0].Estimation((double)left_thumb_pos.x); nr[2][1].Estimation((double)left_thumb_pos.y); nr[2][2].Estimation((double)left_thumb_pos.z);
        nr[3][0].Estimation((double)left_middle_pos.x); nr[3][1].Estimation((double)left_middle_pos.y); nr[3][2].Estimation((double)left_middle_pos.z);
        nr[4][0].Estimation((double)left_pinky_pos.x); nr[4][1].Estimation((double)left_pinky_pos.y); nr[4][2].Estimation((double)left_pinky_pos.z);

        left_wrist_acc = new Vector3((float)nr[0][0].getD(), (float)nr[0][1].getD(), (float)nr[0][2].getD());
        left_index_acc = new Vector3((float)nr[1][0].getD(), (float)nr[1][1].getD(), (float)nr[1][2].getD());
        left_thumb_acc = new Vector3((float)nr[2][0].getD(), (float)nr[2][1].getD(), (float)nr[2][2].getD());
        left_middle_acc = new Vector3((float)nr[3][0].getD(), (float)nr[3][1].getD(), (float)nr[3][2].getD());
        left_pinky_acc = new Vector3((float)nr[4][0].getD(), (float)nr[4][1].getD(), (float)nr[4][2].getD());
        #endregion

        //パーティクル位置の指への追跡
        for (int i = 0; i < 5; i++) {
            if (particle_finger[i] != null) particle_finger[i].transform.position = _lefthand_gobj[i].transform.position;
        }

        //自己相関関数計算用のStackに加速度データをFIFOの形で入れる
        
        ACFStackIn(0, left_wrist_acc);
        ACFStackIn(1, left_index_acc);
        ACFStackIn(2, left_thumb_acc);
        ACFStackIn(3, left_middle_acc);
        ACFStackIn(4, left_pinky_acc);

        //ACFStackIn(0, _left_wrist_pos);

        if (isStackFull) {
            ACFValue[0] = calcACF.autocorr_vec3(ACFFIFOStack[0]);
            ACFValue[1] = calcACF.autocorr_vec3(ACFFIFOStack[1]);
            ACFValue[2] = calcACF.autocorr_vec3(ACFFIFOStack[2]);
        }

        //Debug.Log("acc_y:" + left_wrist_acc.y +"//ave_y_acc:" + CalcAVE(ACFFIFOStack[0]).y);
        //Debug.Log("ave_y_acc:" + CalcAVE(ACFFIFOStack[0]).y + "ACFValue[0][y]:(" + ACFValue[0][1] + ")");


        //手自体の動きによるエフェクト
        float _left_wristDiff_x = Mathf.Abs(_left_wrist_pos.x - past_left_wrist_pos.x);
        float _left_wristDiff_y = Mathf.Abs(_left_wrist_pos.y - past_left_wrist_pos.y);
        float _left_wristDiff_z = Mathf.Abs(_left_wrist_pos.z - past_left_wrist_pos.z);

        double sum__left_wrist_pos = Mathf.Pow(
            (Mathf.Pow(_left_wristDiff_x, 2) + Mathf.Pow(_left_wristDiff_y, 2) + Mathf.Pow(_left_wristDiff_z, 2)), (float)1/2);

        Debug.Log("S:" + sum__left_wrist_pos);
        if ( isStackFull && sum__left_wrist_pos > 4.5)
        {
            Debug.Log("SLASH!!:" + left_wrist_acc.y);
            generateParticle(star_particle_gobj, 0, false);
        }

        if (ACFValue[0][1] > 0.30 && Mathf.Abs(_left_wrist_pos.y - past_left_wrist_pos.y) > 0.3)

        {
            generateParticle(groundLightsBase, 0, false);
        }
        //Debug.Log("Diff:" + Mathf.Abs(_left_wrist_pos.y - past_left_middle_pos.y));

        //指の姿勢によるエフェクト
        float left_index_pos_angle = Vector3.Angle(left_index_pos, Vector3.up);
        float left_thumb_pos_angle = Vector3.Angle(left_thumb_pos, Vector3.up);

        Debug.Log("Angle:" + left_index_pos_angle + "," + left_thumb_pos_angle);

        if ((left_index_pos_angle < 15 /*|| left_index_pos_angle > 75*/)
            && left_thumb_pos_angle > 15) {
            Debug.Log("UpSign!!!");
            generateParticle(RainbowBallBase, 1, false);
        }


        past_left_wrist_pos = _left_wrist_pos;
        past_left_middle_pos = _left_middle_pos;
        past_left_index_pos = _left_index_pos;

    }

    //配列の平均を算出するメソッド
    Vector3 CalcAVE(Vector3[] input) {
        Vector3 ret_ave = Vector3.zero;
        Vector3 sum = Vector3.zero;

        for (int i = 0; i < input.Length; i++) {
            sum += input[i];
        }
        ret_ave = sum / input.Length;

        return ret_ave;
    }
 

    //自己相関関数計算用のStackに加速度データをFIFOの形で入れる関数
    void ACFStackIn(int jointNum, Vector3 data) {

        if (StackCount[jointNum] < StackNum)
        {
            ACFFIFOStack[jointNum][StackCount[jointNum]] = data;
            StackCount[jointNum]++;
            Debug.Log("StackCount["+ jointNum+"]:"+StackCount[jointNum]);
        }
        else {  //FIFOFull

            isStackFull = true;

            for (int i=StackNum;i>0;i--) {
                //Debug.Log("i:"+i);
                if (i == StackNum)
                {
                    ACFFIFOStack[jointNum][i - 1] = Vector3.zero;
                    ACFFIFOStack[jointNum][i - 1] = ACFFIFOStack[jointNum][i - 2];
                }
                else if (i == 1) {
                    ACFFIFOStack[jointNum][0] = data;
                }
                else {
                    ACFFIFOStack[jointNum][i - 1] = ACFFIFOStack[jointNum][i - 2];
                }

            }
        }
    }

    /// <summary>
    /// パーティクルを指定した指の位置へ生成する関数
    /// </summary>
    /// <param name="particle_gobj">発生させるパーティクル</param>
    /// <param name="trac">追跡する指(0:wrist ~ )</param>
    /// <param name="isContinue">追跡するかどうか</param>
    void generateParticle(GameObject particle_gobj, int trac, bool isContinue)
    {
        GameObject p_ins;

        switch (trac) {
            case 0:
                p_ins = (GameObject)Instantiate(particle_gobj, _left_wrist_pos, Quaternion.identity);
                if (isContinue) particle_finger[trac] = p_ins;      //追跡する場合、パーティクルを指に登録する
                //else Destroy(p_ins);
                break;
            case 1:
                p_ins = (GameObject)Instantiate(particle_gobj, _left_index_pos, Quaternion.identity);
                if (isContinue) particle_finger[trac] = p_ins;      //追跡する場合、パーティクルを指に登録する
                //else Destroy(p_ins);
                break;
            case 2:
                p_ins = (GameObject)Instantiate(particle_gobj, _left_thumb_pos, Quaternion.identity);
                if (isContinue) particle_finger[trac] = p_ins;      //追跡する場合、パーティクルを指に登録する
                //else Destroy(p_ins);
                break;
            case 3:
                p_ins = (GameObject)Instantiate(particle_gobj, _left_middle_pos, Quaternion.identity);
                if (isContinue) particle_finger[trac] = p_ins;      //追跡する場合、パーティクルを指に登録する
                //else Destroy(p_ins);
                break;
            case 4:
                p_ins = (GameObject)Instantiate(particle_gobj, _left_pinky_pos, Quaternion.identity);
                if (isContinue) particle_finger[trac] = p_ins;      //追跡する場合、パーティクルを指に登録する
                break;
            default:
                break;


        }

    }



    /// <summary>
    /// デバッグ用
    /// </summary>
    void OnGUI()
    {
        // GUIの見た目を変える。
        GUIStyle guiStyle = new GUIStyle();
        GUIStyleState styleState = new GUIStyleState();

        // GUI背景色のバックアップ
        Color backColor = GUI.backgroundColor;

        // GUI背景の色を設定
        GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);

        // 背景用テクスチャを設定
        styleState.background = Texture2D.whiteTexture;

        // テキストの色を設定
        styleState.textColor = Color.black;

        // スタイルの設定。
        guiStyle.normal = styleState;

        // ラベル表示
        //GUI.Label(new Rect(300, 100, 200, 200), "LABEL TEXT", guiStyle);

        // GUI背景色を元に戻す
        //GUI.backgroundColor = backColor;

        if (systemAdmin.getIsDebugMode() == true) {
            /*
            GUI.Label(new Rect(0, 0, 180, 30), "_wristpos:(" + _left_wrist_pos.x + "," + _left_wrist_pos.y + "," + _left_wrist_pos.z + ")", guiStyle);
            GUI.Label(new Rect(0, 30, 180, 30), "_thumbpos:(" + _left_thumb_pos.x + "," + _left_thumb_pos.y + "," + _left_thumb_pos.z + ")", guiStyle);
            GUI.Label(new Rect(0, 60, 180, 30), "_middlepos:(" + _left_middle_pos.x + "," + _left_middle_pos.y + "," + _left_middle_pos.z + ")", guiStyle);
            GUI.Label(new Rect(0, 90, 180, 30), "_pinkypos:(" + _left_pinky_pos.x + "," + _left_pinky_pos.y + "," + _left_pinky_pos.z + ")", guiStyle);
            */
            /*
            GUI.Label(new Rect(0, 120, 180, 30), 
                "wristAcc_X:" + left_wrist_acc.x.ToString("f2"), guiStyle);
            GUI.Label(new Rect(0, 150, 180, 30),
                "wristAcc_Y:" + left_wrist_acc.y.ToString("f2"), guiStyle);
            GUI.Label(new Rect(0, 180, 180, 30),
                "wristAcc_Z:" + left_wrist_acc.z.ToString("f2"), guiStyle);
                */
            /*
            GUI.Label(new Rect(0, 150, 180, 30), 
                "thumbAcc:(" + FloorFloat(left_thumb_acc.x,4).ToString("f2") + "," + FloorFloat(left_thumb_acc.y,4).ToString("f2") + "," + FloorFloat(left_thumb_acc.z,4).ToString("f2") + ")", guiStyle);
            GUI.Label(new Rect(0, 180, 180, 30), 
                "middleAcc:(" + FloorFloat(left_middle_acc.x,4).ToString("f2") + "," + FloorFloat(left_middle_acc.y,4).ToString("f2") + "," + FloorFloat(left_middle_acc.z,4).ToString("f2") + ")", guiStyle);
            GUI.Label(new Rect(0, 210, 180, 30), 
                "pinkyAcc:(" + FloorFloat(left_pinky_acc.x,4).ToString("f2") + "," + FloorFloat(left_pinky_acc.y,4).ToString("f2") + "," + FloorFloat(left_pinky_acc.z,4).ToString("f2") + ")", guiStyle);
            */
        }
        
    }

    public double FloorFloat(float d, int round)
    {
        float ret = 0.0f;
        int _int;

        _int = (int)(d * Mathf.Pow(10, round));

        if (Mathf.Abs(_int) > 0)
        {
            ret = (float)_int / (float)Mathf.Pow(10, round);

            return ret;
        }
        else
        {
            return 0.0f;
        }


    }


}
