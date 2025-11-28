using System;
using UnityEngine;
using System.Collections.Generic;
using UniversalModule.ObjectPool;

namespace UniversalModule.DelaySystem {
    public static class DelayHelper {
        /// <summary>
        /// 全部延迟节点，用于遍历
        /// </summary>
        internal static List<DelayNodeBase> allNodes;
        /// <summary>
        /// 等待执行的延迟节点列表
        /// </summary>
        internal static Dictionary<object, List<DelayNodeBase>> delayNodes;
        /// <summary>
        /// 延迟节点列表对象池
        /// </summary>
        private static IObjectPool<List<DelayNodeBase>> nodeListPool;

        static DelayHelper() {
            DelaySequence.GetDelayListener = GetDelayNode;
            DelayNodeBase.RecycleNodeListener = RecycleNode;
            DelaySequence.GetDelayWithParametersListener = GetDelayNode;

            nodeListPool = new ObjectPoolOfStack<List<DelayNodeBase>>(30);

            delayNodes = new Dictionary<object, List<DelayNodeBase>>(20);
            allNodes = new List<DelayNodeBase>(20);
        }

        /// <summary>
        /// 执行一次延迟回调
        /// </summary>
        /// <param name="time">延迟等待多久</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static DelayNodeBase Delay(this UnityEngine.Object sender, float time, Action callback) {
            DelayNodeBase node = GetDelayNode(sender, time, callback);
            CacheNode(node);
            return node;
        }
        /// <summary>
        /// 执行一次延迟回调
        /// </summary>
        /// <param name="time">等待时间</param>
        /// <param name="callback">延迟回调事件</param>
        /// <param name="args">回调事件参数</param>
        /// <returns></returns>
        public static DelayNodeBase Delay(this UnityEngine.Object sender, float time, EventHandler callback, EventArgs args) {
            DelayNodeBase node = GetDelayNode(sender, time, callback, args);
            CacheNode(node);
            return node;
        }

        /// <summary>
        /// 执行一次延迟回调
        /// </summary>
        /// <param name="time">延迟等待多久</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        internal static DelayNodeBase Delay(object sender, float time, Action callback) {
            DelayNodeBase node = GetDelayNode(sender, time, callback);
            CacheNode(node);
            return node;
        }
        /// <summary>
        /// 执行一次延迟回调
        /// </summary>
        /// <param name="time">等待时间</param>
        /// <param name="callback">延迟回调事件</param>
        /// <param name="args">回调事件参数</param>
        /// <returns></returns>
        internal static DelayNodeBase Delay(object sender, float time, EventHandler callback, EventArgs args) {
            DelayNodeBase node = GetDelayNode(sender, time, callback, args);
            CacheNode(node);
            return node;
        }

        /// <summary>
        /// 获取一个无参数的延迟节点
        /// </summary>
        /// <param name="time">等待时间</param>
        /// <param name="callback">回调事件</param>
        /// <returns>无参数的延迟节点</returns>
        internal static DelayNodeBase GetDelayNode(object sender, float time, Action callback) {
            DelayNode node = new DelayNode();
            node.registerTime = Time.time;
            node.callback = callback;
            node.delayTime = time;
            node.sender = sender;
            return node;
        }
        /// <summary>
        /// 获取一个有参数的延迟节点
        /// </summary>
        /// <param name="time">延迟等待时间</param>
        /// <param name="callback">回调事件</param>
        /// <param name="parameters">回调事件参数</param>
        /// <returns>有参数的延迟节点</returns>
        internal static DelayNodeBase GetDelayNode(object sender, float time, EventHandler callback, EventArgs args) {
            DelayNodeWithArgs node = new DelayNodeWithArgs();
            node.registerTime = Time.time;
            node.callback = callback;
            node.delayTime = time;
            node.sender = sender;
            node.args = args;
            return node;
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        internal static void ClearCache() {
            foreach (var item in delayNodes.Values) {
                nodeListPool.RecycleObject(item);
                item.Clear();
            }
            delayNodes.Clear();
            allNodes.Clear();
        }

        /// <summary>
        /// 缓存节点
        /// </summary>
        /// <param name="node">延迟节点</param>
        private static void CacheNode(DelayNodeBase node) {
            if (delayNodes.ContainsKey(node.sender)) {
                delayNodes[node.sender].Add(node);
            } else {
                var nodeList = nodeListPool.GetObject();
                delayNodes.Add(node.sender, nodeList);
                delayNodes[node.sender].Add(node);
            }
            allNodes.Add(node);
        }
        /// <summary>
        /// 回收节点，将节点从列表移除并重置，节点无引用后由GC释放
        /// </summary>
        /// <param name="node">要回收的延迟节点</param>
        private static void RecycleNode(DelayNodeBase node) {
            if (node.sender != null && delayNodes.ContainsKey(node.sender)) {
                if (delayNodes[node.sender].Contains(node)) {
                    delayNodes[node.sender].Remove(node);
                    if (delayNodes[node.sender].Count <= 0) {
                        nodeListPool.RecycleObject(delayNodes[node.sender]);
                        delayNodes.Remove(node.sender);
                    }
                    allNodes.Remove(node);
                    node.Reset();
                }
            }
        }
    }
}
