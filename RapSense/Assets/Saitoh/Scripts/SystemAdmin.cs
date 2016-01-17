using UnityEngine;
using System.Collections;
/// <summary>
/// システムのバージョンなどを管理するクラス。シングルトン構造。
/// </summary>
public class SystemAdmin : MonoBehaviour {

    private static float system_version = 0.1f; //  システムのバージョン情報
    private static bool isDegubMode = true;     //デバッグモードのときはtrueにしてください

    private static SystemAdmin sInstance;

    private SystemAdmin () { // Private Constructor

        Debug.Log("Create SystemAdmin instance.");
    }

    public static SystemAdmin Instance {

        get {
            if (sInstance == null) sInstance = new SystemAdmin();
            return sInstance;
        }
    }

    public float getVersion(){
        return system_version;
    }

    public bool getIsDebugMode() {
        return isDegubMode;
    }

}
