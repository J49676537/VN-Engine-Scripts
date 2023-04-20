using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baseball : MonoBehaviour {

    public static Baseball instance;

    bool batting = true;
    int playerStrikes = 0;
    int opponentStrikes = 0;

    void Awake() {
        instance = this;
    }

    public void Command_PlayBaseball() {
        // Links NovelController and all it's variables.
        GameObject canvas = GameObject.Find("Canvas");
        NovelController NovelController = canvas.GetComponent<NovelController>();
        NovelController.data.RemoveAt(NovelController.progress+1);
        System.Random random = new System.Random();

        // Disables Save and Load
        NovelController.buttonSave.SetActive(false);
        NovelController.buttonLoad.SetActive(false);

        if (playerStrikes == 3) {
            NovelController.flags.Add("BaseballLoser");
            NovelController.progress += 12;
            NovelController.next = true;
            NovelController.buttonSave.SetActive(true);
            NovelController.buttonLoad.SetActive(true);
        }

        else if (opponentStrikes == 3) {
            NovelController.flags.Add("BaseballWinner");
            NovelController.progress += 12;
            NovelController.next = true;
            NovelController.buttonSave.SetActive(true);
            NovelController.buttonLoad.SetActive(true);
        }

        else {
            if (batting) {
                string choiceText = "CHOICE|Swing_Hard Swing_Soft Swing_Bunt_Don't Swing|";
                for (int i = 0; i < 5; i++) {
                    if (random.Next(3) == 0) { // 25% player miss
                        choiceText += "0_";
                    } else {
                        choiceText += "3_";
                    }
                }
                batting = !batting;
                NovelController.data.Insert(NovelController.progress+1, choiceText);
                NovelController.next = true;
            }
            else {
                string choiceText = "CHOICE|Curveball_Gyroball_Sinker_Underarm_Just chuck it as hard as I can.|";
                for (int i = 0; i < 5; i++) {
                    if (random.Next(2) == 0) { // 33% shuba miss
                        choiceText += "6_";
                    } else {
                        choiceText += "9_";
                    }
                }
                batting = !batting;
                NovelController.data.Insert(NovelController.progress+1, choiceText);
                NovelController.next = true;
            }
        }
    }

    public void Command_StrikeCounter(string winner) {
        if (winner == "Player") {
            playerStrikes += 1;
        } else {
            opponentStrikes += 1;
        }
    }

}