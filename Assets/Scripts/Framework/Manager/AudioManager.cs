using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameFramework
{
    /// <summary>
    /// 音频管理器
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {

        /// <summary>
        /// 背景音乐(只能存在一个，并且是循环的，播放新的会自动替换旧的)
        /// </summary>
        private AudioSource bgmSource;

        /// <summary>
        /// BGM大小
        /// </summary>

        private float _bgmVol = 0.5f;
        public float bgmVol
        {
            get
            {
                return _bgmVol;
            }
            set
            {
                _bgmVol = value;
                bgmSource.volume = bgmVol;
            }
        }
        /// <summary>
        /// 音乐大小
        /// </summary>
        public float musicVol = 0.5f;

        /// <summary>
        /// 播放中的音频List
        /// </summary>
        public List<AudioPoolData> soundList = new List<AudioPoolData>();

        /// <summary>
        /// 对象池最大数量
        /// </summary>
        private int MAXPOOLNUM = 2;

        private class MonoStub : MonoBehaviour { }

        private MonoStub monoStub;

        private GameObject monoStubObj;
        public void Init()
        {
            monoStubObj = GameObject.Find("MonoStubObj");
            if (monoStubObj == null)
            {
                monoStubObj = new GameObject();
                UnityEngine.Object.DontDestroyOnLoad(monoStubObj);
                monoStubObj.name = "MonoStubObj";
                monoStub = monoStubObj.AddComponent<MonoStub>();
                monoStub.AddComponent<MonoStub>();
                bgmSource = monoStubObj.AddComponent<AudioSource>();
            }

            if (PlayerPrefs.HasKey("BgVol"))
            {
                float bgVol = PlayerPrefs.GetFloat("BgVol");
                bgmVol = bgVol;
            }
            else
            {
                PlayerPrefs.SetFloat("BgVol", 0.5f);
                bgmVol = 0.5f;
            }

            if (PlayerPrefs.HasKey("mVol"))
            {
                float mVol = PlayerPrefs.GetFloat("mVol");
                musicVol = mVol;
            }
            else
            {
                PlayerPrefs.SetFloat("mVol", 0.5f);
                musicVol = 0.5f;
            }
        }

        public void Start()
        {

        }

        public void Update()
        {

        }

        /// <summary>
        /// 获取音频组件
        /// </summary>
        /// <returns></returns>
        private AudioSource GetAudioSource(out AudioPoolData apData)
        {
            apData = soundList.Find(data => data.IsUseing == false);
            if (apData != null)
                apData.IsUseing = true;
            return apData != null ? apData.AudioSource : AddAudioSource(out apData);
        }

        /// <summary>
        /// 添加音频组件
        /// </summary>
        private AudioSource AddAudioSource(out AudioPoolData poolData)
        {
            AudioSource audioSource = monoStubObj.AddComponent<AudioSource>();
            poolData = new AudioPoolData(audioSource, true, false);
            soundList.Add(poolData);
            return audioSource;
        }


        /// <summary>
        /// 播放音乐
        /// </summary>
        /// <param name="musicName"></param>
        /// <param name="clip"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public async void Play(string musicPath, bool isLoop = false, Action cb = null)
        {
            AudioClip clip = await Addressables.LoadAssetAsync<AudioClip>(musicPath).Task;

            AudioSource audioSource = GetAudioSource(out AudioPoolData apDate);
            if (audioSource == null)
            {
                Debug.LogError(" 类型音频播放组件没找到");
            }

            audioSource.loop = isLoop;
            audioSource.volume = musicVol;
            audioSource.clip = clip;
            audioSource.Play();
            monoStub.StartCoroutine(WaitPlayEnd(apDate, cb));
        }

        /// <summary>
        /// 音乐暂停
        /// </summary>
        /// <param name="apData"></param>
        /// <returns></returns>
        public AudioPoolData Pause(AudioPoolData apData)
        {
            apData.AudioSource.Pause();
            apData.IsInPause = true;
            return apData;
        }

        /// <summary>
        /// 音乐恢复
        /// </summary>
        /// <param name="apData"></param>
        /// <returns></returns>
        public AudioPoolData Resume(AudioPoolData apData)
        {
            apData.AudioSource.UnPause();
            apData.IsInPause = false;
            return apData;
        }

        /// <summary>
        /// 音乐停止
        /// </summary>
        /// <param name="apData"></param>
        public void StopMusic(AudioPoolData apData)
        {
            apData.AudioSource.Stop();
            apData.IsUseing = false;
        }


        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="isLoop"></param>
        public async void PlayBGM(string musicPath, bool isLoop = true)
        {
            AudioClip clip = await Addressables.LoadAssetAsync<AudioClip>(musicPath).Task;

            bgmSource.clip = clip;
            bgmSource.loop = isLoop;
            bgmSource.volume = bgmVol;
            bgmSource.Play();
        }

        /// <summary>
        /// 暂停/恢复背景音乐
        /// </summary>
        public void PauseBGM()
        {
            if (bgmSource.clip == null)
            {
                Debug.LogWarning("没有设置背景音乐");
                return;
            }
            if (bgmSource.isPlaying)
            {
                bgmSource.Pause();
            }
            else
            {
                bgmSource.UnPause();
            }
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void StopBGM()
        {
            bgmSource.Stop();
        }


        /// <summary>
        /// 回收音频播放组件对象
        /// </summary>
        private void RecyclePool(AudioPoolData apData)
        {
            if (soundList.Contains(apData))
                apData.IsUseing = false;

            //控制下对象池的大小
            if (soundList.Count > MAXPOOLNUM)
            {
                soundList.Remove(apData);
                UnityEngine.Object.Destroy(apData.AudioSource);
            }
        }

        /// <summary>
        /// 待音效播放完成后 将其移至未使用集合中,
        /// 同时执行播放完毕的回调
        /// </summary>
        private IEnumerator WaitPlayEnd(AudioPoolData apData, Action action)
        {
            if (apData == null) yield break;
            //如果没有在使用中的，或者没有正在播放且没有处于暂停状态的就要进行回收
            yield return new WaitUntil(() => !apData.IsUseing || (!apData.AudioSource.isPlaying && !apData.IsInPause));

            RecyclePool(apData);
            if (action != null)
                action.Invoke();
            yield break;
        }
    }

    public class AudioPoolData
    {

        [SerializeField]
        public AudioSource _audioSource;

        [SerializeField]
        private bool _isUseing; //是否在使用中(暂停中属于使用中，因为考虑到还可以继续播放)

        [SerializeField]
        private bool _isInPause;//是否在暂停中


        public AudioSource AudioSource { get => _audioSource; set => _audioSource = value; }
        public bool IsUseing { get => _isUseing; set => _isUseing = value; }
        public bool IsInPause { get => _isInPause; set => _isInPause = value; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="clip"></param>
        public AudioPoolData(AudioSource _audioSource, bool _isUsed, bool _isInPause)
        {
            this.AudioSource = _audioSource;
            this.IsUseing = _isUsed;
            this.IsInPause = _isInPause;
        }
    }
}
