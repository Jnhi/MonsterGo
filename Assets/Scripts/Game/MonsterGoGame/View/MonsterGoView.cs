
using DG.Tweening;
using FairyGUI;
using GameFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 消除信息
/// </summary>
public struct MatchesData
{
    /// <summary>
    /// 需要上漂数字的方块物体（背后的格子组件）
    /// </summary>
    public GComponent girdComp;
    /// <summary>
    /// 消除的怪物ID
    /// </summary>
    public int monsterID;
    /// <summary>
    /// 消除数量
    /// </summary>
    public int num;
}

public class MonsterGoView : UIBase
{
    public override string PakName => "MonsterGo";

    public override string CompName => "MainView";

    //每一行格子数量
    public int colSize = 8;

    //每一行格子数量
    public int rowSize = 8;

    //存放空格子的组
    public GComponent gEmpty;

    //存放格子的组
    public GComponent gBlock;

    // 存储格子物体的二维数组
    private GComponent[,] Grid;

    /// <summary>
    /// 图鉴按钮
    /// </summary>
    private GButton btnIllustrate;
    private GButton btnRule;

    // 储存方块
    private Block[,] blocks;

    private GProgressBar prgBlockDown;
    /// <summary>
    /// 当前选择的方块
    /// </summary>
    public Block selectedBlock;

    private Coroutine dropCoroutine;

    /// <summary>
    /// 方块是否移动中
    /// </summary>
    public bool isMoving = false;

    // 定义刷新时间间隔（单位：秒）
    float refreshInterval = 15;

    TopBattleComp topBattleComp;

    MidTubeComp midTubeComp;

    TopMapComp topMapComp;

    private GTextField txtScore;

    MonsterGoModel monsterGoModel = MonsterGoModel.Instance;

    MonsterGoUtil monsterGoUtil = MonsterGoUtil.Instance;

    /// <summary>
    /// 打开界面
    /// </summary>
    /// <param name="arg"></param>
    public override void OpenPanel(object arg = null)
    {
        fui.MakeFullScreen();
    
        gEmpty = fui.GetChild("gEmpty").asCom;
        prgBlockDown = fui.GetChild("prgBlockDown").asProgress;
        txtScore = fui.GetChild("txtScore").asTextField;
        gBlock = fui.GetChild("gBlock").asCom;
        midTubeComp = (MidTubeComp)fui.GetChild("midTube").asCom;
        topBattleComp = (TopBattleComp)fui.GetChild("topMap").asCom;
        topMapComp = (TopMapComp)fui.GetChild("TopMapComp").asCom;
        btnIllustrate = fui.GetChild("btnIllustrate").asButton;
        btnRule = fui.GetChild("btnRule").asButton;

        Grid = new GComponent[rowSize, colSize];
        prgBlockDown.value = 0;

        // 初始化数据模块
        monsterGoModel.InitGameData();

        //初始化空格子
        CreateEmptyGrid();

        //初始化地图
        topMapComp.RefreshMap();

        blocks = new Block[colSize, rowSize];

        CoroutineManager.Instance.StartCoroutine(StartGame());

        btnIllustrate.onClick.Set(async () =>
        {
            await UIManager.Instance.OpenUIPanelAsync<IllustrateView>();
        });

        btnRule.onClick.Set(async () =>
        {
            await UIManager.Instance.OpenUIPanelAsync<RuleView>();
        });
    }

    /// <summary>
    /// 开启定时下落
    /// </summary>
    private void StartDrop()
    {
        if (dropCoroutine == null)
        {
            // 启动每隔10秒补充空余格子的协程
            dropCoroutine = CoroutineManager.Instance.StartCoroutine(DropBlocksPeriodically());
        }
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    private IEnumerator StartGame()
    {
        UIManager.Instance.DisableClick();
        selectedBlock = null;
        prgBlockDown.value = 0;
        txtScore.text = "0";
        // 初始化数据模块
        monsterGoModel.InitGameData();
        //初始化地图
        topMapComp.RefreshMap();
        //初始化顶部战斗模块
        topBattleComp.Init(this);
        // 将中间试管设置为初始状态
        midTubeComp.Init();
        // 删除所有方块
        yield return DelAllBlock();

        // 补充所有方块
        yield return FillEmptyBlocks();

        UIManager.Instance.EnableClick();
    }

    // /// <summary>
    // /// 开始游戏
    // /// </summary>
    // /// <returns></returns>
    // IEnumerator StartGame()
    // {
    //     MonsterGoModel model = MonsterGoModel.Instance;

    //     model.InitHeroData();
    //     midTubeComp.Init();
    //     topBattleComp.Init(this);

    //     txtScore.text = "0";

    //     yield return InitializeBlocks();
    //     // yield return InitializeBlocksTest();
    //     List<MatchesData> matchesData = GetMatches();
    //     if (matchesData.Count > 0)
    //     {
    //         // 递归执行连锁反应
    //         yield return PerformMatchAndCollapse(matchesData);
    //     }
    // }

    /// <summary>
    /// 开始下一关
    /// </summary>
    /// <returns></returns>
    public IEnumerator NextLevel()
    {
        UIManager.Instance.DisableClick();
        selectedBlock = null;
        Log.Debug("下一关");
        MonsterGoModel model = MonsterGoModel.Instance;

        //初始化地图
        topMapComp.RefreshMap();

        // 暂停下落
        CoroutineManager.Instance.StopCoroutine(dropCoroutine);

        // 初始化试管数据，步数
        model.NextLevel();

        // 暂停顶部移动
        topBattleComp.StopWalk();

        // 将顶部初始化为原来状态
        topBattleComp.NextLevel();

        // 将中间试管设置为初始状态
        midTubeComp.Init();

        // 删除所有方块
        yield return DelAllBlock();

        // 补充所有方块
        yield return FillEmptyBlocks();

        // 开启下落
        dropCoroutine = CoroutineManager.Instance.StartCoroutine(DropBlocksPeriodically());

        // 开始顶部移动
        topBattleComp.StartWalk();

        UIManager.Instance.EnableClick();
    }

    /// <summary>
    /// 游戏结束
    /// </summary>
    /// <returns></returns>
    public async void GameOver(bool isWin)
    {
        //暂停下落
        CoroutineManager.Instance.StopCoroutine(dropCoroutine);
        //暂停移动
        topBattleComp.StopWalk();

        GameOverParams gameOverParams = new GameOverParams();
        gameOverParams.isWin = isWin;
        gameOverParams.reStartFunc = () =>
        {
            CoroutineManager.Instance.StartCoroutine(StartGame());
        };
        gameOverParams.exitFunc = () =>
        {
            Log.Debug("点击退出");
            ClosePanel();
        };
        await UIManager.Instance.OpenUIPanelAsync<GameOverView>(gameOverParams);
    }

    /// <summary>
    /// 创建方块
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="blockType"></param>
    /// <returns></returns>
    Block CreateBlock(int row, int col, int blockType)
    {
        Vector3 pos = Grid[row, col].position;
        Block block = new(gBlock, pos, blockType);
        block.row = row;
        block.col = col;
        blocks[row, col] = block;
        block.SelectFunc = () =>
        {
            MoveBlock(block);
        };
        return block;
    }

    /// <summary>
    /// 处理定时下落的协程
    /// </summary>
    /// <returns></returns>
    IEnumerator DropBlocksPeriodically()
    {
        float timer = 0; // 下落计时器
        while (true)
        {
            //处理方块定时下落
            timer += Time.deltaTime;
            // Log.Debug(timer.ToString());
            float progress = (timer / refreshInterval) * 100;
            prgBlockDown.value = progress;
            // 检查是否满足刷新条件
            if (timer >= refreshInterval)
            {
                while (isMoving)
                {
                    yield return null;
                }

                // 执行补充空缺的方块操作
                isMoving = true;
                yield return FillEmptyBlocks();
                timer = 0;
            }
            yield return null;
        }

    }

    /// <summary>
    /// 补充新的方块
    /// </summary>
    /// <returns></returns>
    IEnumerator FillEmptyBlocks()
    {
        MonsterGoUtil monsterGoUtil = MonsterGoUtil.Instance;

        // 播放动画
        List<IEnumerator> animationCoroutines = new List<IEnumerator>();

        Block[,] tempBlcok = new Block[colSize, rowSize];

        for (int col = 0; col < colSize; col++)
        {
            for (int row = rowSize - 1; row >= 0; row--)
            {
                if (blocks[row, col] == null)
                {
                    int blockType = monsterGoUtil.GetRandomMonsterID();
                    bool isMatchedX = false;
                    int sameTypeX = 0;
                    bool isMatchedY = false;
                    int sameTypeY = 0;

                    if (col > 1 && tempBlcok[row, col - 1] != null && tempBlcok[row, col - 2] != null && tempBlcok[row, col - 1].type == tempBlcok[row, col - 2].type)
                    {
                        sameTypeX = blocks[row, col - 1].type;
                        isMatchedX = true;
                    }

                    if (row < rowSize - 2 && tempBlcok[row + 1, col] != null && tempBlcok[row + 2, col] != null && tempBlcok[row + 1, col].type == tempBlcok[row + 2, col].type)
                    {
                        sameTypeY = blocks[row + 1, col].type;
                        isMatchedY = true;
                    }

                    if (isMatchedX == false && isMatchedY == false)
                    {
                        //do nothing
                    }
                    else if (isMatchedX == true && isMatchedY == false)
                    {
                        blockType = monsterGoUtil.GetExcludeRandomMonsterID(new int[] { sameTypeX });
                    }
                    else if (isMatchedX == false && isMatchedY == true)
                    {
                        blockType = monsterGoUtil.GetExcludeRandomMonsterID(new int[] { sameTypeY });
                    }
                    else
                    {
                        blockType = monsterGoUtil.GetExcludeRandomMonsterID(new int[] { sameTypeX, sameTypeY });
                    }
                    Block block = CreateBlock(row, col, blockType);
                    tempBlcok[row, col] = block;
                    // 播放生成动画
                    animationCoroutines.Add(block.BornAnim(BornType.Fall));
                }
            }
        }

        yield return CoroutineManager.Instance.WaitForAllCoroutines(animationCoroutines);

        MonsterGoModel.Instance.curComboNum = 0;
        midTubeComp.RefreshCombo(MonsterGoModel.Instance.curComboNum);

        // 补充完成后，检查是否存在匹配的方块
        List<MatchesData> matchesData = GetMatches();
        if (matchesData.Count > 0)
        {
            yield return PerformMatchAndCollapse(matchesData);
        }
        else
        {
            isMoving = false;
        }
    }

    /// <summary>
    /// 关闭面板
    /// </summary>
    /// <param name="arg"></param>
    public override void ClosePanel(object arg = null)
    {
        if (blocks != null && blocks.Length != 0)
        {
            for (int row = 0; row < rowSize; row++)
            {
                for (int col = 0; col < colSize; col++)
                {
                    if (blocks[row, col] != null)
                    {
                        blocks[row, col].obj.Dispose();
                    }
                }
            }
        }
        blocks = null;
        if (Grid != null && Grid.Length != 0)
        {
            for (int row = 0; row < rowSize; row++)
            {
                for (int col = 0; col < colSize; col++)
                {
                    if (Grid[row, col] != null)
                    {
                        Grid[row, col].Dispose();
                    }
                }
            }
        }
        Grid = null;
        UIManager.Instance.CloseUIPanel(this);
    }

    /// <summary>
    /// 创建背景方块
    /// </summary>
    public void CreateEmptyGrid()
    {
        int StartY = 40;
        for (int row = 0; row < rowSize; row++)
        {
            int StartX = 45;
            for (int col = 0; col < colSize; col++)
            {
                GComponent emptyGrid = UIPackage.CreateObject("MonsterGo", "Grid").asCom;
                gEmpty.AddChild(emptyGrid);
                emptyGrid.position = new Vector2(StartX, StartY);
                Grid[row, col] = emptyGrid;
                Grid[row, col].gameObjectName = $"{row}-{col}";
                StartX += 80;
            }
            StartY += 80;
        }
    }

    /// <summary>
    /// 判断两个方块是否相邻
    /// </summary>
    /// <param name="block1"></param>
    /// <param name="block2"></param>
    /// <returns></returns>
    private bool IsAdjacent(Block block1, Block block2)
    {
        // 判断两个方块是否相邻
        // 根据坐标差值判断是否相邻
        int rowDiff = Mathf.Abs(block1.row - block2.row);
        int colDiff = Mathf.Abs(block1.col - block2.col);
        return (rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1);
    }

    /// <summary>
    /// 交换方块
    /// </summary>
    /// <param name="block"></param>
    public void MoveBlock(Block block)
    {
        if (isMoving)
        {
            return;
        }

        // 交换方块的位置
        if (selectedBlock == null)
        {
            // 选择该方块作为当前选中方块
            selectedBlock = block;
            block.Select();
        }
        else
        {
            // 如果点击的方块与当前选中方块相邻，则进行交换
            if (IsAdjacent(block, selectedBlock))
            {
                isMoving = true;

                //第一次交换方块的时候 开启定时下落以及 英雄开始行走
                StartDrop();
                topBattleComp.StartWalk();

                // 交换两个方块的位置
                blocks[block.row, block.col] = selectedBlock;
                blocks[selectedBlock.row, selectedBlock.col] = block;
                selectedBlock.Swap(block, () =>
                {
                    MonsterGoModel.Instance.curComboNum = 0;
                    midTubeComp.RefreshCombo(MonsterGoModel.Instance.curComboNum);

                    // 检查是否有消除的方块
                    List<MatchesData> matchesData = GetMatches();
                    if (matchesData.Count > 0)
                    {
                        // 消除方块并执行下落和连锁反应
                        CoroutineManager.Instance.StartCoroutine(PerformMatchAndCollapse(matchesData));
                    }
                    else
                    {
                        isMoving = false;
                    }
                });

            }
            // 取消选中状态
            selectedBlock.Select();
            selectedBlock = null;
        }
    }

    /// <summary>
    /// 游戏开始初始化方块
    /// </summary>
    /// <returns></returns>
    IEnumerator InitializeBlocksTest()
    {
        MonsterGoUtil monsterGoUtil = MonsterGoUtil.Instance;
        // 播放消除动画
        List<IEnumerator> animationCoroutines = new List<IEnumerator>();

        blocks = new Block[colSize, rowSize];

        //int[,] testBlockData = new int[8, 8]
        //{
        //    { 0,0,0,0,0,0,0,0 },
        //    { 0,0,0,0,0,0,0,0 },
        //    { 0,0,0,0,0,0,0,0 },
        //    { 0,0,0,0,0,0,0,0 },
        //    { 0,0,0,0,0,0,0,0 },
        //    { 0,0,0,0,0,0,0,0 },
        //    { 3,4,1,0,0,0,0,0 },
        //    { 1,1,2,3,4,5,6,7 },
        //};

        // int[,] testBlockData = new int[8, 8]
        // {
        //     { 9,9,9,9,9,9,9,9 },
        //     { 9,9,9,9,9,9,9,9 },
        //     { 9,9,9,9,9,9,9,9 },
        //     { 9,9,9,9,9,9,9,9 },
        //     { 9,9,9,9,9,9,9,9 },
        //     { 9,9,9,9,9,9,9,9 },
        //     { 9,9,9,9,9,9,9,9 },
        //     { 1,3,1,4,2,2,3,2 },
        // };

        int[,] testBlockData = new int[8, 8]
           {
            { 9,9,9,9,9,9,9,9 },
            { 9,9,9,9,9,9,9,9 },
            { 9,9,9,9,9,9,9,9 },
            { 9,9,9,9,9,9,9,9 },
            { 9,9,9,9,9,9,9,9 },
            { 0,1,1,1,1,3,0,3 },
            { 1,1,1,1,1,1,2,3 },
            { 2,1,0,1,0,1,3,1 },
           };
        /// <summary>
        /// 生成列测试
        /// </summary>
        /// <value></value>
        // int[,] testBlockData = new int[8, 8]
        // {
        //     { 9,9,9,9,9,9,9,9 },
        //     { 9,9,9,9,9,9,9,9 },
        //     { 9,9,9,9,9,9,9,9 },
        //     { 9,9,9,9,9,9,9,9 },
        //     { 9,9,9,9,9,9,9,9 },
        //     { 1,1,1,1,1,1,1,1 },
        //     { 1,2,1,1,2,1,1,2 },
        //     { 1,3,1,1,3,1,1,3 },
        // };

        for (int row = 0; row < rowSize; row++)
        {
            for (int col = 0; col < colSize; col++)
            {
                int blockType = testBlockData[row, col];
                if (blockType != 9)
                {
                    Block block = CreateBlock(row, col, blockType);
                    // 播放生成动画
                    animationCoroutines.Add(block.BornAnim());
                }
            }
        }

        // 等待所有生成动画播放完成
        yield return CoroutineManager.Instance.WaitForAllCoroutines(animationCoroutines);
    }


    /// <summary>
    /// 游戏开始初始化方块
    /// </summary>
    /// <returns></returns>
    IEnumerator InitializeBlocks()
    {
        MonsterGoUtil monsterGoUtil = MonsterGoUtil.Instance;
        // 播放消除动画
        List<IEnumerator> animationCoroutines = new List<IEnumerator>();

        blocks = new Block[colSize, rowSize];

        for (int row = 0; row < rowSize; row++)
        {
            for (int col = 0; col < colSize; col++)
            {
                int blockType = monsterGoUtil.GetRandomMonsterID();
                bool isMatchedX = false;
                int sameTypeX = 0;
                bool isMatchedY = false;
                int sameTypeY = 0;

                if (col > 1 && blocks[row, col - 1] != null && blocks[row, col - 2] != null && blocks[row, col - 1].type == blocks[row, col - 2].type)
                {
                    sameTypeX = blocks[row, col - 1].type;
                    isMatchedX = true;
                }

                if (row > 1 && blocks[row - 1, col] != null && blocks[row - 2, col] != null && blocks[row - 1, col].type == blocks[row - 2, col].type)
                {
                    sameTypeY = blocks[row - 1, col].type;
                    isMatchedY = true;
                }

                if (isMatchedX == false && isMatchedY == false)
                {
                    //do nothing
                }
                else if (isMatchedX == true && isMatchedY == false)
                {
                    blockType = monsterGoUtil.GetExcludeRandomMonsterID(new int[] { sameTypeX });
                }
                else if (isMatchedX == false && isMatchedY == true)
                {
                    blockType = monsterGoUtil.GetExcludeRandomMonsterID(new int[] { sameTypeY });
                }
                else
                {
                    blockType = monsterGoUtil.GetExcludeRandomMonsterID(new int[] { sameTypeX, sameTypeY });
                }

                Block block = CreateBlock(row, col, blockType);
                // 播放生成动画
                animationCoroutines.Add(block.BornAnim(BornType.Fall));
            }
        }

        // 等待所有生成动画播放完成
        yield return CoroutineManager.Instance.WaitForAllCoroutines(animationCoroutines);
    }

    /// <summary>
    /// 删除所有方块
    /// </summary>
    /// <returns></returns>
    IEnumerator DelAllBlock()
    {
        if (blocks == null || blocks.Length == 0)
        {
            yield break;
        }
        MonsterGoUtil monsterGoUtil = MonsterGoUtil.Instance;
        // 播放消除动画
        List<IEnumerator> animationCoroutines = new List<IEnumerator>();


        for (int row = 0; row < rowSize; row++)
        {
            for (int col = 0; col < colSize; col++)
            {
                if (blocks[row, col] != null)
                {
                    animationCoroutines.Add((blocks[row, col].Del()));
                }
            }
        }
        // 等待所有生成动画播放完成
        yield return CoroutineManager.Instance.WaitForAllCoroutines(animationCoroutines);

        blocks = new Block[colSize, rowSize];
    }

    /// <summary>
    /// 返回需要消除的方块组
    /// </summary>
    /// <returns></returns>
    private List<MatchesData> GetMatches()
    {
        List<MatchesData> allMatches = new List<MatchesData>();
        // 横向匹配
        for (int row = 0; row < rowSize; row++)
        {
            int col = 0;
            while (col < colSize - 2)
            {
                Block currentBlock = blocks[row, col];
                if (currentBlock == null || currentBlock.isMatchX)
                {
                    col++;
                    continue;
                }

                int matchCount = 1;

                for (int i = col + 1; i < colSize; i++)
                {
                    Block nextBlock = blocks[row, i];
                    if (nextBlock == null || nextBlock.type != currentBlock.type)
                        break;

                    matchCount++;
                }

                MatchesData match = new();

                if (matchCount >= 3)
                {
                    for (int i = col; i < col + matchCount; i++)
                    {
                        Block nextBlock = blocks[row, i];
                        nextBlock.isMatchX = true;
                    }
                    match.monsterID = currentBlock.type;
                    match.girdComp = Grid[row, col];
                    match.num = matchCount;
                    allMatches.Add(match);
                }
                col += matchCount;
            }
        }

        // 纵向匹配
        for (int col = 0; col < colSize; col++)
        {
            int row = 0;
            while (row < rowSize - 2)
            {
                Block currentBlock = blocks[row, col];
                if (currentBlock == null || currentBlock.isMatchY)
                {
                    row++;
                    continue;
                }

                int matchCount = 1;

                for (int i = row + 1; i < rowSize; i++)
                {
                    Block nextBlock = blocks[i, col];
                    if (nextBlock == null || nextBlock.type != currentBlock.type)
                        break;

                    matchCount++;
                }

                MatchesData match = new();

                if (matchCount >= 3)
                {
                    for (int i = row; i < row + matchCount; i++)
                    {
                        Block nextBlock = blocks[i, col];
                        nextBlock.isMatchY = true;
                    }
                    match.monsterID = currentBlock.type;
                    match.girdComp = Grid[row, col];
                    match.num = matchCount;
                    allMatches.Add(match);
                }
                row += matchCount;
            }
        }

        return allMatches;
    }

    /// <summary>
    /// 增加游戏分数
    /// </summary>
    /// <param name="_score"></param>
    private void AddScore(int _score)
    {
        MonsterGoModel.Instance.score += _score;
        txtScore.text = MonsterGoModel.Instance.score.ToString();
    }

    /// <summary>
    /// 增加试管百分比
    /// </summary>
    /// <param name="monsterID"></param>
    /// <param name="Percentage"></param>
    private void AddTubePercentage(int percentage, int monsterID)
    {
        midTubeComp.PlayCauldron();
        (int tubeIndex, int generateMonsterID) = MonsterGoModel.Instance.AddTubeData(percentage, monsterID);
        midTubeComp.AddTubePercent(tubeIndex, generateMonsterID, () =>
        {
            topBattleComp.AddMonster(generateMonsterID);
        });
    }

    private void RemoveBlock(Block block)
    {
        blocks[block.row, block.col] = null;
        if (selectedBlock == block)
        {
            selectedBlock = null;
        }
    }

    /// <summary>
    /// 消除以及下落连锁
    /// </summary>
    /// <param name="matchedBlocks"></param>
    /// <returns></returns>
    private IEnumerator PerformMatchAndCollapse(List<MatchesData> matchedBlocks)
    {
        MonsterGoModel.Instance.curComboNum += 1;
        midTubeComp.RefreshCombo(MonsterGoModel.Instance.curComboNum);
        // 暂停移动操作
        isMoving = true;

        // 播放消除动画
        List<IEnumerator> animationCoroutines = new List<IEnumerator>();

        AudioManager.Instance.Play("MonsterGo/music/match.wav");
        // 移除方块
        foreach (Block block in blocks)
        {
            if (block != null && (block.isMatchX || block.isMatchY))
            {
                RemoveBlock(block);
                animationCoroutines.Add(block.Eliminate());
            }
        }

        // 为每一个匹配块显示分数以及增加分数
        foreach (MatchesData blocksList in matchedBlocks)
        {
            EliminateAnimBlockList(blocksList, matchedBlocks.Count);
        }

        yield return CoroutineManager.Instance.WaitForAllCoroutines(animationCoroutines);

        // 执行方块下落操作
        yield return CollapseBlocks();

        // 检查下落后是否有新的连锁反应
        List<MatchesData> matchesData = GetMatches();
        if (matchesData.Count > 0)
        {
            // 递归执行连锁反应
            yield return PerformMatchAndCollapse(matchesData);
        }
        else
        {
            // 没有新的连锁反应，恢复移动操作
            isMoving = false;
        }
    }

    /// <summary>
    /// 展示果汁上漂效果
    /// </summary>
    /// <param name="matchesData"></param>
    private void ShowJuiceAnim(MatchesData matchesData)
    {
        //创建果汁动画
        GMovieClip juice = UIPackage.CreateObject("MonsterGo", "juice").asMovieClip;
        this.fui.AddChild(juice);
        juice.playing = true;
        juice.SetScale(8, 8);

        //计算果汁开始坐标
        Vector2 startPos = UITool.LocalToScreenPos(matchesData.girdComp);
        //计算果汁结束坐标
        Vector2 endPos = UITool.LocalToScreenPos(midTubeComp.GetTube(matchesData.monsterID));
        juice.position = startPos;
        //设置果汁颜色
        Color NowColor;
        bool isColor = ColorUtility.TryParseHtmlString(monsterGoUtil.GetMonsterColorByMonsterID(matchesData.monsterID), out NowColor);
        if (isColor)
        {
            juice.color = NowColor;
        }

        float duration = 0.5f;

        //设置曲线路径
        GPath _turningPath = new GPath();
        Vector2 mid = new Vector2(startPos.x + (endPos.x - startPos.x) / 2, endPos.y + 30);
        _turningPath.Create(new GPathPoint(startPos), new GPathPoint(mid), new GPathPoint(endPos));
        GTween.To(startPos, endPos, duration).SetUserData(true).SetTarget(this)
            .SetPath(_turningPath)
            .OnUpdate((GTweener tweener) =>
            {
                juice.position = tweener.value.vec2;
            }).OnComplete(() =>
            {
                juice.Dispose();
            });
    }

    /// <summary>
    /// 增加消除分数,展示消除后的效果
    /// </summary>
    /// <param name="matchesData">消除信息</param>
    /// <param name="simultaneousCount">同时需要消除的方块组数量</param>
    /// <returns></returns>
    private void EliminateAnimBlockList(MatchesData matchesData, int simultaneousCount)
    {
        (int percentage, int score, int monsterID) = monsterGoUtil.GetEliminateScore(matchesData, simultaneousCount);

        //增加分数
        AddScore(score);

        //增加分试管百分比
        AddTubePercentage(percentage, monsterID);

        // 怪物果汁上漂到试管处
        ShowJuiceAnim(matchesData);

        // 显示增加的百分比分数效果
        ShowNumEffect(UITool.LocalToScreenPos(matchesData.girdComp), percentage);
    }

    /// <summary>
    /// 显示消除后的数字上飘效果
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    private void ShowNumEffect(Vector2 pos, int percentage)
    {
        //创建分数数字并且上飘
        GTextField aTextField = UITool.CreateText(percentage.ToString() + "%", 40, this.fui, pos);
        aTextField.pivot = new Vector2(0.5f, 0.5f);
        aTextField.pivotAsAnchor = true;
        aTextField.stroke = 3;
        aTextField.strokeColor = Color.black;
        Sequence sequence = AnimationTool.ChangeTextColorSeq(aTextField);

        UITool.DOTweenToDelay(0.3f, () =>
        {
            AnimationTool.FlyUp(aTextField, 20, 0.3f, () =>
            {
                sequence.Kill();
                aTextField.Dispose();
            });
        });
    }

    /// <summary>
    /// 移动列
    /// </summary>
    /// <param name="sourceCol"></param>
    /// <param name="targetCol"></param>
    private IEnumerator MoveColumn(int sourceCol, int targetCol)
    {
        List<IEnumerator> animationCoroutines = new List<IEnumerator>();

        for (int row = 0; row < rowSize; row++)
        {
            Block block = blocks[row, sourceCol];

            if (block != null)
            {
                animationCoroutines.Add(block.Move(Grid[row, targetCol]));
                block.col = targetCol;
                blocks[row, targetCol] = block;
                blocks[row, sourceCol] = null;
            }
        }
        yield return CoroutineManager.Instance.WaitForAllCoroutines(animationCoroutines);
    }

    /// <summary>
    /// 生成新的列
    /// </summary>
    /// <param name="col"></param>
    private IEnumerator GenerateNewColBlocks(int col)
    {
        MonsterGoUtil monsterGoUtil = MonsterGoUtil.Instance;

        // 播放消除动画
        List<IEnumerator> animationCoroutines = new List<IEnumerator>();

        for (int row = 0; row < rowSize; row++)
        {
            bool isMatchedY = false;
            int sameTypeY = 0;
            int blockType = 0;
            if (row > 1 && blocks[row - 1, col] != null && blocks[row - 2, col] != null && blocks[row - 1, col].type == blocks[row - 2, col].type)
            {
                sameTypeY = blocks[row - 1, col].type;
                isMatchedY = true;
            }

            if (isMatchedY)
            {
                blockType = monsterGoUtil.GetExcludeRandomMonsterID(new int[] { sameTypeY });
            }
            else
            {
                blockType = monsterGoUtil.GetRandomMonsterID();
            }
            Block block = CreateBlock(row, col, blockType);
            animationCoroutines.Add(block.BornAnim());
        }
        yield return CoroutineManager.Instance.WaitForAllCoroutines(animationCoroutines);
    }

    /// <summary>
    /// 检查当前列是否为空
    /// </summary>
    /// <param name="col"></param>
    /// <returns></returns>
    private bool IsEmptyCol(int col)
    {
        // 检查当前列是否为空列
        for (int row = 0; row < rowSize; row++)
        {
            if (blocks[row, col] != null)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 下落方块且出现空列生成新列以及旧列右移动
    /// </summary>
    /// <returns></returns>
    private IEnumerator CollapseBlocks()
    {
        // 播放下落动画
        List<IEnumerator> animationDownCoroutines = new List<IEnumerator>();
        // 执行方块下落操作
        for (int col = 0; col < colSize; col++)
        {
            for (int row = rowSize - 1; row >= 0; row--)
            {
                if (blocks[row, col] == null)
                {
                    // 向上查找最近的非空方块
                    for (int r = row - 1; r >= 0; r--)
                    {
                        if (blocks[r, col] != null)
                        {
                            // 将方块下落到空缺位置
                            Block block = blocks[r, col];
                            block.row = row;
                            blocks[row, col] = block;
                            blocks[r, col] = null;
                            animationDownCoroutines.Add(block.Move(Grid[row, col]));
                            break;
                        }
                    }
                }
            }
        }
        if (animationDownCoroutines.Count > 0)
        {
            AudioManager.Instance.Play("MonsterGo/music/drop.wav");
        }
        yield return CoroutineManager.Instance.WaitForAllCoroutines(animationDownCoroutines);


        // 检查每一列是否有空列
        for (int col = colSize - 1; col >= 0; col--)
        {
            // 检查当前列是否为空列
            bool isColumnEmpty = IsEmptyCol(col);
            if (isColumnEmpty)
            {
                //寻找下一个有物体的列，并且记录需要移动的步数
                int moveNum = 1;
                for (int i = col - 1; i >= 0; i--)
                {
                    if (IsEmptyCol(i))
                    {
                        moveNum += 1;
                    }
                    else
                    {
                        break;
                    }
                }
                List<IEnumerator> animationColCoroutines = new List<IEnumerator>();

                // 将左侧非空列移动到右侧空列的位置
                for (int moveCol = col - moveNum; moveCol >= 0; moveCol--)
                {
                    animationColCoroutines.Add(MoveColumn(moveCol, moveCol + moveNum));
                }
                yield return CoroutineManager.Instance.WaitForAllCoroutines(animationColCoroutines);
                // 如果存在多个空列需要多次生成新列
                List<IEnumerator> animationColCoroutines2 = new List<IEnumerator>();
                for (int i = 0; i < moveNum; i++)
                {
                    animationColCoroutines2.Add(GenerateNewColBlocks(i));
                }
                yield return CoroutineManager.Instance.WaitForAllCoroutines(animationColCoroutines2);

            }
        }
    }
}

