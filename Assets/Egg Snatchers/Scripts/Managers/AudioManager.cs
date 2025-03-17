using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Elements")]
    [SerializeField] private AudioSource trampolineSource;
    [SerializeField] private AudioSource landSource;
    [SerializeField] private AudioSource fallSource;
    [SerializeField] private AudioSource grabHatSource;
    [SerializeField] private AudioSource loseHatSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    public void PlayTrampolineSound()
    {
        trampolineSource.Play();
    }

    public void PlayLandSound()
    {
        landSource.Play();
    }

    public void PlayFallSound()
    {
        fallSource.Play();
    }

    public void PlayGrabHatSound()
    {
        grabHatSource.Play();
    }

    public void PlayLoseHatSound()
    {
        loseHatSource.Play();
    }
}
