using NativeWebSocket;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UniversalModule.DelaySystem;

// 注意：SimpleJSON 不需要 using 命名空间（它在全局）

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    // --- UI 引用 ---
    public InputField RoomInputField;
    public Button CreateRoomButton;
    public Button JoinRoomButton;
    public Text StatusText;

    public Transform playerIconParent;
    public PlayerIcon playerIconPrefab;

    private List<PlayerIcon> playerIconsList = new List<PlayerIcon>();

    // --- WebSocket ---
    private WebSocket ws;
    //private WebSocket testWS;
    private string currentRoomId;
    private bool isWebSocketConnected = false;


    public string RoomID => RoomInputField.text;

    AsyncOperation asyncLoad;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);


    }
    private async Task Start()
    {

        //Debug.Log(1);
        CreateRoomButton.onClick.AddListener(OnCreateRoomClicked);
        JoinRoomButton.onClick.AddListener(OnJoinRoomClicked);

        string wsUrl = "wss://slotgame.xin:8080";
        ws = new WebSocket(wsUrl);

        ws.OnOpen += () =>
        {
            isWebSocketConnected = true;
            UpdateUI("Connected to server.");
        };

        ws.OnMessage += OnWebSocketMessage;
        ws.OnError += OnWebSocketError;
        ws.OnClose += OnWebSocketClose;

        await ws.Connect();
        UpdateUI("Connecting to server...");




    }
    int testLogCount = 0;
    void DebugTestMsg(byte[] msg)
    {
        Debug.Log($"{++testLogCount}  {System.Text.Encoding.UTF8.GetString(msg)}");
    }

    void OnCreateRoomClicked()
    {
        string roomId = RoomInputField.text.Trim();
        if (string.IsNullOrEmpty(roomId))
        {
            UpdateUI("Please enter a room name!");
            return;
        }
        SendJoinRoomRequest(roomId);
    }

    void OnJoinRoomClicked()
    {
        string roomId = RoomInputField.text.Trim();
        if (string.IsNullOrEmpty(roomId))
        {
            UpdateUI("Please enter a room name!");
            return;
        }
        SendJoinRoomRequest(roomId);
    }

    void SendJoinRoomRequest(string roomId)
    {
        if (!isWebSocketConnected)
        {
            UpdateUI("Error: Wait until connected.");
            return;
        }

        string playerId = "user_" + UnityEngine.Random.Range(10000, 99999);
        string username = "Player" + UnityEngine.Random.Range(1, 100);

        string json = $"{{\"type\":\"JOIN_ROOM\",\"roomId\":\"{roomId}\",\"playerId\":\"{playerId}\",\"username\":\"{username}\"}}";
        ws.SendText(json);
        UpdateUI($"Joining room: {roomId}...");
    }

    void OnWebSocketMessage(byte[] data)
    {
        string msg = System.Text.Encoding.UTF8.GetString(data);
        Debug.Log("Received from server: " + msg);

        try
        {
            var json = JSON.Parse(msg);
            if (json == null || json["type"] == null || json["type"].IsNull)
            {
                Debug.LogWarning("Invalid JSON: missing 'type' field");
                return;
            }

            string type = json["type"].Value;

            switch (type)
            {
                case "JOIN_SUCCESS":
                    StartCoroutine(HandleJoinSuccess(json));
                    break;
                case "ROOM_UPDATE":
                    HandleRoomUpdate(json);
                    break;
                case "ERROR":
                    HandleError(json);
                    break;
                default:
                    Debug.LogWarning("Unknown message type: " + type);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to parse message: " + e.Message + "\nRaw data: " + msg);
            UpdateUI("Message parse error.");
        }
    }

    IEnumerator HandleJoinSuccess(JSONNode json)
    {
        string roomId = json["roomId"]?.Value ?? "unknown";
        ParseAndShowPlayers(json["players"]);
        currentRoomId = roomId;
        //UpdateUI($"Joined room: {currentRoomId}");
        UpdateUI($"Joined room: {currentRoomId} \n 即将进入游戏");
        yield return new WaitForSeconds(3);
        EnterGameScene();
    }

    void HandleRoomUpdate(JSONNode json)
    {
        ParseAndShowPlayers(json["players"]);
    }

    void HandleError(JSONNode json)
    {
        string message = json["message"]?.Value ?? "Unknown error";
        UpdateUI($"Error: {message}");
    }

    void ParseAndShowPlayers(JSONNode playersNode)
    {
        var players = new List<PlayerInfo>();

        if (playersNode == null || playersNode.IsNull || !playersNode.IsArray)
        {
            UpdatePlayerList(players.ToArray());
            return;
        }

        var array = playersNode.AsArray;
        if (array == null)
        {
            UpdatePlayerList(players.ToArray());
            return;
        }

        foreach (JSONNode playerNode in array)
        {
            if (playerNode != null && !playerNode.IsNull && playerNode.IsObject)
            {
                string playerId = "";
                string username = "";

                var idNode = playerNode["playerId"];
                if (idNode != null && !idNode.IsNull)
                    playerId = idNode.Value;

                var nameNode = playerNode["username"];
                if (nameNode != null && !nameNode.IsNull)
                    username = nameNode.Value;

                if (!string.IsNullOrEmpty(playerId) || !string.IsNullOrEmpty(username))
                {
                    players.Add(new PlayerInfo
                    {
                        playerId = playerId,
                        username = username
                    });
                }
            }
        }

        UpdatePlayerList(players.ToArray());
    }

    void OnWebSocketError(string errorMsg)
    {
        Debug.LogError("WebSocket error: " + errorMsg);
        UpdateUI("Connection failed: " + errorMsg);
    }

    void OnWebSocketClose(WebSocketCloseCode code)
    {
        isWebSocketConnected = false;
        Debug.Log("WebSocket closed, code: " + code);
        UpdateUI("Connection closed.");
    }

    void UpdatePlayerList(PlayerInfo[] players)
    {
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

    // ================== 数据类 ==================
    [Serializable]
    public class PlayerInfo
    {
        public string playerId;
        public string username;
    }

    // ⚠️ 必须调用！否则收不到消息（NativeWebSocket 要求）
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnCreateRoomClicked();
        }

#if UNITY_EDITOR
        if (ws != null && ws.State == WebSocketState.Open)
        {
            ws.DispatchMessageQueue();
        }

#endif
    }

    private void EnterGameScene()
    {
        asyncLoad = SceneManager.LoadSceneAsync("GameScene");
        //asyncLoad.allowSceneActivation = false; // 防止自动激活
        //Debug.Log("Entering Game Scene...");
        asyncLoad.allowSceneActivation = true;
    }
}