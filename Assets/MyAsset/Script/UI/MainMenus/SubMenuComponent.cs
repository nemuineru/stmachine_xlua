using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//サブメニュー　フォー　メインのメニュー。　要はメニュー定義オブジェクトっすね
//深度が深くなるほど親の負担がかかるので、大メニュー事に設定.
public class SubMenuComponent : MonoBehaviour
{
    [SerializeField]
    internal string menuText_Indexes, menuText_Description;

    [SerializeField]
    List<SubMenuComponent> menuTextObjs;

    [SerializeField]
    TMP_Text txt;

    public SubMenuComponent subSelected;
    public int subSelectedIndex;
    public bool currentSelected = false;

    public bool currentConfirmed = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    Color selCol = new Color(1f, 0.8f, 0.2f);
    Color nonSelCol = new Color(0.3f, 0.2f, 0.2f);

    // Update is called once per frame
    //選択されたさいの自分のテキストを設定
    void LateUpdate()
    {
        updateMenuComps();
    }

    void updateMenuComps()
    { 
        if (txt != null)
        {
            if (currentSelected == true)
            {
                txt.color = selCol;
            }
            else
            {
                txt.color = nonSelCol;
            }
        }

        if (menuTextObjs.Count != 0)
        {
            for (int i = 0; i < menuTextObjs.Count; i++)
            {
                SubMenuComponent comp = menuTextObjs[i];
                if (comp != null)
                {
                    comp.currentSelected = subSelectedIndex == i;
                }
            }
        }
    }

    internal string getSelectedMenuText()
    {
        string ret = "";
        return ret;
    }
    internal string getMenuDescriptions()
    {
        string ret = "";
        return ret;
    }

    //メニュー項目の確認など
    internal void MenuSelector(int x)
    {
        //選択済みなら内部のメニューセレクタを呼び出す.
        if (subSelected != null)
        {
            subSelected.MenuSelector(x);
        }
        else
        {
            IndexSelector(x);
        }
    }

    //ループするメニュー項目のインデックス選択. xに変更値を入力
    internal void IndexSelector(int x)
    {
        subSelectedIndex = subSelectedIndex + x >= menuTextObjs.Count ?
        0 : (subSelectedIndex + x < 0 ? menuTextObjs.Count - 1 : subSelectedIndex + x);
    }
}
