using FairyGUI;
using UnityEngine.Scripting;

public class MapInfoItem : GComponent
{
    GLoader MapIcon;
    GTextField txtName;

    [Preserve]
    public MapInfoItem()
    {

    }

    public override void ConstructFromXML(FairyGUI.Utils.XML cxml)
    {
        base.ConstructFromXML(cxml);
        MapIcon = GetChild("icon").asLoader;
        txtName = GetChild("txtName").asTextField;
    }

    public void updataItem(int id)
    {
        MonsterGoModel model = MonsterGoModel.Instance;
        MapIcon.url = "ui://MonsterGo/monster_" + id;
        txtName.text = model.monsterConfig[id].name;
    }
}
