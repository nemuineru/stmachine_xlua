using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class SubMenuComponent_CamSetting : SubMenuComponent
{
    [SerializeField]
    CinemachineVirtualCamera cams;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    override internal string getSelectedMenuDescriptions()
    {
        string ret = "";
        if (subSelected != null)
        {
            ret = subSelected.getSelectedMenuDescriptions();
        }
        else
        {
            //Debug.Log(subMenus[subSelectedIndex].menu);
            ret = subMenus[subSelectedIndex].menu.menuText_Description;
        }
        return ret;
    }
}
