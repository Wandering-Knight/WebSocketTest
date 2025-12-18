using NativeWebSocket;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using DG.Tweening;  // 引入 DOTween

public class FishManager : MonoBehaviour
{
    /// <summary>
    /// 鱼的类型枚举
    /// </summary>
    private enum FishMType
    {
        xiaochouyu = 1,
        sahdingyu = 2,
        sahyu = 3,
    }

    /// <summary>
    /// 所有鱼的预制体字典
    /// </summary>
    private Dictionary<FishMType, GameObject> AllFishDic;

    private WebSocket webSocket;
    private readonly Dictionary<string, GameObject> activeFishes = new();
    private string partialMessage = ""; // 用于拼接分片
    private const string FISH_PREFAB_NAME = "FishPrefab";
    private const string FISH_SERVER_URL = "wss://slotgame.xin:8081";


    private bool InitialEnd;

    async void Start()
    {
        // 初始化鱼的预制体字典
        AllFishDic = new Dictionary<FishMType, GameObject>
        {
            { FishMType.xiaochouyu, Resources.Load<GameObject>("FishPrefabs/xiaochouyu") },
            { FishMType.sahdingyu, Resources.Load<GameObject>("FishPrefabs/sahdingyu") },
            { FishMType.sahyu, Resources.Load<GameObject>("FishPrefabs/sahyu") },
        };

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
                case "FISH_SYNC":
                    HandleFishSync(msg["FishType"]);
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

    void HandleFishSync(JSONNode fishTypeNode)
    {
        Debug.Log("Received fish sync data: " + fishTypeNode.ToString()); // 打印接收到的鱼群数据

        foreach (string type in fishTypeNode.Keys)
        {
            JSONArray groups = fishTypeNode[type].AsArray;
            for (int g = 0; g < groups.Count; g++)
            {
                JSONArray fishes = groups[g].AsArray;
                for (int i = 0; i < fishes.Count; i++)
                {
                    // 使用字段名来获取值
                    string fishId = fishes[i]["id"];  // 获取fishId
                    float x = fishes[i]["x"].AsFloat; // 获取x坐标
                    float y = fishes[i]["y"].AsFloat; // 获取y坐标

                    Debug.Log($"Received fish ID: {fishId}, X: {x}, Y: {y}"); // 打印调试信息

                    // 创建或更新鱼
                    CreateOrUpdateFish(type, fishId, x, y);
                }
            }
        }
    }


void CreateOrUpdateFish(string type, string fishId, float x, float y)
{
        // 检查是否已经存在该鱼，如果已存在，则更新位置
        if (activeFishes.TryGetValue(fishId, out GameObject existing))
        {
            // 先终止当前的动画
            existing.transform.DOKill();  // 销毁之前的动画

            // 获取目标位置
            Vector3 targetPosition = new Vector3(x, y, 0);

            // 使用 DOTween 来平滑过渡到目标位置
            float moveTime = 1f;  // 设置动画时间
            existing.transform.DOMove(targetPosition, moveTime).SetEase(Ease.Linear);
        }
        else
        {
            // 创建新的鱼
            GameObject fishPrefab = AllFishDic[(FishMType)int.Parse(type)];
            if (fishPrefab == null)
            {
                Debug.LogError($"❌ Fish prefab for type '{type}' not found!");
                return;
            }

            // 初始位置
            Vector3 initialPosition = new Vector3(x, y, 0);
            GameObject fishGo = Instantiate(fishPrefab, initialPosition, Quaternion.identity, transform);

            // 存储新创建的鱼
            activeFishes[fishId] = fishGo;

            // 使用 DOTween 为新创建的鱼添加平滑移动动画
            Vector3 targetPosition = new Vector3(x, y, 0);
            float moveTime = 1f;  // 设置动画时间
            fishGo.transform.DOMove(targetPosition, moveTime).SetEase(Ease.Linear);
        }
    }




// ====== 以下逻辑和之前一样 ======

void Update()
    {
#if UNITY_EDITOR || (!UNITY_WEBGL && !UNITY_IOS && !UNITY_ANDROID) // Editor 或 Standalone 平台需要手动 Dispatch
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
}
