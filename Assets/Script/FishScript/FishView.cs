// FishView.cs - 挂在 FishPrefab 上
using UnityEngine;

public class FishView : MonoBehaviour
{
    public int fishType;
    public string fishId;

    public void Initialize(string id, int type)
    {
        fishId = id;
        fishType = type;
        // 可根据 type 切换 sprite 或动画
        // e.g., GetComponent<SpriteRenderer>().sprite = GetSpriteByType(type);
    }

    public void UpdatePosition(float x, float y)
    {
        transform.position = new Vector3(x, y, 0f);
    }
}