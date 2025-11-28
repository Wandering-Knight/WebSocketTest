namespace UniversalModule.ObjectPool {
    public interface IObjectPool<T> {
        T GetObject();
        void RecycleObject(T obj);
        void Release();
    }
}
