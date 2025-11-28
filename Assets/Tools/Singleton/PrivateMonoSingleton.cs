using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 继承自MonoBehaviour的单例类模板,类型是受保护的，不能供外部调用
/// </summary>
/// <typeparam name="T"></typeparam>
public class PrivateMonoSingleton<T> : MonoBehaviour where T : MonoBehaviour {
    private static T instance;
    protected static T Instance {
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