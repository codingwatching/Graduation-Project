using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSelectPopup : PanelUIComponent, IPopup
{
    [Header("Set By Runtime")]
    [SerializeField] SceneMgr.Scene nextScene;

    [Header("Set In Editor")]
    [SerializeField] public SelectUnitBtn[] selectUnitBtns;
    [SerializeField] public PlayerData playerData;

    void Start()
    {
        selectUnitBtns = GetComponentsInChildren<SelectUnitBtn>();
    }
    public void OnClickOK()
    {
        foreach(var v in selectUnitBtns)
        {
            playerData.AddUnitToList(v.unit, v.upgradeCount);
        }
        
        SceneMgr.Instance.LoadScene(nextScene);
    }
    public void OnClickClose()
    {
        UnsetPanel();
    }
    public override void SetPanel(UIParam u)
    {
        UIStageSelectPopupParam usspp = (UIStageSelectPopupParam)u;

        nextScene = usspp.nextScene;
        gameObject.SetActive(true);
    }
    public override void UnsetPanel()
    {
        gameObject.SetActive(false);
    }
}
