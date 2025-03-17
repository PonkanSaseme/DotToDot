using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource bgmSource;    // �ΨӼ���I�����֪� AudioSource
    public AudioSource sfxSource;    // �ΨӼ��񭵮Ī� AudioSource
    public AudioClip bgmClip;        // �I������
    public AudioClip[] sfxClips;     // ����
    public float bgmVolume = 0.5f;   // �I�����֭��q
    public float sfxVolume = 1f;     // ���ĭ��q

    private bool isMusicPlaying = false;

    void Start()
    {
        Instance = this;

        // ����֪���Τ�椬
        bgmSource.Stop();
        bgmSource.volume = bgmVolume;
        sfxSource.volume = sfxVolume;
    }

    // �ϥΪ��I���}�l���s��Ĳ�o���ּ���
    public void StartMusic()
    {
        if (!isMusicPlaying)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;  // �]�m���`������I������
            bgmSource.Play();
            isMusicPlaying = true;
        }
    }

    // ����I������
    public void StopMusic()
    {
        if (isMusicPlaying)
        {
            bgmSource.Stop();
            isMusicPlaying = false;
        }
    }

    // ���񭵮�
    public void PlaySFX(int sfxIndex)
    {
        if (sfxIndex >= 0 && sfxIndex < sfxClips.Length)
        {
            sfxSource.PlayOneShot(sfxClips[sfxIndex]);
        }
    }

    // �]�w�I�����֭��q
    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        bgmSource.volume = bgmVolume;
    }

    // �]�w���ĭ��q
    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        sfxSource.volume = sfxVolume;
    }

    // ����U���s�ɼ��񭵮�
    public void OnPointerDown(PointerEventData eventData)
    {
        // �T�O�u���b���ĥ�����ɤ~���񭵮�
        if (!sfxSource.isPlaying)
        {
            sfxSource.PlayOneShot(sfxClips[4]); // ������s����
        }
    }

    // �o�Ӥ�k�i�H�j�w�bUI�W�A����ĭ��q
    public void OnSFXVolumeSliderChanged(float volume)
    {
        SetSFXVolume(volume);
    }

    // �o�Ӥ�k�i�H�j�w�bUI�W�A����BGM���q
    public void OnBGMVolumeSliderChanged(float volume)
    {
        SetBGMVolume(volume);
    }
}
