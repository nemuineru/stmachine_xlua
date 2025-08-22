using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//サブメニュー　フォー　メインのメニュー。　要はメニュー定義オブジェクトっすね
//深度が深くなるほど親の負担がかかるので、大メニュー事に設定.
public class SubMenuComponent : MonoBehaviour
{
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

    Color selCol = new Color(0xFF,0xCC,0x40);
    Color nonSelCol = new Color(0x40,0x30,0x20);

    // Update is called once per frame
    //選択されたさいの自分のテキストを設定
    void Update()
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
        if (subSelected != null)
        { 

        }
    }
}
