using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(MonoBehaviour), true)]
[CanEditMultipleObjects]
public class EditorButtonAttributeEditor : Editor
{
    static readonly Dictionary<string, object[]> _paramCache = new();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var type = target.GetType();
            var methods = ReflectionCache.GetButtonMethods(type);

            foreach (var m in methods)
            {
                var attrs = (EditorButtonAttribute[])m.GetCustomAttributes(typeof(EditorButtonAttribute), true);
                if (attrs == null || attrs.Length == 0) continue;

                foreach (var attr in attrs)
                {
                    var ps = m.GetParameters();
                    var label = string.IsNullOrEmpty(attr.label) ? ObjectNames.NicifyVariableName(m.Name) : attr.label;
                    bool disabled = (attr.playModeOnly && !Application.isPlaying) || (attr.editorOnly && Application.isPlaying);

                    string cacheKey = BuildKey(targets, m);
                    var values = SyncCache(cacheKey, ps);

                    // 파라미터 UI
                    if (ps.Length > 0)
                    {
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField($"{label}", EditorStyles.boldLabel);

                        for (int i = 0; i < ps.Length; i++)
                            values[i] = ParamDrawerRegistry.DrawField(ObjectNames.NicifyVariableName(ps[i].Name), values[i], ps[i].ParameterType);

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(2);
                    }

                    using (new EditorGUI.DisabledScope(disabled))
                    {
                        if (GUILayout.Button(label))
                        {
                            if (!string.IsNullOrEmpty(attr.confirm) &&
                                !EditorUtility.DisplayDialog("확인", attr.confirm, "실행", "취소")) continue;

                            var args = ps.Length == 0 ? null : values;

                            try
                            {
                                if (m.IsStatic) m.Invoke(null, args);
                                else foreach (var obj in targets) { m.Invoke(obj, args); EditorUtility.SetDirty((UnityEngine.Object)obj); }
                            }
                            catch (TargetInvocationException tex) { Debug.LogError(tex.InnerException); }
                            catch (Exception ex) { Debug.LogError(ex); }
                        }
                    }

                    EditorGUILayout.Space();
                }
            }
        }

        static string BuildKey(UnityEngine.Object[] targets, MethodInfo m)
        {
            string ids = string.Join(",", targets.Select(t => t.GetInstanceID()));
            string sig = $"{m.DeclaringType.FullName}.{m.Name}({string.Join(",", m.GetParameters().Select(p => p.ParameterType.FullName))})";
            return $"{ids}|{sig}";
        }

        static object[] SyncCache(string key, ParameterInfo[] ps)
        {
            if (!_paramCache.TryGetValue(key, out var v) || v == null || v.Length != ps.Length)
            {
                v = new object[ps.Length];
                for (int i = 0; i < ps.Length; i++)
                    v[i] = ps[i].HasDefaultValue ? ps[i].DefaultValue :
                           (typeof(UnityEngine.Object).IsAssignableFrom(ps[i].ParameterType) ? null :
                            (ps[i].ParameterType.IsValueType ? Activator.CreateInstance(ps[i].ParameterType) : null));
                _paramCache[key] = v;
            }
            return v;
        }
}
