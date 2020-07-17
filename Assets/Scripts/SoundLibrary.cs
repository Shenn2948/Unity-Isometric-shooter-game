using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundLibrary : MonoBehaviour
{
    public SoundGroup[] SoundGroups;
    readonly Dictionary<string, AudioClip[]> _groupDictionary = new Dictionary<string, AudioClip[]>();

    void Awake()
    {
        foreach (SoundGroup soundGroup in SoundGroups)
        {
            _groupDictionary.Add(soundGroup.GroupId, soundGroup.Group);
        }
    }

    public AudioClip GetClipFromName(string clipName)
    {
        if (_groupDictionary.ContainsKey(clipName))
        {
            var sounds = _groupDictionary[clipName];
            return sounds[Random.Range(0, sounds.Length - 1)];
        }

        return null;
    }

    [Serializable]
    public class SoundGroup
    {
        public string GroupId;
        public AudioClip[] Group;
    }
}