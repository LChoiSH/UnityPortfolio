using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class EditorButtonSample : MonoBehaviour
{
    [EditorButton]
    public void SampleDebug()
    {
        Debug.Log("Click!");
    }

    [EditorButton("string")]
    public void SampleDebugString(string input)
    {
        Debug.Log($"Click {input}");
    }

    [EditorButton]
    public void SampleDebugInt(int input)
    {
        Debug.Log($"Click {input}");
    }

    [EditorButton]
    public void SampleDebugFloat(float input)
    {
        Debug.Log($"Click {input}");
    }

    [EditorButton]
    public void SampleDebugBool(bool input)
    {
        Debug.Log($"Click {input}");
    }

    [EditorButton]
    public void SampleDebugVector3(Vector3 input)
    {
        Debug.Log($"Click {input}");
    }

    private enum sampleEnum { A, B, C }
    [EditorButton]
    private void SampleDebugEnum(sampleEnum input)
    {
        Debug.Log($"Click {input}");
    }

    [EditorButton]
    private void SampleDebugTwoParam(string a, string b)
    {
        Debug.Log($"Click {a} {b}");
    }
}
