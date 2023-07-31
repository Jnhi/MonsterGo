using FairyGUI;
using GameFramework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 启动器
/// </summary>
public class GameLaunch : MonoSingleton<GameLaunch>
{

    public GameObject RootView;
    void Awake()
    {
        DontDestroyOnLoad(this);

    }
    // 热更结束后调用
    public async void GameStart()
    {

#if UNITY_ANDROID  && !UNITY_EDITOR 
        Application.targetFrameRate = 60;
#else
        Application.targetFrameRate = 60;
#endif
        CoroutineManager.Instance.Startup();

        //加载FairyGUI Package
        AudioManager.Instance.Init();
        await ResourceManager.Instance.Init();

        UIManager.Instance.Init();
        ModuleManager.Instance.Init();

        Button btnStart = RootView.transform.Find("btnStart").GetComponent<Button>();
        AudioManager.Instance.PlayBGM("MonsterGo/music/title.ogg");
        btnStart.onClick.AddListener(async () =>
        {
            // await ModuleManager.Instance.OpenModule<TtfeModule>();
            btnStart.enabled = false;
            await ModuleManager.Instance.OpenModule<MonsterGoModule>();
            RootView.SetActive(false);
            btnStart.enabled = true;
            Log.Debug($"关闭主界面");
        });
    }

    // Start is called before the first frame update
    void Start()
    {
        GameStart();
    }
}
