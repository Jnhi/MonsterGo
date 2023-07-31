using FairyGUI;
namespace GameFramework
{
    public abstract class UIBase
    {

        public virtual string PakName
        {
            get { return ""; }
        }

        public virtual string CompName
        {
            get { return ""; }
        }

        public GComponent fui;  //FairyGUI 对象

        public abstract void OpenPanel(object arg);

        public abstract void ClosePanel(object arg);

        public void Dispose()
        {
            this.fui.Dispose();
        }
    }
}

