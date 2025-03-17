using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource _footStepSfx;

    [SerializeField]
    private AudioSource _landingSfx;

    [SerializeField]
    private AudioSource _punchSfx;

    [SerializeField]
    private AudioSource _glideSfx;


    private void PlayFootStepSfx()
    {
        _footStepSfx.volume = Random.Range(0.8f, 1f);
        _footStepSfx.pitch = Random.Range(0.8f, 1.5f);
        _footStepSfx.Play();
    }

    private void PlayLandingSfx()
    {
        _landingSfx.Play();
    }

    private void PlayPunchSfx()
    {
        _punchSfx.volume = Random.Range(0.8f, 1f);
        _punchSfx.pitch = Random.Range(0.8f, 1.5f);
        _punchSfx.Play();
    }

    public void PlayGlideSfx()
    {
        _glideSfx.Play();
    }

    public void StopGlideSfx()
    {
        _glideSfx.Stop();
    }
}
