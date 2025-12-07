using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnitSystem
{
    public class UnitManager : MonoBehaviour
{
    private static UnitManager _instance;
    public static UnitManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("UnitManager");
                _instance = obj.AddComponent<UnitManager>();
            }
            return _instance;
        }
    }

    [SerializeField] private UnitFactory unitFactory;
  
    private Dictionary<int, List<Unit>> madeUnits = new Dictionary<int, List<Unit>>();

    public IEnumerable<Unit> MadeUnits => madeUnits.Values.SelectMany(unitList => unitList);
    public IEnumerable<Unit> GetTeamUnits(int teamNumber) => madeUnits.TryGetValue(teamNumber, out var teamUnits) ? teamUnits : null;
    public IEnumerable<Unit> GetEnemyUnits(int teamNumber) => madeUnits.Where(pair => pair.Key != teamNumber).SelectMany(pair => pair.Value);
    public List<Unit> GetUnitsById(int teamNumber, string unitId) => madeUnits.TryGetValue(teamNumber, out var teamUnits) ? teamUnits.FindAll(unit => unit.Id == unitId) : null;
    public int TeamCount(int team) => madeUnits.ContainsKey(team) ? madeUnits[team].Count : 0;
    public UnitFactory UnitFactory => unitFactory;

    public event Action<Unit> onUnitRegister;
    public event Action<Unit> onUnitUnregister;
    public event Action<int> onTeamCountChanged;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Register(Unit unit)
    {
        if (!madeUnits.ContainsKey(unit.Team)) madeUnits[unit.Team] = new List<Unit>();
        if (madeUnits[unit.Team].Contains(unit)) return;
        
        madeUnits[unit.Team].Add(unit);

        onUnitRegister?.Invoke(unit);
        onTeamCountChanged?.Invoke(unit.Team);
    }

    public void Unregister(Unit unit)
    {
        if (!madeUnits.ContainsKey(unit.Team)) return;

        madeUnits[unit.Team].Remove(unit);

        onUnitUnregister?.Invoke(unit);
        onTeamCountChanged?.Invoke(unit.Team);
    }

#if UNITY_EDITOR
    public async void ReadCSV()
    {
        const string UnitPath = "Assets/CSV/Unit.csv";

        var unitCSV = await CSVReader.ReadByAddressablePathAsync(UnitPath);

        if (unitCSV == null)
        {
            Debug.LogError("There are not csv");
            return;
        }

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        Debug.Log($"Stage Read Success");
    }
#endif
}
}
    