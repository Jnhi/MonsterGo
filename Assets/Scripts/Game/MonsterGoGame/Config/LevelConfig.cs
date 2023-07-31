[System.Serializable]
public class LevelConfig
{
    /// <summary>
    /// 关卡名称
    /// </summary>
    public string name;
    /// <summary>
    /// 关卡ID
    /// </summary>
    public int id;
    /// <summary>
    /// 出现的怪物ID
    /// </summary>
    public int[] monster;
    /// <summary>
    /// 生成方块的间隔s
    /// </summary>
    public float generateTime;
    /// <summary>
    /// 关卡游戏时间
    /// </summary>
    public int maxWalkStep;

    public LevelConfig()
    {
        // 默认构造函数
    }
}
