using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimlistObject))]
public class AnimlistEditor : Editor
{
    GameObject PrevObj;
    AnimDef selected;
    void AnimPrev()
    {
        PreviewRenderUtility util = new PreviewRenderUtility();
    }
    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        base.OnPreviewGUI(r, background);
    }
}


[CustomEditor(typeof(AnimNodeMain))]
public class AnimNodeMainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //元のInspector部分を表示
        base.OnInspectorGUI ();

        //ボタンを表示
        if (GUILayout.Button("Change ID")){
            (target as AnimNodeMain).ChangeAnim();
        }  
    }
}