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
    int sDefSelectedIndex = 0;

    //stateList_Index : List<state>の選択インデックス値
    int stateListIndex = -1;


    //StateDefのList表示用管理クラス
    ReorderableList _stateDefList;
    //選択したStateDefをSerializedPropertyとして考える
    SerializedProperty DefListProperty, SelectedDefProperty;
    StateDefListObject targetStateDefLists;
    StateDef targetedStates;


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
        targetStateDefLists = (StateDefListObject)target;
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
            if (SelectedDefProperty != null)
            {
                selectedDef_stateList_OnGUI();
            }
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
        using (GUILayout.ScrollViewScope verticalScScope =
        new GUILayout.ScrollViewScope(ParamScrollPos,EditorStyles.helpBox, GUILayout.MaxWidth(240f) , GUILayout.MaxHeight(640f)))
        {
            ParamScrollPos = verticalScScope.scrollPosition;
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

        //stateDefList自体はそれぞれが一段として表記される.

        _stateDefList.elementHeightCallback = index =>
        {
            selectedDef_statelist_OnEnable(index);
            return EditorGUIUtility.singleLineHeight;
        };

        //ステート宣言データの消去時.　バグの温床.. 
        _stateDefList.onRemoveCallback = (index) =>
        {
            targetStateDefLists.stateDefs.RemoveAt(index.index);
            SelectedDefProperty = null;
        };

        //追加時の挙動 - DeepCopyを行いたい.
        _stateDefList.onAddCallback = (index) =>
        {
            StateDef AddDef = new StateDef();
            targetStateDefLists.stateDefs.Add(AddDef);

            /*
            //プロパティ検索
            var properties = type.GetProperties();
            foreach (var pr in properties)
            {
                pr.SetValue(AddDef, pr.GetValue(targetStateDefLists.stateDefs[index.index]));
            }
            targetStateDefLists.stateDefs.Add(AddDef);
            */
        };

        //StateDefListの描写時.

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
                targetedStates = targetStateDefLists.stateDefs[index];
                SelectedDefProperty = currentDef;
                //前回と違うものを選択されているなら選択ステートをリセットする.
                if (sDefSelectedIndex != index)
                {
                    sDefSelectedIndex = index;
                    stateListIndex = -1;
                }
            }

        }
            ;
    }

    //Enable時でのStateControllerListの描写
    void selectedDef_statelist_OnEnable(int ID)
    {
        if (_stateList.ContainsKey(ID)) return;

        SerializedProperty selectedDefElem = DefListProperty.GetArrayElementAtIndex(ID);
        //選択したstateListをselectedDefsとして登録..
        SerializedProperty selectedDefs = selectedDefElem.FindPropertyRelative("StateList");

        var _stateListDraw = new ReorderableList(serializedObject, selectedDefs, draggable: true, displayHeader: false,
        displayAddButton: true, displayRemoveButton: true);

        //_stateListDraw.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "te");
        _stateListDraw.drawElementCallback = (rect, index, active, focused) =>
        {
            SerializedProperty indexProperty = selectedDefs.GetArrayElementAtIndex(index);
            if (indexProperty != null)
            {
                SerializedProperty stNames =
                indexProperty.FindPropertyRelative(nameof(StateController.stateControllerSubName));
                SerializedProperty stIDs =
                indexProperty.FindPropertyRelative(nameof(StateController.ID));


                //このstNamesが存在しないなら、おそらくNULL値が全てに入っているはず.
                if (stNames != null)
                {
                    string stSubname = stNames.stringValue;
                    using (GUILayout.HorizontalScope HScope = new GUILayout.HorizontalScope())
                    {
                        Rect horizonSize = rect;
                        horizonSize.height = EditorGUIUtility.singleLineHeight;
                        string stateTypeName = targetedStates.StateList[index].GetType().ToString();

                        //ステートコントローラーの名称.
                        EditorGUI.LabelField(horizonSize, stateTypeName);
                        horizonSize.x = horizonSize.x + (stateTypeName.Length + 2f) * 8f;
                        horizonSize.width -= (stateTypeName.Length + 2f) * 8f;

                        //ID表記. これいる？
                        /*
                        string IDName = "____";
                        EditorGUI.LabelField(horizonSize, stIDs.intValue.ToString());
                        horizonSize.x = horizonSize.x + IDName.Length * 4f;
                        horizonSize.width -= IDName.Length * 4f;
                        */


                        //ユーザー定義の名称
                        EditorGUI.TextField(horizonSize, stSubname);
                    }
                    //選択されているなら..
                    if (active)
                    {
                        stateListIndex = index;
                        rect.y += EditorGUIUtility.singleLineHeight;
                        EditorGUI.PropertyField(rect, indexProperty);
                    }
                    Rect NextSize = rect;
                    NextSize.y += EditorGUIUtility.singleLineHeight;
                    NextSize.height += EditorGUIUtility.singleLineHeight;
                }
            }
        };

        _stateListDraw.onAddDropdownCallback = (rect, index) =>
        {
            PropertyDropDown(rect, index);
        };

        _stateListDraw.elementHeightCallback = (index) =>
        {
            //選択されているならそれを拡大表記したい.
            if (index == stateListIndex)
            {
                SerializedProperty indexProperty = selectedDefs.GetArrayElementAtIndex(index);
                return PropertyDrawerUtility.GetDefaultPropertyHeight
                (indexProperty, label: GUIContent.none) + EditorGUIUtility.singleLineHeight;
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        };


        _stateList[ID] = _stateListDraw;
    }

    //GUI描写時. この時点での_stateListを描写する.
    void selectedDef_stateList_OnGUI()
    {
        if (SelectedDefProperty != null)
        {
            using (GUILayout.ScrollViewScope StateDefScr =
                new GUILayout.ScrollViewScope(ParamScrollPos, EditorStyles.helpBox, GUILayout.MinWidth(120), GUILayout.MaxWidth(900)))
            {
                //LuaScript, ベースIDなどを記述.
                SerializedProperty stDefNameProperty = SelectedDefProperty.FindPropertyRelative(nameof(StateDef.StateDefName));
                SerializedProperty stDefIDProperty = SelectedDefProperty.FindPropertyRelative(nameof(StateDef.StateDefID));
                //LuaConditionの文章習得.
                SerializedProperty LuScript = SelectedDefProperty.FindPropertyRelative(nameof(StateDef.LuaAsset));
                
                SerializedProperty stDefinitCtrlProperty = SelectedDefProperty.FindPropertyRelative(nameof(StateDef.setCtrl));
                SerializedProperty ststateTypeProperty = SelectedDefProperty.FindPropertyRelative(nameof(StateDef.stateType));
                SerializedProperty stphysTypeProperty = SelectedDefProperty.FindPropertyRelative(nameof(StateDef.physType));
                SerializedProperty stmoveTypeProperty = SelectedDefProperty.FindPropertyRelative(nameof(StateDef.moveType));

                SerializedProperty preStateVerdict = SelectedDefProperty.FindPropertyRelative("preStateVerdictName");
                SerializedProperty preParamLoad = SelectedDefProperty.FindPropertyRelative("ParamLoadName");

                //基本情報の表示
                EditorGUILayout.PropertyField(stDefNameProperty);
                EditorGUILayout.PropertyField(stDefIDProperty);
                EditorGUILayout.PropertyField(LuScript);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(stDefinitCtrlProperty);
                EditorGUILayout.PropertyField(ststateTypeProperty);
                EditorGUILayout.PropertyField(stphysTypeProperty);
                EditorGUILayout.PropertyField(stmoveTypeProperty);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(preStateVerdict);
                EditorGUILayout.PropertyField(preParamLoad);
                EditorGUILayout.Space();


                //stateControllerListを描写
                _stateList[sDefSelectedIndex].DoLayoutList();
            }
        }
    }

    //アセットの追加ボタンメニュー. StateControllerを継承したサブクラスをメニューとして出す.
    private void PropertyDropDown(Rect buttonRect, ReorderableList list)
    {
        GenericMenu menu = new GenericMenu();

        foreach (Type executeState in executableStateControllerType)
        {
            //属性設定がうまくいかない.
            SCHiearchyAttribute scName =
            Attribute.GetCustomAttribute(executeState,typeof(SCHiearchyAttribute)) as SCHiearchyAttribute;
            menu.AddItem(new GUIContent(scName.Name), on: false, func: () =>
            //menu.AddItem(new GUIContent("Nemuineru/" + executeState.FullName), on: false, func: () =>
            {
                StateController Instance = Activator.CreateInstance(executeState) as StateController;
                targetedStates.StateList.Add(Instance);
            });
        }

        menu.DropDown(buttonRect);
    }
}

//ステートコントローラー全体で用いるためのクラス.
//
[CustomPropertyDrawer(typeof(StateController), true)]
public class StateContDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        PropertyDrawerUtility.DrawDefaultGUI(position, property, label);
        return;
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return PropertyDrawerUtility.GetDefaultPropertyHeight(property, label);
    }
}


