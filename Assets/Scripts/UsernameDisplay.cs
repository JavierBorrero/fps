using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class UsernameDisplay : MonoBehaviour
{
    [Header("Photon View")]
    public PhotonView pv;

    [Header("Username Text")]
    public TMP_Text text;

    void Start()
    {
        // Cuando miremos hacia arriba no queremos ver el display de nuestro nombre de usuario
        if (pv.IsMine) gameObject.SetActive(false);

        text.text = pv.Owner.NickName;
    }
}
