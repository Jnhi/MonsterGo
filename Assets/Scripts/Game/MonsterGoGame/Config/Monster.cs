[System.Serializable]
public class Monster
{
    /// <summary>
    /// 怪物ID
    /// </summary>
    public int id;

    /// <summary>
    /// 怪物名称
    /// </summary>
    public string name;

    /// <summary>
    /// 攻击力
    /// </summary>
    public int att;

    /// <summary>
    /// 特殊攻击
    /// </summary>
    public int satt;

    /// <summary>
    /// 血量
    /// </summary>
    public int hp;

    /// <summary>
    /// 防御
    /// </summary>
    public int def;

    /// <summary>
    /// 特殊防御
    /// </summary>
    public int sdef;

    /// <summary>
    /// 怪物对应的颜色
    /// </summary>
    public string color;

    public Monster()
    {
        // 默认构造函数
    }
    public Monster(int monsterID)
    {
        MonsterGoModel model = MonsterGoModel.Instance;

        this.id = monsterID;
        this.name = model.monsterConfig[monsterID].name;
        this.att = model.monsterConfig[monsterID].att;
        this.hp = model.monsterConfig[monsterID].hp;
        this.def = model.monsterConfig[monsterID].def;
        this.satt = model.monsterConfig[monsterID].satt;
        this.color = model.monsterConfig[monsterID].color;
    }

    public void Attack()
    {

    }
}