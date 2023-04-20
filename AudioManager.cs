using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour {

    public static AudioManager instance;
    public static SONG activeSong = null;
    public static List<SONG> allSongs = new List<SONG>();
    public float GlobalMaxVolume = 1f;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            DestroyImmediate(gameObject);
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            PlaySFX(Resources.Load("Audio/SFX/PEKO") as AudioClip);
        }
    }

    public static AudioSource CreateNewSource(string sourceName) {
        AudioSource newSource = new GameObject(sourceName).AddComponent<AudioSource>();
        newSource.transform.SetParent(instance.transform);
        return newSource;
    }

    public void PlaySFX(AudioClip effect, float volume = 1f, float pitch = 1f) {
        AudioSource source = CreateNewSource(string.Format("SFX [{0}]", effect.name));
        source.clip = effect;
        source.volume = Mathf.Min(volume, GlobalMaxVolume);
        source.pitch = pitch;
        source.Play();
        Destroy(source.gameObject, effect.length);
    }

//######################################################################################################################################################
    public void PlaySong(AudioClip song, float maxVolume = 1f, float pitch = 1f, float startingVolume = 0f, bool playOnStart = true, bool loop = true, bool smoothTransition = false, float transitionSpeed = 0.5f) {

        maxVolume = Mathf.Min(maxVolume, GlobalMaxVolume);

        if (song != null) {
            for(int i = 0; i < allSongs.Count; i++) {
                SONG s = allSongs[i];
                if (s.clip == song) {
                    activeSong = s;
                    break;
                }
            }
            if (activeSong == null || activeSong.clip != song) {
                activeSong = new SONG(song, maxVolume, pitch, startingVolume, playOnStart, loop);
            }
        }
        else {
            activeSong = null;
        }

        StopAllCoroutines();
        StartCoroutine(VolumeLeveling(smoothTransition, transitionSpeed));
    }

    public void PauseSong() {
        activeSong.Pause();
    }

    public void UnPauseSong() {
        activeSong.UnPause();
    }

    IEnumerator VolumeLeveling(bool songSmoothTransition, float songTransitionSpeed) {
        while(TransitionSongs(songSmoothTransition, songTransitionSpeed)) {
            yield return new WaitForEndOfFrame();
        }
    }

    bool TransitionSongs(bool songSmoothTransition, float songTransitionSpeed) {
        bool anyValueChanged = false;
        float speed = songTransitionSpeed * Time.deltaTime;

        for (int i = allSongs.Count - 1; i >= 0; i--) {
            SONG song = allSongs [i];
            if (song == activeSong) {
                if (song.volume < song.maxVolume) {
                    song.volume = songSmoothTransition ? Mathf.Lerp(song.volume, song.maxVolume, speed) : Mathf.MoveTowards(song.volume, song.maxVolume, speed);
                    anyValueChanged = true;
                }
            } else {
                if (song.volume > 0) {
                    song.volume = songSmoothTransition ? Mathf.Lerp(song.volume, 0f, speed) : Mathf.MoveTowards(song.volume, 0f, speed);
                    anyValueChanged = true;
                } else {
                     allSongs.RemoveAt(i);
                     song.DestroySong();
                     continue;
                }
            }
        }

        return anyValueChanged;
    }

//######################################################################################################################################################
    public class SONG {
        public AudioSource source;
        public AudioClip clip {get{return source.clip;} set{source.clip = value;}}
        public float maxVolume = AudioManager.instance.GlobalMaxVolume;

        public SONG(AudioClip clip, float _maxVolume, float pitch, float startingVolume, bool playOnStart, bool loop) {
            source = AudioManager.CreateNewSource(string.Format("SONG [{0}]", clip.name));
            source.clip = clip;
            source.volume = startingVolume;
            maxVolume = _maxVolume;
            source.pitch = pitch;
            source.loop = loop;

            AudioManager.allSongs.Add(this);

            if (playOnStart) {
                source.Play();
            }
        }

        public float volume {get{return source.volume;} set{source.volume = value;}}
        public float pitch {get{return source.pitch;} set{source.pitch = value;}}

        public void Play() {
            source.Play();
        }
        public void Stop() {
            source.Stop();
        }
        public void Pause() {
            source.Pause();
        }
        public void UnPause() {
            source.UnPause();
        }
        public void DestroySong() {
            AudioManager.allSongs.Remove(this);
            DestroyImmediate(source.gameObject);
        }
    }

}