using FairyGUI;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using LitJson;

namespace GameFramework
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        public static Action<string, byte[]> OnFBLoadedHandle = null;
        private IDictionary<string, int> _pkgMap = new Dictionary<string, int>();

        public async Task Init()
        {
            await ResourceManager.Instance.LoadFairyGUIPackage("Common");

            NTexture.CustomDestroyMethod += (Texture t) =>
            {
                var name = t.name;
                Addressables.Release(t);
                // Debug.Log("release C# :" + name);
                Log.Debug(".... release addressable: " + t.name);
            };

            NAudioClip.CustomDestroyMethod += (AudioClip t) =>
            {
                var name = t.name;
                Addressables.Release(t);
                // Debug.Log("release C# :" + name);
                Log.Debug(".... release addressable: " + t.name);
            };
        }

        public void ReleaseFGUIPackage(string packageName)
        {
            bool isHave = this._pkgMap.TryGetValue(packageName, out int count);

            if (isHave && count > 1)
            {
                this._pkgMap[packageName] = this._pkgMap[packageName] - 1;
            }
            else
            {
                Log.Debug($"release fagui package:{packageName}");
                this._pkgMap.Remove(packageName);
                UIPackage.RemovePackage(packageName);
            }
        }

        public async Task LoadFairyGUIPackage(string packageName)
        {

            bool isHave = this._pkgMap.TryGetValue(packageName, out int count);

            if (isHave == false || count < 1)
            {
                //没有缓存，加载
                string address = packageName + "_fui.bytes";
                var pkgAsset = await Addressables.LoadAssetAsync<TextAsset>(packageName + "/" + address).Task;
                UIPackage.AddPackage(
                    pkgAsset.bytes,
                    packageName,
                    async (string name, string extension, Type type, PackageItem ite) =>
                    {
                        Log.Debug($"{packageName + "/" + name}, {extension}, {type}, {ite}");
                        if (type == typeof(Texture))
                        {
                            Texture t = await Addressables.LoadAssetAsync<Texture>(packageName + "/" + name + extension).Task;
                            ite.owner.SetItemAsset(ite, t, DestroyMethod.Custom);
                        }
                        else if (type == typeof(AudioClip))
                        {
                            AudioClip t = await Addressables.LoadAssetAsync<AudioClip>(packageName + "/" + name + extension).Task;
                            ite.owner.SetItemAsset(ite, t, DestroyMethod.Custom);
                        }
                    });
                Log.Debug($"加载完成FGUI包{packageName}");
                Addressables.Release(pkgAsset);
                this._pkgMap[packageName] = 1;
            }
            else
            {
                this._pkgMap[packageName] = this._pkgMap[packageName] + 1;
            }
        }

        public async Task<SceneInstance> LoadScene(string sceneName, LoadSceneMode mode, Action<float> update, bool isActiveOnLoaded = true, int priority = 100)
        {
            //try
            //{
            var handle = Addressables.LoadSceneAsync("Scenes/" + sceneName + ".unity", mode, isActiveOnLoaded, priority);

            var _update = GlobalMonoBehavior.Instance.AddUpdate(e: () =>
            {
                update?.Invoke(handle.PercentComplete);
            });

            var res = await handle.Task;

            GlobalMonoBehavior.Instance.RemoveUpdate(_update);

            return res;
            //}
            //catch(Exception ex)
            //{
            //    Log.Error(LogGroups.Engine, $"Load Scene:${sceneName} : ${ex}");
            //    return null;
            //}

        }

        public async Task<SceneInstance> UnloadScene(SceneInstance sceneInstance, bool autoReleaseHandler = true)
        {
            var res = await Addressables.UnloadSceneAsync(sceneInstance, autoReleaseHandler).Task;
            return res;
        }

        public void UnloadSceneByName(string sceneName)
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }

        public async Task<GameObject> LoadPrefab(string address)
        {
            var res = await Addressables.LoadAssetAsync<GameObject>(address).Task;

            return res;
        }


        public async Task<TextAsset> LoadTextAsset(string address)
        {
            var res = await Addressables.LoadAssetAsync<TextAsset>(address).Task;

            return res;
        }

        public async Task<T> LoadJson<T>(string address)
        {
            var res = await Addressables.LoadAssetAsync<TextAsset>(address).Task;
            //T t = JsonUtility.FromJson<T>(res.text);
            T t = JsonMapper.ToObject<T>(res.text);
            return t;
        }

        public async Task<Sprite> LoadSprite(string address)
        {
            var res = await Addressables.LoadAssetAsync<Sprite>(address).Task;

            return res;
        }

        public async Task<Font> LoadFont(string address)
        {
            var res = await Addressables.LoadAssetAsync<Font>(address).Task;

            return res;
        }

        public void ReleaseAddressGO(UnityEngine.Object go)
        {
            Addressables.Release(go);
        }


        public string GetStatusSummary()
        {
            return "";
        }
    }
}