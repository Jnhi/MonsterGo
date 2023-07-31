using FairyGUI.Utils;
using FairyGUI;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Scripting;

public class TopMapComp : GComponent
{
    List<GImage> reds = new List<GImage>();
    List<GImage> lines = new List<GImage>();
    Sequence redSeq;

    [Preserve]
    public TopMapComp()
    {
        
    }
    public override void ConstructFromXML(XML xml)
    {
        base.ConstructFromXML(xml);
        for (int i = 0; i < 7; i++) {
            GImage img = this.GetChild($"red{i}").asImage;
            reds.Add(img);
        }
        for (int i = 0; i < 6; i++)
        {
            GImage imgLine = this.GetChild($"line{i}").asImage;
            lines.Add(imgLine);
        }
        RefreshMap();
    }

    /// <summary>
    /// 刷新地图
    /// </summary>
    public void RefreshMap()
    {
        MonsterGoModel model = MonsterGoModel.Instance;

        for (int i = 0;i < 7; i++) {
            if (i < model.curLevelIndex)
            {
                reds[i].alpha = 1f;
            }
            else if(i == model.curLevelIndex)
            {
                reds[i].alpha = 1f;
                redSeq.Kill();
                redSeq = AnimationTool.BlinkingEffect(reds[model.curLevelIndex],0.2f);
            }
            else 
            {
                reds[i].alpha = 0.3f;
            }
        }

        for (int i = 0; i < 6; i++)
        {
            if (i < model.curLevelIndex )
            {
                lines[i].visible = true;
            }
            else
            {
                lines[i].visible = false;
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();
    }
   
}