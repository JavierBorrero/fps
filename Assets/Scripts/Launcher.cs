using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
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
        // Conectarse a Photon usando el archivo de configuracion (PhotonServerSettings)
        PhotonNetwork.ConnectUsingSettings();
    }

    // Esta funcion se ejecuta cuando el jugador se conecta al master
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true; // Cambiar a todos los jugadores de escena cuando el host cambia de escena
    }

    public override void OnJoinedLobby()
    {
        MenuManager.instance.OpenMenu("title");
    }

    public void CreateRoom()
    {
        // Si el campo del nombre de la sala esta vacio no hacemos nada
        if (string.IsNullOrEmpty(roomNameInputField.text))
            return;

        // Creamos la sala con el nombre de la sala indicado y cargamos el menu de loading mientras esperamos
        // CreateRoom crea y nos une a la sala automaticamente
        PhotonNetwork.CreateRoom(roomNameInputField.text);
        MenuManager.instance.OpenMenu("loading");
    }

    // Callback al unirse a una sala
    public override void OnJoinedRoom()
    {
        // Mostramos el menu de sala y ponemos el nombre indicado en el input de la pantalla anterior
        MenuManager.instance.OpenMenu("room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        // Array con la lista de players
        Player[] players = PhotonNetwork.PlayerList;

        // Reiniciamos las CustomProperties del jugador
        // Importante para cuando se empieza otra partida, no tener las estadisticas de la anterior
        Hashtable props = new Hashtable();
        props["kills"] = 0;
        props["deaths"] = 0;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // Destruir la lista de players para evitar jugadores duplicados
        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        // Instanciamos a los jugadores en la lista
        for (int i = 0; i < players.Count(); i++)
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
        // En caso de que ahora seamos el nuevo host, activamos el boton de iniciar partida
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    // En caso de que la creacion de la sala falle por algun motivo esta funcion se activa
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        // Ponemos el texto del error y cargamos la pantalla de error
        errorText.text = "Room Creation Failed: " + message;
        MenuManager.instance.OpenMenu("error");
    }

    // Funcion para el boton Start Game
    public void StartGame()
    {
        // LoadLevel(1)  - 1 es el índice de nuestra escena del juego en los build settings
        PhotonNetwork.LoadLevel(1);
    }

    // Funcion para el boton Leave Room
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.instance.OpenMenu("loading");
    }

    // Funcion para el boton Join Room
    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.instance.OpenMenu("loading");
    }

    // Al salir de la sala se llama esta funcion
    public override void OnLeftRoom()
    {
        MenuManager.instance.OpenMenu("title");
    }

    // Cada vez que se actualiza la lista de rooms
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Recorremos la lista de rooms
        foreach (Transform transform in roomListContent)
        {  
            // Eliminamos los elementos
            Destroy(transform.gameObject);
        }

        // Volvemos a recorrer la lista de rooms
        for (int i = 0; i < roomList.Count; i++)
        {
            // Photon no borra las salas de la lista de Rooms, simplemente pone el bool `RemovedFromList`
            // Tenemos que comprobar si la sala tiene esa opcion marcada y si lo esta, saltamos al siguiente item
            if (roomList[i].RemovedFromList)
                continue; // Saltamos al siguiente item de la lista
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().Setup(roomList[i]);
        }
    }

    // Funcion que se ejecuta cuando un jugador entra en la sala
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(newPlayer);
    }
}
