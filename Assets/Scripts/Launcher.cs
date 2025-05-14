using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;

    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] GameObject playerListItemPrefab;
    [SerializeField] GameObject startGameButton;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Debug.Log("Connecting to Master");
        // Conectarse a Photon usando el archivo de configuracion (PhotonServerSettings)
        PhotonNetwork.ConnectUsingSettings();
    }

    // Called when the client is connected to the Master Server
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true; // Cambiar a todos los jugadores de escena cuando el host cambia de escena
    }

    public override void OnJoinedLobby()
    {
        MenuManager.instance.OpenMenu("title");
        Debug.Log("Joined lobby");
        PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
    }

    public void CreateRoom()
    {
        if(string.IsNullOrEmpty(roomNameInputField.text))
            return;

        PhotonNetwork.CreateRoom(roomNameInputField.text);
        MenuManager.instance.OpenMenu("loading");
    }

    public override void OnJoinedRoom()
    {
        MenuManager.instance.OpenMenu("room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        Player[] players = PhotonNetwork.PlayerList;

        // Destruir la lista de players para evitar jugadores duplicados
        foreach(Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        for(int i = 0; i < players.Count(); i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(players[i]);
        }

        // El boton de iniciar partida estara activo si somos el host de la sala
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    // Hay situaciones en las que el host podria cambiar. Por suerte Photon integra migracion de hosts.
    // Si el host de la partida saliese de la sala, otro jugador que este dentro pasaria a ser el host.
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // En caso de que ahora seamos el nuevo host, activamos el boton de iniciar partidaz
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        MenuManager.instance.OpenMenu("error");
    }

    public void StartGame()
    {
        // LoadLevel(1)  - 1 es el índice de nuestra escena del juego en los build settings
        PhotonNetwork.LoadLevel(1);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.instance.OpenMenu("loading");
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.instance.OpenMenu("loading");
    }

    public override void OnLeftRoom()
    {
        MenuManager.instance.OpenMenu("title");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform transform in roomListContent)
        {
            Destroy(transform.gameObject);
        }

        for(int i = 0; i < roomList.Count; i++)
        {   
            // Photon no borra las salas de la lista de Rooms, simplemente pone el bool `RemovedFromList`
            // Tenemos que comprobar si la sala tiene esa opcion marcada, y si esta saltamos al siguiente item
            if(roomList[i].RemovedFromList)
                continue; // Saltamos al siguiente item de la lista
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().Setup(roomList[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(newPlayer);
    }
}
