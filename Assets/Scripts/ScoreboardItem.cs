using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Realtime;

public class ScoreboardItem : MonoBehaviour
{
    [Header("Scoreboard Texts")]
    public TMP_Text usernameText;
    public TMP_Text killsText;
    public TMP_Text deathsText;

    public void Initialize(Player player)
    {
        usernameText.text = player.NickName;
    }
}
