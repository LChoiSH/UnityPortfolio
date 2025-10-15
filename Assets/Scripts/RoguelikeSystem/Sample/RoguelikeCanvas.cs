using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoguelikeSystem;
using UnityEngine.UI;
using VInspector.Libs;

public class RoguelikeCanvas : MonoBehaviour
{
    public RoguelikeItem item;
    public RectTransform wrap;
    public RoguelikeGachaPool gachaPool;

    public Toggle tierToggle;
    public Toggle eachTierToggle;
    public Toggle duplicateToggle;

    void Awake()
    {
        Clear();
    }

    void Start()
    {
        tierToggle.isOn = gachaPool.IsTierGacha;
        eachTierToggle.isOn = gachaPool.IsEachTier;
        duplicateToggle.isOn = gachaPool.AllowDuplicates;

        tierToggle.onValueChanged.AddListener(OnTierToggleChanged);
        eachTierToggle.onValueChanged.AddListener(OnEachTierToggleChanged);
        duplicateToggle.onValueChanged.AddListener(OnDuplicateToggleChanged);
    }

    void OnDestroy()
    {
        tierToggle.onValueChanged.RemoveListener(OnTierToggleChanged);
        eachTierToggle.onValueChanged.RemoveListener(OnEachTierToggleChanged);
        duplicateToggle.onValueChanged.RemoveListener(OnDuplicateToggleChanged);
    }

    public void ShowItems(List<RogueEffect> effects)
    {
        Clear();

        foreach(RogueEffect effect in effects)
        {
            RoguelikeItem madeItem = Instantiate(item, wrap);
            madeItem.SetRogueEffect(effect);
        }
    }

    public void Clear()
    {
        foreach (Transform child in wrap)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnTierToggleChanged(bool toggle)
    {
        gachaPool.SetTierGacha(toggle);
    }

    private void OnEachTierToggleChanged(bool toggle)
    {
        gachaPool.SetEachTier(toggle);
    }
    
    private void OnDuplicateToggleChanged(bool toggle)
    {
        gachaPool.SetAllowDuplicates(toggle);
    }
}
