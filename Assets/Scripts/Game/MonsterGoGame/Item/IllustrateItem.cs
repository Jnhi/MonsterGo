using FairyGUI;
using UnityEngine.Scripting;

public class IllustrateItem : GComponent
{
    GLoader loadIcon;
    GTextField txtName;
    GTextField txtHp;
    GTextField txtPDef;
    GTextField txtSDef;
    GTextField txtPAtk;
    GTextField txtSAtk;

    [Preserve]
    public IllustrateItem()
    {

    }

    public override void ConstructFromXML(FairyGUI.Utils.XML cxml)
    {
        base.ConstructFromXML(cxml);
        loadIcon = GetChild("loadIcon").asLoader;
        txtName = GetChild("txtName").asTextField;
        txtHp = GetChild("txtHp").asTextField;
        txtPDef = GetChild("txtPDef").asTextField;
        txtSDef = GetChild("txtSDef").asTextField;
        txtPAtk = GetChild("txtPAtk").asTextField;
        txtSAtk = GetChild("txtSAtk").asTextField;
    }

    public void updataItem(int id,IllustrateItemType type)
    {
        MonsterGoModel model = MonsterGoModel.Instance;
        if (type == IllustrateItemType.Monter)
        {
            loadIcon.url = "ui://MonsterGo/monsters_" + id;
            txtName.text = model.monsterConfig[id].name;
            txtHp.SetVar("value", model.monsterConfig[id].hp.ToString()).FlushVars();
            txtPDef.SetVar("value", model.monsterConfig[id].def.ToString()).FlushVars();
            txtSDef.SetVar("value", model.monsterConfig[id].sdef.ToString()).FlushVars();
            txtPAtk.SetVar("value", model.monsterConfig[id].att.ToString()).FlushVars();
            txtSAtk.SetVar("value", model.monsterConfig[id].satt.ToString()).FlushVars();
        }
        else if (type == IllustrateItemType.Hero)
        {
            loadIcon.url = "ui://MonsterGo/heroes_" + id;
            txtName.text = model.heroConfig[id].name;
            txtHp.SetVar("value", model.heroConfig[id].hp.ToString()).FlushVars();
            txtPDef.SetVar("value", model.heroConfig[id].def.ToString()).FlushVars();
            txtSDef.SetVar("value", model.heroConfig[id].sdef.ToString()).FlushVars();
            txtPAtk.SetVar("value", model.heroConfig[id].att.ToString()).FlushVars();
            txtSAtk.SetVar("value", model.heroConfig[id].satt.ToString()).FlushVars();
        }


    }
}
