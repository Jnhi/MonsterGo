using FairyGUI;
using GameFramework;

public class Ttfe_SetView : UIBase
{
    public override string PakName => "TtfeGame";

    public override string CompName => "SetView";

    GSlider sldVol;
    GSlider sldBgVol;

    public override void ClosePanel(object arg = null)
    {
        UIManager.Instance.CloseUIPanel(this);
    }

    public override void OpenPanel(object arg = null)
    {

        sldVol = fui.GetChild("sldVol").asSlider;
        sldBgVol = fui.GetChild("sldBgVol").asSlider;

        sldVol.onChanged.Add(sldVolChanged);
        sldBgVol.onChanged.Add(sldBgVolChanged);
        sldVol.value = AudioManager.Instance.musicVol*100;
        sldBgVol.value = AudioManager.Instance.bgmVol * 100;

        fui.GetChild("btnClose").onClick.Set(() => {
            AudioManager.Instance.Play("TtfeGame/m_btn.mp3");
            ClosePanel();
        });
    }

    void sldVolChanged(EventContext context)
    {
        AudioManager.Instance.musicVol = (float)(sldVol.value / 100);
        PlayerPrefs.SetFloat("mVol", (float)(sldVol.value / 100));
    }

    void sldBgVolChanged(EventContext context)
    {
        AudioManager.Instance.bgmVol = (float)(sldBgVol.value / 100);
        PlayerPrefs.SetFloat("BgVol", (float)(sldBgVol.value / 100));
    }
}
