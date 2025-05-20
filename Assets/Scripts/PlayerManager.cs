using System.IO;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerManager : MonoBehaviour
{
    PhotonView pv;
    GameObject playerController;

    int kills;
    int deaths;

    bool winnerAnnounced = false;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    void Start()
    {
        // pv.IsMine es true si el PhotonView pertenece al local player
        // Cuando se crean los distintos prefabs de PlayerManager (Segun las personas en la sala), cada uno tiene un dueño distinto
        if (pv.IsMine)
        {
            CreateController();
        }
    }

    void Update()
    {
        CheckWinner();   
    }

    void CreateController()
    {
        // Obtenemos el punto de spawn de la clase `SpawnManager` con el metodo `GetSpawnPoint`
        // Instanciar el PlayerController
        // 0 es un parametro que significa `group` (no se exactamente para que sirve, pero Photon lo maneja automaticamente)
        // `new object[]` sirve para mandar el ViewID por el metodo `Instantiate` y luego el PlayerController puede leer este valor y encontrar su PlayerManager
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        playerController = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnPoint.position, spawnPoint.rotation, 0, new object[] { pv.ViewID });
    }

    public void Die()
    {
        // Destruimos el PlayerController y luego lo creamos otra vez (Muerte y Respawn)
        PhotonNetwork.Destroy(playerController);
        CreateController();

        deaths++;

        Hashtable hash = new Hashtable();
        hash.Add("deaths", deaths);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public void GetKill()
    {
        pv.RPC(nameof(RPC_GetKill), pv.Owner);
    }

    [PunRPC]
    void RPC_GetKill()
    {
        kills++;

        Hashtable hash = new Hashtable();
        hash.Add("kills", kills);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    void CheckWinner()
    {
        if (winnerAnnounced) return;

        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("kills", out object kills))
        {
            if (kills.ToString() == "2")
            {
                winnerAnnounced = true;
                pv.RPC(nameof(SetWinner), RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
            }
        }
    }

    [PunRPC]
    void SetWinner(string playerName)
    {
        WinnerManager.instance.DisplayWinner(playerName);
    }

    public static PlayerManager Find(Player player)
    {
        // Esta no es la forma mas adecuada de encontrar un PlayerManager, porque tiene que recorrer todos
        // Los PlayerManagers de la escena, pero por ahora se queda asi
        return FindObjectsOfType<PlayerManager>().SingleOrDefault(x => x.pv.Owner == player);
    }
}
