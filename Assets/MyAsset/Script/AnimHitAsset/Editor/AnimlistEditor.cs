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
