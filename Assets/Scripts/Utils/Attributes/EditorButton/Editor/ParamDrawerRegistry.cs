using System;
using System.Collections.Generic;
using System.Linq;

public static class ParamDrawerRegistry
{
    static readonly List<IParamFieldDrawer> _drawers = new()
        {
            new IntDrawer(), new FloatDrawer(), new StringDrawer(), new BoolDrawer(),
            new EnumDrawer(), new Vector3Drawer(), new UnityObjectDrawer()
        };

    public static object DrawField(string label, object current, Type type)
    {
        var d = _drawers.FirstOrDefault(x => x.CanDraw(type));
        return d != null ? d.Draw(label, current, type) : current;
    }

    // 필요 시 Register로 외부 확장 허용
    public static void Register(IParamFieldDrawer drawer) => _drawers.Insert(0, drawer);
}