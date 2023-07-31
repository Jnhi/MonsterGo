using FairyGUI;
using GameFramework;
using System;
using System.Collections;
using UnityEngine;

public class Block
{

    public bool isMatchX = false;
    public bool isMatchY = false;

    /// <summary>
    /// 方块的类型
    /// </summary>
    public int type;

    /// <summary>
    /// 横坐标
    /// </summary>
    public int row;

    /// <summary>
    /// 纵坐标
    /// </summary>
    public int col;

    /// <summary>
    /// 选中后执行的方法
    /// </summary>
    public Action SelectFunc;

    /// <summary>
    /// 是否被选中
    /// </summary>
    private bool isSelected = false;

    public GMovieClip aMovie;
    private GLoader icon;
    // 方块物体
    public GComponent obj;
    public Block(GComponent parent, Vector3 position, int type)
    {
        obj = UIPackage.CreateObject("MonsterGo", "Block").asCom;
        parent.AddChild(obj);

        icon = obj.GetChild("icon").asLoader;
        icon.url = "ui://MonsterGo/monsters_" + type;

        obj.onTouchBegin.Add(() =>
        {
            SelectFunc();
        });

        obj.GetChild("imgSelect").visible = false;

        aMovie = obj.GetChild("anim").asMovieClip;

        this.type = type;
        obj.position = position;
    }
    public void Select()
    {
        AudioManager.Instance.Play("MonsterGo/music/click.wav");
        isSelected = !isSelected;
        obj.GetChild("imgSelect").visible = isSelected;
    }


    public void Swap(Block otherBlock, Action callback)
    {
        AudioManager.Instance.Play("MonsterGo/music/swap.wav");
        int tempRow = row;
        int tempCol = col;
        row = otherBlock.row;
        col = otherBlock.col;
        otherBlock.row = tempRow;
        otherBlock.col = tempCol;
        Vector3 selectPos = obj.position;
        GTweener gt1 = obj.TweenMove(otherBlock.obj.position, 0.1f);
        GTweener gt2 = otherBlock.obj.TweenMove(selectPos, 0.1f);

        obj.gameObjectName = $"{row}-{col}-{type}";
        // 注册第一个动画完成时的回调
        gt1.OnComplete(() =>
        {
            // 注册第二个动画完成时的回调
            gt2.OnComplete(() =>
            {
                // 动画完成后的逻辑
                // 调用回调函数
                callback?.Invoke();
            });
        });
    }

    /// <summary>
    /// 生成动画
    /// </summary>
    /// <returns></returns>
    public IEnumerator BornAnim(BornType Type = BornType.ShowGrid)
    {
        if (Type == BornType.ShowGrid)
        {
            obj.scale = new Vector2((float)0.1, (float)0.1);
            yield return obj.TweenScale(new Vector2(1, 1), 0.2f);
        }
        else if(Type == BornType.Fall)
        {
            float tempY = obj.y;
            obj.y = obj.y - 300;
            yield return obj.TweenMoveY(tempY, 0.2f);
        }
    }

    /// <summary>
    /// 删除方块的时候调用
    /// </summary>
    /// <returns></returns>
    public IEnumerator Del()
    {
        bool isAnimationComplete = false;
        aMovie.visible = true;
        aMovie.SetPlaySettings(0, -1, 1, -1);
        aMovie.playing = true;
        aMovie.onPlayEnd.Add(() =>
        {
            obj.Dispose();
            isAnimationComplete = true;
        });
        // 等待动画播放完成
        yield return new WaitUntil(() => isAnimationComplete);
    }

    /// <summary>
    /// 删除方块的方法
    /// </summary>
    /// <returns></returns>
    public IEnumerator Eliminate()
    {
        bool isAnimationComplete = false;
        aMovie.visible = true;
        aMovie.SetPlaySettings(0, -1, 1, -1);
        aMovie.playing = true;
        aMovie.onPlayEnd.Add(() =>
        {
            obj.Dispose();
            isAnimationComplete = true;
        });
        // 等待动画播放完成
        yield return new WaitUntil(() => isAnimationComplete);
    }

    public IEnumerator Move(GComponent emptyGrid)
    {
        bool isAnimationComplete = false;
        GTweener gt1 = obj.TweenMove(emptyGrid.position, 0.2f);
        gt1.OnComplete(() =>
        {
            isAnimationComplete = true;
        });
        yield return new WaitUntil(() => isAnimationComplete);

    }
}