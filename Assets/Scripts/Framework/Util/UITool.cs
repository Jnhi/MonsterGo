using DG.Tweening;
using FairyGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITool
{

    public static GTextField CreateText(string str, int fontSize, GComponent parent, Vector3 position, Color? fontColor = null)
    {
        GTextField aTextField = new();
        aTextField.autoSize = AutoSizeType.Both;
        // 根据传入的颜色值或默认颜色值设置文本颜色
        aTextField.color = fontColor.HasValue ? fontColor.Value : Color.white;
        TextFormat tf = aTextField.textFormat;
        tf.size = fontSize;
        tf.bold = true;
        parent.AddChild(aTextField);
        aTextField.text = str;
        aTextField.position = position;

        return aTextField;
    }

    /// <summary>
    /// 延迟时间触发
    /// </summary>
    /// <param name="delayedTimer"></param>
    /// <param name="callback"></param>
    public static void DOTweenToDelay(float delayedTimer, Action callback)
    {
        float timer = 0;
        //DOTwwen.To()中参数：前两个参数是固定写法，第三个是到达的最终值，第四个是渐变过程所用的时间
        Tween t = DOTween.To(() => timer, x => timer = x, 1, delayedTimer).OnStepComplete(() =>{callback?.Invoke();});
    }

    /// <summary>
    /// 获取组件在屏幕上的坐标
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Vector2 LocalToScreenPos(GObject obj){
        return GRoot.inst.GlobalToLocal(obj.LocalToGlobal(Vector2.zero));
    }

    // 将FairyGUI的本地坐标转换为屏幕坐标，再转换为Unity的世界坐标
    public static Vector3 GetScreenPosition(GObject comp)
    {
        Vector2 screenPos = GRoot.inst.LocalToGlobal(comp.position);
        //原点位置转换
        screenPos.y = Screen.height - screenPos.y; 
        // 一般情况下，还需要提供距离摄像机视野正前方distance长度的参数作为screenPos.z(如果需要，将screenPos改为Vector3类型）
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        return worldPos;
    }
}
