using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class RGSInputField : InputField
{
    public event Action OnSelectEvent;
    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        OnSelectEvent?.Invoke();
    }
}
