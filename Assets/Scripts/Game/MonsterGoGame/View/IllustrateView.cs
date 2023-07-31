
using FairyGUI;
using GameFramework;

public class IllustrateView : UIBase
{
    public override string PakName => "MonsterGo";

    public override string CompName => "IllustrateView";

    public GButton btn1;
    public GButton btn2;
    public GButton btnClose;
    public GList list;

    MonsterGoModel monsterModel = MonsterGoModel.Instance;
    MonsterGoUtil monsterGoUtil = MonsterGoUtil.Instance;

    public override void OpenPanel(object arg = null)
    {
        fui.MakeFullScreen();
        btn1 = fui.GetChild("btn1").asButton;
        btn2 = fui.GetChild("btn2").asButton;
        btnClose = fui.GetChild("btnClose").asButton;
        list = fui.GetChild("list").asList;
        list.SetVirtual();
        TabsComp tabsComp = new TabsComp(new GButton[2] { btn1, btn2 }, 0, (index) =>
        {
            if (index == 0)
            {
                list.itemRenderer = RenderListMonsterItem;
                list.numItems = monsterModel.monsterConfig.Length;
            }
            else if (index == 1)
            {
                list.itemRenderer = RenderListHeroItem;
                list.numItems = monsterModel.heroConfig.Length;
            }
        });
        btnClose.onClick.Set(()=>{
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

    void RenderListMonsterItem(int index, GObject obj)
    {
        IllustrateItem item = (IllustrateItem)obj;
        item.updataItem(monsterModel.monsterConfig[index].id,IllustrateItemType.Monter);
    }

    void RenderListHeroItem(int index, GObject obj)
    {
        IllustrateItem item = (IllustrateItem)obj;
        item.updataItem(monsterModel.heroConfig[index].id,IllustrateItemType.Hero);
    }
}

