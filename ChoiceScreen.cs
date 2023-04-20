using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChoiceScreen : MonoBehaviour {

    public static ChoiceScreen instance;
    public GameObject root;
    public ChoiceButton choicePrefab;
    public static List<ChoiceButton> choices = new List <ChoiceButton>();
    public VerticalLayoutGroup LayoutGroup;

    void Awake() {
        instance = this;
    }

    public static void Show(string title, params string[] choices) {
        lastChoiceMade.Reset();
        instance.root.SetActive(true);

        if (isShowingChoices) {
            instance.StopCoroutine(showingChoices);
        }

        ClearAllCurrentChoices();
        showingChoices = instance.StartCoroutine(ShowingChoices(choices));
    }

    public static void Hide() {
        if (isShowingChoices) {
            instance.StopCoroutine(showingChoices);
        }
        showingChoices = null;
        ClearAllCurrentChoices();
        instance.root.SetActive(false);
    }

    public static void ClearAllCurrentChoices() {
        foreach (ChoiceButton b in choices) {
            DestroyImmediate(b.gameObject);
        }
        choices.Clear();
    }

    public static bool isWaitingForChoiceToBeMade {get{return isShowingChoices && !lastChoiceMade.hasBeenMade;}}
    public static bool isShowingChoices {get{return showingChoices != null;}}
    static Coroutine showingChoices = null;
    public static IEnumerator ShowingChoices(string[] choices) {
        yield return new WaitForEndOfFrame();
        lastChoiceMade.Reset();

        for (int i = 0; i < choices.Length; i++) {
            if (choices[i].Contains("/")) {
                string[] iconInfo = choices[i].Split('/');
                CreateChoice(iconInfo[0], iconInfo[1]);
            }
            else {
                CreateChoice(choices[i]);
            }
        }

        SetLayoutSpacing();

        while(isWaitingForChoiceToBeMade) {
            yield return new WaitForEndOfFrame();
        }

        Hide();
    }

    static void SetLayoutSpacing() {
        int i = choices.Count;
        if (i >= 5)
            instance.LayoutGroup.spacing = 1;
    }

    static void CreateChoice(string choice, string iconInfo = null) {
        GameObject ob = Instantiate(instance.choicePrefab.gameObject, instance.choicePrefab.transform.parent);
        ob.SetActive(true);
        ChoiceButton b = ob.GetComponent<ChoiceButton>();

        b.text = choice;
        b.choiceIndex = choices.Count;
        if (iconInfo != null) {
            string[] iconList= iconInfo.Split(',');
            try { b.icon1.GetComponent<UnityEngine.UI.Image>().sprite =  Resources.Load <Sprite>("Images/Icons/" + iconList[0]); }
            catch (Exception) {}
            try { b.icon2.GetComponent<UnityEngine.UI.Image>().sprite =  Resources.Load <Sprite>("Images/Icons/" + iconList[1]); }
            catch (Exception) {}
        }

        choices.Add(b);
    }

//######################################################################################################################################################
    [System.Serializable]
    public class CHOICE {
        public bool hasBeenMade {get{return title != "" && index != -1;}}
        public string title = "";
        public int index = -1;

        public void Reset() {
            title = "";
            index = -1;
        }
    }

    public CHOICE choice = new CHOICE();
    public static CHOICE lastChoiceMade{get{return instance.choice;}}

    public void MakeChoice(ChoiceButton button) {
        choice.index = button.choiceIndex;
        choice.title = button.text;
    }

}