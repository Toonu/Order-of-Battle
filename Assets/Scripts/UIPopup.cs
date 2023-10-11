using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup : MonoBehaviour {
	private GameObject popupObject;
	private TextMeshProUGUI textLabelUI; //Popup UI for the text label.

	/// <summary>
	/// Method sets up the Components on startup and switches the popup off.
	/// </summary>
	void Awake() {
		textLabelUI = transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
		popupObject = transform.GetChild(0).gameObject;
	}

	/// <summary>
	/// Method pops up the Popup for the specified duration with textLabelUI.
	/// </summary>
	/// <param name="title">String textLabelUI to show</param>
	/// <param name="duration">Float time duration in seconds</param>
	public void PopUp(string title = "Error!", float duration = 1.75f) {
		textLabelUI.text = title;
		popupObject.SetActive(true);
		StartCoroutine(Begone(duration));
	}

	/// <summary>
	/// Method pops up the Popup for the specified duration with specified textLabelUI and waits for it to finish.
	/// </summary>
	/// <param name="title">String textLabelUI to show</param>
	/// <param name="duration">Float time duration in seconds</param>
	/// <returns></returns>
	public async Task PopUpAsync(string title = "Error!", float duration = 1.75f) {
		textLabelUI.text = title;
		popupObject.SetActive(true);
		await Task.Delay(Convert.ToInt16(duration) * 1000);
		popupObject.SetActive(false);
	}

	/// <summary>
	/// Method starts new coroutine for the duration. Used for non-async version of the Popup.
	/// </summary>
	/// <param name="duration">Float time duration in seconds</param>
	/// <returns></returns>
	private IEnumerator Begone(float duration) {
		yield return new WaitForSeconds(duration);
		popupObject.SetActive(false);
	}

	/// <summary>
	/// Method pops up the Popup for the specified duration with textLabelUI.
	/// </summary>
	/// <param name="title">String textLabelUI to show</param>
	/// <param name="duration">Float time duration in seconds</param>
	public void PopUpSticky(string title = "Error!") {
		textLabelUI.text = title;
		popupObject.SetActive(true);
		Button button = popupObject.GetComponentInChildren<Button>();
		if (button != null) button.onClick.AddListener(() => { popupObject.SetActive(false); button.onClick.RemoveAllListeners(); });
	}

	/// <summary>
	/// Closes sticky popup.
	/// </summary>
	public void CloseSticky() {
		popupObject.SetActive(false);
	}
}
