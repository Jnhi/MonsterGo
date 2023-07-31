
using FairyGUI;
using GameFramework;

public class RuleView : UIBase
{
    public override string PakName => "MonsterGo";
    public override string CompName => "RuleView";
    public GButton btnClose;
    public override void OpenPanel(object arg = null)
    {
        fui.MakeFullScreen();
        btnClose = fui.GetChild("btnClose").asButton;

        btnClose.onClick.Set(() =>
        {
            ClosePanel();
        });
    }

    /// <summary>
    /// 关闭面板
    /// </summary>
    /// <param name="arg"></param>
    public override void ClosePanel(object arg = null)
    {
        UIManager.Instance.CloseUIPanel(this);
    }
}

