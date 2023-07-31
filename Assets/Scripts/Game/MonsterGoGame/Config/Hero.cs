using System;

[System.Serializable]

public class Hero
{
    /// <summary>
    /// 英雄ID
    /// </summary>
    public int id;

    /// <summary>
    /// 英雄名称
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

    public Hero()
    {
        // 默认构造函数
    }
    public Hero(int heroID)
    {
        MonsterGoModel model = MonsterGoModel.Instance;

        this.id = heroID;
        this.name = model.heroConfig[heroID].name;
        this.att = model.heroConfig[heroID].att;
        this.hp = model.heroConfig[heroID].hp;
        this.def = model.heroConfig[heroID].def;
        this.satt = model.heroConfig[heroID].satt;
    }
}