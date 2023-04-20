using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using TMPro;

public class SaveLoad : MonoBehaviour {

    public static SaveLoad instance;
    public GameObject MenuScreen;
    public GameObject SaveList;
    public GameObject LoadList;
    public GameObject FileDetails;
    public GameObject Onigiri;
    public TextMeshProUGUI details1;
    public TextMeshProUGUI details2;
    public TextMeshProUGUI details3;
    public TextMeshProUGUI details4;
    public TextMeshProUGUI details5;
    public bool menuOpen = false;

    void Awake() {
        instance = this;
        // Load Save File Times
        for (int i = 1; i < 6; i++) {
            string filepath = Application.persistentDataPath + "/SaveFiles/save" + i + ".txt";
            if (File.Exists(filepath)) {
                using(StreamReader reader = new StreamReader(filepath)) {
                    string rawText = reader.ReadToEnd();
                    string[] chapterText = rawText.Split("\n"[0]);
                    switch (i) {
                        case 1: details1.text = chapterText[0]; break;
                        case 2: details2.text = chapterText[0]; break;
                        case 3: details3.text = chapterText[0]; break;
                        case 4: details4.text = chapterText[0]; break;
                        case 5: details5.text = chapterText[0]; break;
                    }
                }
            }
        }
    }

    public void EnterMenu(string mode) {
        menuOpen = true;
        MenuScreen.SetActive(true);
        if (mode == "Save") { SaveList.SetActive(true); }
        if (mode == "Load") { LoadList.SetActive(true); }
    }

    public void ExitMenu() {
        menuOpen = false;
        MenuScreen.SetActive(false);
        SaveList.SetActive(false);
        LoadList.SetActive(false);
        Onigiri.SetActive(false);
    }

    public void SaveFile(string index) {
        Debug.Log("Saving...");

        // Links NovelController and all it's variables.
        GameObject canvas = GameObject.Find("Canvas");
        NovelController NovelController = canvas.GetComponent<NovelController>();

        Directory.CreateDirectory(Application.persistentDataPath + "/SaveFiles/");
        string path = Application.persistentDataPath + "/SaveFiles/save" + index + ".txt";

        // Scene Information
        string background = BCFController.instance.background.activeImage != null ? BCFController.instance.background.activeImage.texture.name : "null";
        string cinematic = BCFController.instance.cinematic.activeImage != null ? BCFController.instance.cinematic.activeImage.texture.name : "null";
        string foreground = BCFController.instance.foreground.activeImage != null ? BCFController.instance.foreground.activeImage.texture.name : "null";
        string bgm = AudioManager.activeSong != null ? AudioManager.activeSong.source.name : "null";
        if (bgm != "null") {
            bgm = bgm.Substring(6);
            bgm = bgm.TrimEnd(']');
        }

        // Character Information
        string characters = "Next()";
        for (int i = 0; i < CharacterManager.instance.characterList.Count; i++) {
            string name = CharacterManager.instance.characterList[i].characterName;
            string body = CharacterManager.instance.characterList[i].renderers.bodyRenderer.sprite.name;
            string expression = CharacterManager.instance.characterList[i].renderers.expressionRenderer.sprite.name;

            characters += "_enter("+name+")";
            characters += "_setPosition("+name+","+CharacterManager.instance.characterList[i].root.anchorMin[0]+","+CharacterManager.instance.characterList[i].root.anchorMin[1]+")";
            if (body == "null" && expression == "null") {
                characters += "_exit("+name+")";
            } else {
                characters += "_setBody("+name+","+body+")";
                characters += "_setExpression("+name+","+expression+")";
            }
        }

        // Records all triggered flags
        string flagInfo =  "";
        for (int i = 0; i < NovelController.flags.Count; i++) {
            flagInfo += NovelController.flags[i];
            if (i != NovelController.flags.Count - 1) {flagInfo += ",";}
        }

        // Dumps every line (from the progress index onwards) into the text file.
        string chapterInfo = "";
        for (int i = NovelController.progress - 1; i < NovelController.data.Count; i++) {
            chapterInfo += NovelController.data[i] + "\n";
        }

        // Record Save File Information
        string info = System.DateTime.Now.ToString();
        switch (index) {
            case "1": details1.text = info; break;
            case "2": details2.text = info; break;
            case "3": details3.text = info; break;
            case "4": details4.text = info; break;
            case "5": details5.text = info; break;
        }

        File.WriteAllText(path,
            info + "\n" +
            flagInfo + "\n" +
            NovelController.cachedLastSpeaker + "\n" +
            "closeDialogue()_setBackground(null)_setCinematic(null)_setForeground(null)_playBGM(null)_ClearCharacters()_Next()\n" +
            "setBackground("+background+")_setCinematic("+cinematic+")_setForeground("+foreground+")_playBGM("+bgm+")_Next()\n" +
            characters + "\n" +
            chapterInfo
        );

        Onigiri.SetActive(true);
        AudioManager.instance.PlaySFX(Resources.Load("Audio/SFX/mogumogu") as AudioClip);
    }

    public void LoadFile(string index) {
        Debug.Log("Loading...");

        // Links NovelController and all it's variables.
        GameObject canvas = GameObject.Find("Canvas");
        NovelController NovelController = canvas.GetComponent<NovelController>();

        // Save File Path
        string path = Application.persistentDataPath + "/SaveFiles/save" + index + ".txt";

        // Load Functionality
        if (File.Exists(path)) {

            NovelController.progress = 0;
            NovelController.data.Clear();

            using(StreamReader reader = new StreamReader(path)) {
                string rawText = reader.ReadToEnd();
                string[] chapterText = rawText.Split("\n"[0]);

                // Replace Flag List
                NovelController.instance.flags = new List<string>();
                string[] flagsList = chapterText[1].Split(',');
                for (int i = 0; i < flagsList.Length; i++) {
                    NovelController.instance.flags.Add(flagsList[i]);
                }

                // Set Cached Speaker
                NovelController.cachedLastSpeaker = chapterText[2];

                for (int i = 3; i < chapterText.Length; i++) {
                    if (chapterText[i] != "\r" && !chapterText[i].Contains("=")) {
                        NovelController.data.Add(chapterText[i]);
                    }
                }
            }

            NovelController.Next();
            ExitMenu();
        }
    }

}