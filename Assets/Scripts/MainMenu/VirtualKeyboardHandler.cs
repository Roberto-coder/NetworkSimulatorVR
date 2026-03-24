using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System;

public class VirtualKeyboardManager : MonoBehaviour
{
    public NonNativeKeyboard keyboard;

    TMP_InputField activeInputField;
    bool isTextSubmitted = false;

    void OnEnable()
    {
        keyboard.OnTextSubmitted += HandleTextSubmitted;
        keyboard.OnTextUpdated += HandleTextUpdated;
    }

    void OnDisable()
    {
        keyboard.OnTextSubmitted -= HandleTextSubmitted;
        keyboard.OnTextUpdated -= HandleTextUpdated;
    }

    // Se llama cuando se selecciona un campo
    public void ActivateInputField(TMP_InputField field)
    {
        activeInputField = field;

        keyboard.InputField.text = field.text;

        keyboard.PresentKeyboard();
    }

    void HandleTextSubmitted(object sender, EventArgs e)
    {
        isTextSubmitted = true;

        if (activeInputField != null)
        {
            string capturedText = keyboard.InputField.text;
            activeInputField.text = capturedText;

            Debug.Log("Captured Text: " + capturedText);
        }
    }

    void HandleTextUpdated(string text)
    {
        if (!isTextSubmitted && activeInputField != null)
        {
            activeInputField.text = text;
        }

        isTextSubmitted = false;
    }
}