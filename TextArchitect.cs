using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextArchitect {

	// A dictionary keeping tabs on all architects present in a scene. Prevents multiple architects from influencing the same text object simultaneously.
	private static Dictionary<TextMeshProUGUI, TextArchitect> activeArchitects = new Dictionary<TextMeshProUGUI, TextArchitect>();

	private string preText;
	private string targetText;

	private int charactersPerFrame;
	private float speed;

	public bool skip = false;

	public bool isConstructing {get{return buildProcess != null;}}
	Coroutine buildProcess = null;

	TextMeshProUGUI tmpro;

	public TextArchitect(TextMeshProUGUI tmpro, string targetText, string preText = "", int charactersPerFrame = 1, float speed = 0.5f)
	{
		this.tmpro = tmpro;
		this.targetText = targetText;
		this.preText = preText;
		this.charactersPerFrame = charactersPerFrame;
		this.speed = speed;

		Initiate();
	}

	public void Stop()
	{
		if (isConstructing)
		{
			DialogueSystem.instance.StopCoroutine(buildProcess);
		}
		buildProcess = null;
	}

	IEnumerator Construction()
	{
		int runsThisFrame = 0;

		tmpro.text = "";
		tmpro.text += preText;

		tmpro.ForceMeshUpdate();
		TMP_TextInfo inf = tmpro.textInfo;
		int vis = inf.characterCount;

		tmpro.text += targetText;

		tmpro.ForceMeshUpdate();
		inf = tmpro.textInfo;
		int max = inf.characterCount;

		tmpro.maxVisibleCharacters = vis;

		while(vis < max)
		{
			// Allow skipping by increasing the characters per frame and the speed of occurrence.
			if (skip)
			{
				speed = 1;
				charactersPerFrame = charactersPerFrame < 5 ? 5 : charactersPerFrame + 3;
			}

			// Reveal a certain number of characters per frame.
			while(runsThisFrame < charactersPerFrame)
			{
				vis++;
				tmpro.maxVisibleCharacters = vis;
				runsThisFrame++;
			}

			// Wait for the next available revelation time.
			runsThisFrame = 0;
			yield return new WaitForSeconds(0.01f * speed);
		}

		// Terminate the architect and remove it from the active log of architects.
		Terminate();
	}

	void Initiate()
	{
		// Check if an architect for this text object is already running. If it is, terminate it. Do not allow more than one architect to affect the same text object at once.
		TextArchitect existingArchitect = null;
		if (activeArchitects.TryGetValue(tmpro, out existingArchitect))
			existingArchitect.Terminate();

		buildProcess = DialogueSystem.instance.StartCoroutine(Construction());
		activeArchitects.Add(tmpro, this);
	}

	/// Terminate this architect. Stops the text generation process and removes it from the cache of all active architects.
	public void Terminate()
	{
		activeArchitects.Remove(tmpro);
		if (isConstructing)
			DialogueSystem.instance.StopCoroutine(buildProcess);
		buildProcess = null;
	}

}