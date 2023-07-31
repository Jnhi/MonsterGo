using System;
public class Ttfe_OverViewOpenParam
{
    /// <summary>
    /// 重新开始方法
    /// </summary>
    public Action RestartFunc;
    /// <summary>
    /// 退出游戏方法
    /// </summary>
    public Action ExittFunc;

    public Ttfe_OverViewOpenParam()
    {
        RestartFunc = null;
        ExittFunc = null;
    }
    public static Ttfe_OverViewOpenParam Create(Action restartFunc,Action exittFunc) {
        Ttfe_OverViewOpenParam ttfe_OverViewOpenParam = new();
        ttfe_OverViewOpenParam.RestartFunc = restartFunc;
        ttfe_OverViewOpenParam.ExittFunc = exittFunc;
        return ttfe_OverViewOpenParam;
    }
}

