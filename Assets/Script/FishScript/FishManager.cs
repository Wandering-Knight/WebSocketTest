using NativeWebSocket;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;

public class FishManager : MonoBehaviour
{
    private WebSocket webSocket;
    private readonly Dictionary<string, GameObject> activeFishes = new();
    private string partialMessage = ""; // 用于拼接分片

    private const string FISH_PREFAB_NAME = "FishPrefab";
    private const string FISH_SERVER_URL = "wss://slotgame.xin:8081";

    async void Start()
    {
        string roomId = RoomManager.Instance?.RoomInputField?.text?.Trim();
        if (string.IsNullOrEmpty(roomId))
        {
            Debug.LogError("❌ RoomID is empty! Make sure you joined a room first.");
            return;
        }

        webSocket = new WebSocket(FISH_SERVER_URL);

        webSocket.OnOpen += () =>
        {
            Debug.Log("✅ Connected to FishServer");
            var joinMsg = new JSONObject();
            joinMsg["type"] = "JOIN_FISH_ROOM";
            joinMsg["roomId"] = roomId;
            webSocket.SendText(joinMsg.ToString());
            Debug.Log($"📤 Sent JOIN_FISH_ROOM for room: {roomId}");
        };

        webSocket.OnMessage += OnWebSocketMessage; // 统一处理分片
        webSocket.OnError += (err) => Debug.LogError("🐟 FishServer error: " + err);
        webSocket.OnClose += (code) => Debug.Log($"🐟 FishServer closed (code: {code})");

        await webSocket.Connect();
    }

    // === 关键：处理分片消息 ===
    void OnWebSocketMessage(byte[] data)
    {
        string chunk = System.Text.Encoding.UTF8.GetString(data);
        partialMessage += chunk;
        Debug.Log(partialMessage);
        // 尝试解析完整 JSON
        while (!string.IsNullOrEmpty(partialMessage))
        {
            if (TryParseCompleteJson(ref partialMessage, out JSONNode msg))
            {
                ProcessMessage(msg);
            }
            else
            {
                // 还不完整，等待更多数据
                break;
            }
        }
    }

    bool TryParseCompleteJson(ref string buffer, out JSONNode json)
    {
        json = null;
        if (string.IsNullOrWhiteSpace(buffer)) return false;

        int openBraces = 0;
        int closeBraces = 0;
        bool inString = false;
        char prevChar = '\0';

        for (int i = 0; i < buffer.Length; i++)
        {
            char c = buffer[i];
            if (c == '"' && prevChar != '\\')
            {
                inString = !inString;
            }

            if (!inString)
            {
                if (c == '{') openBraces++;
                else if (c == '}') closeBraces++;
            }

            prevChar = c;

            // 找到一个完整对象
            if (openBraces > 0 && openBraces == closeBraces)
            {
                string completeJson = buffer.Substring(0, i + 1);
                try
                {
                    json = JSON.Parse(completeJson);
                    buffer = buffer.Substring(i + 1).TrimStart(); // 剩余部分留着下次处理
                    return true;
                }
                catch
                {
                    // 解析失败，继续找
                }
            }
        }
        return false;
    }

    void ProcessMessage(JSONNode msg)
    {
        try
        {
            string type = msg["type"];
            switch (type)
            {
                case "fishSnapshot":
                    HandleFishSnapshot(msg["data"].AsArray);
                    break;
                case "fishUpdate":
                    HandleFishUpdate(msg["data"].AsArray);
                    break;
                case "fishRemoved":
                    HandleFishRemoved(msg["id"]);
                    break;
                case "ERROR":
                    Debug.LogError("🐟 FishServer error: " + msg["message"]);
                    break;
                default:
                    Debug.LogWarning("🐟 Unknown message type: " + type);
                    break;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to process fish message: " + ex.Message + "\n" + msg);
        }
    }

    // ====== 以下逻辑和之前一样 ======
    void Update()
    {
#if UNITY_EDITOR || (!UNITY_WEBGL && !UNITY_IOS && !UNITY_ANDROID)
        // Editor 或 Standalone 平台需要手动 Dispatch
        if (webSocket?.State == WebSocketState.Open)
        {
            webSocket.DispatchMessageQueue();
        }
#endif
    }

    void OnDestroy()
    {
        foreach (var go in activeFishes.Values) Destroy(go);
        activeFishes.Clear();
        webSocket?.Close();
    }

    void HandleFishSnapshot(JSONArray snapshot)
    {
        foreach (var go in activeFishes.Values) Destroy(go);
        activeFishes.Clear();

        foreach (JSONNode fish in snapshot)
        {
            CreateOrUpdateFish(fish["id"], fish["type"], fish["x"], fish["y"]);
        }
        Debug.Log($"🐟 Received fishSnapshot with {snapshot.Count} fishes");
    }

    void HandleFishUpdate(JSONArray update)
    {
        var currentIds = new HashSet<string>();
        foreach (JSONNode fish in update)
        {
            string id = fish["id"];
            currentIds.Add(id);
            CreateOrUpdateFish(id, fish["type"], fish["x"], fish["y"]);
        }

        // 移除消失的鱼
        var toRemove = new List<string>();
        foreach (var id in activeFishes.Keys)
            if (!currentIds.Contains(id))
                toRemove.Add(id);

        foreach (var id in toRemove)
        {
            Destroy(activeFishes[id]);
            activeFishes.Remove(id);
        }
    }

    void HandleFishRemoved(string fishId)
    {
        if (activeFishes.TryGetValue(fishId, out GameObject go))
        {
            Destroy(go);
            activeFishes.Remove(fishId);
        }
    }

    void CreateOrUpdateFish(JSONNode id, JSONNode type, JSONNode x, JSONNode y)
    {
        string strId = id.Value;
        int intType = type.AsInt;
        float fx = x.AsFloat;
        float fy = y.AsFloat;

        if (activeFishes.TryGetValue(strId, out GameObject existing))
        {
            existing.GetComponent<FishView>()?.UpdatePosition(fx, fy);
        }
        else
        {
            GameObject prefab = Resources.Load<GameObject>(FISH_PREFAB_NAME);
            if (prefab == null)
            {
                Debug.LogError($"❌ Fish prefab '{FISH_PREFAB_NAME}' not found!");
                return;
            }

            GameObject fishGo = Instantiate(prefab, transform);
            FishView view = fishGo.GetComponent<FishView>() ?? fishGo.AddComponent<FishView>();
            view.Initialize(strId, intType);
            view.UpdatePosition(fx, fy);
            activeFishes[strId] = fishGo;
        }
    }
}