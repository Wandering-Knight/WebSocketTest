using NativeWebSocket;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FishManager : MonoBehaviour
{

    private WebSocket testWS;

    // Start is called before the first frame update
    async void Start()
    {
        //测试代码//
        // --- 测试 FishServer (8081) ---
        string testUrl = "wss://slotgame.xin:8081";
        testWS = new WebSocket(testUrl);

        testWS.OnOpen += () => {
            Debug.Log("✅ Connected to FishServer (8081)");
            //testWS.SendText("{\"type\":\"PING\"}"); // 可选：发个消息
        };

        testWS.OnMessage += (data) => {
            string msg = System.Text.Encoding.UTF8.GetString(data);
            Debug.Log("🐟 FishServer says: " + msg);
        };

        testWS.OnError += (err) => Debug.LogError("FishServer error: " + err);
        testWS.OnClose += (code) => Debug.Log("FishServer closed: " + code);

        await testWS.Connect();
        // 注意：这会并行连接，可能影响启动速度
        // 如果只是测试连通性，连上后可以立即关闭
        // await Task.Delay(1000);
        // testWS.Close();
        // ----------------------------
        ///////////
    }

    // Update is called once per frame
    void Update()
    {

#if UNITY_EDITOR
        if (testWS != null && testWS.State == WebSocketState.Open)
        {
            testWS.DispatchMessageQueue();
        }
#endif
    }
}
