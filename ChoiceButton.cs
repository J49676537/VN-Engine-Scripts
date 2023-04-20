using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChoiceButton : MonoBehaviour {

    public TextMeshProUGUI tmpro;
    public string text {get{return tmpro.text;} set{tmpro.text = value;}}
    public GameObject icon1;
    public GameObject icon2;
    public int choiceIndex = -1;

}