using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityStatus
{
    public float maxHP = 100f;
    public float currentHP = 100f;
    public float maxEnergy = 50f;
    public float currentEnergy = 50f;
    public float BaseAttack = 10f;



    public void setCurrentValue()
    {
        currentHP = Mathf.Min(currentHP, maxHP);
        currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
    }

    //get CurrentHP. might needs to update current..
    public int GetCurrentHP()
    {
        return Mathf.CeilToInt(Mathf.Max(0, currentHP));
    }
}
