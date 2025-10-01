using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface HaveLoad
{
    bool IsLoaded { get; }

    void Load();
}
