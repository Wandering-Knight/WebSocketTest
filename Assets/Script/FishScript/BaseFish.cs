using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BaseFish : MonoBehaviour
{
    public TextMeshPro fishNameText;

    public void SetFishName(string name)
    {
        if (fishNameText != null)
        {
            fishNameText.text = name;
        }
    }
}
