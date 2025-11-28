using UnityEngine;
using System.Collections.Generic;

namespace UniversalModule.ObjectPool {
    /// <summary>
    /// 队列结构实现的对象池，用于存放Unity实现的Object对象
    /// </summary>
    /// <typeparam name="T">继承自Unity Object的可实例化对象</typeparam>
    public class ComponentPoolOfQueue<T> : PoolBuilder, IObjectPool<T> where T : Component, new() {
        /// <summary>
        /// 实例模板，对象池的所有资源都基于该模板创建
        /// </summary>
        private T template;
        /// <summary>
        /// 对象缓存池
        /// </summary>
        protected Queue<T> Pool;
        /// <summary>
        /// 用于获取对象池的容量
        /// </summary>
        public int Count { get { return Pool.Count; } }

        #region 构造函数
        public ComponentPoolOfQueue(T template) {
            this.template = template;
            Pool = new Queue<T>(MAXCOUNT);
            for (int i = 0; i < MAXCOUNT; i++) {
                T newObj = Object.Instantiate(this.template, Parent);
                RecycleObject(newObj);
            }
        }
        public ComponentPoolOfQueue(int count, T template) {
            MAXCOUNT = count;
            this.template = template;
            Pool = new Queue<T>(count);
            for (int i = 0; i < count; i++) {
                T newObj = Object.Instantiate(template, Parent);
                RecycleObject(newObj);
            }
        }
        public ComponentPoolOfQueue(IEnumerable<T> cache) {
            if (cache != null) {
                template = cache.GetEnumerator().Current;
                Pool = new Queue<T>(cache);
                MAXCOUNT = Pool.Count;
            } else {
                throw new System.Exception("无法使用空缓存创建对象池");
            }
        }
        #endregion

        #region 接口实现
        /// <summary>
        /// 获取对象
        /// </summary>
        /// <returns>T 类型的缓存对象</returns>
        /// <exception cref="System.Exception">对象池没有初始化</exception>
        public T GetObject() {
            if (Pool == null || template == null) {
                throw new System.Exception("对象池尚未初始化!");
            }
            T obj = null;

            if (Pool.Count <= 0)
                obj = Object.Instantiate(template, Parent);
            else
                obj = Pool.Dequeue();

            obj.gameObject.SetActive(true);
            return obj;
        }
        /// <summary>
        /// 回收对象，如果对象池没有初始化或者超出最大限制将直接销毁该对象
        /// </summary>
        /// <param name="obj">被回收的对象</param>
        public void RecycleObject(T obj) {
            if (Pool == null || Pool.Count >= MAXCOUNT) {
                Object.Destroy(obj.gameObject);
                return;
            }
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(Parent);
            obj.transform.localPosition = Vector3.zero;
            Pool.Enqueue(obj);
        }
        /// <summary>
        /// 释放资源，清空缓存
        /// </summary>
        public void Release() {
            var count = Count;
            for (int i = 0; i < Count; i++) {
                Object.Destroy(Pool.Dequeue().gameObject);
            }
            Object.Destroy(template.gameObject);
            Pool = null;
        }
        #endregion
    }
}
