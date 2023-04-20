using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueSystem : MonoBehaviour {

    public static DialogueSystem instance;
    public ELEMENTS elements;

    [System.Serializable]
    public class ELEMENTS {
        public GameObject userInterface;
        public GameObject speechBox;
        public TextMeshProUGUI speechText;
        public GameObject speakerBox;
        public TextMeshProUGUI speakerText;
    }

    public GameObject userInterface {get{return elements.userInterface;}}
    public GameObject speechBox {get{return elements.speechBox;}}
    public TextMeshProUGUI speechText {get{return elements.speechText;}}
    public GameObject speakerBox {get{return elements.speakerBox;}}
    public TextMeshProUGUI speakerText {get{return elements.speakerText;}}

    // Function References
    [HideInInspector]
    public bool isWaitingForUserInput = false;
    public string targetSpeech = "";

    Coroutine speaking = null;
    public bool isSpeaking {get{return speaking != null;}}

    // Connecting the DialogueSystem to TextArchitect
    TextArchitect textArchitect = null;

//######################################################################################################################################################
    void Awake() {
        instance = this;
    }

    public void Say(string speech, string speaker = "", bool additive = false) {
        StopSpeaking();
        if (additive) {
            speechText.text = targetSpeech;
        }
        speaking = StartCoroutine(Speaking(speech, additive, speaker));
    }

    public void StopSpeaking() {
        if (isSpeaking) {
            StopCoroutine(speaking);
        }
        // Stop textArchitect if active and running.
        if (textArchitect != null && textArchitect.isConstructing) {
            textArchitect.Stop();
        }
        speaking = null;
    }

//######################################################################################################################################################
    IEnumerator Speaking(string speech, bool additive, string speaker = "") {
        userInterface.SetActive(true);
        speechBox.SetActive(true);
        speakerBox.SetActive(speaker != "Narrator");
        string additiveSpeech = additive ? speechText.text : "";
        targetSpeech = additiveSpeech + speech;
        speakerText.text = DetermineSpeaker(speaker); // temporary? // what??
        isWaitingForUserInput = false;

        textArchitect = new TextArchitect(speechText, speech, additiveSpeech);

        while (textArchitect.isConstructing) {
            // Instant Text Generation
            // if (Input.GetKey(KeyCode.X)) {textArchitect.skip = true;}
            yield return new WaitForEndOfFrame();
        }

        // Text is finished rendering
        isWaitingForUserInput = true;
        while(isWaitingForUserInput) {
            yield return new WaitForEndOfFrame();
        }

        StopSpeaking();
    }

    // Returns the current name of the speaker, the default return is the current name.
    string DetermineSpeaker(string newName) {
        string speakerName = speakerText.text;
        if (newName != speakerText.text && newName != "")
            speakerName = (newName.Contains("Narrator")) ? "" : newName;
        return speakerName;
    }

    public void CloseDialogue() {
        userInterface.SetActive(false);
        speechBox.SetActive(false);
        speakerBox.SetActive(false);
    }

}