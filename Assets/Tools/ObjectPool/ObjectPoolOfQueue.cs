using System.Collections.Generic;

namespace UniversalModule.ObjectPool {
    /// <summary>
    /// 队列结构实现的对象池，用于存放非Unity实现的普通类型
    /// </summary>
    /// <typeparam name="T">非Unity实现的class类型</typeparam>
    public class ObjectPoolOfQueue<T> : PoolBuilder, IObjectPool<T> where T : class, new() {
        /// <summary>
        /// 对象缓存池
        /// </summary>
        protected Queue<T> Pool;
        /// <summary>
        /// 用于获取对象池的容量
        /// </summary>
        public int Count { get { return Pool.Count; } }

        #region 构造函数
        public ObjectPoolOfQueue() {
            Pool = new Queue<T>(MAXCOUNT);
            for (int i = 0; i < MAXCOUNT; i++) {
                T newObj = new T();
                RecycleObject(newObj);
            }
        }
        public ObjectPoolOfQueue(int count) {
            MAXCOUNT = count;
            Pool = new Queue<T>(count);
            for (int i = 0; i < count; i++) {
                T newObj = new T();
                RecycleObject(newObj);
            }
        }
        public ObjectPoolOfQueue(IEnumerable<T> cache) {
            if (cache != null) {
                Pool = new Queue<T>(cache);
                MAXCOUNT = Pool.Count;
            } else {
                Pool = new Queue<T>(MAXCOUNT);
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
            if (Pool == null) {
                throw new System.Exception("对象池尚未初始化!");
            }

            T obj = null;

            if (Pool.Count <= 0)
                obj = new T();
            else
                obj = Pool.Dequeue();

            return obj;
        }
        /// <summary>
        /// 回收对象，如果对象池没有初始化或者超出最大限制将直接销毁该对象
        /// </summary>
        /// <param name="obj">被回收的对象</param>
        public void RecycleObject(T obj) {
            if (Pool == null || Pool.Count > MAXCOUNT)
                return;
            Pool.Enqueue(obj);
        }
        /// <summary>
        /// 释放资源，清空缓存
        /// </summary>
        public void Release() {
            Pool.Clear();
            Pool = null;
        }
        #endregion
    }
}
