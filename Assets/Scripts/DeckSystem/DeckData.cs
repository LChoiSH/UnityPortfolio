using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeckSystem
{
    public class DeckData
    {
        public List<string> selected;
        public Dictionary<string, UserUnitInfo> collected;
    
        public DeckData()
        {
            selected = new List<string>();
            collected = new Dictionary<string, UserUnitInfo>();
        }
    }
}
