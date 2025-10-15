using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogPanel : MonoBehaviour
{
    public static LogPanel Instance;
    public LogItem logItem;

    public float duration = 3f;
    public float fadeOutTime = 1f;

    //private List<LogItem> mades = new List<LogItem>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void Log(string message)
    {
        LogItem madeItem = Instantiate(logItem, transform);
        madeItem.SetMessage(message);

        StartCoroutine(DestroyItem(madeItem));

        //mades.Add(madeItem);
    }

    public void LocalizingLog(string table, string key, params object[] args)
    {
        Log(Localizing.GetFormat(table, key, args));
    }

    public IEnumerator DestroyItem(LogItem madeItem)
    {
        yield return new WaitForSecondsRealtime(duration);

        float current = 0;

        while(current < fadeOutTime)
        {
            current += Time.unscaledDeltaTime;
            madeItem.canvasGroup.alpha = Mathf.Lerp(1, 0, current / fadeOutTime);
            yield return Time.unscaledDeltaTime;
        }

        Destroy(madeItem.gameObject);
    }

#if UNITY_EDITOR
    private int sampleLog = 0;
    [VInspector.Button]
    public void MakeSampleLog()
    {
        Log($"{++sampleLog} This is sample Log");
    }
#endif
}
