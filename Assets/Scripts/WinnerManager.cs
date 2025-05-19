using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinnerManager : MonoBehaviour
{
    public static WinnerManager instance;

    [Header("Winner Canvas")]
    public GameObject winnerCanvas;
    public TMP_Text winnerText;
    public Button backToMenuButton;

    void Awake()
    {
        instance = this;
    }

    // No se si manejar esta logica desde el PlayerManager es lo mas logico, pero como 
    // es el script que tiene el contador de las kills me parecia lo mas logico
    void Update()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("kills", out object kills))
        {
            Debug.Log(kills.ToString());
            if (kills.ToString() == "2")
            {
                WinnerManager.instance.DisplayWinner(PhotonNetwork.LocalPlayer.NickName);
            }
        }
    }

    public void DisplayWinner(string playerNickname)
    {
        winnerCanvas.SetActive(true);
        winnerText.text = "Winner: " + playerNickname;
    }
}
