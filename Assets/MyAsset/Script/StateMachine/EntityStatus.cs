using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//よく使う属性を登録・管理.
[System.Serializable]
public class EntityAttr
{
    //操作可能状態かチェック.
    public bool ctrl = true;
    //生きてるか？
    public bool alive = true;
    //止まってるか？
    public bool isPaused = false;
    public bool isStateChanged = false;
    public bool isStateHit = false;

    public bool isFall = false;

    public bool isEraseReady = false;
}

//キャラクターの特殊フラッグ指定管理..
public class EntityFlag
{
    public string flagName;
    public bool isEternal;
    public int flagDefaultTime;
    internal int flagElapsedTime;
}


[System.Serializable]
public class EntityStatus
{
    //基本情報
    public float maxHP = 100f;
    public float currentHP = 100f;
    public float maxEnergy = 50f;
    public float currentEnergy = 50f;

    //100%上限
    public float BaseAttackPerc = 100f;
    public float BaseDefencePerc = 100f;


    //チャージ. 特殊ボタンの押しっぱなしを判別.
    //ダメージを受けたりすると0に戻る.
    //こういう変数はあんまり設定したくないんだよね
    internal float ChargeTime = 0f;    
    internal float setChargeTime_Lv1 = 0.5f;
    internal float setChargeTime_Lv2 = 1f;
    internal float setChargeTime_End = 2f;

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

