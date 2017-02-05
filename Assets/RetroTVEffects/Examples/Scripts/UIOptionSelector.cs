using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class UIOptionChangedEvent : UnityEvent<int> { }

public class UIOptionSelector : MonoBehaviour
{
	public string[] Options;
	public Text CurrentOptionLabel;

	public int DefaultOption = 0;

	public UIOptionChangedEvent OnOptionChanged = new UIOptionChangedEvent();

	private int currentSelection = 0;

	void Start()
	{
		currentSelection = DefaultOption;
		updateCurrentOption();
	}

	public void Previous()
	{
		currentSelection--;
		if (currentSelection < 0)
			currentSelection = 0;

		updateCurrentOption();

		OnOptionChanged.Invoke(currentSelection);
	}

	public void Next()
	{
		currentSelection++;
		if (currentSelection >= Options.Length)
			currentSelection = Options.Length - 1;

		updateCurrentOption();

		OnOptionChanged.Invoke(currentSelection);
	}

	void updateCurrentOption()
	{
		CurrentOptionLabel.text = Options[currentSelection];
	}
}