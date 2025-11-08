using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

//サブメニュー　フォー　メインのメニュー。　要はメニュー定義オブジェクトっすね
//深度が深くなるほど親の負担がかかるので、大メニュー事に設定.
public class SubMenuComponent : MonoBehaviour
{
    [SerializeField]
    internal string menuText_Indexes, menuText_Description;

    [System.Serializable]
    internal class SubMenuSets
    {
        //あんまし内部にマジックナンバー仕込みたくねえんだよな
        Color selCol = new Color(1f, 0.67f, 0.0f);
        Color nonSelCol = new Color(0.5f, 0.48f, 0.45f);


        [SerializeField]
        internal SubMenuComponent menu;
        [SerializeField]
        internal TMP_Text texts;
        internal bool isSelected;

        internal void SetTxtUpdate()
        {
            if (texts != null)
            {
                if (isSelected == true)
                {
                    texts.color = selCol;
                }
                else
                {
                    texts.color = nonSelCol;
                }
            }
        }
    }

    [SerializeField]
    internal bool willSelectable = true;

    [SerializeField]
    internal List<SubMenuSets> subMenus;

    [SerializeField]
    internal TMP_Text txt;

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
        if (subMenus.Count != 0)
        {
            for (int i = 0; i < subMenus.Count; i++)
            {
                SubMenuSets comp = subMenus[i];
                comp.isSelected = subSelectedIndex == i;
                comp.SetTxtUpdate();
            }
        }
    }

    internal string getSelectedMenuText()
    {
        string ret = "";
        if (subSelected != null)
        {
            ret = subSelected.getSelectedMenuText();
        }
        else
        {
            ret = menuText_Indexes;
        }
        return ret;
    }

    virtual internal string getSelectedMenuDescriptions()
    {
        string ret = "";
        if (subSelected != null)
        {
            ret = subSelected.getSelectedMenuDescriptions();
        }
        else
        {
            //Debug.Log(subMenus[subSelectedIndex].menu);
            if(subMenus != null && subMenus.Count > 0 && subMenus[subSelectedIndex].menu != null)
            ret = subMenus[subSelectedIndex].menu.menuText_Description;
        }
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
        if (subSelected != null)
        {
            subSelected.IndexSelector(x);
        }
        else
        {
            subSelectedIndex = subSelectedIndex + x >= subMenus.Count ?
            0 : (subSelectedIndex + x < 0 ? subMenus.Count - 1 : subSelectedIndex + x);
        }
    }

    internal void setSubMenuActive()
    {
        this.OnSelect();
        if (subSelected != null)
        {
            subSelected.setSubMenuActive();
        }
        else
        {
            var subComponent = subMenus[subSelectedIndex].menu;
            if (subComponent != null && subComponent.willSelectable == true)
            {
                subSelected = subComponent;
                if (subSelected != null)
                {
                    subSelected.ExecuteOnActive();
                    subSelected.gameObject.SetActive(true);
                    gameObject.SetActive(false);
                }
            }
            else if (subComponent != null)
            {
                Debug.Log("ExecuteOnly Executed");
                subComponent.ExecuteOnly();
            }
        }
    }

    virtual internal void ExecuteOnActive()
    {
        
    }
    
    virtual internal void ExecuteOnly()
    {

    } 
    virtual internal void OnSelect()
    {

    }


    internal void setSubMenuBack()
    {
        if (subSelected != null)
        {
            if (subSelected.subSelected != null)
                subSelected.setSubMenuBack();
            else
            {
                subSelected.gameObject.SetActive(false);
                subSelected = null;
                gameObject.SetActive(true);
            }
        }
    }
}
