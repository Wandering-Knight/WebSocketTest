using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using NativeWebSocket;
using SimpleJSON;

// 注意：SimpleJSON 不需要 using，它定义在全局命名空间

public class RoomManager : MonoBehaviour
{
    // --- UI 引用 ---
    public InputField RoomInputField;           // 标准 UnityEngine.UI.InputField
    public Button CreateRoomButton;
    public Button JoinRoomButton;
    public Text StatusText;

    public Transform playerIconParent;
    public PlayerIcon playerIconPrefab;

    private List<PlayerIcon> playerIconsList = new List<PlayerIcon>();

    // --- WebSocket ---
    private WebSocket ws;
    private string currentRoomId;
    private bool isWebSocketConnected = false;

    void Start()
    {
        CreateRoomButton.onClick.AddListener(OnCreateRoomClicked);
        JoinRoomButton.onClick.AddListener(OnJoinRoomClicked);

        string wsUrl = "wss://slotgame.xin:8080";
        ws = new WebSocket(wsUrl);

        ws.OnOpen += () =>
        {
            isWebSocketConnected = true;
            UpdateUI("✅ 已连接到服务器");
        };

        ws.OnMessage += OnWebSocketMessage;
        ws.OnError += OnWebSocketError;
        ws.OnClose += OnWebSocketClose;

        ws.Connect();
        UpdateUI("正在连接服务器...");
    }

    void OnCreateRoomClicked()
    {
        string roomId = RoomInputField.text.Trim();
        if (string.IsNullOrEmpty(roomId))
        {
            UpdateUI("请输入房间名称！");
            return;
        }
        SendJoinRoomRequest(roomId);
    }

    void OnJoinRoomClicked()
    {
        string roomId = RoomInputField.text.Trim();
        if (string.IsNullOrEmpty(roomId))
        {
            UpdateUI("请输入房间名称！");
            return;
        }
        SendJoinRoomRequest(roomId);
    }

    void SendJoinRoomRequest(string roomId)
    {
        if (!isWebSocketConnected)
        {
            UpdateUI("❌ 请等待连接成功后再操作");
            return;
        }

        string playerId = "user_" + UnityEngine.Random.Range(10000, 99999);
        string username = "Player" + UnityEngine.Random.Range(1, 100);

        // 手动构造 JSON 字符串（或用 SimpleJSON 构建，这里简单拼接）
        string json = $"{{\"type\":\"JOIN_ROOM\",\"roomId\":\"{roomId}\",\"playerId\":\"{playerId}\",\"username\":\"{username}\"}}";
        ws.SendText(json);
        UpdateUI($"正在加入房间: {roomId}...");
    }

    void OnWebSocketMessage(byte[] data)
    {
        string msg = System.Text.Encoding.UTF8.GetString(data);
        Debug.Log("📡 收到服务器消息: " + msg);

        try
        {
            var json = JSON.Parse(msg);
            if (json == null || !json.HasKey("type"))
            {
                Debug.LogWarning("无效的 JSON 消息");
                return;
            }

            string type = json["type"];

            switch (type)
            {
                case "JOIN_SUCCESS":
                    HandleJoinSuccess(json);
                    break;
                case "ROOM_UPDATE":
                    HandleRoomUpdate(json);
                    break;
                case "ERROR":
                    HandleError(json);
                    break;
                default:
                    Debug.LogWarning("未知消息类型: " + type);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("解析消息失败: " + e.Message + "\n原始数据: " + msg);
            UpdateUI("❌ 消息解析失败");
        }
    }

    void HandleJoinSuccess(JSONNode json)
    {
        string roomId = json["roomId"].Value; // 👈 也建议加 .Value
        var playersArray = json["players"].AsArray;

        var players = new List<PlayerInfo>();
        foreach (var playerNode in playersArray)
        {
            players.Add(new PlayerInfo
            {
                playerId = playerNode.Key,
                username = playerNode.Value
            });
        }

        currentRoomId = roomId;
        UpdateUI($"🎉 成功进入房间: {currentRoomId}");
        UpdatePlayerList(players.ToArray());
    }

    void HandleRoomUpdate(JSONNode json)
    {
        var playersArray = json["players"].AsArray;
        var players = new List<PlayerInfo>();
        foreach (var playerNode in playersArray)
        {
            players.Add(new PlayerInfo
            {
                playerId = playerNode.Key,
                username = playerNode.Value
            });
        }
        UpdatePlayerList(players.ToArray());
    }

    void HandleError(JSONNode json)
    {
        string message = json["message"] ?? "未知错误";
        UpdateUI($"❌ 错误: {message}");
    }

    void OnWebSocketError(string errorMsg)
    {
        Debug.LogError("WebSocket 错误: " + errorMsg);
        UpdateUI("❌ 连接失败: " + errorMsg);
    }

    void OnWebSocketClose(WebSocketCloseCode code)
    {
        isWebSocketConnected = false;
        Debug.Log("WebSocket 连接关闭，代码: " + code);
        UpdateUI("🔌 连接已断开");
    }

    void UpdatePlayerList(PlayerInfo[] players)
    {
        // 清理旧图标
        foreach (var icon in playerIconsList)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }
        playerIconsList.Clear();

        if (players == null) return;

        foreach (var p in players)
        {
            var playerIcon = Instantiate(playerIconPrefab, playerIconParent);
            playerIcon.transform.localScale = Vector3.one;
            playerIcon.SetPlayerIconShow(p.username, p.playerId);
            playerIconsList.Add(playerIcon);
        }
    }

    void UpdateUI(string text)
    {
        if (StatusText != null)
            StatusText.text = text;
    }

    void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
        }
    }

    // ================== 数据类（仅用于前端组织数据）==================

    [Serializable]
    public class PlayerInfo
    {
        public string playerId;
        public string username;
    }


    void Update()
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            ws.DispatchMessageQueue();
        }
    }
}