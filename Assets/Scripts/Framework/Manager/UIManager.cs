using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FairyGUI;
using UnityEngine;
using WeChatWASM;

namespace GameFramework
{
    public class UIManager : Singleton<UIManager>
    {
        public const int UIWidth = 720;
        public const int UIHeight = 1280;
        ////栈结构维护基础面板
        //public Stack<UIBase> uiCtrlStack = new Stack<UIBase>();
        //创建的面板
        public Dictionary<string, UIBase> uiLoaded = new Dictionary<string, UIBase>();
        // 安全区
        private GComponent saveArea;
        // Bg区
        private GComponent bgArea;
        // game区
        private GComponent gameArea;
        // message区
        private GComponent messageArea;
        // top区
        private GComponent topArea;
        // 最顶部
        private GComponent frameArea;
        // 用来禁止点击的组件
        private GComponent enableClickMask;
        public void SetExtension()
        {

        }

        public void Init()
        {
            SetExtension();
            Log.Debug($"原生安全区: {Screen.safeArea}");


#if UNITY_WEBGL && !UNITY_EDITOR
            var info = WX.GetSystemInfoSync();
            Debug.Log($"微信SDK安全区: {info.safeArea.left* info.pixelRatio},{info.safeArea.right* info.pixelRatio},{info.safeArea.top* info.pixelRatio},{info.safeArea.bottom* info.pixelRatio},{info.safeArea.height* info.pixelRatio},{info.safeArea.width* info.pixelRatio}");

            float safeAreaWidth = (float)(info.safeArea.width * info.pixelRatio);
            float safeAreaHeight = (float)(info.safeArea.height * info.pixelRatio);
            float safeAreaTop = (float)(info.safeArea.top * info.pixelRatio);
#else
            float safeAreaWidth = Screen.safeArea.width;
            float safeAreaHeight = Screen.safeArea.height;
            float safeAreaTop = Screen.safeArea.y;
#endif

            //Log.Debug(LogGroups.Engine, $"微信SDK安全区: {info}"); 

            //float py = (float)info.safeArea.top / (float)info.windowHeight;

            // 将fui的显示区域设置成安全区大小
            Stage.inst.SetSize(safeAreaWidth, safeAreaHeight);
            // 设置默认字体
            UIConfig.defaultFont = "pixelFont";
            // 设置适配类型
            GRoot.inst.SetContentScaleFactor(UIWidth, UIHeight, UIContentScaler.ScreenMatchMode.MatchWidth);

            // 将fui整体下移到刘海屏下方
            GRoot.inst.y = safeAreaTop;
            UIPackage.unloadBundleByFGUI = false;
            
            saveArea = new GComponent();
            saveArea.gameObjectName = "saveArea";
            GRoot.inst.AddChild(saveArea);

            bgArea = new GComponent();
            bgArea.gameObjectName = "bgArea";
            saveArea.AddChild(bgArea);

            gameArea = new GComponent();
            gameArea.gameObjectName = "gameArea";
            saveArea.AddChild(gameArea);

            messageArea = new GComponent();
            messageArea.gameObjectName = "messageArea";
            saveArea.AddChild(messageArea);

            topArea = new GComponent();
            topArea.gameObjectName = "topArea";
            saveArea.AddChild(topArea);

            topArea = new GComponent();
            topArea.gameObjectName = "topArea";
            saveArea.AddChild(topArea);

            // 创建刘海屏和下方区域图片
            frameArea = new GComponent();
            frameArea.gameObjectName = "frameArea";
            GRoot.inst.AddChild(frameArea);

            GComponent topEdge = UIPackage.CreateObject("Common", "edge").asCom;
            topEdge.gameObjectName = "topEdge";
            GComponent downEdge = UIPackage.CreateObject("Common", "edge").asCom;
            downEdge.gameObjectName = "downEdge";
            topEdge.y = -topEdge.height;
            downEdge.y = GRoot.inst.height;
            frameArea.AddChild(topEdge);
            frameArea.AddChild(downEdge);

            enableClickMask = new GComponent();
            enableClickMask.MakeFullScreen();
            enableClickMask.opaque = false;
            enableClickMask.gameObjectName = "enableClickMask";
            GRoot.inst.AddChild(enableClickMask);

            
            //设置通用按钮点击声音
            UIConfig.buttonSound = (NAudioClip)UIPackage.GetItemAsset("Common", "click");
            Debug.Log("设置音乐");
        }

        /// <summary>
        /// 屏蔽点击
        /// </summary>
        public void DisableClick()
        {
            enableClickMask.opaque = true;
        }

        /// <summary>
        /// 启用点击
        /// </summary>
        public void EnableClick()
        {
            enableClickMask.opaque = false;
        }

        /// <summary>
        /// 打开面板
        /// </summary>
        /// <param name="panelName">Panel name.</param>
        public async Task<T> OpenUIPanelAsync<T>(object arg = null) where T : UIBase, new()
        {
            string panelName = typeof(T).ToString();
            if (!uiLoaded.ContainsKey(panelName))
            {
                T ui = new();
                string pkg = ui.PakName;
                string compName = ui.CompName;
                await ResourceManager.Instance.LoadFairyGUIPackage(pkg);
                Log.Debug($"create UI: {pkg}:{compName}");
                GComponent comp = UIPackage.CreateObject(pkg, compName).asCom;
                gameArea.AddChild(comp);
                comp.gameObjectName = $"{pkg}:{compName}";
                ui.fui = comp;
                uiLoaded.Add(panelName, ui);
                ui.OpenPanel(arg);
                return ui;
            }
            else
            {
                T ui = (T)uiLoaded[panelName];
                gameArea.AddChild(ui.fui);
                ui.OpenPanel(arg);
                return ui;
            }

        }


        public void CloseUIPanel<T>(T uiPanel) where T : UIBase
        {
            gameArea.RemoveChild(uiPanel.fui);
        }
    }
}