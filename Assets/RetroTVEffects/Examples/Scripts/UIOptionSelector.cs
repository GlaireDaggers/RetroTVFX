using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using UnityEngine.Events;

namespace RetroTVFX.Examples
{
	[System.Serializable]
	public class UIOptionChangedEvent : UnityEvent<int> { }

	public class UIOptionSelector : MonoBehaviour
	{
		public string[] Options;
		public Text CurrentOptionLabel;

		public int DefaultOption = 0;

		public UIOptionChangedEvent OnOptionChanged = new UIOptionChangedEvent();

		private int _currentSelection = 0;

		void Start()
		{
			_currentSelection = DefaultOption;
			updateCurrentOption();
		}

		public void Previous()
		{
			_currentSelection--;
			if (_currentSelection < 0)
				_currentSelection = 0;

			updateCurrentOption();

			OnOptionChanged.Invoke(_currentSelection);
		}

		public void Next()
		{
			_currentSelection++;
			if (_currentSelection >= Options.Length)
				_currentSelection = Options.Length - 1;

			updateCurrentOption();

			OnOptionChanged.Invoke(_currentSelection);
		}

		void updateCurrentOption()
		{
			CurrentOptionLabel.text = Options[_currentSelection];
		}
	}
}