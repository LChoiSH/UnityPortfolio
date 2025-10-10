using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IParamFieldDrawer
{
    bool CanDraw(Type type);
    object Draw(string label, object currentValue, Type type);
}
