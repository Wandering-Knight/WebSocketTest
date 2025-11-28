using System;
using UnityEngine;
using System.Collections.Generic;

namespace UniversalModule.DelaySystem {
    /// <summary>
    /// 延迟序列，由多个延迟节点组成，根据添加顺序逐一执行
    /// </summary>
    public class DelaySequence {
        /// <summary>
        /// 延迟序列
        /// </summary>
        private Queue<DelayNodeBase> sequence;
        /// <summary>
        /// 添加无参数延迟节点事件
        /// </summary>
        internal static Func<object, float, Action, DelayNodeBase> GetDelayListener;
        /// <summary>
        /// 获取有参数延迟节点事件
        /// </summary>
        internal static Func<object, float, EventHandler, EventArgs, DelayNodeBase> GetDelayWithParametersListener;

        public DelaySequence() {
            sequence = new Queue<DelayNodeBase>();
        }
        /// <summary>
        /// 添加无参数的延迟节点
        /// </summary>
        /// <param name="time">当前节点需要等待的时间</param>
        /// <param name="callback">延迟回调事件</param>
        /// <exception cref="Exception">若无法得到新节点，将会抛出该异常</exception>
        public void AddDelay(object sender, float time, Action callback) {
            if (GetDelayListener == null) {
                throw new Exception(string.Format("Delay Sequence 创建节点失败，{0}事件尚未注册", GetDelayListener.ToString()));
            }
            DelayNodeBase node = GetDelayListener(sender, time, callback);
            sequence.Enqueue(node);
        }
        /// <summary>
        /// 添加有参数的延迟节点
        /// </summary>
        /// <param name="time">当前节点需要等待的时间</param>
        /// <param name="callback">延迟回调事件</param>
        /// <param name="obj">回调事件参数</param>
        /// <exception cref="Exception">若无法得到新节点，将会抛出该异常</exception>
        public void AddDelay(object sender, float time, EventHandler callback, EventArgs args) {
            if (GetDelayWithParametersListener == null) {
                throw new Exception(string.Format("Delay Sequence 创建节点失败，{0}事件尚未注册", GetDelayWithParametersListener.ToString()));
            }
            DelayNodeBase node = GetDelayWithParametersListener(sender, time, callback, args);
            sequence.Enqueue(node);
        }
        /// <summary>
        /// 从序列中读取一个延迟节点
        /// </summary>
        /// <returns>接下来要执行的延迟节点</returns>
        internal DelayNodeBase GetNode() {
            if (sequence.Count > 0) {

                return sequence.Peek();
            }
            return null;
        }
        /// <summary>
        /// 完成一个延迟节点
        /// </summary>
        public void FinishOneNode() {
            DelayNodeBase currNode = sequence.Dequeue();
            if (currNode != null) {
                try {
                    currNode.Invoke();
                } catch (Exception e) {
                    Debug.LogWarning("DelayCallback:  " + e.Message);
                }
            }
            DelayNodeBase newNode = GetNode();
            if (newNode != null) {
                newNode.registerTime = Time.time;
            }
        }
        /// <summary>
        /// 清理所有的延迟节点
        /// </summary>
        internal void Clear() {
            int count = sequence.Count;
            for (int i = 0; i < count; i++) {
                DelayNodeBase currNode = sequence.Dequeue();
                if (currNode != null) {
                    currNode.Recycle();
                }
            }
        }
    }
}
