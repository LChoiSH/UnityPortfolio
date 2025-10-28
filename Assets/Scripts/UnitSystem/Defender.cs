using System;
using Unity.VisualScripting;
using UnityEngine;

namespace UnitSystem
{
    public class Defender : MonoBehaviour
    {
        public event Action onDeath;
    }
}