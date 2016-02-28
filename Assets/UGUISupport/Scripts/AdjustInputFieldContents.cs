using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AdjustInputFieldContents : MonoBehaviour {
  public InputField inputField;

  public void AddTextToInputField(string text) {
    inputField.text += text;
  }

  public void AddUITextToInputField(Text text) {
    AddTextToInputField(text.text);
  }

  public void RemoveLastCharacterFromInputField() {
    inputField.text = inputField.text.Remove(inputField.text.Length - 1);
  }

  public void ClearTextFromInputField() {
    inputField.text = "";
  }
}
