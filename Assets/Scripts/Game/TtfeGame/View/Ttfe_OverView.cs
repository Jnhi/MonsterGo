using FairyGUI;
using GameFramework;
using System;

public class Ttfe_OverView : UIBase
{

    Action Restart;
    public override string PakName => "TtfeGame";

    public override string CompName => "OverView";

    public override void ClosePanel(object arg = null)
    {
        UIManager.Instance.CloseUIPanel(this);
    }

    public override void OpenPanel(object arg = null)
    {
        Ttfe_OverViewOpenParam ttfe_OverViewOpenParam = arg as Ttfe_OverViewOpenParam;

        fui.GetChild("txtScore").text = TtfeModel.Instance.score.ToString();
        fui.GetChild("btnClose").onClick.Set(() => {
            AudioManager.Instance.Play("TtfeGame/m_btn.mp3");
            ClosePanel();
        });
        fui.GetChild("btnReStart").onClick.Set(() =>
        {
            AudioManager.Instance.Play("TtfeGame/m_btn.mp3");
            ClosePanel();
            ttfe_OverViewOpenParam.RestartFunc.Invoke();
        });
    }
}
