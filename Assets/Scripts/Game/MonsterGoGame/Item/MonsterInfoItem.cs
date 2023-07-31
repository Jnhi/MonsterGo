using FairyGUI;
using UnityEngine.Scripting;

public class MonsterInfoItem : GComponent
{
    GLoader monsterIcon;
    GTextField txtName;
    GTextField txtHp;
    GTextField txtPDef;
    GTextField txtSDef;
    GTextField txtPAtk;
    GTextField txtSAtk;

    [Preserve]
    public MonsterInfoItem()
    {

    }

    public override void ConstructFromXML(FairyGUI.Utils.XML cxml)
    {
        base.ConstructFromXML(cxml);
        monsterIcon = GetChild("loadIcon").asLoader;
        txtName = GetChild("txtName").asTextField;
        txtHp = GetChild("txtHp").asTextField;
        txtPDef = GetChild("txtPDef").asTextField;
        txtSDef = GetChild("txtSDef").asTextField;
        txtPAtk = GetChild("txtPAtk").asTextField;
        txtSAtk = GetChild("txtSAtk").asTextField;
    }

    public void updataItem(int id)
    {
        MonsterGoModel model = MonsterGoModel.Instance;
        monsterIcon.url = "ui://MonsterGo/monster_" + id;
        txtName.text = model.monsterConfig[id].name;
        txtHp.text = model.monsterConfig[id].hp.ToString();
        txtPDef.text = model.monsterConfig[id].def.ToString();
        txtSDef.text = model.monsterConfig[id].sdef.ToString();
        txtPAtk.text = model.monsterConfig[id].att.ToString();
        txtSAtk.text = model.monsterConfig[id].satt.ToString();
    }
}
