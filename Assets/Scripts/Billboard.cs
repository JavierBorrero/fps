using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    Camera cam;

    // Para que los nombres encima de los jugadores se giren segun a donde esta mirando el jugador
    void Update()
    {
        if (cam == null)
        {
            cam = FindObjectOfType<Camera>();
        }

        transform.LookAt(cam.transform);
        transform.Rotate(Vector3.up * 180);
    }
}
