using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 不继承MonoBehaviour的单例类模板
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : class,new() {
    private static T instance;
    public static T Instance {
        get {
            return instance;
        }
        private set {
            instance = value;
        }
    }

    public Singleton() {
        if (instance == null) {
            Instance = this as T;
        }
    }
}
