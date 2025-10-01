using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;

public class CalcValueDouble
{
    [SerializeField] private double initialValue;

    private List<CalcFormula> formulas = new List<CalcFormula>();

    public double InitialValue => initialValue;

    public double Value { get; private set; }

#if UNITY_EDITOR
    public List<CalcFormula> FormulasForDebug => formulas;
#endif

    public CalcValueDouble()
    {

    }

    public CalcValueDouble(double initialValue)
    {
        AddFormula(new CalcFormula("default", initialValue, MathCalc.AddInitial));
    }

    public void CalcReturn()
    {
        double returnVal = 0;
        double newInitialValue = 0;

        for (int i = 0; i < formulas.Count; i++)
        {
            switch (formulas[i].calc)
            {
                case MathCalc.AddInitial:
                    newInitialValue += formulas[i].value;
                    returnVal += formulas[i].value;
                    break;
                case MathCalc.AddInitialByPercent:
                    newInitialValue += newInitialValue * formulas[i].value * 0.01d;
                    returnVal = newInitialValue;
                    break;
                case MathCalc.AddPercent:
                    returnVal += newInitialValue * formulas[i].value * 0.01d;
                    break;
                case MathCalc.Multiple:
                    returnVal *= formulas[i].value;
                    break;
                case MathCalc.Add:
                    returnVal += formulas[i].value;
                    break;
            }

            initialValue = newInitialValue;
        }

        Value = returnVal;
    }

    public void AddFormula(CalcFormula formula)
    {
        int aleady = -1;

        for (int i = 0; i < formulas.Count; i++)
        {
            if (formulas[i].id == formula.id)
            {
                aleady = i;
                break;
            }
        }

        if (aleady == -1)
        {
            formulas.Add(formula);
            SortFormula();
        }
        else
        {
            formulas[aleady] = formula;
        }

        CalcReturn();
    }

    public void RemoveFormula(string id)
    {
        for (int i = 0; i < formulas.Count; i++)
        {
            if (formulas[i].id == id)
            {
                formulas.RemoveAt(i);

                CalcReturn();
                return;
            }
        }
    }

    private void SortFormula()
    {
        formulas.Sort((a, b) => a.calc.CompareTo(b.calc));
    }

    public void ClearTempFormula()
    {
        formulas.RemoveAll(f => f.isTemp);
        CalcReturn();
    }

#if UNITY_EDITOR
    public void CheckFormula()
    {
        for(int i = 0;i < formulas.Count;i++)
        {
            Debug.Log(formulas[i]);
        }
    }
#endif
}

public class CalcFormula
{
    public string id;
    public double value;
    public MathCalc calc;
    public bool isTemp;

    public CalcFormula(string id, double value, MathCalc calc, bool isTemp = false)
    {
        this.id = id;
        this.value = value;
        this.calc = calc;
        this.isTemp = isTemp;
    }

    public override string ToString()
    {
        return $"{id} {value} {calc}";
    }
}

public enum MathCalc { Add = 4, Multiple = 3, AddPercent = 2, AddInitialByPercent = 1, AddInitial = 0}
