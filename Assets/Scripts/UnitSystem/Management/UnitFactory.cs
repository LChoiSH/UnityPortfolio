using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace UnitSystem
{
    public class UnitFactory : MonoBehaviour
    {
        [SerializeField] private List<UnitFactorySet> unitSets;

        private Dictionary<string, IObjectPool<Unit>> unitPoolDic = new();

        public event Action<Unit> onUnitMade;
        public event Action<Unit> onUnitRelease;

        /// <summary>
        /// Gets a unit from the pool by unit ID.
        /// </summary>
        /// <param name="unitId">The ID of the unit to retrieve</param>
        /// <returns>A unit instance from the pool, or null if not found</returns>
        public Unit GetUnit(string unitId)
        {
            if (!unitPoolDic.ContainsKey(unitId))
            {
                Debug.LogError($"Unit '{unitId}' not found in factory. Available units: {string.Join(", ", unitPoolDic.Keys)}");
                return null;
            }

            return unitPoolDic[unitId].Get();
        }

        /// <summary>
        /// Gets a unit from the pool and sets its position.
        /// </summary>
        /// <param name="unitId">The ID of the unit to retrieve</param>
        /// <param name="position">The world position to place the unit</param>
        /// <returns>A unit instance from the pool, or null if not found</returns>
        public Unit GetUnit(string unitId, Vector3 position)
        {
            Unit unit = GetUnit(unitId);
            if (unit != null)
            {
                unit.transform.position = position;
            }
            return unit;
        }

        /// <summary>
        /// Checks if a unit with the given ID exists in the factory.
        /// </summary>
        /// <param name="unitId">The unit ID to check</param>
        /// <returns>True if the unit exists in the factory</returns>
        public bool HasUnit(string unitId)
        {
            return unitPoolDic.ContainsKey(unitId);
        }

        /// <summary>
        /// Gets all available unit IDs in the factory.
        /// </summary>
        /// <returns>Collection of unit IDs</returns>
        public IEnumerable<string> GetAvailableUnitIds()
        {
            return unitPoolDic.Keys;
        }

        void Awake()
        {
            if (unitSets == null) return;

            foreach (UnitFactorySet unitSet in unitSets)
            {
                if (unitSet.usePool)
                {
                    ObjectPool<Unit> madePool = new ObjectPool<Unit>(
                        createFunc: () => Create(unitSet.unit),
                        actionOnGet: PoolOnGet,
                        actionOnRelease: PoolOnRelease,
                        actionOnDestroy: PoolOnDestroy,
                        collectionCheck: true,
                        defaultCapacity: 4,
                        maxSize: unitSet.maxSize
                    );

                    if (unitPoolDic.ContainsKey(unitSet.unit.Id)) Debug.LogWarning($"duplicated unit {unitSet.unit.Id}");
                    unitPoolDic[unitSet.unit.Id] = madePool;

                    for (int i = 0; i < unitSet.prewarmCount; i++)
                    {
                        madePool.Release(Create(unitSet.unit));
                    }
                }
            }
        }

        private Unit Create(Unit unitPrefab)
        {
            Unit madeUnit = Instantiate(unitPrefab, transform);
            madeUnit.gameObject.SetActive(false);
            madeUnit.onDestroy += UnitRelease;

            return madeUnit;
        }

        private void PoolOnGet(Unit unit)
        {
            unit.gameObject.SetActive(true);
            onUnitMade?.Invoke(unit);
        }

        private void PoolOnRelease(Unit unit)
        {
            unit.StopAllCoroutines();
            unit.Animator?.Rebind();
            unit.Animator?.Update(0f);
            unit.gameObject.SetActive(false);
            onUnitRelease?.Invoke(unit);
        }

        void PoolOnDestroy(Unit unit)
        {
            if (unit) Destroy(unit.gameObject);
        }

        private void UnitRelease(Unit unit)
        {
            if (!unitPoolDic.ContainsKey(unit.Id))
            {
                Destroy(unit.gameObject);
                return;
            }
            
            unitPoolDic[unit.Id].Release(unit);
        }

        [System.Serializable]
        private struct UnitFactorySet
        {
            public Unit unit;
            public bool usePool;
            public int maxSize;
            public int prewarmCount;
        }
    }
}