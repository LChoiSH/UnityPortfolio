using System.Collections;
using System.Collections.Generic;
using RoguelikeSystem;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class RoguelikeItem : MonoBehaviour
{
    private RogueEffect effect;
    public TextMeshProUGUI tierText;
    public TextMeshProUGUI titleText;
    public Button selectButton;
    public Image backgroundImage;

    public void SetRogueEffect(RogueEffect effect)
    {
        this.effect = effect;
        tierText.text = effect.tier.ToString();
        titleText.text = effect.title;

        selectButton.onClick.AddListener(Select);
    }

    public void Select()
    {
        effect.Action();
    }

    public void SetColor(RogueTier tier)
    {
        Color targetColor = Color.white;

        switch (tier)
        {
            case RogueTier.Common:
                targetColor = Color.white;
                break;
            case RogueTier.Rare:
                targetColor = Color.green;
                break;
            case RogueTier.Unique:
                targetColor = Color.yellow;
                break;
        }

        backgroundImage.color = targetColor;
    }
}
