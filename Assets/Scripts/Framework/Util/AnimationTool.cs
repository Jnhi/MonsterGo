using DG.Tweening;
using FairyGUI;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class AnimationTool
{

    /// <summary>
    /// 快速改变图片颜色
    /// </summary>
    /// <param name="text">需要改变颜色的文本组件</param>
    /// <param name="changeColors">颜色数组</param>
    /// <param name="changeTime">每一个颜色的间隔时间</param>
    public static Sequence ChangeMovieColorSeq(GMovieClip obj,float changeTime = 0.05f, Color[] changeColors = null)
    {
        if (changeColors == null)
        {
            changeColors = new Color[8] { 
                Color.yellow,
                Color.white,
                Color.green,
                Color.white,
                Color.magenta,
                Color.red,
                Color.blue,
                Color.cyan,
            };
        }
        // 创建一个序列动画
        Sequence sequence = DOTween.Sequence();

        // 循环添加颜色变换的Tween动画
        for (int i = 0; i < changeColors.Length; i++)
        {
            // 添加Tween动画，设置目标颜色和持续时间
            sequence.Append(DOTween.To(() => obj.color, x => obj.color = x, changeColors[i], changeTime).SetAutoKill(true));

        }
        // 设置循环播放
        sequence.SetLoops(-1);

        return sequence;
    }

    /// <summary>
    /// 快速改变文本颜色
    /// </summary>
    /// <param name="text">需要改变颜色的文本组件</param>
    /// <param name="changeColors">颜色数组</param>
    /// <param name="changeTime">每一个颜色的间隔时间</param>
    public static Sequence ChangeTextColorSeq(GTextField text,float changeTime = 0.05f, Color[] changeColors = null)
    {
        if (changeColors == null)
        {
            changeColors = new Color[8] { 
                Color.yellow,
                Color.white,
                Color.green,
                Color.white,
                Color.magenta,
                Color.red,
                Color.blue,
                Color.cyan,
            };
        }
        // 创建一个序列动画
        Sequence sequence = DOTween.Sequence();

        // 循环添加颜色变换的Tween动画
        for (int i = 0; i < changeColors.Length; i++)
        {
            // 添加Tween动画，设置目标颜色和持续时间
            sequence.Append(DOTween.To(() => text.color, x => text.color = x, changeColors[i], changeTime).SetAutoKill(true));

        }
        // 设置循环播放
        sequence.SetLoops(-1);

        return sequence;
    }

    /// <summary>
    /// 上移物体
    /// </summary>
    /// <param name="obj">物体</param>
    /// <param name="distance">距离</param>
    /// <param name="time">持续时间</param>
    /// <returns></returns>
    public static Tween FlyUp(GObject obj, float distance, float time,Action cb = null,Ease type = Ease.Linear)
    {

        return DOTween.To(() => obj.y, x => obj.y = x, obj.y - distance, time).SetAutoKill(true).OnComplete(() =>
        {
            cb?.Invoke();
        }).SetEase(type);
    }

    public static Sequence BlinkingEffect(GObject obj,float time,bool isLoop = true,Action cb = null)
    {
        // 创建一个序列动画
        Sequence sequence = DOTween.Sequence();

        sequence.Append(DOTween.To(() => obj.alpha, x => obj.alpha = x, 0, time).SetAutoKill(true));
        sequence.Append(DOTween.To(() => obj.alpha, x => obj.alpha = x, 1, time).SetAutoKill(true));

        if (isLoop)
        {
            // 设置循环播放
            sequence.SetLoops(-1);
            return sequence;
        }
        else
        {
            return sequence.OnComplete(()=>{
                cb?.Invoke();
            });
        }
    }

    public static void KillAnim(object obj){
        DOTween.Kill(obj);
    }
}
