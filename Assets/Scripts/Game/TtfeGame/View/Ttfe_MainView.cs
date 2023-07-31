using FairyGUI;
using GameFramework;
using System;
using System.Collections.Generic;
using UnityEngine;


public class Ttfe_MainView : UIBase
{
    public override string PakName => "TtfeGame";

    public override string CompName => "MainView";

    // 用于存储游戏状态的栈
    private Stack<int[,]> GameStateStack;

    // 存储方块的二维数组
    private Tile[,] Tile;

    // 存储格子物体的二维数组
    private GComponent[,] Grid;

    //每一行格子数量
    public int GridSize = 4;

    //下方背景
    public GObject ImgBg2;

    //存放空格子的组
    public GComponent gEmpty;

    //存放数字组
    public GComponent gNum;

    private int touchId;
    private float _lastStageX;
    private float _lastStageY;

    public override void OpenPanel(object arg = null)
    {
        fui.MakeFullScreen();

        touchId = -1;
        ImgBg2 = fui.GetChild("touch");

        gEmpty = new GComponent();
        gEmpty.touchable = false;
        fui.AddChild(gEmpty);
        gEmpty.position = new Vector2(35, 400);

        gNum = new GComponent();
        gNum.touchable = false;
        fui.AddChild(gNum);
        gNum.position = new Vector2(35, 400);
        Tile = new Tile[GridSize, GridSize];
        Grid = new GComponent[GridSize, GridSize];

        fui.GetChild("btnReStart").onClick.Set(() =>
        {
            AudioManager.Instance.Play("TtfeGame/m_btn.mp3");
            ReStart();
        });

        fui.GetChild("btnRestore").onClick.Set(() =>
        {
            AudioManager.Instance.Play("TtfeGame/m_btn.mp3");
            RestoreGameState();
        });

        fui.GetChild("btnSetting").onClick.Set(() =>
        {
            AudioManager.Instance.Play("TtfeGame/m_btn.mp3");
            _ = UIManager.Instance.OpenUIPanelAsync<Ttfe_SetView>();
        });


        //初始化空格子
        CreateEmptyGrid();
        //游戏开始
        GameStart();

        //添加触摸事件
        ImgBg2.onTouchBegin.Add(this.OnTouchBegin);
        ImgBg2.onTouchEnd.Add(this.OnTouchEnd);
    }

    /// <summary>
    /// 返回上一步
    /// </summary>
    private void RestoreGameState()
    {
        // 从栈中取出前一个游戏状态，并将其设置为当前游戏状态
        if (GameStateStack.Count > 1)
        {
            //销毁所有物体
            foreach (Tile item in Tile)
            {
                if (item != null)
                {
                    item.Destory();
                }
            }
            Tile = new Tile[GridSize, GridSize];

            GameStateStack.Pop(); // 弹出当前游戏状态
            int[,] previousState = GameStateStack.Peek(); // 获取前一个游戏状态

            int preSocre = 0;
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    if (previousState[x, y] != 0)
                    {
                        if (previousState[x, y] > 2)
                        {
                            preSocre = preSocre + 4 * (previousState[x, y] / 4);
                        }
                        CreateTestTile(x, y, previousState[x, y]);
                    }
                }
            }
            TtfeModel.Instance.score = preSocre;
            fui.GetChild("txtScore").text = TtfeModel.Instance.score.ToString();
        }
    }

    /// <summary>
    /// 保存游戏状态
    /// </summary>
    private void SaveGameState()
    {
        // 将当前游戏状态保存到栈中
        int[,] gameState = new int[GridSize, GridSize];
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                gameState[x, y] = Tile[x, y] == null ? 0 : Tile[x, y].value;
            }
        }
        GameStateStack.Push(gameState);
    }

    public void GameStart()
    {
        GameStateStack = new Stack<int[,]>();
        //随机生成两个新方块
        CreateRandomTile();
        CreateRandomTile();
        //初始化分数
        TtfeModel.Instance.score = 0;
        fui.GetChild("txtScore").text = TtfeModel.Instance.score.ToString();
        //下面是测试代码
        //CreateTestTile(0, 0);
        //CreateTestTile(1, 0);
        //CreateTestTile(2, 0);
        //CreateTestTile(3, 0);
        SaveGameState();
    }

    private void MoveTiles(Vector2 direction)
    {
        // 根据移动方向移动方块
        // 例如，向左移动所有方块，合并相同数字的方块

        bool moved = false;

        bool isMerage = false;

        if (direction == Vector2.left)
        {
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 1; x < GridSize; x++)
                {
                    if (Tile[x, y] != null)
                    {
                        Tile currentTile = Tile[x, y];
                        int targetX = x;

                        while (targetX > 0 && Tile[targetX - 1, y] == null)
                        {
                            targetX--;
                        }

                        if (targetX > 0 && Tile[targetX - 1, y].value == currentTile.value)
                        {
                            // 合并相同数字的方块
                            Tile mergedTile = Tile[targetX - 1, y];
                            MergeNum(currentTile, mergedTile, Grid[targetX - 1, y].position);
                            Tile[x, y] = null;
                            moved = true;
                            isMerage = true;
                        }
                        else if (targetX != x)
                        {
                            // 移动方块到目标位置
                            Tile[x, y] = null;
                            Tile[targetX, y] = currentTile;
                            MoveToPostion(currentTile.obj, Grid[targetX, y].position);
                            moved = true;
                        }
                    }
                }
            }
        }
        else if (direction == Vector2.right)
        {
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = GridSize - 2; x >= 0; x--)
                {
                    if (Tile[x, y] != null)
                    {
                        Tile currentTile = Tile[x, y];
                        int targetX = x;

                        while (targetX < GridSize - 1 && Tile[targetX + 1, y] == null)
                        {
                            targetX++;
                        }

                        if (targetX < GridSize - 1 && Tile[targetX + 1, y].value == currentTile.value)
                        {
                            // 合并相同数字的方块
                            Tile mergedTile = Tile[targetX + 1, y];
                            MergeNum(currentTile, mergedTile, Grid[targetX + 1, y].position);
                            Tile[x, y] = null;
                            moved = true;
                            isMerage = true;
                        }
                        else if (targetX != x)
                        {
                            // 移动方块到目标位置
                            Tile[x, y] = null;
                            Tile[targetX, y] = currentTile;
                            MoveToPostion(currentTile.obj, Grid[targetX, y].position);
                            moved = true;
                        }
                    }
                }
            }
        }
        else if (direction == Vector2.up)
        {
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 1; y < GridSize; y++)
                {
                    if (Tile[x, y] != null)
                    {
                        Tile currentTile = Tile[x, y];
                        int targetY = y;

                        while (targetY > 0 && Tile[x, targetY - 1] == null)
                        {
                            targetY--;
                        }

                        if (targetY > 0 && Tile[x, targetY - 1].value == currentTile.value)
                        {
                            // 合并相同数字的方块
                            Tile mergedTile = Tile[x, targetY - 1];
                            MergeNum(currentTile, mergedTile, Grid[x, targetY - 1].position);
                            Tile[x, y] = null;
                            moved = true;
                            isMerage = true;
                        }
                        else if (targetY != y)
                        {
                            // 移动方块到目标位置
                            Tile[x, y] = null;
                            Tile[x, targetY] = currentTile;
                            MoveToPostion(currentTile.obj, Grid[x, targetY].position);
                            moved = true;
                        }
                    }
                }
            }
        }
        else if (direction == Vector2.down)
        {
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = GridSize - 2; y >= 0; y--)
                {
                    if (Tile[x, y] != null)
                    {
                        Tile currentTile = Tile[x, y];
                        int targetY = y;

                        while (targetY < GridSize - 1 && Tile[x, targetY + 1] == null)
                        {
                            targetY++;
                        }

                        if (targetY < GridSize - 1 && Tile[x, targetY + 1].value == currentTile.value)
                        {
                            // 合并相同数字的方块
                            Tile mergedTile = Tile[x, targetY + 1];
                            MergeNum(currentTile, mergedTile, Grid[x, targetY + 1].position);
                            Tile[x, y] = null;
                            moved = true;
                            isMerage = true;
                        }
                        else if (targetY != y)
                        {
                            // 移动方块到目标位置
                            Tile[x, y] = null;
                            Tile[x, targetY] = currentTile;
                            MoveToPostion(currentTile.obj, Grid[x, targetY].position);
                            moved = true;
                        }
                    }
                }
            }
        }

        if (isMerage)
        {
            AudioManager.Instance.Play("TtfeGame/m_merage.mp3");
        }


        if (moved)
        {
            // 移动后新增数字
            CreateRandomTile();
            SaveGameState();
        }
    }

    /// <summary>
    /// 增加分数
    /// </summary>
    /// <param name="score"></param>
    public void AddScore(int add)
    {
        TtfeModel.Instance.score = TtfeModel.Instance.score + add; 
        fui.GetChild("txtScore").text = TtfeModel.Instance.score.ToString();
    }

    private bool IsGameOver()
    {
        // 检查是否有空格子
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                if (Tile[x, y] == null)
                {
                    return false;
                }
            }
        }

        // 检查是否有相邻的格子具有相同的值
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                int currentValue = Tile[x, y].value;

                // 检查右边格子
                if (x < GridSize - 1 && Tile[x + 1, y].value == currentValue)
                {
                    return false;
                }

                // 检查下边格子
                if (y < GridSize - 1 && Tile[x, y + 1].value == currentValue)
                {
                    return false;
                }
            }
        }

        return true; // 没有空格子且没有相邻的格子具有相同的值，游戏失败
    }

    /// <summary>
    /// 合并方块
    /// </summary>
    /// <param name="oldTile"></param>
    /// <param name="newTile"></param>
    public void MergeNum(Tile oldTile, Tile newTile, Vector2 pos)
    {
        AddScore(newTile.value * 2);
        newTile.SetValue(newTile.value * 2);
        oldTile.obj.TweenMove(pos, 0.2f).OnComplete(() =>
        {
            newTile.SetValueAndRefresh(newTile.value);
            oldTile.Destory();
        });
    }

    /// <summary>
    /// 将方块移动到空位置
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="pos"></param>
    public void MoveToPostion(GComponent obj,Vector2 pos)
    {
        obj.TweenMove(pos, 0.2f);
    }

    private void OnTouchBegin(EventContext context)
    {
        if (touchId == -1)//First touch
        {
            context.CaptureTouch();
            InputEvent evt = (InputEvent)context.data;
            touchId = evt.touchId;

            Vector2 pt = GRoot.inst.GlobalToLocal(new Vector2(evt.x, evt.y));
            float bx = pt.x;
            float by = pt.y;
            _lastStageX = bx;
            _lastStageY = by;
        }
    }

    private void OnTouchEnd(EventContext context)
    {
        InputEvent evt = (InputEvent)context.data;
        if (touchId != -1 && evt.touchId == touchId)
        {
            Vector2 pt = GRoot.inst.GlobalToLocal(new Vector2(evt.x, evt.y));
            float bx = pt.x;
            float by = pt.y;
            float moveX = bx - _lastStageX;
            float moveY = by - _lastStageY;

            if (Math.Abs(moveX) >= 10 || Math.Abs(moveY) >= 10)
            {
                if (Math.Abs(moveX) > Math.Abs(moveY))
                {
                    if (moveX < 0)
                    {
                        MoveTiles(Vector2.left);
                    }
                    else
                    {
                        MoveTiles(Vector2.right);
                    }
                }
                else
                {
                    if (moveY < 0)
                    {
                        MoveTiles(Vector2.up);
                    }
                    else
                    {
                        MoveTiles(Vector2.down);
                    }
                }
            }
            if (IsGameOver())
            {
                _ = UIManager.Instance.OpenUIPanelAsync<Ttfe_OverView>(Ttfe_OverViewOpenParam.Create(
                    () => { this.ReStart(); }, () => { this.ReStart(); }
                    ));
            }
            touchId = -1;
        }
    }

    /// <summary>
    /// 游戏重新开始
    /// </summary>
    public void ReStart()
    {
        //销毁所有物体
        foreach (Tile item in Tile)
        {
            if (item != null)
            {
                item.Destory();
            }
        }
        Tile = new Tile[GridSize, GridSize];
        GameStart();
    }

    /// <summary>
    /// 在指定位置创建数字
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void CreateTestTile(int x,int y,int value = 2)
    {
        if (Tile[x, y] == null)
        {
            Tile newTile = new(gNum, Grid[x, y], value);
            newTile.SetValue(value);
            Tile[x, y] = newTile;
        }
        else
        {
            // 如果随机位置已经有方块了，则重新生成一个新位置
            CreateRandomTile();
        }
    }

    /// <summary>
    /// 随机创建数字
    /// </summary>
    public void CreateRandomTile()
    {
        int x = UnityEngine.Random.Range(0, GridSize);
        int y = UnityEngine.Random.Range(0, GridSize);

        if (Tile[x, y] == null)
        {
            Tile newTile = new(gNum,Grid[x,y],2);
            // 设置新方块的值为2
            newTile.SetValue(2);
            Tile[x, y] = newTile;
        }
        else
        {
            // 如果随机位置已经有方块了，则重新生成一个新位置
            CreateRandomTile();
        }
    }

    /// <summary>
    /// 创建背景方块
    /// </summary>
    public void CreateEmptyGrid()
    {
        int StartY = 10;
        for (int i = 0; i < GridSize; i++)
        {
            int StartX = 10;
            for (int j = 0; j < GridSize; j++)
            {
                GComponent emptyGrid = UIPackage.CreateObject("TtfeGame", "Grid").asCom;
                gEmpty.AddChild(emptyGrid);
                emptyGrid.position = new Vector2((float)StartX, (float)StartY);
                Grid[j, i] = emptyGrid;
                Grid[j, i].gameObjectName = $"{j}-{i}";
                StartX = StartX + 150 + 10;
            }
            StartY = StartY + 150 + 10;
        }

    }

    /// <summary>
    /// 关闭面板
    /// </summary>
    /// <param name="arg"></param>
    public override void ClosePanel(object arg = null)
    {
        UIManager.Instance.CloseUIPanel(this);
    }
}

