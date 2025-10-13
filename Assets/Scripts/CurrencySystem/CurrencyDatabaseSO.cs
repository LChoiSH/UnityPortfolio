using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CurrencySystem
{
    [CreateAssetMenu(fileName = "CurrencyDatabase", menuName = "Game/Currency Database")]
    public class CurrencyDatabaseSO : ScriptableObject
    {

        [SerializeField] private List<Currency> items = new List<Currency>();

        public IReadOnlyList<Currency> Items => items;

        // 런타임 조회 유틸
        public Currency GetByTitle(string title)
            => items.FirstOrDefault(x => x.Title == title);

        public bool TryGet(string title, out Currency def)
        {
            def = GetByTitle(title);
            return def != null;
        }

#if UNITY_EDITOR
        //             // 에디터 편의: 새 통화 추가(자동 ID 발급)
        //             [ContextMenu("Add New Currency")]
        //             public void Editor_AddNewCurrency()
        //             {
        //                 var def = new Currency();
        //                 def.SetIdIfEmpty(System.Guid.NewGuid().ToString("N")); // 32자리
        //                 def.DisplayName = "New Currency";
        //                 items.Add(def);
        //                 EditorUtility.SetDirty(this);
        //             }

        // 에디터 검증: ID 중복 등 간단 체크
        [ContextMenu("Validate")]
        public void Editor_Validate()
        {
            var duplicates = items
                .GroupBy(i => i.Title)
                .Where(g => !string.IsNullOrEmpty(g.Key) && g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Count > 0)
            {
                var msg = "Duplicate Currency IDs:\n" + string.Join("\n", duplicates);
                Debug.LogError(msg, this);
            }
            else
            {
                Debug.Log("CurrencyDatabase: OK", this);
            }
        }
#endif
    }
}