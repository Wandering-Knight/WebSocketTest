using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerIcon : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameTmp;

    [SerializeField] private TextMeshProUGUI playerIDTmp;
    public void SetPlayerIconShow(string name, string playerID)
    {
        playerNameTmp.text = name;
        playerIDTmp.text = playerID;
    }
}
