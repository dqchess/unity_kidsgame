﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using uAudio;
using System;
public class AudioPlay : MonoBehaviour//, uAudio_backend.IAudioPlayer
{

    public static AudioPlay main;
    public uAudioStreamer uAudioNet;


    public uAudioStreamer_UI uAudioUI;

    private AudioSource audioSource;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        audioSource = this.gameObject.AddComponent<AudioSource>();
        AudioPlay.main = this;
        /* 
                if(instance==null){
                    instance = this;
                    DontDestroyOnLoad(this);
                }else if(this!=instance){
                    //防止重复创建
                    Destroy(this.gameObject);
                }
        */



        uAudioUI = AppSceneBase.main.objuAudio.GetComponent<uAudioStreamer_UI>();

        // uAudioNet = this.gameObject.AddComponent<uAudioStreamer>();
        // uAudioNet.sendPlaybackState += new System.Action<uAudio.uAudio_backend.PlayBackState>(uAudioPlayStatus);

    }
    // Use this for initialization
    void Start()
    {
        bool ret = Common.GetBool(AppString.STR_KEY_BACKGROUND_MUSIC);
        Debug.Log("AudioPlay Start");
        if (ret)
        {
            audioSource.clip = AudioCache.main.Load("App/Audio/Bg");
            Play();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Stop()
    {
        audioSource.Stop();
    }
    public void Play()
    {
        Debug.Log("AudioPlay play()");
        audioSource.Play();
    }

    public void Pause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }


    }

    public void PlayAudioClip(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }
        audioSource.PlayOneShot(clip);
    }

    public void PlayFile(string audiofile)
    {
        AudioClip clip = AudioCache.main.Load(audiofile);
        if (clip == null)
        {
            return;
        }
        audioSource.PlayOneShot(clip);
    }


    public void PlayUrl(string url)
    {
        if (Common.isAndroid || Common.isiOS)
        {
            StartCoroutine(PlayUrlEnumerator(url));
        }
        else
        {
            //url = "https://cdn.feilaib.top/img/sounds/bg.mp3";
            //  uAudioNet.SetURL(url);
            //   uAudioNet.SetFile(url);
            // uAudioNet.Play(null);

            Debug.Log("PlayUrl:url=" + url);
            //    uAudioUI.Stop();
            //    uAudioUI.targetFilePath =url;
            //    uAudioUI.Play(null);
        }

    }

    //@uAudio
    public void uAudioPlayStatus(uAudio.uAudio_backend.PlayBackState v)
    {
        Debug.Log("sendPlaybackState: " + v.ToString() + "---- 7ts87dt");
    }
    //@uAudio

    //https://blog.csdn.net/qq_15386973/article/details/78696116 
    IEnumerator PlayUrlEnumerator(string url)
    {
        using (var uwr = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError)
            {
                Debug.LogError(uwr.error);
                yield break;
            }
            AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
            // use audio clip
            PlayAudioClip(clip);
        }
    }
}
