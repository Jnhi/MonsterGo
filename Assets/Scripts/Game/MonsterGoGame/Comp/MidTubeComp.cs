using FairyGUI.Utils;
using FairyGUI;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System;
using GameFramework;
using UnityEngine.Scripting;

public class MidTubeComp : GComponent
{
    private List<GComponent> tubes = new();
    private List<Sequence> tubesAnim = new();

    private GTextField txtCombo;

    private GMovieClip movieCauldron;

    private Tween comboTween;
    [Preserve]
    public MidTubeComp()
    {
        
    }
    //如果你有需要访问容器内容的初始化工作，必须在这个方法里，而不是在构造函数里。各个SDK的函数原型的参数可能略有差别，请以代码提示为准。在Cocos2dx/CocosCreator里，方法的名字是onConstruct，且不带参数
    public override void ConstructFromXML(XML xml)
    {
        base.ConstructFromXML(xml);
        movieCauldron = GetChild("movieCauldron").asMovieClip;
        txtCombo = GetChild("txtCombo").asTextField;
        txtCombo.visible = false;
        Sequence sequence = AnimationTool.ChangeTextColorSeq(txtCombo);

    }

    public void Init()
    {
        tubesAnim.Clear();
        tubes.Clear();
        for (int i = 1; i < 7; i++)
        {
            GComponent tube = GetChild("tube" + i.ToString()).asCom;
            GSlider sld = tube.GetChild("sld").asSlider;
            GLoader icon = tube.GetChild("icon").asLoader;
            sld.value = 0;
            icon.url = null;
            Sequence quence = DOTween.Sequence();
            tubesAnim.Add(quence);
            tubes.Add(tube);
        }
        Log.Debug("初始化中间组件");
    }

    public override void Dispose()
    {
        base.Dispose();
    }

    /// <summary>
    /// 刷新连击次数
    /// </summary>
    /// <param name="combo">连击次数</param>
    public void RefreshCombo(int combo){
        if (combo < 2)
        {
            txtCombo.visible = false;
            
        }else
        {
            txtCombo.visible = true;
            if (comboTween != null)
            {
                comboTween.Kill();
            }
            txtCombo.y = 30;
            comboTween = AnimationTool.FlyUp(txtCombo, 20, 0.2f,()=>{
                UITool.DOTweenToDelay(0.3f,()=>{
                    txtCombo.visible = false;
                });
            });
        }
        txtCombo.text = "x" + combo.ToString();
    }

    /// <summary>
    /// 播放火炉动画
    /// </summary>
    public void PlayCauldron()
    {
        movieCauldron.visible = true;
        movieCauldron.SetPlaySettings(0, -1, 1, -1);
        movieCauldron.playing = true;
    }

    public void AddTubePercent(int tubeIndex, int generateMonsterID,Action cb = null)
    {
        MonsterGoModel model = MonsterGoModel.Instance;
        GComponent tube = tubes[tubeIndex];
        GSlider sld = tube.GetChild("sld").asSlider;
        GLoader icon = tube.GetChild("icon").asLoader;
        GMovieClip movieSpawn = tube.GetChild("movieSpawn").asMovieClip;

        icon.url = "ui://MonsterGo/monsters_" + model.tubeDatas[tubeIndex].monsterID;

        if (tubesAnim[tubeIndex] == null || !tubesAnim[tubeIndex].IsActive())
        {
            tubesAnim[tubeIndex] = DOTween.Sequence();
        }
        // 如果存在大于100%的情况 进度条先到100 然后执行生成怪物的动画 然后在到当前值
        if (generateMonsterID != -1)
        {

            //要飞出去的怪物图片
            GImage aImage = UIPackage.CreateObject("MonsterGo", "monsters_" + model.tubeDatas[tubeIndex].monsterID).asImage;
            GRoot.inst.AddChild(aImage);
            Vector2 iconScreenPos = icon.LocalToGlobal(Vector2.zero);
            Vector2 logicScreenPos = GRoot.inst.GlobalToLocal(iconScreenPos);
            aImage.x = logicScreenPos.x;
            aImage.y = logicScreenPos.y;
            aImage.width = 46;
            aImage.height = 46;
            aImage.visible = false;

            Tween tween1 = DOTween.To(() => sld.value, x => sld.value = x, 100, 0.2f).SetAutoKill(true);
            tween1.SetEase(Ease.Linear);
            tween1.OnComplete(() =>
            {
                //闪光特效
                Sequence sequence = AnimationTool.ChangeMovieColorSeq(movieSpawn);

                AudioManager.Instance.Play("MonsterGo/music/spawn.wav");
                movieSpawn.SetPlaySettings(0, -1, 1, -1);
                movieSpawn.visible = true;
                movieSpawn.playing = true;
                movieSpawn.onPlayEnd.Add(() =>
                {
                    aImage.visible = true;
                    //图片飞出去
                    DOTween.To(() => aImage.scale, x => aImage.scale = x, Vector2.zero, 0.2f).SetAutoKill(true);

                    aImage.TweenMove(new Vector2(0, 175), 0.5f).OnComplete(() =>
                    {
                        aImage.Dispose();
                        //需要增加怪物
                        cb?.Invoke();
                    });
                    sequence.Kill();
                    movieSpawn.visible = false;
                });
            });
            tubesAnim[tubeIndex].Append(tween1);



            Tween tween2 = DOTween.To(() => sld.value, x => sld.value = x, model.tubeDatas[tubeIndex].percentage, 0.2f).SetAutoKill(true);
            tween2.SetEase(Ease.Linear);
            tubesAnim[tubeIndex].Append(tween2);
        }
        else
        {
            //没有到100%
            Tween tween = DOTween.To(() => sld.value, x => sld.value = x, model.tubeDatas[tubeIndex].percentage, 0.2f);
            tween.SetEase(Ease.Linear);
            tubesAnim[tubeIndex].Append(tween);
        }
    }

    /// <summary>
    /// 获取试管物体
    /// </summary>
    /// <param name="monsterID"></param>
    /// <returns></returns>
    public GComponent GetTube(int monsterID){

        int tubeIndex = MonsterGoUtil.Instance.GetTubeIndexByMonsterID(monsterID);
        return this.tubes[tubeIndex];
    }
}