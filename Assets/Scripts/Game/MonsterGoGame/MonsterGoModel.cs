using System;
using GameFramework;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MonsterGoModel : Singleton<MonsterGoModel>
{
    public class TubeData
    {
        /// <summary>
        /// 怪物ID
        /// </summary>
        public int monsterID;
        /// <summary>
        /// 当前试管百分百
        /// </summary>
        public int percentage;
    }
    /// <summary>
    /// 怪物移动间隔（秒）
    /// </summary>
    public float intervalWalkTime = 1f;

    public LevelConfig[] levelConfig;
    public Monster[] monsterConfig;
    public Hero[] heroConfig;


    /// <summary>
    /// 是否战斗中
    /// </summary>
    public bool isBattle = false;

    /// <summary>
    /// 是否暂停
    /// </summary>
    public bool isPause = false;

    /// <summary>
    /// 魔法屏障持续时间
    /// </summary>
    public float magicBarrierTime = 0;

    /// <summary>
    /// 当前连击次数
    /// </summary>
    public int curComboNum = 0;

    /// <summary>
    /// 分数
    /// </summary>
    public int score = 0;

    /// <summary>
    /// 试管ID，试管数据
    /// </summary>
    public Dictionary<int, TubeData> tubeDatas = new Dictionary<int, TubeData>();

    /// <summary>
    /// 当前关卡
    /// </summary>
    public int curLevelIndex = 0;

    /// <summary>
    /// 当前走了几步
    /// </summary>
    public int curStepNum = 0;

    /// <summary>
    /// 英雄ID
    /// </summary>
    public int heroID = 1;

    /// <summary>
    /// 准备战斗的怪物栈
    /// </summary>
    /// <typeparam name="Monster"></typeparam>
    /// <returns></returns>
    public Stack<Monster> monsterBattleStack = new Stack<Monster>();
    /// <summary>
    /// 正在战斗的怪物
    /// </summary>
    public Monster curMonster;

    /// <summary>
    /// 准备战斗的英雄栈
    /// </summary>
    /// <typeparam name="Hero"></typeparam>
    /// <returns></returns>
    public Stack<Hero> heroBattleStack = new Stack<Hero>();

    /// <summary>
    /// 正在战斗的英雄
    /// </summary>
    public Hero curHero;
    public async Task InitConfig()
    {
        levelConfig = await ResourceManager.Instance.LoadJson<LevelConfig[]>("MonsterGo/LevelConfig.json");
        monsterConfig = await ResourceManager.Instance.LoadJson<Monster[]>("MonsterGo/MonsterConfig.json");
        heroConfig = await ResourceManager.Instance.LoadJson<Hero[]>("MonsterGo/HeroConfig.json");
    }

    /// <summary>
    /// 初始化英雄数据
    /// </summary>
    public void InitHeroData()
    {
        for (int i = heroConfig.Length - 1; i >= 0; i--)
        {
            Hero hero = heroConfig[i];
            heroBattleStack.Push(hero);
        }
    }

    /// <summary>
    /// 初始化游戏数据
    /// </summary>
    public void InitGameData()
    {
        heroBattleStack.Clear();
        InitHeroData();
        monsterBattleStack.Clear();
        tubeDatas.Clear();
        curStepNum = 0;
        heroID = 0;
        score = 0;
        curLevelIndex = 0;
        magicBarrierTime = 0;
        curMonster = null;
        curHero = null;
    }

    /// <summary>
    /// 下一关
    /// </summary>
    public void NextLevel()
    {
        foreach (var item in tubeDatas)
        {
            magicBarrierTime += item.Value.percentage/25;
        }
        Log.Debug($"魔法屏障持续时间：{magicBarrierTime}");
        monsterBattleStack.Clear();
        tubeDatas.Clear();
        curStepNum = 0;
    }

    

    /// <summary>
    /// 增加试管数据
    /// </summary>
    public (int, int) AddTubeData(int percentage, int monsterID)
    {
        bool isHave = false;
        int tubeIndex = 0;
        int generateMonsterID = -1;
        foreach (int key in tubeDatas.Keys)
        {
            TubeData tubeData = tubeDatas[key];
            if (monsterID == tubeData.monsterID)
            {
                isHave = true;
                tubeData.percentage += percentage;
                if (tubeData.percentage >= 100)
                {
                    generateMonsterID = monsterID;
                    tubeData.percentage -= 100;
                }
                tubeIndex = key;
                break;
            }
        }
        if (!isHave)
        {
            TubeData tubeData = new TubeData();
            tubeData.monsterID = monsterID;
            tubeData.percentage = percentage;
            tubeIndex = tubeDatas.Keys.Count;
            tubeDatas.Add(tubeDatas.Keys.Count, tubeData);
        }

        return (tubeIndex, generateMonsterID);
    }
}
