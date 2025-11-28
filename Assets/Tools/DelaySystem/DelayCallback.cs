using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniversalModule.DelaySystem {
    public enum DelayType {
        All,
        Delay,
        Sequence,
        Coroutine,
    }

    /// <summary>
    /// 延迟回调系统，用于将一个回调函数延迟一定时间后执行
    /// </summary>
    public class DelayCallback : PrivateMonoSingleton<MonoBehaviour> {
        /// <summary>
        /// 等待执行的延迟序列列表
        /// </summary>
        private static List<DelaySequence> delaySequences = new List<DelaySequence>(20);

        #region 私有API
        private void Update() {
            RefreshDelayNode();
            RefreshDelaySequence();
        }
        /// <summary>
        /// 刷新延迟节点，每帧更新
        /// </summary>
        private void RefreshDelayNode() {
            if (DelayHelper.allNodes.Count > 0) {
                for (int i = 0; i < DelayHelper.allNodes.Count;) {
                    DelayNodeBase node = DelayHelper.allNodes[i];
                    if (Time.time - node.registerTime >= node.delayTime) {
                        try {
                            node.Invoke();
                        } catch (Exception e) {
                            Debug.LogWarning("DelayCallback:  " + e.Message);
                        }
                    } else {
                        i++;
                    }
                }
            }
        }
        /// <summary>
        /// 刷新延迟序列，每帧更新
        /// </summary>
        private void RefreshDelaySequence() {
            if (delaySequences.Count > 0) {
                for (int i = 0; i < delaySequences.Count;) {
                    DelaySequence sequence = delaySequences[i];
                    DelayNodeBase node = sequence.GetNode();
                    if (node != null) {
                        if (Time.time - node.registerTime >= node.delayTime) {
                            sequence.FinishOneNode();
                        }
                        i++;
                    } else {
                        delaySequences.Remove(sequence);
                        sequence.Clear();
                    }
                }
            }
        }
        #endregion

        #region 公共API
        /// <summary>
        /// 执行一次延迟回调
        /// </summary>
        /// <param name="time">延迟等待多久</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static DelayNodeBase Delay(object sender, float time, Action callback) {
            return DelayHelper.Delay(sender, time, callback);
        }
        /// <summary>
        /// 执行一次延迟回调
        /// </summary>
        /// <param name="time">等待时间</param>
        /// <param name="callback">延迟回调事件</param>
        /// <param name="args">回调事件参数</param>
        /// <returns></returns>
        public static DelayNodeBase Delay(object sender, float time, EventHandler callback, EventArgs args) {
            return DelayHelper.Delay(sender, time, callback, args);
        }

        /// <summary>
        /// 取消一个延迟回调
        /// </summary>
        /// <param name="node">需要取消的延迟节点</param>
        public static void StopDelay(DelayNodeBase node) {
            if (node != null && node.sender != null)
            {
                node.Recycle();
            }
        }
        /// <summary>
        /// 取消一个对象身上开启的所有延迟回调
        /// </summary>
        /// <param name="sender">延迟事件调用者</param>
        public static void StopAllDelayForObject(object sender) {
            if (DelayHelper.delayNodes.ContainsKey(sender)) {
                int count = DelayHelper.delayNodes[sender].Count;
                for (int i = 0; i < count; i++) {
                    if (DelayHelper.delayNodes[sender][0] != null) {
                        StopDelay(DelayHelper.delayNodes[sender][0]);
                    } else {
                        DelayHelper.delayNodes[sender].RemoveAt(0);
                    }
                }
            }
        }
        /// <summary>
        /// 取消所有延迟回调
        /// </summary>
        /// <param name="type">要需要的类型</param>
        public static void ClearAll(DelayType type = DelayType.All) {
            if (type == DelayType.Delay || type == DelayType.All) {
                if (DelayHelper.allNodes.Count > 0) {
                    int count = DelayHelper.allNodes.Count;
                    for (int i = 0; i < count; i++) {
                        if (DelayHelper.allNodes[0] != null) {
                            StopDelay(DelayHelper.allNodes[0]);
                        } else {
                            DelayHelper.allNodes.RemoveAt(0);
                        }
                    }
                    DelayHelper.ClearCache();
                }
            }
            if (type == DelayType.Sequence || type == DelayType.All) {
                for (int i = 0; i < delaySequences.Count; i++) {
                    DelaySequence sequence = delaySequences[i];
                    sequence.Clear();
                }
                delaySequences.Clear();
            }
            if (type == DelayType.Coroutine || type == DelayType.All) {
                Instance.StopAllCoroutines();
            }
        }
        /// <summary>
        /// 取消一段延迟序列
        /// </summary>
        /// <param name="sequence">要取消的延迟序列</param>
        public static void StopSequence(DelaySequence sequence) {
            if (delaySequences.Contains(sequence)) {
                delaySequences.Remove(sequence);
                sequence.Clear();
            }
        }
        /// <summary>
        /// 执行一段延迟序列
        /// </summary>
        /// <param name="sequence">要开始执行的序列</param>
        public static void ExecuteSequence(DelaySequence sequence) {
            delaySequences.Add(sequence);
        }
        /// <summary>
        /// 开启一个协程
        /// </summary>
        /// <param name="enumerator">协程需要的迭代器</param>
        /// <returns>Unity 的 Coroutine 节点</returns>
        public static Coroutine BeginCoroutine(IEnumerator enumerator) {
            return Instance.StartCoroutine(enumerator);
        }
        /// <summary>
        /// 终止一个协程
        /// </summary>
        /// <param name="coroutine">要终止的协程节点</param>
        public static void AbortCoroutine(Coroutine coroutine) {
            if (coroutine != null) {
                Instance.StopCoroutine(coroutine);
            }
        }
        #endregion
    }
}
