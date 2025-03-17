using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource bgmSource;    // 用來播放背景音樂的 AudioSource
    public AudioSource sfxSource;    // 用來播放音效的 AudioSource
    public AudioClip bgmClip;        // 背景音樂
    public AudioClip[] sfxClips;     // 音效
    public float bgmVolume = 0.5f;   // 背景音樂音量
    public float sfxVolume = 1f;     // 音效音量

    private bool isMusicPlaying = false;

    void Start()
    {
        Instance = this;

        // 停止音樂直到用戶交互
        bgmSource.Stop();
        bgmSource.volume = bgmVolume;
        sfxSource.volume = sfxVolume;
    }

    // 使用者點擊開始按鈕來觸發音樂播放
    public void StartMusic()
    {
        if (!isMusicPlaying)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;  // 設置為循環播放背景音樂
            bgmSource.Play();
            isMusicPlaying = true;
        }
    }

    // 停止背景音樂
    public void StopMusic()
    {
        if (isMusicPlaying)
        {
            bgmSource.Stop();
            isMusicPlaying = false;
        }
    }

    // 播放音效
    public void PlaySFX(int sfxIndex)
    {
        if (sfxIndex >= 0 && sfxIndex < sfxClips.Length)
        {
            sfxSource.PlayOneShot(sfxClips[sfxIndex]);
        }
    }

    // 設定背景音樂音量
    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        bgmSource.volume = bgmVolume;
    }

    // 設定音效音量
    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        sfxSource.volume = sfxVolume;
    }

    // 當按下按鈕時播放音效
    public void OnPointerDown(PointerEventData eventData)
    {
        // 確保只有在音效未播放時才播放音效
        if (!sfxSource.isPlaying)
        {
            sfxSource.PlayOneShot(sfxClips[4]); // 播放按鈕音效
        }
    }

    // 這個方法可以綁定在UI上，控制音效音量
    public void OnSFXVolumeSliderChanged(float volume)
    {
        SetSFXVolume(volume);
    }

    // 這個方法可以綁定在UI上，控制BGM音量
    public void OnBGMVolumeSliderChanged(float volume)
    {
        SetBGMVolume(volume);
    }
}
