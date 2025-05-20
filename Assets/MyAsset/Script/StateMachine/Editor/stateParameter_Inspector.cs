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
//ステートコントローラ内のパラメータ表記用クラス.
[CustomPropertyDrawer(typeof(stParams<>), true)]
public class sParams_Drawer : PropertyDrawer
{
    public string[] options = new string[] { "Constant Value", "via Condition", "via Calclation" };
    int index = 0;
    //ステートコントローラーのValue値を
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty toggleLoadLua = property.FindPropertyRelative("useID");

        //index = EditorGUI.Popup(position,index,options);

        //position.y -= EditorGUIUtility.singleLineHeight;

        PropertyDrawerUtility.DrawDefaultGUI(position, property, label);
        return;
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return PropertyDrawerUtility.GetDefaultPropertyHeight(property, label);
    }
}