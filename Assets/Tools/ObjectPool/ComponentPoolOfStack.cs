using UnityEngine;
using System.Collections.Generic;

namespace UniversalModule.ObjectPool {
    /// <summary>
    /// 栈结构实现的对象池，用于存放Unity实现的Object对象
    /// </summary>
    /// <typeparam name="T">继承自Unity Object的可实例化对象</typeparam>
    public class ComponentPoolOfStack<T> : PoolBuilder, IObjectPool<T> where T : Component, new() {
        /// <summary>
        /// 实例模板，对象池的所有资源都基于该模板创建
        /// </summary>
        private T template;
        /// <summary>
        /// 对象缓存池
        /// </summary>
        protected Stack<T> Pool;
        /// <summary>
        /// 用于获取对象池的容量
        /// </summary>
        public int Count { get { return Pool.Count; } }

        #region 构造函数
        public ComponentPoolOfStack(T template) {
            this.template = template;
            Pool = new Stack<T>(MAXCOUNT);
            for (int i = 0; i < MAXCOUNT; i++) {
                T newObj = Object.Instantiate(template, Parent);
                RecycleObject(newObj);
            }
        }
        public ComponentPoolOfStack(int count, T template) {
            MAXCOUNT = count;
            Pool = new Stack<T>();
            this.template = template;
            for (int i = 0; i < count; i++) {
                T newObj = Object.Instantiate(template, Parent);
                RecycleObject(newObj);
            }
        }
        public ComponentPoolOfStack(IEnumerable<T> cache) {
            if (cache != null) {
                template = cache.GetEnumerator().Current;
                Pool = new Stack<T>(cache);
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
                obj = Pool.Pop();

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
            obj.transform.rotation = Quaternion.identity;
            Pool.Push(obj);
        }
        /// <summary>
        /// 释放资源，清空缓存
        /// </summary>
        public void Release() {
            var count = Count;
            for (int i = 0; i < Count; i++) {
                Object.Destroy(Pool.Pop().gameObject);
            }
            Object.Destroy(template.gameObject);
            Pool = null;
        }
        #endregion
    }
}