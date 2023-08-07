using FairyGUI.Utils;
using FairyGUI;
using GameFramework;
using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

public class TopBattleComp : GComponent
{
    GLoader loadBg1;
    GLoader loadBg2;
    GLoader loadHero;
    GLoader loadMonster;
    GSlider prgBattle;
    GTextField txtMapName;
    GList monsterList;
    GGroup gMonster;
    GGroup gHero;
    GLoader loadMonsterAtkEffect;
    GLoader loadHeroAtkEffect;

    GTextField txtMHP;
    GTextField txtMATK;
    GTextField txtHHP;
    GTextField txtHATK;
    GGroup gBattleInfo;
    GGroup gMonsterInfo;
    GGroup gHeroInfo;
    GGroup gMap;

    public GMovieClip mcRock;
    GTextField txtMonsterDmg;
    GTextField txtHeroDmg;
    GTextField txtHeroNum;

    /// <summary>
    /// 怪物打架时起始坐标点
    /// </summary>
    float monsterBeginFightX;

    /// <summary>
    /// 英雄打架时起始坐标点
    /// </summary>
    float heroBeginFightX;

    /// <summary>
    /// 地板速度
    /// </summary>
    int GroundV = 10;

    /// <summary>
    /// 主游戏界面
    /// </summary>
    MonsterGoView monsterGoView = null;

    /// <summary>
    /// 战斗协程
    /// </summary>
    Coroutine doBattleAnimCoroutine = null;

    /// <summary>
    /// 地面移动协程
    /// </summary>
    Coroutine walkCoroutine = null;

    MonsterGoModel monsterGoModel = MonsterGoModel.Instance;
    MonsterGoUtil monsterGoUtil = MonsterGoUtil.Instance;

    [Preserve]
    public TopBattleComp()
    {

    }
    /// <summary>
    /// 战斗轮次
    /// </summary>
    int battleNo = 0;

    //如果你有需要访问容器内容的初始化工作，必须在这个方法里，而不是在构造函数里。各个SDK的函数原型的参数可能略有差别，请以代码提示为准。在Cocos2dx/CocosCreator里，方法的名字是onConstruct，且不带参数
    public override void ConstructFromXML(XML xml)
    {

        base.ConstructFromXML(xml);
        gMonster = GetChild("gMonster").asGroup;
        gHero = GetChild("gHero").asGroup;
        loadMonsterAtkEffect = GetChild("loadMonsterAtkEffect").asLoader;
        loadHeroAtkEffect = GetChild("loadHeroAtkEffect").asLoader;
        loadBg1 = GetChild("loadBg1").asLoader;
        loadBg2 = GetChild("loadBg2").asLoader;
        loadHero = GetChild("loadHero").asLoader;
        loadMonster = GetChild("loadMonster").asLoader;
        prgBattle = GetChild("prgBattle").asSlider;
        txtMapName = GetChild("txtMapName").asTextField;
        monsterList = GetChild("monsterList").asList;
        mcRock = GetChild("mcRock").asMovieClip;
        txtMHP = GetChild("txtMHP").asTextField;
        txtMATK = GetChild("txtMATK").asTextField;
        txtHHP = GetChild("txtHHP").asTextField;
        txtHATK = GetChild("txtHATK").asTextField;
        gBattleInfo = GetChild("gBattleInfo").asGroup;
        gMonsterInfo = GetChild("gMonsterInfo").asGroup;
        gHeroInfo = GetChild("gHeroInfo").asGroup;
        gMap = GetChild("gMap").asGroup;

        txtHeroNum = GetChild("txtHeroNum").asTextField;
        txtMonsterDmg = GetChild("txtMonsterDmg").asTextField;
        txtHeroDmg = GetChild("txtHeroDmg").asTextField;
        txtMonsterDmg.visible = false;
        txtHeroDmg.visible = false;
        monsterList.SetVirtual();

        monsterList.itemRenderer = RenderListItem;

        gMonster.x = -50;
        gMonster.x = -50;
        monsterBeginFightX = this.width / 2 - 20 - loadMonster.width;
        heroBeginFightX = this.width / 2 + 20;
        gBattleInfo.visible = false;
        mcRock.visible = false;
    }

    void RenderListItem(int index, GObject obj)
    {
        MonsterIcon item = (MonsterIcon)obj;
        item.updataMonsterIcon(monsterGoModel.monsterBattleStack.ToArray()[index].id);
    }

    public override void Dispose()
    {
        base.Dispose();
    }

    /// <summary>
    /// 显示岩石障碍
    /// </summary>
    public void ShowRock(){
        mcRock.visible = true;
        mcRock.SetPlaySettings(0, -1, 1, -1);
        mcRock.playing = true;
    }

    /// <summary>
    /// 隐藏岩石障碍
    /// </summary>
    public void HideRock(){
        mcRock.visible = false;
        mcRock.playing = false;
    }

    /// <summary>
    /// 初始化顶部组件 生成第一个英雄
    /// </summary>
    /// <param name="monsterGoView"></param>
    public void Init(MonsterGoView monsterGoView)
    {
        this.monsterGoView = monsterGoView;
        prgBattle.value = 0;
        string levelName = monsterGoModel.levelConfig[monsterGoModel.curLevelIndex].name;
        txtMapName.text = levelName;

        txtHeroNum.text = "x" + monsterGoModel.heroBattleStack.Count.ToString();
        Hero curHero = monsterGoUtil.getNextHero();
        loadHero.url = "ui://MonsterGo/heroes_" + curHero.id;
        loadBg1.url = "ui://MonsterGo/environments_" + monsterGoModel.curLevelIndex;
        loadBg2.url = "ui://MonsterGo/environments_" + monsterGoModel.curLevelIndex;
        RefreshBattleInfo();
    }


    /// <summary>
    /// 添加一个怪物
    /// </summary>
    /// <param name="monsterID"></param>
    public void AddMonster(int monsterID)
    {
        Monster monster = new Monster(monsterID);
        monsterGoModel.monsterBattleStack.Push(monster);
        monsterList.numItems = monsterGoModel.monsterBattleStack.Count;

        Log.Debug($"增加怪物ID：{monsterID}");

        if (doBattleAnimCoroutine == null)
        {
            doBattleAnimCoroutine = CoroutineManager.Instance.StartCoroutine(DoBattleAnim());
        }
    }

    /// <summary>
    /// 播放攻击动画
    /// </summary>
    /// <param name="effectLoader"></param>
    /// <param name="attackEffectType"></param>
    public void PlayAttackEffect(GLoader effectLoader, AttackEffectType attackEffectType)
    {
        effectLoader.visible = true;
        string url = "";
        if (attackEffectType == AttackEffectType.pAttack)
        {
            url = "ui://MonsterGo/pAttack";
        }
        else if (attackEffectType == AttackEffectType.sAttack)
        {
            url = "ui://MonsterGo/sAttack";
        }
        else if (attackEffectType == AttackEffectType.pDefend)
        {
            url = "ui://MonsterGo/pDefend";

        }
        else if (attackEffectType == AttackEffectType.sDefend)
        {
            url = "ui://MonsterGo/sDefend";
        }
        effectLoader.url = url;
        effectLoader.movieClip.SetPlaySettings(0, -1, 1, -1);
        effectLoader.visible = true;
        effectLoader.movieClip.playing = true;
        effectLoader.movieClip.onPlayEnd.Add(() =>
        {
            effectLoader.visible = false;
        });
    }

    /// <summary>
    /// 展示伤害数字
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="dmg"></param>
    public void showDamage(GTextField txt, float dmg)
    {
        txt.visible = true;
        txt.text = dmg.ToString();
        AnimationTool.FlyUp(txt, 20, 0.5f, () =>
        {
            UITool.DOTweenToDelay(0.2f, () =>
            {
                txt.visible = false;
                txt.y = 28;
            });
        }, Ease.OutBack);
    }

    public IEnumerator AttackAnim(Monster monster, Hero hero)
    {
        battleNo += 1;
        int mRoll = 0;
        int hRoll = 0;

        // 怪物最终受到伤害值
        int mDmg = 0;

        // 英雄最终受到伤害值
        int hDmg = 0;

        //怪物受到的物理攻击伤害值
        int mPDmg = -1;
        //怪物受到的特殊攻击伤害值
        int mSDmg = -1;
        if (hero.att != 0)
        {
            mPDmg = 1;
            hRoll = Random.Range(1, 20) + hero.att;
            mRoll = monster.def + 10;
            if (hRoll >= mRoll)
            {
                int t = hero.att - monster.def;
                mPDmg = t > 1 ? t : 1;
            }
        }
        if (hero.satt != 0)
        {
            mSDmg = 1;
            hRoll = Random.Range(1, 20) + hero.satt;
            mRoll = monster.sdef + 10;
            if (hRoll >= mRoll)
            {
                int t = hero.satt - monster.sdef;
                mSDmg = t > 1 ? t : 1;
            }
        }
        //英雄受到的物理攻击伤害值
        int hPDmg = -1;
        //英雄受到的特殊攻击伤害值
        int hSDmg = -1;
        if (monster.att != 0)
        {
            hPDmg = 0;
            mRoll = Random.Range(1, 20) + monster.att;
            hRoll = hero.def + 10;
            if (mRoll >= hRoll)
            {
                int t = monster.att - hero.def;
                hPDmg = t > 1 ? t : 1;
            }
        }
        if (monster.satt != 0)
        {
            hSDmg = 0;
            mRoll = Random.Range(1, 20) + monster.satt;
            hRoll = hero.sdef + 10;
            if (mRoll >= hRoll)
            {
                int t = monster.satt - hero.sdef;
                hSDmg = t > 1 ? t : 1;
            }
        }

        bool monsterReady = false;

        DOTween.To(() => gMonster.x, x => gMonster.x = x, monsterBeginFightX + 20, 0.5f).SetEase(Ease.InBack).SetAutoKill(true).OnComplete(() =>
        {
            if (mSDmg < 0)
            {
                if (mPDmg == 0)
                {
                    mDmg = 0;
                    PlayAttackEffect(loadMonsterAtkEffect, AttackEffectType.pDefend);
                    AudioManager.Instance.Play("MonsterGo/music/block.wav");
                }
                else if (mPDmg > 0)
                {
                    mDmg = mPDmg;
                    PlayAttackEffect(loadMonsterAtkEffect, AttackEffectType.pAttack);
                    AudioManager.Instance.Play("MonsterGo/music/damage.wav");
                }
            }
            else if (mSDmg == 0)
            {
                mDmg = 0;
                PlayAttackEffect(loadMonsterAtkEffect, AttackEffectType.sDefend);
                AudioManager.Instance.Play("MonsterGo/music/block.wav");
            }
            else if (mSDmg > 0)
            {
                mDmg = mSDmg;
                PlayAttackEffect(loadMonsterAtkEffect, AttackEffectType.sAttack);
                AudioManager.Instance.Play("MonsterGo/music/damage.wav");
            }
            monster.hp -= mDmg;
            showDamage(txtMonsterDmg, mDmg);
            Log.Debug($"轮次{battleNo}｜怪物受到物理{mPDmg},特殊{mSDmg},血量{monster.hp}");
            if (monster.hp > 0)
            {
                DOTween.To(() => gMonster.x, x => gMonster.x = x, monsterBeginFightX, 0.5f).SetAutoKill(true).OnComplete(() =>
                {
                    monsterReady = true;
                });
            }
            else
            {
                DOTween.To(() => gMonster.alpha, x => gMonster.alpha = x, 0, 0.2f).SetAutoKill(true).OnComplete(() =>
                {
                    gMonster.x = -50;
                    monsterReady = true;
                });
            }
        });

        bool heroReady = false;

        DOTween.To(() => gHero.x, x => gHero.x = x, heroBeginFightX - 20, 0.5f).SetEase(Ease.InBack).SetAutoKill(true).OnComplete(() =>
        {
            if (hSDmg < 0)
            {
                if (hPDmg == 0)
                {
                    hDmg = 0;
                    PlayAttackEffect(loadHeroAtkEffect, AttackEffectType.pDefend);
                    AudioManager.Instance.Play("MonsterGo/music/block.wav");

                }
                else if (hPDmg > 0)
                {
                    hDmg = hPDmg;
                    PlayAttackEffect(loadHeroAtkEffect, AttackEffectType.pAttack);
                    AudioManager.Instance.Play("MonsterGo/music/damage.wav");

                }
            }
            else if (hSDmg == 0)
            {
                hDmg = 0;
                PlayAttackEffect(loadHeroAtkEffect, AttackEffectType.sDefend);
                AudioManager.Instance.Play("MonsterGo/music/block.wav");
            }
            else if (hSDmg > 0)
            {
                hDmg = hSDmg;
                PlayAttackEffect(loadHeroAtkEffect, AttackEffectType.sAttack);
                AudioManager.Instance.Play("MonsterGo/music/damage.wav");
            }
            showDamage(txtHeroDmg, hDmg);
            hero.hp -= hDmg;
            Log.Debug($"轮次{battleNo}｜英雄受到物理{hPDmg},特殊{hSDmg},血量{hero.hp}");
            if (hero.hp <= 0)
            {
                DOTween.To(() => gHero.alpha, x => gHero.alpha = x, 0, 0.2f).SetAutoKill(true).OnComplete(() =>
                {
                    gHero.x = 400;
                    Hero curHero = monsterGoUtil.getNextHero();
                    txtHeroNum.text = "x" + (monsterGoModel.heroBattleStack.Count + 1).ToString();
                    if (curHero != null)
                    {
                        loadHero.url = "ui://MonsterGo/heroes_" + curHero.id;
                        DOTween.To(() => gHero.alpha, x => gHero.alpha = x, 1, 0.1f).SetAutoKill(true);
                        DOTween.To(() => gHero.x, x => gHero.x = x, heroBeginFightX, 0.2f).SetAutoKill(true).OnComplete(() =>
                        {
                            heroReady = true;
                        });
                    }
                    else
                    {
                        monsterGoView.GameOver(true);
                    }

                });
            }
            else
            {
                DOTween.To(() => gHero.x, x => gHero.x = x, heroBeginFightX, 0.5f).SetAutoKill(true).OnComplete(() =>
                {
                    heroReady = true;
                });
            }

        });

        RefreshBattleInfo();

        yield return new WaitUntil(() => monsterReady && heroReady);
        yield return new WaitForSeconds(0.2f);


        if (monster.hp > 0)
        {
            yield return AttackAnim(monster, monsterGoModel.curHero);
        }
    }


    /// <summary>
    /// 刷新战斗信息 血量 攻击信息
    /// </summary>
    /// <returns></returns>
    private void RefreshBattleInfo()
    {
        if (monsterGoModel.curMonster != null)
        {
            gMonsterInfo.visible = true;
            txtMHP.text = $"血量：{monsterGoModel.curMonster.hp.ToString()}";
            txtMATK.text = $"攻击：{(monsterGoModel.curMonster.att + monsterGoModel.curMonster.satt).ToString()}";
        }
        else
        {
            gMonsterInfo.visible = false;
        }
        if (monsterGoModel.curHero != null)
        {
            gHeroInfo.visible = true;
            txtHHP.text = $"血量：{monsterGoModel.curHero.hp.ToString()}";
            txtHATK.text = $"攻击：{monsterGoModel.curHero.att.ToString()}";
        }
        else
        {
            gHeroInfo.visible = false;
        }
    }

    /// <summary>
    /// 怪物战斗动画
    /// </summary>
    /// <returns></returns>
    public IEnumerator DoAnim(Monster monster)
    {

        monsterList.numItems = monsterGoModel.monsterBattleStack.Count;

        RefreshBattleInfo();

        loadMonster.url = "ui://MonsterGo/monsters_" + monster.id;
        loadMonster.alpha = 0;
        // 怪物从出生点到战斗准备点
        Log.Debug($"怪物从出生点到战斗准备点");
        DOTween.To(() => gMonster.alpha, x => gMonster.alpha = x, 1, 0.1f).SetAutoKill(true);

        yield return DOTween.To(() => gMonster.x, x => gMonster.x = x, monsterBeginFightX, 1).SetAutoKill(true).WaitForCompletion();

        Log.Debug($"初始怪物血量{monster.hp}");
        yield return AttackAnim(monster, monsterGoModel.curHero);

        if (monsterGoModel.monsterBattleStack.Count > 0)
        {
            Monster nextMonster = monsterGoUtil.GetNextMonster();
            yield return DoAnim(nextMonster);
        }

    }

    public void PauseBattle(){
        DOTween.PauseAll();
    }

    public void reStertBattle(){
        DOTween.PlayAll();
    }

    /// <summary>
    /// 战斗动画的入口
    /// </summary>
    /// <returns></returns>
    public IEnumerator DoBattleAnim()
    {
        if (monsterGoModel.monsterBattleStack.Count > 0)
        {
            monsterGoModel.isBattle = true;
            Monster monster = monsterGoUtil.GetNextMonster();
            yield return DoAnim(monster);
        }

        CoroutineManager.Instance.StopCoroutine(doBattleAnimCoroutine);
        doBattleAnimCoroutine = null;
        monsterGoModel.isBattle = false;
    }

    /// <summary>
    /// 初始化为原来状态
    /// </summary>
    public void NextLevel()
    {
        loadBg1.url = "ui://MonsterGo/environments_" + monsterGoModel.curLevelIndex;
        loadBg2.url = "ui://MonsterGo/environments_" + monsterGoModel.curLevelIndex;
        string levelName = monsterGoModel.levelConfig[monsterGoModel.curLevelIndex].name;
        txtMapName.text = levelName;
        prgBattle.value = 0;
        gMonster.alpha = 0;
        gMonster.x = -61;
        monsterGoModel.monsterBattleStack.Clear();
    }

    /// <summary>
    /// 开始移动
    /// </summary>
    public void StartWalk()
    {
        if (walkCoroutine == null)
        {
            walkCoroutine = CoroutineManager.Instance.StartCoroutine(CoroutineWalk());
        }
    }


    /// <summary>
    /// 暂停移动
    /// </summary>
    public void StopWalk()
    {
        if (walkCoroutine != null)
        {
            CoroutineManager.Instance.StopCoroutine(walkCoroutine);
            walkCoroutine = null;
        }
    }

    private IEnumerator CoroutineWalk()
    {
        float topBgTimer = 0; // 上方背景计时器
        while (true)
        {
            while (monsterGoModel.isPause)
            {
                yield return null;
            }

            if (monsterGoModel.magicBarrierTime > 0)
            {
                monsterGoModel.magicBarrierTime -= Time.deltaTime;
                // Log.Debug(monsterGoModel.magicBarrierTime.ToString());
                if (monsterGoModel.magicBarrierTime <= 0)
                {
                    HideRock();
                }
            }
            else
            {
            //处理关卡倒计时
                topBgTimer += Time.deltaTime;

                if (topBgTimer >= monsterGoModel.intervalWalkTime)
                {
                    while (monsterGoModel.isBattle)
                    {
                        yield return null;
                    }
                    monsterGoModel.curStepNum += 1;
                    yield return Walk();
                    topBgTimer = 0;
                    if (monsterGoModel.curStepNum >= monsterGoUtil.GetCurLevelMaxWalkStep() && monsterGoView.isMoving != true)
                    {
                        monsterGoModel.curLevelIndex += 1;
                        if (monsterGoModel.curLevelIndex < monsterGoModel.levelConfig.Length)
                        {
                            //下一关
                            CoroutineManager.Instance.StartCoroutine(monsterGoView.NextLevel());
                            yield break;
                        }
                        else
                        {
                            //游戏结束
                            monsterGoView.GameOver(false);
                            yield break;
                        }
                    }
                }
            }
            yield return null;
        }
    }

    public IEnumerator Walk()
    {
        yield return gMap.TweenMoveX(gMap.x + GroundV, 0.4f).SetEase(EaseType.BackOut);

        if (loadBg2.x >= 600)
        {
            loadBg1.x = -(loadBg2.width - loadBg2.x);
        }
        if (loadBg1.x >= 1300)
        {
            loadBg2.x = -(loadBg1.width - loadBg1.x);
        }
        string levelName = monsterGoModel.levelConfig[monsterGoModel.curLevelIndex].name;
        prgBattle.value = ((float)monsterGoModel.curStepNum / (float)monsterGoUtil.GetCurLevelMaxWalkStep()) * 100;
    }
}