using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 继承自MonoBehaviour的单例类模板
/// </summary>
/// <typeparam name="T"></typeparam>
public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour {
    private static T instance;
    public static T Instance {
        get {
            return instance;
        }
        private set {
            instance = value;
        }
    }

    protected virtual void Awake() {
        if (instance == null) {
            Instance = this as T;
        } else {
            Destroy(this);
        }
    }
    protected virtual void OnDestory() {
        Instance = null;
    }
}
