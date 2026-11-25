using UnityEngine;
using UnityEngine.UI;

public class SliderSom : MonoBehaviour
{
    [Header("slider")]
    public Slider SliderDeVolume;

    void Start()
    {
        if (PlayerPrefs.HasKey("volumeDoSom"))
        {
            CarregarVolume();
        }
        else
        {
            PlayerPrefs.SetFloat("soundVolume", 1);
            CarregarVolume();
        }
    }

    public void DefinirVolume()
    {
        AudioListener.volume = SliderDeVolume.value;
        SalvarVolume();
    }

    public void SalvarVolume()
    {
        PlayerPrefs.SetFloat("volumeDoSom", SliderDeVolume.value);
    }

    public void CarregarVolume()
    {
        SliderDeVolume.value = PlayerPrefs.GetFloat("volumeDoSom");
    }

}
