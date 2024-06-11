using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    /// <summary>
    /// To use: Create game object with sound manager script
    /// To add sound clips: create gameobject with AudioSource, add the gameobject to Sound Manager
    /// </summary>

    public AudioSource[] SoundClips;

    public static SoundManager Instance {get; private set;}

    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(this);
            DontDestroyOnLoad(gameObject);
        }
        else {
            Instance = this;
        }
    }

    void Start() {
        // Initialize Audio Clips
    }

    void Update() {
        
    }

    public void PlaySound(string soundName) {
        bool soundPlayed = false;
        foreach (AudioSource sound in SoundClips){
            if(sound.name == soundName){
                sound.Play();
                soundPlayed = true;
            }
            else if(sound.gameObject.name == soundName){
                sound.Play();
                soundPlayed = true;
            }
        }

        if(!soundPlayed){
            Debug.Log("Couldn't find " + soundName + ", check spelling and make sure to add to SoundManager");
        }        
    }

    public AudioSource GetSound(string soundName) {
        AudioSource soundToReturn = null;
        foreach (AudioSource sound in SoundClips){
            if(sound.name == soundName){
                soundToReturn = sound;
                break;
            }
            else if(sound.gameObject.name == soundName){
                sound.Play();
                soundToReturn = sound;
            }
        }
        if(soundToReturn == null){
            Debug.Log("Couldn't find " + soundName + ", check spelling and make sure to add to SoundManager");
        } 
        return soundToReturn;
    }
}
