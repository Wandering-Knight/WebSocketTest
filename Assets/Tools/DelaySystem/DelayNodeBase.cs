using System;

namespace UniversalModule.DelaySystem {
    public enum NodeType {
        Invalid,
        NoParameters,
        HasParameters,
    }

    public class DelayNodeBase {
        /// <summary>
        /// 事件发送者
        /// </summary>
        internal object sender;
        /// <summary>
        /// 延迟时间
        /// </summary>
        internal float delayTime;
        /// <summary>
        /// 注册时间
        /// </summary>
        internal float registerTime;

        /// <summary>
        /// 节点类型
        /// </summary>
        internal virtual NodeType NodeType { get; }

        /// <summary>
        /// 节点回收事件
        /// </summary>
        internal static Action<DelayNodeBase> RecycleNodeListener;

        /// <summary>
        /// 调用回调事件
        /// </summary>
        internal virtual void Invoke() { throw new Exception("This interface has not yet been implemented."); }
        /// <summary>
        /// 重置节点所有参数
        /// </summary>
        internal virtual void Reset() {
            delayTime = 0;
            sender = null;
            registerTime = 0;
        }
        /// <summary>
        /// 回收该节点
        /// </summary>
        /// <exception cref="Exception"></exception>
        internal void Recycle() {
            if (RecycleNodeListener == null) {
                throw new Exception("Recycle event has not been registered.");
            }
            if (NodeType == NodeType.Invalid) {
                throw new Exception("Invalid delay node!");
            }
            RecycleNodeListener(this);
        }
    }
}
