using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoguelikeSystem;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;

public enum UnitClass { Melee, Range, Tanker, Mage, Support };
public enum UnitTier { Normal = 0, Rare = 1, Unique = 2, Legend = 3 }

namespace DeckSystem
{
    [System.Serializable]
    public class UnitInfo
    {
        public UnitTier tier;
        public string id;
        public Sprite titleImage;
        public string description;
        public GameObject unitObject;
        public UnitLevelStat levelStat;
        public List<RogueEffect> rogueEffects;

        public GameObject Unit => unitObject;

#if UNITY_EDITOR
        public static GameObject FindUnitObject(string unitId)
        {
            string assetKey = $"Assets/Units/{unitId}/{unitId}.prefab";

            bool keyExists = Addressables.ResourceLocators
                .SelectMany(locator => locator.Keys)
                .Contains(assetKey);

            if (keyExists)
            {
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(assetKey);
                handle.WaitForCompletion();

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject findUnit = handle.Result.GetComponent<GameObject>();

                    if (findUnit == null) Debug.LogError($"{unitId} doesn't have Unit component");

                    return findUnit;
                }
            }
            else
            {
                Debug.LogWarning($"{assetKey} is not hear");
            }

            return null;
        }
#endif
    }
}