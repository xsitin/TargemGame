using Platformer.Core;
using UnityEngine;
using UnityEngine.UI;

public class EffectsVolumeSliderScript : MonoBehaviour
{
    private void Awake()
    {
        var soundController = GameObject.FindWithTag(Tags.GameController).GetComponent<SoundController>();
        soundController.Sync();
        GetComponent<Slider>().value = soundController
            .EffectsVolume;
    }
}