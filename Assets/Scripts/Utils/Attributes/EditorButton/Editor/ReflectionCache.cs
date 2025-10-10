using System;
using System.Collections.Generic;
using System.Reflection;

public static class ReflectionCache
{
    static readonly Dictionary<(Type, string), MethodInfo[]> _cache = new();

    public static MethodInfo[] GetButtonMethods(Type type)
    {
        var key = (type, "buttons");
        if (_cache.TryGetValue(key, out var hit)) return hit;

        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        _cache[key] = methods;
        return methods;
    }
}