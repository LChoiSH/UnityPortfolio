using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class EditorButtonAttribute : Attribute
{
    public readonly string label;
    public readonly bool playModeOnly;   // 재생 중에만 활성
    public readonly bool editorOnly;     // 에디터 모드에서만 활성
    public readonly string confirm;      // 실행 전 확인 메시지

    public EditorButtonAttribute(
        string label = null,
        bool playModeOnly = false,
        bool editorOnly = false,
        string confirm = null)
    {
        this.label = label;
        this.playModeOnly = playModeOnly;
        this.editorOnly = editorOnly;
        this.confirm = confirm;
    }
}
