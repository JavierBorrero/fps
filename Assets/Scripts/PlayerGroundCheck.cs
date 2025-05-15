using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    PlayerController playerController;

    void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    void OnTriggerEnter(Collider other)
    {
        // Si lo que esta activando el trigger es nuestro player hacemos return
        if (other.gameObject == playerController.gameObject)
            return;
        playerController.SetGroundedState(true);
    }

    void OnTriggerExit(Collider other)
    {
        // Si lo que esta activando el trigger es nuestro player hacemos return
        if (other.gameObject == playerController.gameObject)
            return;
        playerController.SetGroundedState(false);
    }

    void OnTriggerStay(Collider other)
    {
        // Si lo que esta activando el trigger es nuestro player hacemos return
        if (other.gameObject == playerController.gameObject)
            return;
        playerController.SetGroundedState(true);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Si lo que esta activando el trigger es nuestro player hacemos return
        if (collision.gameObject == playerController.gameObject)
            return;
        playerController.SetGroundedState(true);
    }

    void OnCollisionExit(Collision collision)
    {
        // Si lo que esta activando el trigger es nuestro player hacemos return
        if (collision.gameObject == playerController.gameObject)
            return;
        playerController.SetGroundedState(false);
    }

    void OnCollisionStay(Collision collision)
    {
        // Si lo que esta activando el trigger es nuestro player hacemos return
        if (collision.gameObject == playerController.gameObject)
            return;
        playerController.SetGroundedState(true);
    }
}
