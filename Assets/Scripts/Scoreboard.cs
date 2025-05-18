using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class Scoreboard : MonoBehaviourPunCallbacks
{
    [Header("Container")]
    public Transform container;

    [Header("Scoreboard Item Prefab")]
    public GameObject scoreboardItemPrefab;

    [Header("Canvas Group")]
    public CanvasGroup canvasGroup;

    // Como necesitamos referencias a los scoreboardItems en base al Player que sea, creamos
    // un diccionario con clave el Player y ScoreboardItem su scoreboardItem correspondiente
    Dictionary<Player, ScoreboardItem> scoreboardItems = new Dictionary<Player, ScoreboardItem>();

    void Start()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            AddScoreboardItem(player);
        }
    }

    void AddScoreboardItem(Player player)
    {
        ScoreboardItem item = Instantiate(scoreboardItemPrefab, container).GetComponent<ScoreboardItem>();
        item.Initialize(player);
        scoreboardItems[player] = item;
    }

    void RemoveScoreboardItem(Player player)
    {
        Destroy(scoreboardItems[player].gameObject);
        scoreboardItems.Remove(player);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddScoreboardItem(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RemoveScoreboardItem(otherPlayer);
    }

    // No he podido pasar al PlayerInput la accion de sacar el marcador con el Tab
    // Se va a quedar hecho con el Input antiguo
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            canvasGroup.alpha = 1;
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            canvasGroup.alpha = 0;
        }
    }
    
}
