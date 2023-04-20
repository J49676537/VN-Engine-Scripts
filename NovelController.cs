using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NovelController : MonoBehaviour {

    public static NovelController instance;

    public int progress = 0;
    public string cachedLastSpeaker = "";
    bool coroutineRunning = false;
    public bool paused = false;
    public bool next = false;

    public List<string> flags = new List<string>();
    public List<string> data = new List<string>();

    [HideInInspector] public GameObject arrowAuto;
    [HideInInspector] public GameObject arrowSkip;
    [HideInInspector] public GameObject buttonAuto;
    [HideInInspector] public GameObject buttonSkip;
    [HideInInspector] public GameObject buttonSave;
    [HideInInspector] public GameObject buttonLoad;
    public GameObject volumeControl;

    void Awake() {
        instance = this;
    }

    void Start() {
        LoadChapterFile("Chapter00");
        DialogueSystem.instance.isWaitingForUserInput = true;
        Next();
        Next();
    }

    void Update() {
        if (next == true) {
            Next();
        }
        if (Input.GetKeyDown(KeyCode.Space) && SaveLoad.instance.menuOpen == false) {
            Next();
        }
        if (Input.GetKeyDown(KeyCode.A)) {
            Auto();
        }
        if (Input.GetKey(KeyCode.X)) {
            if (paused == false) {HandleLine(data[progress]);}
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            Log();
        }
        if (Input.GetKeyDown(KeyCode.W)) {
            ScreenSize();
        }
    }

    // Called on every click in [ Canvas -> DialogueSystem ]
    public void Next() {
        if (DialogueSystem.instance.isWaitingForUserInput == true && paused == false) {
            HandleLine(data[progress]);
        }
    }

    public void ScreenSize() {
        Debug.Log("Toggle Screen Size");
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void Auto() {
        Debug.Log("Toggle Auto");
        arrowAuto.SetActive(true);
        arrowSkip.SetActive(false);
        buttonSkip.SetActive(false);
        if (paused == false && coroutineRunning == false) {
                StartCoroutine(WaitTime("auto"));
        } else {
            paused = true;
            arrowAuto.SetActive(false);
            buttonSkip.SetActive(true);
        }
    }

    public void Skip() {
        Debug.Log("Toggle Skip");
        arrowAuto.SetActive(true);
        arrowSkip.SetActive(true);
        buttonAuto.SetActive(false);
        if (paused == false && coroutineRunning == false) {
            StartCoroutine(WaitTime("skip"));
        } else {
            paused = true;
            arrowAuto.SetActive(false);
            arrowSkip.SetActive(false);
            buttonAuto.SetActive(true);
        }
    }

    public IEnumerator WaitTime(string type) {
        float waitTime = 0f;
        coroutineRunning = true;
        while (paused == false) {
            if (type == "skip") {waitTime = 0.1f;}
            if (type == "auto") {
                string speechOnly = data[progress].Contains("|") ? data[progress].Split('|')[1] : "";
                waitTime = speechOnly.Length * 0.07f > 2f ? speechOnly.Length * 0.1f : 2f;
            }
            HandleLine(data[progress]);
            yield return new WaitForSeconds(waitTime);
        }
        coroutineRunning = false;
        paused = false;
    }

    public void Log() {
        Debug.Log("Toggle Log");
    }

    public void SetMaxVolume() {
        float newVolume = 1f;
        switch (AudioManager.instance.GlobalMaxVolume) {
            case 1f:
                newVolume = 0f;
                volumeControl.GetComponent<Image>().sprite = Resources.Load<Sprite> ("Images/UI/UI_Volume0");
                break;
            case 0f:
                newVolume = 0.33f;
                volumeControl.GetComponent<Image>().sprite = Resources.Load<Sprite> ("Images/UI/UI_Volume1");
                break;
            case 0.33f:
                newVolume = 0.66f;
                volumeControl.GetComponent<Image>().sprite = Resources.Load<Sprite> ("Images/UI/UI_Volume2");
                break;
            case 0.66f:
                newVolume = 1f;
                volumeControl.GetComponent<Image>().sprite = Resources.Load<Sprite> ("Images/UI/UI_Volume3");
                break;
        }

        AudioManager.instance.GlobalMaxVolume = newVolume;
        if (AudioManager.activeSong != null) {
            AudioManager.activeSong.maxVolume = newVolume;
            AudioManager.activeSong.volume = newVolume;
        }
    }

//######################################################################################################################################################

    public void LoadChapterFile(string fileName) {
        cachedLastSpeaker = "";
        progress = 0;
        data.Clear();

        TextAsset textAsset = Resources.Load("Story/" + fileName) as TextAsset;
        string rawText = textAsset.text;
        string[] chapterText = rawText.Split("\n"[0]);
        for (int i = 0; i < chapterText.Length; i++) {
            if (chapterText[i] != "\r" && !chapterText[i].Contains("=")) {
                data.Add(chapterText[i]);
            }
        }
    }

    public void InsertChapterFile(string fileName) {
        TextAsset textAsset = Resources.Load("Story/" + fileName) as TextAsset;
        string rawText = textAsset.text;
        string[] chapterText = rawText.Split("\n"[0]);
        for (int i = chapterText.Length - 1; i > 0; i--) {
            if (chapterText[i] != "\r" && !chapterText[i].Contains("=")) {
                data.Insert(progress+1, chapterText[i]);
            }
        }
    }

    void HandleLine(string line) {
        next = false;
        string[] dialogueAndActions = line.Split('|');

        if (dialogueAndActions[0] == "CHOICE") {
            arrowAuto.SetActive(false);
            arrowSkip.SetActive(false);
            buttonAuto.SetActive(false);
            buttonSkip.SetActive(false);
            buttonLoad.SetActive(false);
            paused = true;
            Command_SetLayerImage("ScreenOption", BCFController.instance.cinematic);
            StartCoroutine(HandleChoiceEvent(dialogueAndActions[1], dialogueAndActions[2]));
        }

        else if (dialogueAndActions[0] == "FLAG") {
            if (!flags.Contains(dialogueAndActions[1])) {
                progress += int.Parse(dialogueAndActions[2]);
            }
            next = true;
        }

        else {
            if (dialogueAndActions.Length == 1) {
                HandleEventsFromLine(dialogueAndActions[0]);
            }
            else {
                HandleDialogueFromLine(dialogueAndActions[0], dialogueAndActions[1]);
                HandleEventsFromLine(dialogueAndActions[2]);
            }
        }

        progress++;
    }

    void HandleDialogueFromLine(string dialogueDetails, string dialogue) {
        string speaker = cachedLastSpeaker;
        bool additive = dialogueDetails.Contains("+");
        if (additive) {
            dialogueDetails = dialogueDetails.Remove(dialogueDetails.Length-1);
            dialogue = " " + dialogue;
        }
        if (dialogueDetails.Length > 0) {
            speaker = dialogueDetails;
            cachedLastSpeaker = speaker;
        }
        DialogueSystem.instance.Say(dialogue, speaker, additive);
    }

    void HandleEventsFromLine(string events) {
        string[] actions = events.Split('_');
        foreach (string action in actions) {
            HandleAction(action);
        }
    }

    IEnumerator HandleChoiceEvent(string optionText, string optionAction) {
        string[] options = optionText.Split('_');
        string[] actions = optionAction.Split('_');

        ChoiceScreen.Show("", options);
        while(ChoiceScreen.isWaitingForChoiceToBeMade) {
            paused = true;
            yield return new WaitForEndOfFrame();
        }

        Command_SetLayerImage("null", BCFController.instance.cinematic);
        progress += int.Parse(actions[ChoiceScreen.lastChoiceMade.index]);
        buttonAuto.SetActive(true);
        buttonSkip.SetActive(true);
        buttonLoad.SetActive(true);
        paused = false;
        next = true;
    }

//######################################################################################################################################################

    void HandleAction(string action) {
        string[] data = action.Split('(',')');
        switch (data[0]) {
            case "ClearCharacters":
                CharacterManager.instance.ClearCharacters(); break;
            case "Load":
                LoadChapterFile(data[1]); break;
            case "Insert":
                InsertChapterFile(data[1]); break;
            case "Flag":
                if (!flags.Contains(data[1])) {flags.Add(data[1]);} break;
            case "UnFlag":
                flags.Remove(data[1]); break;
            case "Next":
                next = true; break;
            case "skip":
                progress += int.Parse(data[1]); break;
            case "closeDialogue":
                DialogueSystem.instance.CloseDialogue(); break;
            case "setBackground":
                Command_SetLayerImage(data[1], BCFController.instance.background); break;
            case "setCinematic":
                Command_SetLayerImage(data[1], BCFController.instance.cinematic); break;
            case "setForeground":
                Command_SetLayerImage(data[1], BCFController.instance.foreground); break;
            case "playSFX":
                Command_PlaySFX(data[1]); break;
            case "playBGM":
                Command_PlayBGM(data[1]); break;
            case "pauseBGM":
                AudioManager.instance.PauseSong(); break;
            case "unpauseBGM":
                AudioManager.instance.UnPauseSong(); break;
            case "enter":
                Command_Enter(data[1]); break;
            case "exit":
                Command_Exit(data[1]); break;
            case "setBody":
                Command_SetBody(data[1]); break;
            case "setExpression":
                Command_SetExpression(data[1]); break;
            case "setPosition":
                Command_SetPosition(data[1]); break;
            case "movePosition":
                Command_MovePosition(data[1]); break;
            case "playBaseball":
                Baseball.instance.Command_PlayBaseball(); break;
            case "Strike":
                Baseball.instance.Command_StrikeCounter(data[1]); break;
            case "GAMEOVER":
                SceneManager.LoadScene(0); break;
        }
    }

    // [fileName][*transitionSpeed][*smoothTransition]
    void Command_SetLayerImage(string data, BCFController.LAYER layer) {
        string textureName = data.Contains(",") ? data.Split(',')[0] : data;
        float speed = 1f;
        bool smooth = false;
        Texture2D texture = textureName == "null" ? null : Resources.Load("Images/Backgrounds/" + textureName) as Texture2D;
        if (data.Contains(",")) {
            string[] parameters = data.Split(',');
            foreach(string p in parameters) {
                float fVal = 0;
                bool bVal = false;
                if (float.TryParse(p, out fVal)) {
                    speed = fVal;
                    continue;
                }
                if (bool.TryParse(p, out bVal)) {
                    smooth = bVal;
                    continue;
                }
            }
        }
        layer.TransitionToTexture(texture, speed, smooth);
    }

    // [fileName]
    void Command_PlaySFX(string data) {
        AudioClip clip = Resources.Load("Audio/SFX/" + data) as AudioClip;
        AudioManager.instance.PlaySFX(clip);
    }

    // [fileName][*transitionSpeed][*maxVolume][*loop]
    void Command_PlayBGM(string data) {
        string[] parameters = data.Split(',');
        float speed = parameters.Length > 1 ? float.Parse(parameters[1]) : 0.5f;
        float volume = parameters.Length > 2 ? float.Parse(parameters[2]) : 1f;
        bool loopSong = parameters.Length > 3 ? false : true;
        AudioClip clip = Resources.Load("Audio/BGM/" + parameters[0]) as AudioClip;
        AudioManager.instance.PlaySong(clip, maxVolume:volume, loop:loopSong, transitionSpeed:speed);
    }

    // [characterName]
    void Command_Enter(string data) {
        string[] parameters = data.Split(',');
        Character c = CharacterManager.instance.GetCharacter(parameters[0]);
        c.FadeIn();
    }

    // [characterName]
    void Command_Exit(string data) {
        string[] parameters = data.Split(',');
        Character c = CharacterManager.instance.GetCharacter(parameters[0]);
        c.FadeOut();
    }

    // [characterName][bodyFileName]
    void Command_SetBody(string data) {
        string[] parameters = data.Split(',');
        if (parameters[1] != "null") {
            Character character = CharacterManager.instance.GetCharacter(parameters[0]);
            character.TransitionBody(character.GetSprite(parameters[1]), 2f, false);
        }
    }

    // [characterName][expressionFileName]
    void Command_SetExpression(string data) {
        string[] parameters = data.Split(',');
        if (parameters[1] != "null") {
            Character character = CharacterManager.instance.GetCharacter(parameters[0]);
            character.TransitionExpression(character.GetSprite(parameters[1]), 2f, false);
        }
    }

    // [characterName][targetX][targetY]
    void Command_SetPosition(string data) {
        string[] parameters = data.Split(',');
        Vector2 position = new Vector2 (float.Parse(parameters[1]), float.Parse(parameters[2])); 
        Character character = CharacterManager.instance.GetCharacter(parameters[0]);
        character.SetPosition(position);
    }

    // [characterName][targetX][targetY][*speed]
    void Command_MovePosition(string data) {
        string[] parameters = data.Split(',');
        Vector2 position = new Vector2 (float.Parse(parameters[1]), float.Parse(parameters[2])); 
        Character character = CharacterManager.instance.GetCharacter(parameters[0]);
        character.MoveTo(position, float.Parse(parameters[3]), false);
    }

}