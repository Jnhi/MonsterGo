
using System;
using FairyGUI;
using GameFramework;

public class GameOverView : UIBase
{
    public override string PakName => "MonsterGo";

    public override string CompName => "GameOverView";

    public GMovieClip mcFail;
    public GMovieClip mcWin;
    public GButton btnReStart;
    public GButton btnExit;
    public GTextField txtInfo;
    public override void OpenPanel(object arg = null)
    {
        fui.MakeFullScreen();
        mcFail = fui.GetChild("mcFail").asMovieClip;
        mcWin = fui.GetChild("mcWin").asMovieClip;
        btnReStart = fui.GetChild("btnReStart").asButton;
        btnExit = fui.GetChild("btnExit").asButton;
        txtInfo = fui.GetChild("txtInfo").asTextField;

        GameOverParams gameOverParams = (GameOverParams)arg;
        bool isWin = gameOverParams.isWin;
        Action reStartFunc = gameOverParams.reStartFunc;
        Action exitFunc = gameOverParams.exitFunc;

        btnReStart.onClick.Set(() =>
        {
            ClosePanel();
            reStartFunc?.Invoke();
        });

        btnExit.onClick.Set(() =>
        {
            ClosePanel();
            exitFunc?.Invoke();
        });

        GMovieClip mc;
        Log.Debug($"isWin{isWin}");
        if (isWin)
        {
            mcWin.visible = true;
            mcFail.visible = false;
            mc = mcWin;
            txtInfo.text = "恭喜你嘞，战胜了勇者";
        }
        else
        {
            mcWin.visible = false;
            mcFail.visible = true;
            mc = mcFail;
            txtInfo.text = "虽然你努力了，但是还是被勇者摧毁了城堡呢！";
        }

        mc.SetPlaySettings(0, -1, 1, -1);
        mc.playing = true;
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

