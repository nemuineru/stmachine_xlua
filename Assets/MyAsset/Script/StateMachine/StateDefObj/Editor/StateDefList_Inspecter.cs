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

[CustomEditor(typeof(StateDefListObject))]
public class StateDefList_Inspector : Editor
{
    /*
        表示形式としてはこのようにする

        Horizontal
        {
         -> VertScrollScope{
            ReorderableList [ List of stateDefs ]
            [+][-]
         }
         -> VertScrollScope [ List of stateControllers (inside of selected StateDef)]
            [+▼][-]
        }
        このため、ReorderableListを２つ利用する必要があるが、設計面で手違いがあったのかどこをバグとして認識するべきか分からなくなってきた.

        --具体的なバグとしては
         -> stateDefの追加の挙動がおかしい. 追加された配列要素がindex(0)と同じものを指している.
         -> 別のstateDefを選択した際、表示されるべきstateDefのstateController数が前の数を参照している.

    */


    //stateDefList_Index : List<stateDef>の選択インデックス値

    int sDefSelectedIndex = -1;

    //stateList_Index : List<state>の選択インデックス値
    int scSelectedIndex = 0;

    //StateDefのList表示用管理クラス
    ReorderableList _stateDefList;
    //選択したStateDefをSerializedPropertyとして考える
    SerializedProperty DefListProperty, SelectedDefProperty;


    //選択されたStateDef内のステコンのList表示用管理クラス
    //Dictionaryで管理..
    Dictionary<int, ReorderableList> _stateList = new();

    //stateDefの詳細表示時のスクロールポジション.
    Vector2 ParamScrollPos;


    //stateControllerのTypeを取得、ステコン追加時の挙動を表す
    Type[] executableStateControllerType;

    /*

    serializedObject内で編集を完了させるにあたり..
    
    */

    void OnEnable()
    {
        stateDefPicker_OnEnable();
    }

    public override void OnInspectorGUI()
    {
        // 内部キャッシュから値をロードする
        serializedObject.Update();
        executableStateListUp();


        using (new GUILayout.HorizontalScope())
        {
            stateDefPicker_OnGUI();
            selectedDef_stateList_OnGUI();
        }
        serializedObject.ApplyModifiedProperties();
    }

    //ステコン追加ボタンの設定.
    void executableStateListUp()
    {
        executableStateControllerType = System.Reflection.Assembly.GetAssembly(typeof(StateDef))
        .GetTypes()
        .Where(x => x.IsSubclassOf(typeof(StateController)) && !x.IsAbstract)
        .ToArray();
    }

    //ONGUIに呼び出す. _stateDefListの要素をreordableListを用いて
    void stateDefPicker_OnGUI()
    {
        //スコープの作成
        using (GUILayout.VerticalScope verticalScope =
        new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUILayout.Label("StateDef");
            _stateDefList.DoLayoutList();
        }
    }

    //Enable時でのStateDefListの描写など
    void stateDefPicker_OnEnable()
    {
        //stateDefPropertyの指定
        DefListProperty = serializedObject.FindProperty(nameof(StateDefListObject.stateDefs));
        _stateDefList = new ReorderableList(serializedObject, DefListProperty, draggable: true, displayHeader: false,
        displayAddButton: true, displayRemoveButton: true);

        _stateDefList.drawElementCallback = (rect, index, active, focused) =>
        {
            SerializedProperty currentDef = DefListProperty.GetArrayElementAtIndex(index);

            int stateIDs = currentDef.FindPropertyRelative("StateDefID").intValue;
            string stateDefName = currentDef.FindPropertyRelative("StateDefName").stringValue;

            string stateNameCombined = string.Format("ID {0} : {1}", stateIDs, stateDefName);
            rect.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.LabelField(rect, stateNameCombined);
            selectedDef_statelist_OnEnable(index);
            if (active == true)
            {
                SelectedDefProperty = currentDef;
                scSelectedIndex = index;
            }

        };
        _stateDefList.elementHeightCallback = index =>
        {
            return EditorGUIUtility.singleLineHeight;
        };
    }

    //Enable時でのStateControllerListの描写
    void selectedDef_statelist_OnEnable(int ID)
    {
        if (_stateList.ContainsKey(ID)) return;

        SerializedProperty selectedDefElem = DefListProperty.GetArrayElementAtIndex(ID);
        SerializedProperty selectedDefs = selectedDefElem.FindPropertyRelative("StateList");
        var _stateListDraw = new ReorderableList(serializedObject, selectedDefs, draggable: true, displayHeader: false,
        displayAddButton: true, displayRemoveButton: true);

        _stateListDraw.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Inner Items");
        _stateListDraw.drawElementCallback = (rect, index, active, focused) =>
        {
            SerializedProperty x = selectedDefs.GetArrayElementAtIndex(index);
            EditorGUI.LabelField(rect,x.FindPropertyRelative("stateControllerSubName").ToString());
        };

        _stateList[ID] = _stateListDraw;
    }

    //GUI描写時. この時点での_stateListを描写する.
    void selectedDef_stateList_OnGUI()
    {
        using (GUILayout.ScrollViewScope StateDefScr =
        new GUILayout.ScrollViewScope(ParamScrollPos, EditorStyles.helpBox, GUILayout.MinWidth(120), GUILayout.MaxWidth(900)))
        {
            //LuaScript, ベースIDなどを記述.
            SerializedProperty stDefNameProperty = SelectedDefProperty.FindPropertyRelative(nameof(StateDef.StateDefName));
            SerializedProperty stDefIDProperty = SelectedDefProperty.FindPropertyRelative(nameof(StateDef.StateDefID));
            //LuaConditionの文章習得.
            SerializedProperty LuScript = SelectedDefProperty.FindPropertyRelative(nameof(StateDef.LuaAsset));

            //基本情報の表示
            EditorGUILayout.PropertyField(stDefNameProperty);
            EditorGUILayout.PropertyField(stDefIDProperty);
            EditorGUILayout.PropertyField(LuScript);

            //stateControllerListを描写
            _stateList[scSelectedIndex].DoLayoutList();
        }
    }
}
