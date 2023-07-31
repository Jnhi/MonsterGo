
using System.Threading.Tasks;
using GameFramework;

public class TtfeModule : ModuleBase
{
    public override string PakName => "TtfeGame";
    public override void CloseMudule(object obj)
    {

    }

    public override async Task Open(object obj)
    {
        await base.Open(obj);

        await UIManager.Instance.OpenUIPanelAsync<Ttfe_MainView>();

        AudioManager.Instance.PlayBGM("TtfeGame/m_bg.mp3");

    }
}

