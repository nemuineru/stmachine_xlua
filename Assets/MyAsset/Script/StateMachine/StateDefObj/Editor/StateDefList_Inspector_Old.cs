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

//[CustomEditor(typeof(StateDefListObject))]
public class StateDefList_Inspector_Old : Editor
{
    int sDefSelectedIndex = -1;
    int scSelectedIndex = -1;
    Type[] executableStateControllerType;


    //StateDefのList表示用管理クラス
    ReorderableList _stateDefList;

    //選択されたStateDef内のステコンのList表示用管理クラス
    ReorderableList _stateList;

    Vector2 StateScrollPos;
    Vector2 ParamScrollPos;
    StateDefListObject CurObj;
    StateDef selectedStDef;

    SerializedProperty SelectedDefProperty;
    StateController CurStateController;

    public override void OnInspectorGUI()
    {
        executableStateControllerType = System.Reflection.Assembly.GetAssembly(typeof(StateDef))
        .GetTypes()
        .Where(x => x.IsSubclassOf(typeof(StateController)) && !x.IsAbstract)
        .ToArray();

        serializedObject.Update();
        CurObj = (StateDefListObject)target;
        using (new GUILayout.HorizontalScope())
        {
            StateDefSelect();
            //StateDefSelect_VertScope();
            if (SelectedDefProperty != null)
            {
                StateDefInspect();
            }
        }
        serializedObject.ApplyModifiedProperties();
    }


    // 引数は表示名, メソッド名
    [SerializeField, ContextMenuItem("Show Log", "Sample")]
    private int _sample;

    private void Sample()
    {
        Debug.Log(_sample);
    }



    void StateDefSelect_VertScope()
    {
        using (GUILayout.VerticalScope verticalScope =
        new GUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(120), GUILayout.MaxWidth(600)))
        {
            GUILayout.Label("StateDef Datas");

            string propertNameGet = nameof(CurObj.stateDefs);
            Debug.Log(propertNameGet);

            //List<stateDef> StateDefsの名称を取得
            var stateDefsProperty = serializedObject.FindProperty(propertNameGet);
            //stateDefListを取得..
            //SerializedProperty stDef = SelectedDefProperty.FindPropertyRelative(nameof(selectedStDef));
            //Debug.Log(propertNameGet);

            //StateDef Makings
            if (stateDefsProperty != null)
            {
                //stateDefsのエレメント、タイプに応じたプロパティの取得など
                _stateDefList = new ReorderableList(
                    CurObj.stateDefs, typeof(StateDef),
                    draggable: true, displayHeader: false, displayAddButton: true, displayRemoveButton: true
                    );
            }

            //アセット選択メニューを表記..
            //ここでの値を次のStateDefInspect
            _stateDefList.onSelectCallback = (list) =>
            {
                if (sDefSelectedIndex == list.index)
                {
                    _stateDefList.Deselect(sDefSelectedIndex);
                }
                sDefSelectedIndex = list.index;

                Debug.Log(list.index);

            };

            //アセット追加メニュー(+)のコールバック.
            _stateDefList.onAddCallback = (list) =>
            {
                stateDefsProperty.InsertArrayElementAtIndex(list.index);
                //追加項目の位置を右側位置
                sDefSelectedIndex = list.index;
                SelectedDefProperty =  list.serializedProperty.GetArrayElementAtIndex(sDefSelectedIndex + 1);
            };

            //アセット消去メニュー(-)のコールバック.
            _stateDefList.onRemoveCallback = (list) =>
            {
                stateDefsProperty.DeleteArrayElementAtIndex(list.index);
                sDefSelectedIndex = 0;
            };

            //stateDefの描写.
            _stateDefList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var t = CurObj.stateDefs[index];
                string subname = "StateDef " + t.StateDefID.ToString() + " - " + t.StateDefName;
                EditorGUI.LabelField(rect, subname);
            };



            /*
            //選択されたオブジェが正当なら..
            if (sDefSelectedIndex < CurObj.stateDefs.Count && sDefSelectedIndex > -1)
            {
                int i = sDefSelectedIndex;

                //Index内stateDefsの要素を選択,右のメニューに表示
                var t = CurObj.stateDefs[i];
                var defElem = stateDefsProperty.GetArrayElementAtIndex(i);

                selectedStDef = t;
                SelectedDefProperty = defElem;
            }
            */
            SelectedDefProperty = stateDefsProperty.GetArrayElementAtIndex(sDefSelectedIndex);
            _stateDefList.DoLayoutList();

            var t = CurObj.stateDefs[sDefSelectedIndex];
            selectedStDef = t;
        }
        
    }

    void StateDefSelect()
    {
        string propertNameGet = nameof(CurObj.stateDefs);

        var stateDefsProperty = serializedObject.FindProperty(propertNameGet);
        //stateDefListを取得..
        //SerializedProperty stDef = SelectedDefProperty.FindPropertyRelative(nameof(selectedStDef));
        //Debug.Log(propertNameGet);
        //アセット追加メニューを記述.
        //_stateDefList.onAddDropdownCallback = defenitionDropDown;
        //_stateDefList.DoLayoutList();


        int index = 0;
        using (GUILayout.ScrollViewScope StateDefScr =
        new GUILayout.ScrollViewScope(StateScrollPos, EditorStyles.helpBox, GUILayout.MinWidth(180), GUILayout.MaxWidth(160)))
        {
            GUILayout.Label("StateDef Data");
            StateScrollPos = StateDefScr.scrollPosition;
            for (int i = 0; i < CurObj.stateDefs.Count; i++)
            {
                var t = CurObj.stateDefs[i];
                var element = stateDefsProperty.GetArrayElementAtIndex(i);
                if (GUILayout.Button("StateDef " + t.StateDefID.ToString() + " - " + t.StateDefName))
                {
                    _stateList = null;
                    selectedStDef = t;
                    index = i;
                    string prop = string.Format("{0}.Array.data[{1}]", CurObj.stateDefs[i].ToString(), i.ToString());

                    SelectedDefProperty = element;
                }
            }
            
            //Button Drawers.
            if (GUILayout.Button("+"))
            {
                stateDefsProperty.InsertArrayElementAtIndex(0);
            }


            if (GUILayout.Button("-"))
            {
                stateDefsProperty.DeleteArrayElementAtIndex(index);
            }
        }

    }


    void StateDefInspect()
    {
        using (GUILayout.ScrollViewScope StateDefScr =
        new GUILayout.ScrollViewScope(ParamScrollPos, EditorStyles.helpBox, GUILayout.MinWidth(120), GUILayout.MaxWidth(900)))
        {
            //LuaScript, ベースIDなどを記述.
            SerializedProperty stDefNameProperty = SelectedDefProperty.FindPropertyRelative(nameof(selectedStDef.StateDefName));
            SerializedProperty stDefIDProperty = SelectedDefProperty.FindPropertyRelative(nameof(selectedStDef.StateDefID));
            //LuaConditionの文章習得.
            SerializedProperty LuScript = SelectedDefProperty.FindPropertyRelative(nameof(selectedStDef.LuaAsset));
            SerializedProperty stDefinitCtrlProperty = SelectedDefProperty.FindPropertyRelative(nameof(StateDef.setCtrl));

            EditorGUILayout.PropertyField(stDefNameProperty, label: new GUIContent("StateDef Name"));
            EditorGUILayout.PropertyField(stDefIDProperty, label: new GUIContent("StateDef ID"));


            EditorGUILayout.PropertyField(LuScript, label: new GUIContent("Lua Calcration Info"));
            EditorGUILayout.PropertyField(stDefinitCtrlProperty);

            //SerializedProperty LSC = LuScript.FindPropertyRelative("text");
            //EditorGUILayout.PropertyField(LuScript,label : new GUIContent("Lua Calcration Info"));

            // LSC.stringValue = EditorGUILayout.TextField(LSC.stringValue, GUILayout.MinHeight(200f),GUILayout.ExpandHeight(true));

            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(4));




            //stateDef中のstateListを登録.
            SerializedProperty states = SelectedDefProperty.FindPropertyRelative(nameof(selectedStDef.StateList));

            //StateListの中の派生クラスをそれぞれに
            //nullなら新規作成.
            if (_stateList == null)
            {
                //_stateList = new ReorderableList(CurObj.stateDefs[sDefSelectedIndex].StateList, typeof(StateController));
                _stateList = new ReorderableList(SelectedDefProperty.serializedObject, states);
            }
            _stateList.onSelectCallback = (list) =>
            {
                scSelectedIndex = list.index;
                Debug.Log(scSelectedIndex);
            };
            //_stateList.elementHeightCallback += index => isSelected == index ? 200f : EditorGUIUtility.singleLineHeight;

            //まず、StateList中の0番の設定を読み出す.

            SerializedProperty sr = SelectedDefProperty.FindPropertyRelative(nameof(selectedStDef.StateList)).GetArrayElementAtIndex(0);

            //エレメントの高さ表示用.
            //この時点でisSelected == indexが全体で適応されているのが気がかり.
            _stateList.elementHeightCallback = (index) =>
            {
                sr = SelectedDefProperty.FindPropertyRelative(nameof(selectedStDef.StateList)).GetArrayElementAtIndex(index);
                if (scSelectedIndex == index)
                {
                    //return 320f;              
                    return EditorGUI.GetPropertyHeight(sr, true) + EditorGUIUtility.singleLineHeight * 1.2f;
                }
                else
                {
                    return EditorGUIUtility.singleLineHeight;
                }
            };

            //ステート内容のメイン部分.
            //StateList[index]中の読み込まれたステート内容を読み出す..んだけどなんかおかしい.
            //最初の項目を選択後、すべての項目に見かけ上適応されている.
            _stateList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                //rectの位置を設定.
                var stateIDRect = new Rect(rect);
                var stateNameRect = new Rect(rect);
                stateIDRect.height = EditorGUIUtility.singleLineHeight;
                stateNameRect.height = stateIDRect.height;

                //Debug.Log("Loaded state Index : " + index);

                var elementProperty = states.GetArrayElementAtIndex(index);


                //2025-05-27 index値Maxの値が更新されないのはﾅﾝﾃﾞ...
                //Debug.Log(states.arraySize);


                var subnameProperty = elementProperty.FindPropertyRelative("stateControllerSubName");
                //subnameのプロパティを表示.
                string subname = subnameProperty.stringValue;
                //string StateIDPref = elementProperty.FindPropertyRelative("ID.stateID").intValue.ToString();

                //ステート名称を表記.
                //基本的に無いなら空欄のまま..
                subnameProperty.stringValue = EditorGUI.TextField(stateNameRect, subname);

                //EditorGUI.LabelField(rect, new GUIContent(StateIDPref + " - " + subname));
                //インデックスで同じ配列中の要素からheight, y値を表記.


                //2025-05-18 HF : すべて同じステートが適応されていたため、ここで選択されたindexに応じ代入することで解決. 一応.
                if (scSelectedIndex == index)
                {
                    Debug.Log("showing Index of.. " + index);

                    Rect tRect = new Rect(rect);
                    float lane = EditorGUIUtility.singleLineHeight;
                    tRect.height = tRect.height - lane;
                    tRect.y = tRect.y + lane;
                    //SelectedDefProperty.FindPropertyRelative(nameof(selectedStDef.StateList) + string.Format(".Array.data[{0}]", index));
                    //ここで初めてsrに選択項目の内容を代入.
                    sr = SelectedDefProperty.FindPropertyRelative(nameof(selectedStDef.StateList)).GetArrayElementAtIndex(index);
                    sr.isExpanded = true;
                    var label = new GUIContent(sr.type);
                    EditorGUI.PropertyField(tRect, sr, label, true);

                }
            };


            //アセット追加メニューを記述.
            _stateList.onAddDropdownCallback = PropertyDropDown;
            _stateList.DoLayoutList();

            /*

            for (int i = 0; i < states.arraySize; i++)
            {
                //Debug.Log(selectedStDef.StateList[i].ToString());
                SerializedProperty StateDefs = SelectedDefProperty.FindPropertyRelative(nameof(selectedStDef.StateList) + string.Format(".Array.data[{0}]", i));

                var label = new GUIContent(StateDefs.type);
                //var label = new GUIContent(StateDefs.FindPropertyRelative("StateControllerName").stringValue);
                //EditorGUILayout.PropertyField(StateDefs, label);
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(4));
            }

            */

            /*
            EditorGUILayout.PropertyField(SelectedDefProperty);
            GUILayout.Label("StateControllers");
            StateScrollPos = StateDefScr.scrollPosition;
            if(selectedStDef != null)
            {                
                var fields = selectedStDef.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach(var field in fields)
                {
                    var testVal = field.GetValue(selectedStDef);
                    string set = testVal != null ? testVal.ToString() : string.Empty;
                    if(testVal == null)
                    {
                        GUILayout.Label(field.Name + " is Empty");
                    }
                    else if(!testVal.GetType().Equals(typeof(List<StateController>)))
                    {
                        GUILayout.Label(field.Name + " - " +  set);
                    }
                    else
                    {
                        foreach(StateController checkController in testVal as List<StateController>)
                        {               
                            var SObject = serializedObject.FindProperty(nameof(checkController));
                            EditorGUILayout.PropertyField(SObject, new GUIContent("here"));
                        }                
                    }
                }
            }           
            */
        }
    }

    public static string NameOf<T>(Expression<Func<T, object>> selector)
    {
        const string joinWith = ".";
        return nameof(T) + joinWith + string.Join(joinWith, selector.ToString().Split('.').Skip(1));
    }


    //アセットの追加ボタンメニュー. StateControllerを継承したサブクラスをメニューとして出す.
    private void PropertyDropDown(Rect buttonRect, ReorderableList list)
    {
        GenericMenu menu = new GenericMenu();

        foreach (Type executeState in executableStateControllerType)
        {
            menu.AddItem(new GUIContent(executeState.FullName), on: false, func: () =>
            {
                StateController Instance = Activator.CreateInstance(executeState) as StateController;
                selectedStDef.StateList.Add(Instance);
            });
        }

        menu.DropDown(buttonRect);
    }
    
    
    //ステート基本情報のリストアップなど.
    private void defenitionDropDown(Rect buttonRect, ReorderableList list)
    {
        GenericMenu menu = new GenericMenu();
        menu.DropDown(buttonRect);
    }
}

//set non-foldout but seems not working..
public class NoFoldoutDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.Next(true); // get first child of your property
        do
        {
            EditorGUI.PropertyField(position, property, true); // Include children
            position.y += EditorGUIUtility.singleLineHeight + 2;
        }
        while (property.Next(false)); // get property's next sibling
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        property.Next(true);
        float result = 0;
        do
        {
            result += EditorGUI.GetPropertyHeight(property, true) + 2; // include children
        }
        while (property.Next(false));
        return result;
    }
}

//単純に全要素を表示するだけのスクリプト.
//this section borrowed from here - 
//https://light11.hatenadiary.com/entry/2019/05/13/215814
public static class PropertyDrawerUtility
{
    public static void DrawDefaultGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property = property.serializedObject.FindProperty(property.propertyPath);
        var fieldRect = position;
        fieldRect.height = EditorGUIUtility.singleLineHeight;

        using ( new EditorGUI.PropertyScope(fieldRect, label, property)) 
        {
            if (property.hasChildren) {
                // 子要素があれば折り畳み表示
                property.isExpanded = EditorGUI.Foldout (fieldRect, property.isExpanded, label);
            }
            else {
                // 子要素が無ければラベルだけ表示
                EditorGUI.LabelField(fieldRect, label);
                return;
            }
            fieldRect.y += EditorGUIUtility.singleLineHeight;
            fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded) {

                using (new EditorGUI.IndentLevelScope()) 
                {
                    // 最初の要素を描画
                    property.NextVisible(true);
                    var depth = property.depth;
                    EditorGUI.PropertyField(fieldRect, property, true);
                    fieldRect.y += EditorGUI.GetPropertyHeight(property, true);
                    fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

                    // それ以降の要素を描画
                    while(property.NextVisible(false)) {
                        
                        // depthが最初の要素と同じもののみ処理
                        if (property.depth != depth) {
                            break;
                        }
                        EditorGUI.PropertyField(fieldRect, property, true);
                        fieldRect.y += EditorGUI.GetPropertyHeight(property, true);
                        fieldRect.y += EditorGUIUtility.standardVerticalSpacing;
                    }
                }
            }
        }
    }

    public static float GetDefaultPropertyHeight(SerializedProperty property, GUIContent label)
    {
        property = property.serializedObject.FindProperty(property.propertyPath);
        var height = 0.0f;
        
        // プロパティ名
        height += EditorGUIUtility.singleLineHeight;
        height += EditorGUIUtility.standardVerticalSpacing;

        if (!property.hasChildren) {
            // 子要素が無ければラベルだけ表示
            return height;
        }
        
        if (property.isExpanded) {
        
            // 最初の要素
            property.NextVisible(true);
            var depth = property.depth;
            height += EditorGUI.GetPropertyHeight(property, true);
            height += EditorGUIUtility.standardVerticalSpacing;
            
            // それ以降の要素
            while(property.NextVisible(false))
            {
                // depthが最初の要素と同じもののみ処理
                if (property.depth != depth) {
                    break;
                }
                height += EditorGUI.GetPropertyHeight(property, true);
                height += EditorGUIUtility.standardVerticalSpacing;
            }
            // 最後はスペース不要なので削除
            height -= EditorGUIUtility.standardVerticalSpacing;
        }

        return height;
    }

        /// <summary>
        /// Gets the object the property represents.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }
}

