using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Game Object Visuals")]
    public GameObject graphics;

    // La parte visual de los SpawnPoints es solo para el desarrollo del nivel
    // Los desactivamos cuando empieza el juego
    void Awake()
    {
        graphics.SetActive(false);
    }
}
