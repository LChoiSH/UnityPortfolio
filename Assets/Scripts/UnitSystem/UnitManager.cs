using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnitSystem;

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

    [SerializeField] private List<Unit> units;
    
    private Dictionary<int, List<Unit>> madeUnits = new Dictionary<int, List<Unit>>();

    public List<Unit> Units => units;
    public List<Unit> MadeUnits => madeUnits.Values.SelectMany(unitList => unitList).ToList();
    public List<Unit> GetTeamUnits(int teamNumber) => madeUnits[teamNumber].Where(unit => unit.gameObject.activeInHierarchy).ToList(); 
    public List<Unit> GetEnemyUnits(int teamNumber) => madeUnits.Where(pair => pair.Key != teamNumber).SelectMany(pair => pair.Value).Where(unit => unit.gameObject.activeInHierarchy).ToList();
    public List<Unit> GetUnitsById(int teamNumber, string unitId) => madeUnits[teamNumber].FindAll(made => made.Id == unitId);
    public int TeamCount(int team) => madeUnits.ContainsKey(team) ? madeUnits[team].Count : 0;

    public System.Action<Unit> onUnitMade;
    public System.Action<Unit> onUnitDestroy;
    public System.Action<Unit> onUnitMerged;
    public System.Action<int> onTeamCountChanged;

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

        onUnitMade?.Invoke(unit);
    }

    public void Unregister(Unit unit)
    {
        if (!madeUnits.ContainsKey(unit.Team)) return;
       
        madeUnits[unit.Team].Remove(unit);

        onUnitDestroy?.Invoke(unit);
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
    