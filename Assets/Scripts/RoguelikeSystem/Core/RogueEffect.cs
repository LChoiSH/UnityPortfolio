using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RoguelikeSystem
{
    [System.Serializable]
    public class RogueEffect
    {
        public string id;
        public string title;
        public RogueTier tier;
        public RogueEffectPair[] effects;
        public List<RogueConstrictData> constricts;

        public Sprite sprite;
        public int limit = 99;

        private float value;
        private int currentLimit = 0;

        public int Limit => limit;
        public int CurrentLimit => currentLimit;

        [JsonIgnore]
        public System.Action onAction;

        public void Action()
        {
            // Check all constraints before execution
            foreach (var constrictData in constricts)
            {
                IConstrictStrategy strategy = RogueConstrictRegistry.GetStrategy(constrictData.type);
                if (!strategy.IsUsable(constrictData.name, constrictData.needAmount))
                {
                    return;
                }
            }

            // Execute all constraint after-actions (e.g., consume resources)
            foreach (var constrictData in constricts)
            {
                IConstrictStrategy strategy = RogueConstrictRegistry.GetStrategy(constrictData.type);
                strategy.AfterAction(constrictData.name, constrictData.needAmount);
            }

            // Execute effects
            foreach(var pair in effects)
            {
                pair.Action();
            }

            currentLimit++;

            onAction?.Invoke();
        }

        public string DescriptionText()
        {
            return string.Join("\n", effects.Select(pair => pair.Description()));
        }

        public RogueEffect Clone()
        {
            RogueEffect clone = new RogueEffect();

            clone.id = id;
            clone.tier = this.tier;
            clone.title = this.title;

            if (this.effects != null)
            {
                clone.effects = new RogueEffectPair[this.effects.Length];
                for (int i = 0; i < this.effects.Length; i++)
                {
                    RogueEffectPair originalPair = this.effects[i];
                    RogueEffectPair newPair = new RogueEffectPair();

                    newPair.effectCategory = originalPair.effectCategory;
                    newPair.args = originalPair.args;

                    clone.effects[i] = newPair;
                }
            }

            if(constricts != null)
            {
                clone.constricts = new List<RogueConstrictData>(constricts);
            }

            clone.sprite = this.sprite; // Sprite is a reference type but doesn't need deep copy
            clone.limit = this.limit;
            clone.value = this.value;
            clone.currentLimit = this.currentLimit;

            // onAction should not be cloned
            clone.onAction = null;

            return clone;
        }

#if UNITY_EDITOR
        public static RogueEffect MakeEffect(Dictionary<string, object> data)
        {
            RogueEffect effect = new RogueEffect();

            effect.id = data["id"].ToString();
            effect.title = data["title"].ToString();
            int limit; 
            if(!int.TryParse(data["limit"].ToString(), out limit)) limit = 0;
            effect.limit = limit == 0 ? 99 : limit;

            if (!System.Enum.TryParse(data["tier"].ToString(), out effect.tier))
            {
               Debug.LogError($"{effect.id} Incorrect tier {data["tier"].ToString()}");
            }
            effect.sprite = FindSprite(effect.id);

            string planeEffectStrings = data["effect"].ToString();
            string[] effects = planeEffectStrings.Split(',');
            effect.effects = new RogueEffectPair[effects.Length];

            int index = 0;

            foreach(string effectString in effects)
            {
                string[] divideEffect = effectString.Split(' ');

                string[] argsStrings = divideEffect
                .Skip(1)
                .ToArray();

                RogueEffectPair newPair = new RogueEffectPair();
                if (!System.Enum.TryParse(divideEffect[0], out newPair.effectCategory))
                {
                    Debug.LogError($"{effect.id} effectCategory error {divideEffect[0]}");
                }
                newPair.args = new EffectArgs(argsStrings);

                effect.effects[index] = newPair;

                index++;
            }


            return effect;
        }

        public static Sprite FindSprite(string id)
        {
            string assetKey = $"Assets/Image/Treasure/{id}.png";

            bool keyExists = Addressables.ResourceLocators
                .SelectMany(locator => locator.Keys)
                .Contains(assetKey);

            if (keyExists)
            {
                AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(assetKey);
                handle.WaitForCompletion();

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    return handle.Result;
                }
            }
            else
            {
                Debug.LogWarning($"{assetKey} is not here");
            }

            return null;
        }
#endif
    }
}