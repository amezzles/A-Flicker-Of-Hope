using UnityEngine;
using System.Collections;

public class MusicSignalReceiver : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioClip evilMusic;
    public AudioClip happyMusic;
    public AudioClip glitch;
    public AudioClip endHeal;
    public float fadeDuration = 1.5f;

    private Coroutine fadeCoroutine;

    public void PlayEvilMusic()
    {
        if (musicSource != null && evilMusic != null)
            StartFadeToClip(evilMusic);
    }

    public void PlayEvilMusicInstant()
    {
        if (musicSource != null && evilMusic != null)
            musicSource.Play();
    }

    public void PlayHappyMusic()
    {
        if (musicSource != null && happyMusic != null)
            StartFadeToClip(happyMusic);
    }

    public void PlayGlitch()
    {
        if (musicSource != null && glitch != null)
        {
            //musicSource.Stop();
            musicSource.clip = glitch;
            musicSource.loop = false;
            musicSource.Play();
        }
    }

    public void PlayEndHeal()
    {
        if (musicSource != null && endHeal != null)
        {
            //musicSource.Stop();
            musicSource.clip = endHeal;
            musicSource.loop = false;
            musicSource.Play();
        }
    }



    private void StartFadeToClip(AudioClip newClip)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToNewClip(newClip));
    }

    private IEnumerator FadeToNewClip(AudioClip newClip)
    {
        float startVolume = musicSource.volume;

        //fade out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.clip = newClip;
        musicSource.loop = true;
        musicSource.Play();


        //fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = startVolume;
    }
}
