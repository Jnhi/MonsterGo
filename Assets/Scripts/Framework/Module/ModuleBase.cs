using System.Threading.Tasks;
using GameFramework;

public class ModuleBase
{
    public virtual string PakName
    {
        get { return ""; }
    }

    public virtual async Task Open(object obj= null){
        await ResourceManager.Instance.LoadFairyGUIPackage(PakName);
    }

    public virtual void CloseMudule(object obj = null)
    {

    }

    public void Dispose()
    {

    }
}

