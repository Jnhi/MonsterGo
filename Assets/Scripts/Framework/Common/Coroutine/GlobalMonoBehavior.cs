using System;
namespace GameFramework
{
    public class GlobalMonoBehavior : MonoSingleton<GlobalMonoBehavior>
    {


        public delegate void OnUpdate();

        private OnUpdate onUpdate = null;

        private void Update()
        {
            if (this.onUpdate != null)
            {
                try
                {
                    this.onUpdate();

                }
                catch (Exception e)
                {
                    Log.Error("Global Update Error" + e.Message);
                }
            }
        }

        public OnUpdate AddUpdate(OnUpdate e)
        {
            this.onUpdate += e;
            return e;
        }

        public void RemoveUpdate(OnUpdate e)
        {
            try
            {
                if (e != null) this.onUpdate -= e;
            }
            catch (Exception ex)
            {
                Log.Error("remove error: " + ex.Message);
            }
        }

    }
}