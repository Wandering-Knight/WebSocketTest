using System;
using System.Threading.Tasks;
using NativeWebSocket;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class WebSocketTest3 : MonoBehaviour
{
    [SerializeField] private Transform msgParent;
    [SerializeField] private GameObject msgObj;

    [SerializeField] private TextMeshPro requestText;
    private int index = 0;

    private WebSocket websocket;

    void Start()
    {
        string wsUrl = "wss://slotgame.xin:8081";
        ConnectToWebSocket(wsUrl);
    }

    private async void ConnectToWebSocket(string url)
    {
        websocket = new WebSocket(url);

        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket connection opened!");
            AddMsg("Connected to server");
        };

        websocket.OnMessage += (byte[] data) =>
        {
            // NativeWebSocket 默认接收 byte[]，我们转成字符串
            string message = System.Text.Encoding.UTF8.GetString(data);
            Debug.Log("Received: " + message);
            AddMsg("Server: " + message);
        };

        websocket.OnError += (string errorMsg) =>
        {
            Debug.LogError("WebSocket error: " + errorMsg);
            AddMsg("Error: " + errorMsg);
        };

        websocket.OnClose += (WebSocketCloseCode closeCode) =>
        {
            Debug.Log("WebSocket closed with code: " + closeCode);
            AddMsg("Connection closed: " + closeCode);
        };

        // 开始连接（必须调用）
        await websocket.Connect();
    }

    // 可选：发送消息的方法（如果你需要）
    public async void SendMessage(string message)
    {
        if (websocket?.State == WebSocketState.Open)
        {
            await websocket.SendText(message);
            Debug.Log("Sent: " + message);
        }
        else
        {
            Debug.LogWarning("WebSocket is not open, cannot send message.");
        }
    }

    int requestCount = 0;

    private void AddMsg(string msg)
    {
        Debug.Log(msg);
        try
        {
            //var newMsg = Instantiate(msgObj, msgParent);
            //newMsg.GetComponentInChildren<TextMeshProUGUI>().text = $"{++index}: {msg}";
            requestText.text = $"{++requestCount} :  {msg}";
        }
        catch (Exception e)
        {
            Debug.LogError("AddMsg error: " + e.Message);
        }
    }

    // 确保在退出时关闭连接
    private async void OnDestroy()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }

    // NativeWebSocket 需要每帧调用 Dispatch() 来处理消息（非常重要！）
    void Update()
    {
#if UNITY_EDITOR

        if (websocket?.State == WebSocketState.Open)
        {
            // 必须调用，否则收不到消息！
            websocket.DispatchMessageQueue();
        }
#endif
    }
}