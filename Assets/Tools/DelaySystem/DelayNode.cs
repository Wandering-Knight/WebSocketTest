using System;
namespace UniversalModule.DelaySystem {
    public class DelayNode : DelayNodeBase {
        /// <summary>
        /// 回调事件
        /// </summary>
        internal Action callback;
        internal override NodeType NodeType => NodeType.NoParameters;

        internal override void Invoke() {
            callback?.Invoke();
            Recycle();
        }
        internal override void Reset() {
            callback = null;
            base.Reset();
        }
    }
}