using UnityEngine;
using System.Collections;

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
    Vector3 _left_wrist_pos, _left_thumb_pos, _left_middle_pos, _left_pinky_pos;
    Vector3 left_thumb_pos, left_middle_pos, left_pinky_pos;
    GameObject _left_wrist, _left_thumb, _left_middle, _left_pinky; //元データ取得のためのGameObject
    Vector3 left_wrist_acc, left_thumb_acc, left_middle_acc, left_pinky_acc;

    //基底遷移アルゴリズムによる加速度計算用の関節位置の配列[4つの関節,3軸]
    // 0: wrist, 1:thumb, 2:middle, 3:pinky
    //(注)ただし、wristだけはカメラからの位置を用いる
    NoiseReduction[][] nr;
    private int nr_nFIFO = 10;  //何個前の位置情報から加速度を推定するか：10の場合2秒間の残像



    #endregion


    // Use this for initialization
    void Start () {
        systemAdmin = SystemAdmin.Instance;   //システムインスタンス(シングルトン)の生成

        _left_wrist = GameObject.Find("first_left_wrist");
        _left_thumb = GameObject.Find("first_left_thumb-tip");
        _left_middle = GameObject.Find("first_left_middle-tip");
        _left_pinky = GameObject.Find("first_left_pinky-tip");

        NRinitialize();     //基底遷移アルゴリズムによる加速度計算用配列の初期化
    }

    //基底遷移アルゴリズムによる加速度計算用配列の初期化
    void NRinitialize()
    {
        nr = new NoiseReduction[4][];
        for (int i = 0; i < 4; i++)
        {
            nr[i] = new NoiseReduction[3];
            for (int j = 0; j < 3; j++)
            {
                nr[i][j] = new NoiseReduction();
                nr[i][j].Initialize(nr_nFIFO);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        //RealSenseからの直接のデータを取得
        _left_wrist_pos = _left_wrist.transform.position;
        _left_thumb_pos = _left_thumb.transform.position;
        _left_middle_pos = _left_middle.transform.position;
        _left_pinky_pos = _left_pinky.transform.position;

        //wristを中心とした相対位置情報に変換
        left_thumb_pos = _left_thumb_pos - _left_wrist_pos;
        left_middle_pos = _left_middle_pos - _left_wrist_pos;
        left_pinky_pos = _left_pinky_pos - _left_wrist_pos;

        #region EstimateAcc
        //基底遷移による加速度情報の推定
        //(注)wristのみ、カメラからの位置情報を用いる
        nr[0][0].Estimation((double) _left_wrist_pos.x);nr[0][1].Estimation((double)_left_wrist_pos.y);nr[0][2].Estimation((double)_left_wrist_pos.z);
        nr[1][0].Estimation((double)left_thumb_pos.x); nr[1][1].Estimation((double)left_thumb_pos.y); nr[1][2].Estimation((double)left_thumb_pos.z);
        nr[2][0].Estimation((double)left_middle_pos.x); nr[2][1].Estimation((double)left_middle_pos.y); nr[2][2].Estimation((double)left_middle_pos.z);
        nr[3][0].Estimation((double)left_pinky_pos.x); nr[3][1].Estimation((double)left_pinky_pos.y); nr[3][2].Estimation((double)left_pinky_pos.z);

        left_wrist_acc = new Vector3((float)nr[0][0].getD(), (float)nr[0][1].getD(), (float)nr[0][2].getD());
        left_thumb_acc = new Vector3((float)nr[1][0].getD(), (float)nr[1][1].getD(), (float)nr[1][2].getD());
        left_middle_acc = new Vector3((float)nr[2][0].getD(), (float)nr[2][1].getD(), (float)nr[2][2].getD());
        left_pinky_acc = new Vector3((float)nr[3][0].getD(), (float)nr[3][1].getD(), (float)nr[3][2].getD());
        #endregion


    }

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
            GUI.Label(new Rect(0, 120, 180, 30), 
                "wristAcc_X:" + FloorFloat(left_wrist_acc.x,4).ToString("f2"), guiStyle);
            GUI.Label(new Rect(0, 150, 180, 30),
                "wristAcc_Y:" + FloorFloat(left_wrist_acc.y, 4).ToString("f2"), guiStyle);
            GUI.Label(new Rect(0, 180, 180, 30),
                "wristAcc_Z:" + FloorFloat(left_wrist_acc.z, 4).ToString("f2"), guiStyle);
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
