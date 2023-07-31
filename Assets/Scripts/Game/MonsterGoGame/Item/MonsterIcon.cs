using FairyGUI;
using UnityEngine.Scripting;

public class MonsterIcon : GComponent
{
    GLoader monsterIcon;
    
    [Preserve]
    public MonsterIcon()
    {
        
    }
    public override void ConstructFromXML(FairyGUI.Utils.XML cxml)
    {
        base.ConstructFromXML(cxml);
        monsterIcon = GetChild("icon").asLoader;
    }

    public void updataMonsterIcon(int monsterID)
    {
        monsterIcon.url = "ui://MonsterGo/monsters_" + monsterID;
    }
}
