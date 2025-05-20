using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinnerManager : MonoBehaviour
{
    public static WinnerManager instance;

    [Header("Winner Canvas")]
    public GameObject winnerCanvas;
    public TMP_Text winnerText;
    public Button backToMenuButton;

    [Header("Winner Cam")]
    public GameObject cam;

    void Awake()
    {
        instance = this;
    }

    public void DisplayWinner(string playerNickname)
    {
        cam.SetActive(true);

        winnerCanvas.SetActive(true);
        winnerText.text = "Winner: " + playerNickname;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PhotonNetwork.AutomaticallySyncScene = false;
    }

    public void LoadMainMenu()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
        winnerCanvas.SetActive(false);

        // Destruir RoomManager instance al cargar para evitar problemas de objetos duplicados con Photon
        if (RoomManager.instance != null)
        {
            Destroy(RoomManager.instance.gameObject);
            RoomManager.instance = null;
        }

        PhotonNetwork.LoadLevel(0);
    }
}
