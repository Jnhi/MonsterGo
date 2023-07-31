using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using static MonsterGoModel;

public class MonsterGoUtil : Singleton<MonsterGoUtil>
{

    MonsterGoModel monsterModel = MonsterGoModel.Instance;

    /// <summary>
    /// 获取一个当前关卡随机怪物ID
    /// </summary>
    /// <returns></returns>
    public int GetRandomMonsterID()
    {
        int[] curMonsterData = monsterModel.levelConfig[monsterModel.curLevelIndex].monster;

        return curMonsterData[UnityEngine.Random.Range(0, curMonsterData.Length)];
    }

    /// <summary>
    /// 随机获取怪物ID 排除掉某些值
    /// </summary>
    /// <returns></returns>
    public int GetExcludeRandomMonsterID(int[] exclude)
    {
        List<int> t = new List<int>();
        int[] curMonsterData = monsterModel.levelConfig[monsterModel.curLevelIndex].monster;
        for (int i = 0; i < curMonsterData.Length; i++)
        {
            int id = curMonsterData[i];
            if (!exclude.Contains(id))
            {
                t.Add(id);
            }
        }

        if (t.Count > 0)
        {
            return t[UnityEngine.Random.Range(0, t.Count)];
        }
        else
        {
            return 1;
        }
    }


    /// <summary>
    /// 获取对应的分数以及百分比
    /// </summary>
    /// <param name="blockList">需要消除的方块组</param>
    /// <param name="simultaneousCount">同时有几个组需要被消除</param>
    /// <returns></returns>
    public (int, int, int) GetEliminateScore(MatchesData blockList, int simultaneousCount)
    {
        int percentage = 0;
        int monsterID = blockList.monsterID;
        int score = 0;

        int amount = (int)(math.pow(blockList.num - 3, 2) + 1);

        percentage = (amount * 10 + (simultaneousCount - 1) * 5);
        score = percentage;
        // 如果存在连击次数
        if (monsterModel.curComboNum >= 2)
        {
            percentage *= monsterModel.curComboNum;
        }
        return (percentage, score, monsterID);
    }

    /// <summary>
    /// 获取当前关卡最大步数
    /// </summary>
    /// <returns></returns>
    public int GetCurLevelMaxWalkStep()
    {
        return monsterModel.levelConfig[monsterModel.curLevelIndex].maxWalkStep;
    }

    /// <summary>
    /// 获取需要战斗的怪物ID，并且设置为当前怪物
    /// </summary>
    /// <returns></returns>
    public Monster GetNextMonster()
    {
        Monster monster = null;
        if (monsterModel.monsterBattleStack.Count > 0)
        {
            monster = monsterModel.monsterBattleStack.Pop();
        }
        monsterModel.curMonster = monster;

        return monster;
    }

    /// <summary>
    /// 获取需要战斗的英雄ID，并且设置为当前英雄
    /// </summary>
    /// <returns></returns>
    public Hero getNextHero()
    {
        Hero hero = null;
        if (monsterModel.heroBattleStack.Count > 0)
        {
            hero = monsterModel.heroBattleStack.Pop();
        }
        monsterModel.curHero = hero;
        return hero;
    }

    /// <summary>
    /// 根据怪物ID获取对应的试管ID
    /// </summary>
    /// <param name="monsterID"></param>
    /// <returns></returns>
    public int GetTubeIndexByMonsterID(int monsterID)
    {
        foreach (int key in monsterModel.tubeDatas.Keys)
        {
            TubeData tubeData = monsterModel.tubeDatas[key];
            if (monsterID == tubeData.monsterID)
            {
                return key;
            }
        }
        return 0;
    }

    /// <summary>
    /// 根据怪物ID获取对应的颜色
    /// </summary>
    /// <param name="monsterID"></param>
    /// <returns></returns>
    public string GetMonsterColorByMonsterID(int monsterID)
    {
        return monsterModel.monsterConfig[monsterID].color;
    }
}
