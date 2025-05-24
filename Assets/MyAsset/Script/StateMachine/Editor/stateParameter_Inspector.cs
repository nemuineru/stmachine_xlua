using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq.Expressions;
using System.Linq;
using UnityEditor.UIElements;
using UnityEditorInternal;
using Unity.Properties;
using System.Reflection.Emit;
//ステートコントローラ内のパラメータ表記用クラス.
[CustomPropertyDrawer(typeof(stParams<>), true)]
public class sParams_Drawer : PropertyDrawer
{
    public string[] options = new string[] { "Constant Value", "via Condition", "via Calclation" };

    //呼び出し方法の記述. 後にDrawerに直にenum形式として記述したい.
    int index = 0;
    // ステートコントローラーのValue値をそのまま使用 or Luaにより事前計算された値を用いるか 
    // or Luaに計算させるか のどれかで実際のステート動作時のパラメータを決定する.
    // おそらく速度的に - Constant Value >> via Condition >> via Calclationの順で早いはず.
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty constantModel = property.FindPropertyRelative("stParamValue");
        SerializedProperty loadTypes = property.FindPropertyRelative("loadTypes");
        SerializedProperty toggleLoadLua = property.FindPropertyRelative("useID");
        SerializedProperty luaLoadString = property.FindPropertyRelative("stLuaLoads");
        //個人的にはこのようにしたい - 
        // --- 基礎stController情報記述後 ---    
        // world Velocity (Vector3) [[▼ via Calculation]]
        //  - <Calc Name>    [ "wVelCalc" ]
        // local Velocity (Vector3)  [[▼ Constant Value]]
        //  - <x> [ 0.0 ]  <y> [ 0.0 ]  <z> [ 8.0 ]
        // setImpulse (bool) [[▼ via Condition]]
        //  - <Condition ID> [ 1 ]
        //
        // 読み出し時,
        //position.y -= EditorGUIUtility.singleLineHeight;

        //ラベル表記.
        EditorGUI.LabelField(position, label, new GUIContent());
        Rect basePos = position;
        Rect popUpPos = position;
        GUIStyle labStyle = GUIStyle.none;

        //PopUp調整

        popUpPos.x += labStyle.CalcSize(label).x + 12f;
        popUpPos.width -= labStyle.CalcSize(label).x + 12f;

        loadTypes.enumValueIndex = EditorGUI.Popup(popUpPos, loadTypes.enumValueIndex, options);

        //選択後のドロワーの高さなどを表記.

        Rect DrawerPos = position;
        DrawerPos.y = popUpPos.y + EditorGUIUtility.singleLineHeight + 2f;
        switch (loadTypes.enumValueIndex)
        {
            // ConstantValueの使用時.
            case 0:
                {
                    GUIContent lab1 = new GUIContent(constantModel.propertyType.ToString());
                    //propertyDrawerUtilityが使えないためこのまま..
                    //PropertyDrawerUtility.DrawDefaultGUI(DrawerPos, constantModel, label);
                    //labelも同様に表記.
                    EditorGUI.PropertyField(DrawerPos, constantModel,lab1,true);
                }
                break;
            // Conditionの使用時.
            case 1:
                {
                    GUIContent lab1 = new GUIContent("Lua Output ID");
                    //statedef中のLuaから呼び出すfunction名を登録する. 
                    toggleLoadLua.intValue = EditorGUI.IntField(DrawerPos ,lab1, toggleLoadLua.intValue);
                }
                break;
            // Calclationの使用時.
            case 2:
                {
                    GUIContent lab1 = new GUIContent("Lua function Name");
                    //statedef中のLuaから呼び出すfunction名を登録する.          
                    //現状だとエラーが発生するため　改善必須.          
                    luaLoadString.stringValue = EditorGUI.TextField(DrawerPos, lab1, luaLoadString.stringValue);
                }
                break;
            default:
                break;

        }

        //position.y -= EditorGUIUtility.singleLineHeight;

        //PropertyDrawerUtility.DrawDefaultGUI(position, property, label);
        return;
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * (1 + property.CountInProperty()) * 1.05f;
    }
}