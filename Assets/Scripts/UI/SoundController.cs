using System;
using System.Linq;
using Platformer.Core;
using UnityEngine;
using UnityEngine.UI;

public class SoundController : MonoBehaviour
{
    private static SoundController controller;

    private float musicVolume;
    private float effectsVolume;
    private AudioSource musicAudioSource;

    public float MusicVolume
    {
        get => controller.musicVolume;
        set
        {
            controller.musicVolume = value;
            musicAudioSource.volume = controller.musicVolume;
        }
    }

    public float EffectsVolume
    {
        get => controller.effectsVolume;
        set
        {
            controller.effectsVolume = value;
            foreach (var audioSource in Resources.FindObjectsOfTypeAll<AudioSource>().Where(x => x != musicAudioSource))
                audioSource.volume = controller.effectsVolume;
        }
    }


    public AudioSource MusicAudioSource
    {
        get => musicAudioSource;
        set
        {
            musicAudioSource = value;
            musicAudioSource.volume = controller.musicVolume;
        }
    }

    public SoundController()
    {
        controller ??= this;
    }

    private void Awake()
    {
        Sync();
        EffectsVolume = 0.5f;
        MusicVolume = 0.5f;
    }

    public void Sync()
    {
        musicAudioSource = GetComponent<AudioSource>();
        musicAudioSource.volume = controller.musicVolume;
        var audioSources = Resources.FindObjectsOfTypeAll<AudioSource>().Where(x => x != musicAudioSource);
        foreach (var audioSource in audioSources)
            audioSource.volume = controller.effectsVolume;
    }
}