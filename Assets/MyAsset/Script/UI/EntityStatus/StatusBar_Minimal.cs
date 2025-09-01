using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityQuaternion;
using Shapes;
using UnityEngine;

//敵キャラに設定されるHPバーの設定など.
public class StatusBar_Minimal : MonoBehaviour
{
    [SerializeField]
    TMPro.TMP_Text HPText;

    Entity entityForUIBar;

    [SerializeField]
    Line maskLine;

    [SerializeField]
    Line fillerLine;

    [SerializeField]
    Line laterLine;

    public void SetEntity(Entity entity)
    {
        entityForUIBar = entity;
    }
    // Update is called once per frame
    void Update()
    {
        float length = (entityForUIBar.status.GetCurrentHP() / entityForUIBar.status.maxHP);
        fillerLine.End = maskLine.Start + (maskLine.End - maskLine.Start) * (length);
        laterLine.Start = fillerLine.End;
        laterLine.End = Vector3.Lerp(laterLine.End, laterLine.Start, 0.05f);
        HPText.text = entityForUIBar.status.GetCurrentHP().ToString();
        transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward,Vector3.up);
    }
}
