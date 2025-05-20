using System;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPreferencesManager : MonoBehaviour
{
    [Header("Input Username")]
    public TMP_InputField usernameInput;

    [Header("Sensitivity Slider")]
    public Slider slider;
    public TMP_Text sliderValueText;

    [Header("Game Buttons")]
    public Button createRoomButton;
    public Button findRoomButton;

    void Start()
    {
        if (PlayerPrefs.HasKey("username"))
        {
            usernameInput.text = PlayerPrefs.GetString("username");
            PhotonNetwork.NickName = PlayerPrefs.GetString("username");
        }
        else
        {
            usernameInput.text = "Player " + UnityEngine.Random.Range(0, 1000).ToString("0000");
            OnUsernameInputValueChanged();
        }
    }

    public void OnUsernameInputValueChanged()
    {
        if (usernameInput.text.Trim() == "")
        {
            createRoomButton.enabled = false;
            findRoomButton.enabled = false;
        }
        else
        {
            createRoomButton.enabled = true;
            findRoomButton.enabled = true;
        }

        PhotonNetwork.NickName = usernameInput.text;
        PlayerPrefs.SetString("username", usernameInput.text);
    }

    public void OnSliderValueChanged()
    {
        PlayerPrefs.SetFloat("sensitivity", slider.value);
        sliderValueText.text = Math.Round(slider.value, 2).ToString();
    }
}
