using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LogItem : MonoBehaviour
{
    public TextMeshProUGUI logText;
    public CanvasGroup canvasGroup;

    public void SetMessage(string message)
    {
        logText.text = message;
    }
}
