// Script from https://johnleonardfrench.com/how-to-fade-audio-in-unity-i-tested-every-method-this-ones-the-best/#:~:text=Overall%2C%20the%20best%20way%20to,even%20at%20lower%20frame%20rates.
// Thanks!!!

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class FadeAudioSource {
    public static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = audioSource.volume;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        yield break;
    }
}