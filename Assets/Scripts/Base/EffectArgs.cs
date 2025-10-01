using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct EffectArgs
{
    [SerializeField] private string[] values;
    public string[] AllValue => values;
    public EffectArgs(string[] tokens) => values = tokens ?? Array.Empty<string>();

    public static EffectArgs From(params string[] tokens) => new EffectArgs(tokens);

    public int Int(int index, int defaultValue = 0) => (index < values.Length && int.TryParse(values[index], out var returnValue)) ? returnValue : defaultValue;
    public float Float(int index, float defaultValue = 0f) => (index < values.Length && float.TryParse(values[index], out var returnValue)) ? returnValue : defaultValue;
    public bool Bool(int index, bool defaultValue = false) => (index < values.Length && bool.TryParse(values[index], out var returnValue)) ? returnValue : defaultValue;
    public string Str(int index, string defaultValue = "") => (index < values.Length) ? values[index] : defaultValue;

    public int Count => values.Length;
}
