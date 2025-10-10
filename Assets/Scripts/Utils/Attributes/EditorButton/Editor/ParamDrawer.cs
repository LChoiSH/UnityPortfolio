using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public sealed class IntDrawer : IParamFieldDrawer
{
    public bool CanDraw(Type t) => t == typeof(int);
    public object Draw(string label, object v, Type _) => EditorGUILayout.IntField(label, v is int i ? i : default);
}

public sealed class FloatDrawer : IParamFieldDrawer
{
    public bool CanDraw(Type t) => t == typeof(float);
    public object Draw(string label, object v, Type _) => EditorGUILayout.FloatField(label, v is float f ? f : default);
}

public sealed class StringDrawer : IParamFieldDrawer
{
    public bool CanDraw(Type t) => t == typeof(string);
    public object Draw(string label, object v, Type _) => EditorGUILayout.TextField(label, v as string ?? "");
}

public sealed class BoolDrawer : IParamFieldDrawer
{
    public bool CanDraw(Type t) => t == typeof(bool);
    public object Draw(string label, object v, Type _) => EditorGUILayout.Toggle(label, v is bool b ? b : default);
}

public sealed class EnumDrawer : IParamFieldDrawer
{
    public bool CanDraw(Type t) => t.IsEnum;
    public object Draw(string label, object v, Type t)
    {
        if (v == null) v = Activator.CreateInstance(t);
        return EditorGUILayout.EnumPopup(label, (Enum)v);
    }
}

public sealed class Vector3Drawer : IParamFieldDrawer
{
    public bool CanDraw(Type t) => t == typeof(Vector3);
    public object Draw(string label, object v, Type _) => EditorGUILayout.Vector3Field(label, v is Vector3 vv ? vv : default);
}

// UnityEngine.Object 계열 전반
public sealed class UnityObjectDrawer : IParamFieldDrawer
{
    public bool CanDraw(Type t) => typeof(UnityEngine.Object).IsAssignableFrom(t);
    public object Draw(string label, object v, Type t) => EditorGUILayout.ObjectField(label, v as UnityEngine.Object, t, true);
}
