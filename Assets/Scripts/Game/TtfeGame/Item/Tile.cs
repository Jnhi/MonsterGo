
using FairyGUI;
using UnityEngine;

public class Tile
{
    // 方块的值
    public int value;

    // 方块物体
    public GComponent obj;

    public Tile(GComponent parent, GComponent emptyGrid,int value)
    {
        obj = UIPackage.CreateObject("TtfeGame", "Num").asCom;
        parent.AddChild(obj);
        obj.scale = new Vector2((float)0.1, (float)0.1);
        obj.TweenScale(new Vector2(1, 1),0.2f);
        obj.position = emptyGrid.position;
        SetValueAndRefresh(value);
    }

    public void Destory()
    {
        obj.Dispose();
    }
    /// <summary>
    /// 设置当前值并且更新界面
    /// </summary>
    /// <param name="newValue"></param>
    public void SetValueAndRefresh(int newValue)
    {
        value = newValue;
        UpdateUI();
    }

    /// <summary>
    /// 设置当前值 不更新界面
    /// </summary>
    /// <param name="newValue"></param>
    public void SetValue(int newValue)
    {
        value = newValue;
    }

    /// <summary>
    /// 刷新界面
    /// </summary>
    private void UpdateUI()
    {
        obj.GetChild("txtNum").text = value.ToString();
        // 根据方块的值更新UI显示
        // 例如，将方块的值显示在方块的Sprite或UI Text组件上
    }
}
