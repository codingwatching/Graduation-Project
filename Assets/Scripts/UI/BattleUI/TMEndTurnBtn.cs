using ObserverPattern;
using UnityEngine;

public class TMEndTurnBtn : PanelUIComponent
{
    public void OnClick_EndTurnBtn()
    {
        AudioMgr.Instance.PlayAudio(AudioMgr.AudioClipType.UI_Clicked, AudioMgr.AudioType.UI);
        TurnMgr.Instance.NextTurn();
    }

    public override void SetPanel(UIParam u = null) { if (u == null) gameObject.SetActive(true); }
    public override void UnsetPanel() => gameObject.SetActive(false);
}
