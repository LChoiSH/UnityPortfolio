using System.Collections.Generic;
using UnityEngine;
using RoguelikeSystem;
using System.Linq;
using VInspector;
using UnityEditor;

public class RoguelikeManager : MonoBehaviour
{
    [SerializeField] RoguelikeGachaPool gachaPool;
    [SerializeField] RoguelikeCanvas gachaCanvas;
    [SerializeField] private int gachaChoiceCount = 3;

    public void AddGachaCount()
    {
        gachaChoiceCount++;
    }

    public void Gacha()
    {
        gachaCanvas.ShowItems(gachaPool.GetRandomEffects(gachaChoiceCount));
    }
}
