using System;
/// <summary>
/// 生成方块类型
/// </summary>
public enum BornType
{
    /// <summary>
    /// 下落动画
    /// </summary>
    Fall,
    /// <summary>
    /// 在格子中出现
    /// </summary>
    ShowGrid,
    /// <summary>
    /// 没有任何效果
    /// </summary>
    Normal
}

/// <summary>
/// 攻击效果枚举
/// </summary>
public enum AttackEffectType
{
    /// <summary>
    /// 物理攻击
    /// </summary>
    pAttack,
    /// <summary>
    /// 特殊攻击
    /// </summary>
    sAttack,
    /// <summary>
    /// 物理防御
    /// </summary>
    pDefend,
    /// <summary>
    /// 特殊防御
    /// </summary>
    sDefend,
}

/// <summary>
/// 图鉴Item类型
/// </summary>
public enum IllustrateItemType
{
    /// <summary>
    /// 怪物
    /// </summary>
    Monter,
    /// <summary>
    /// 英雄
    /// </summary>
    Hero,
}

public class GameOverParams
{
    public bool isWin = false;
    public Action reStartFunc;
    public Action exitFunc;
}
