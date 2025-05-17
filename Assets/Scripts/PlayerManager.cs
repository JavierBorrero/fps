using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    PhotonView pv;
    GameObject playerController;

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

    void CreateController()
    {
        Debug.Log("Instantiated Player Controller");
        // Instanciar el PlayerController
        // 0 es un parametro que significa `group` (no se exactamente para que sirve)
        // el nuevo objeto sirve para mandar el ViewID por el metodo `Instantiate` y luego el PlayerController puede leer este valor y encontrar el PlayerManager
        playerController = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), Vector3.zero, Quaternion.identity, 0, new object[] { pv.ViewID });
    }

    public void Die()
    {
        // Destruimos el PlayerController y luego lo creamos otra vez (Muerte y Respawn)
        PhotonNetwork.Destroy(playerController);
        CreateController();
    }
}
