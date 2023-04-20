using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public AudioSource music;
    public GameObject creditsButton;
    BCFController controller;

    void Start() {
        controller = BCFController.instance;
        //controller.background.SetTexture(Resources.Load("Images/Backgrounds/MenuScreen") as Texture);
        controller.foreground.SetTexture(Resources.Load("Images/Backgrounds/BlackScreen") as Texture);
        controller.foreground.TransitionToTexture(null, 0.5f);
    }

    void Update() {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) {
            controller.cinematic.TransitionToTexture(null, 2f);
            creditsButton.SetActive(true);
        }
    }

    public void StartGame() {
            controller.foreground.TransitionToTexture(Resources.Load("Images/Backgrounds/BlackScreen") as Texture, 0.5f);
            AudioManager.instance.PlaySFX(Resources.Load("Audio/SFX/PEKO") as AudioClip);
            IEnumerator fadeMusic = AudioFadeOut.FadeOut(music, 2.5f);
            StartCoroutine(fadeMusic);
    }

    public void LoadGame() {
            //controller.foreground.TransitionToTexture(Resources.Load("Images/Backgrounds/BlackScreen") as Texture, 0.5f);
            //IEnumerator fadeMusic = AudioFadeOut.FadeOut(music, 2.5f);
            //StartCoroutine(fadeMusic);
    }

    public void CreditsScreen() {
        controller.cinematic.TransitionToTexture(Resources.Load("Images/Backgrounds/Credits") as Texture, 3f);
        creditsButton.SetActive(false);
    }

    public static class AudioFadeOut {
        public static IEnumerator FadeOut (AudioSource audioSource, float FadeTime) {
            float startVolume = audioSource.volume;
            while (audioSource.volume > 0) {
                audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
                yield return null;
            }
            audioSource.Stop ();
            // Load new scene after audio has completely stopped
            SceneManager.LoadScene(1);
        }
    }

}