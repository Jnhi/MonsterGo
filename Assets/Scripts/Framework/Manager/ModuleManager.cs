using System.Threading.Tasks;

namespace GameFramework
{
    public class ModuleManager : Singleton<ModuleManager>
    {

        public void Init()
        {

        }



        /// <summary>
        /// 打开模块
        /// </summary>
        /// <param name="panelName">Panel name.</param>
        public async Task<T> OpenModule<T>(object obj = null) where T : ModuleBase, new()
        {
            T module = new();
            Log.Debug($"open Module: {module.PakName}-{obj}");
            await module.Open(obj);
            return module;
        }

    }
}
