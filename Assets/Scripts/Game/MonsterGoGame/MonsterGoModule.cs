
using System.Threading.Tasks;
using FairyGUI;
using GameFramework;

public class MonsterGoModule : ModuleBase
{
    public override string PakName => "MonsterGo";

    public override void CloseMudule(object obj)
    {

    }

    public override async Task Open(object obj)
    {
        await base.Open(obj);

        UIObjectFactory.SetPackageItemExtension("ui://MonsterGo/TopBattleComp", typeof(TopBattleComp));
        UIObjectFactory.SetPackageItemExtension("ui://MonsterGo/MidTubeComp", typeof(MidTubeComp));
        UIObjectFactory.SetPackageItemExtension("ui://MonsterGo/MonsterIcon", typeof(MonsterIcon));
        UIObjectFactory.SetPackageItemExtension("ui://MonsterGo/MapComp", typeof(TopMapComp));
        UIObjectFactory.SetPackageItemExtension("ui://MonsterGo/IllustrateItem", typeof(IllustrateItem));

        AudioManager.Instance.PlayBGM("MonsterGo/music/gameloop.ogg");

        await MonsterGoModel.Instance.InitConfig();
        await UIManager.Instance.OpenUIPanelAsync<MonsterGoView>();


    }
}

