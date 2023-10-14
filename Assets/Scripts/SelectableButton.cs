using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectableButton : MonoBehaviour {
	private Button button;
	private FileManager controller;
	public bool isMinimal = false;
	public int tierValue;

	private void Start() {
		button = GetComponent<Button>();
		button.onClick.AddListener(HandleButtonClick);
		controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<FileManager>();
	}

	private void HandleButtonClick() {
		button.interactable = !button.interactable;
		for (int i = 1; i < transform.parent.childCount; i++) {
			transform.parent.GetChild(i).TryGetComponent(out Button b);
			if (b != null && b != button) b.interactable = true;
		}
		button.interactable = false;
		if (isMinimal) controller.SetMinTier(tierValue);
		else controller.SetTier(tierValue);
	}
}
