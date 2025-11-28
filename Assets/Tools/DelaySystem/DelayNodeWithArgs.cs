
using System;

namespace UniversalModule.DelaySystem {
    public class DelayNodeWithArgs : DelayNodeBase {
        /// <summary>
        /// 回调参数
        /// </summary>
        internal EventArgs args;
        /// <summary>
        /// 带参数的回调事件
        /// </summary>
        internal EventHandler callback;

        internal override NodeType NodeType => NodeType.HasParameters;

        /// <summary>
        /// 调用回调事件
        /// </summary>
        internal override void Invoke() {
            callback?.Invoke(sender, args);
            Recycle();
        }
        /// <summary>
        /// 重置节点所有参数
        /// </summary>
        internal override void Reset() {
            args = null;
            sender = null;
            base.Reset();
        }
    }
}
