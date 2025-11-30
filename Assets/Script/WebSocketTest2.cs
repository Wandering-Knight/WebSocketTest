using NativeWebSocket;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class WebSocketTest2 : MonoBehaviour
{
    // 👇 这些字段和方法对所有平台都需要，放外面！
    [SerializeField] private Transform msgParent;
    [SerializeField] private GameObject msgObj;


    private async void Start()
    {

        //测试代码//
        // --- 测试 FishServer (8081) ---
        string testUrl = "wss://slotgame.xin:8080";
        var testWS = new WebSocket(testUrl);

        testWS.OnOpen += () => {
            Debug.Log("✅ Connected to FishServer (8080)");
            testWS.SendText("{\"type\":\"PING\"}"); // 可选：发个消息
        };

        testWS.OnMessage += (data) => {
            string msg = System.Text.Encoding.UTF8.GetString(data);
            Debug.Log("🐟 FishServer says: " + msg);
        };

        testWS.OnError += (err) => Debug.LogError("FishServer error: " + err);
        testWS.OnClose += (code) => Debug.Log("FishServer closed: " + code);

        await testWS.Connect();
        Debug.Log("Connecting to Fishserver...");
    }



    //#if UNITY_WEBGL && !UNITY_EDITOR
    //    [System.Runtime.InteropServices.DllImport("__Internal")]
    //    private static extern void WebSocketConnect(string url);

    //    [System.Runtime.InteropServices.DllImport("__Internal")]
    //    private static extern bool WebSocketSend(string data);

    //    [System.Runtime.InteropServices.DllImport("__Internal")]
    //    private static extern void WebSocketClose();
    //#else
    //    private WebSocket ws;
    //    private System.Collections.Generic.Queue<string> messageQueue = new System.Collections.Generic.Queue<string>();
    //#endif


    //    private int index = 0;

    //    // ✅ 公共方法：所有平台都可以调用
    //    private void AddMsg(string msg)
    //    {
    //        Debug.Log(msg);
    //        try
    //        {
    //            var newMsg = Instantiate(msgObj, msgParent);
    //            newMsg.GetComponentInChildren<TextMeshProUGUI>().text = $"{++index}: {msg}";
    //        }
    //        catch (System.Exception e)
    //        {
    //            Debug.LogError("AddMsg error: " + e.Message);
    //        }
    //    }

    //#if !UNITY_WEBGL || UNITY_EDITOR
    //    // 非 WebGL 平台专用逻辑
    //    private void EnqueueMessage(string msg)
    //    {
    //        lock (messageQueue) messageQueue.Enqueue(msg);
    //    }

    //    private void WebSocketConnect(string wsUrl)
    //    {
    //        //ws = new WebSocket(wsUrl);
    //        //ws.SslConfiguration.ServerCertificateValidationCallback = (s, c, ch, e) => true; // 开发用
    //        //// ... 绑定事件 ...
    //        //ws.OnMessage += (s, e) => EnqueueMessage("Server: " + e.Data);
    //        //ws.ConnectAsync();
    //    }

    //    // ... 其他非 WebGL 方法（WebSocketSend, Close, Update 等）...
    //#endif

    //    void Start()
    //    {
    //        string wsUrl = "wss://slotgame.xin:8080";
    //#if UNITY_WEBGL && !UNITY_EDITOR
    //        WebSocketConnect(wsUrl);
    //#else
    //        WebSocketConnect(wsUrl);
    //#endif
    //    }

    //    // ✅ WebGL 回调：现在可以安全调用 AddMsg！
    //    public void OnMessage(string message)
    //    {
    //        Debug.Log("Received (WebGL): " + message);
    //        AddMsg("JS: " + message); // ✅ 现在没问题了！
    //    }

    //    public void OnClose(string reason)
    //    {
    //        Debug.Log("Closed (WebGL): " + reason);
    //        AddMsg("Closed: " + reason);
    //    }

    //    public void OnError(string error)
    //    {
    //        Debug.LogError("Error (WebGL): " + error);
    //        AddMsg("Error: " + error);
    //    }

    //#if !UNITY_WEBGL || UNITY_EDITOR
    //    void Update()
    //    {
    //        if (messageQueue.Count > 0)
    //        {
    //            lock (messageQueue)
    //            {
    //                while (messageQueue.Count > 0)
    //                {
    //                    AddMsg(messageQueue.Dequeue());
    //                }
    //            }
    //        }
    //    }

    //    // ... 其他非 WebGL 方法 ...
    //#endif
}