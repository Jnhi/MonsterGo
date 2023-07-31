using System;
using FairyGUI;
namespace GameFramework
{
    public class TabsComp
    {

        private GButton[] btns;
        private int curIndex;
        private Action<int> refreshFunc;
        public TabsComp(GButton[] buttons, int startIndex, Action<int> refreshFunc)
        {
            this.btns = buttons;
            this.refreshFunc = refreshFunc;

            for (int i = 0; i < this.btns.Length; i++)
            {
                GButton btn = this.btns[i];
                btn.mode = ButtonMode.Radio;
                btn.changeStateOnClick = false;
                int u = i;
                btn.onClick.Set(() =>
                {
                    if (!btn.selected)
                    {
                        this.refresh(u);
                    }

                });
            };

            this.refresh(startIndex);
        }

        public void refresh(int index)
        {
            this.curIndex = index;
            for (int i = 0; i < this.btns.Length; i++)
            {
                GButton btn = this.btns[i];
                if (i == index)
                {
                    btn.selected = true;
                    this.refreshFunc(index);
                }
                else
                {
                    btn.selected = false;
                }
            }
        }
    }
}

