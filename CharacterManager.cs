using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour {

    public static CharacterManager instance;

    void Awake() {
        instance = this;
    }

    /// All characters must be attatched to the character panel.
    public RectTransform characterPanel;
    public List<Character> characterList = new List<Character>();
    public Dictionary<string, int> characterDictionary = new Dictionary<string, int>();

    /// Try to get a character by the name provided from the character list. Create a character if it does not exist.
    public Character GetCharacter(string characterName, bool enableCreatedCharacterOnStart = true, bool doesNotExist = true) {
        int index = -1;
        if (characterDictionary.TryGetValue(characterName, out index)) {
            return characterList[index];
        } else if (doesNotExist) {
            return CreateCharacter(characterName, enableCreatedCharacterOnStart);
        }
        return null;
    }

    public Character CreateCharacter(string characterName, bool enableOnStart = true) {
        Character newCharacter = new Character (characterName, enableOnStart);
        characterDictionary.Add(characterName, characterList.Count);
        characterList.Add(newCharacter);
        return newCharacter;
    }

    // I don't know how this works.
    // Throws an error if used while characters are still transitioning.
    public void ClearCharacters() {
        characterList = new List<Character>();
        characterDictionary = new Dictionary<string, int>();
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }
        Resources.UnloadUnusedAssets();
    }

}