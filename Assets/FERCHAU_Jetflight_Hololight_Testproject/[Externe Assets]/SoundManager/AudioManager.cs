using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private Sound[] sounds;

    private void Awake()
    {
        foreach(Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;

            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop; 
        }

    }

    public void Play(string soundName)
    {
        Sound s = Array.Find(sounds, sound  => sound.name == soundName);
        s.source.Play();
    }

    public void Stop(string soundName)
    {
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s != null && s.source.isPlaying)
        {
            s.source.Stop();
        }
    }
}

// FindObjectOfType<AudioManager>().Play("name");