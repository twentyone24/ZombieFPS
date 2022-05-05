using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Audio_control : MonoBehaviour
{

    [Header("按钮声音 button audio")]
    public AudioClip audio_clip_btn;

    [Header("胜利的音效 win sound")]
    public AudioClip audio_clip_win;

    [Header("audio_clip_fail")]
    public AudioClip audio_clip_fail;

    //单列模式  Singleton mode
    public static Audio_control instance;

    //音源 suido source
    private AudioSource audio_source;

    void Awake()
    {
        //单列模式 Singleton mode
        Audio_control.instance = this;
    }

    // Use this for initialization
    void Start()
    {
        //查找音源
        this.audio_source = this.GetComponent<AudioSource>();
    }

    public void play_btn_sound()
    {
        this.audio_source.PlayOneShot(this.audio_clip_btn, 0.3f);
    }
    public void play_win()
    {
        this.audio_source.PlayOneShot(this.audio_clip_win, 2f);
    }

    public void play_fail_audio()
    {
        this.audio_source.PlayOneShot(this.audio_clip_fail, 0.01f);
    }
}

