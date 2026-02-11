using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;


public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown QualityDropdown;
    public Toggle MusicToggle;
    public Toggle SFXToggle;
    public Toggle ShadowToggle;
    public Slider VolumeSlider;

    public AudioMixer mixer;

    void Start()
    {
        LoadSettings();
        SaveSettings();
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("quality", index);
    }

    public void SetMusic(bool isOn)
    {
        mixer.SetFloat("musicVolume", isOn ? 0 : -80);
        PlayerPrefs.SetInt("music", isOn ? 1 : 0);
    }

    public void SetSFX(bool isOn)
    {
        mixer.SetFloat("sfxVolume", isOn ? 0 : -80);
        PlayerPrefs.SetInt("sfx", isOn ? 1 : 0);
    }

    public void SetShadow(bool isOn)
    {
        QualitySettings.shadows = isOn ? ShadowQuality.All : ShadowQuality.HardOnly;
        QualitySettings.shadowDistance = isOn ? 150f : 0f;
        PlayerPrefs.SetInt("shadow", isOn ? 1 : 0);
    }

    public void SetVolume(float level)
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(level) * 20f);
        PlayerPrefs.SetFloat("volume", level);
    }

    public void LoadSettings()
    {
        QualityDropdown.value = PlayerPrefs.GetInt("quality", 1);
        QualityDropdown.RefreshShownValue();

        MusicToggle.isOn = PlayerPrefs.GetInt("music", 1) == 1;
        SFXToggle.isOn = PlayerPrefs.GetInt("sfx", 1) == 1;
        ShadowToggle.isOn = PlayerPrefs.GetInt("shadow", 1) == 1;

        VolumeSlider.value = PlayerPrefs.GetFloat("volume", 1f);
    }

    public void SaveSettings()
    {
        SetQuality(QualityDropdown.value);
        SetMusic(MusicToggle.isOn);
        SetSFX(SFXToggle.isOn);
        SetShadow(ShadowToggle.isOn);
        SetVolume(VolumeSlider.value);
    }
}
