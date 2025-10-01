using System;
using UnityEngine;

public static class Calc
{
    public static float MagnitudeXZ(Vector3 a)
    {
        return (float)Math.Sqrt(a.x * a.x + a.z * a.z);
    }

    public static float MagnitudeXZ(Vector3 a, Vector3 b)
    {
        Vector3 c = a - b;

        return (float)Math.Sqrt(c.x * c.x + c.z * c.z);
    }

    public static bool isSameDist(Vector3 a, Vector3 b, float tolerance = 0.1f)
    {
        Vector3 c = a - b;
        
        return (float)Math.Sqrt(c.x * c.x + c.z * c.z) <= tolerance ? true : false;
    }

    public static Vector3 RandomVector(float size = 1.0f)
    {
        return new Vector3(
            UnityEngine.Random.Range(-size, size), 
            0, 
            UnityEngine.Random.Range(-size, size)
        );
    }

    public static string NumToString(int num) 
    {
        string returnVal;

        if(num >= 1000000)
        {
            returnVal = (num / 1000000).ToString() + "." + (num % 1000000 / 100000) + "m";
        } else if(num >= 1000)
        {
            returnVal = (num / 1000).ToString() + "." + (num % 1000 / 100) + "k";
        } else
        {
            returnVal = num.ToString();
        }

        return returnVal;
    }

    public static string MakeUniqueId()
    {
        return Guid.NewGuid().ToString();   
    }

    public static float CovertHeight(float heightA, Vector2 referenceResolutionA, float matchA, Vector2 referenceResolutionB, float matchB)
    {
        float screenRatioA = Mathf.Lerp(referenceResolutionA.x / Screen.width, referenceResolutionA.y / Screen.height, matchA);
        float screenRatioB = Mathf.Lerp(referenceResolutionB.x / Screen.width, referenceResolutionB.y / Screen.height, matchB);

        return heightA * (screenRatioA / screenRatioB);
    }

    public static string CalcValue(int value)
    {
        float k = value / 1000.0f;
        float m = value / 1000000.0f;

        if (k < 1 && m < 1)
        {
            return $"{value}";
        }

        if (m < 1)
        {
            return $"{String.Format("{0:N2}", k)}k";
        }

        return $"{String.Format("{0:N2}", m)}m";
    }

    public static string CalcTime(float time)
    {
        int hour = 0;
        int min = 0;
        float sec = 0;

        sec = (time % 60);
        min = (int)(time / 60);
        hour = (int)(min / 60);
        min = (int)(min % 60);

        if (hour == 0 && min == 0)
        {
            return $"{Mathf.Round(sec * 10f) / 10f}s";
        }

        if (hour == 0)
        {
            return $"{min}m{(int)sec}s";
        }

        // 기니까 분까지만
        return $"{hour}h{min}m";
        //return $"{hour}h{min}m{(int)sec}s";
    }

    public static Color CalcColor(string colorString)
    {
        Color newColor;
        ColorUtility.TryParseHtmlString(colorString, out newColor);
        newColor.a = 1;

        return newColor;
    }
}
