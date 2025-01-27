using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ObserverPattern;

public class DevSceneLoadBtn : PanelUIComponent
{
    [SerializeField] public SceneMgr.Scene SceneToLoad;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClickedBtn);
    }

    public override void SetPanel(UIParam param = null)
    {

    }
    public override void UnsetPanel()
    {

    }
    public void OnClickedBtn()
    {
        SceneMgr.Instance.LoadScene(SceneToLoad);
    }
}
