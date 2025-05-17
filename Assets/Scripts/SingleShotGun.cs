using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleShotGun : Gun
{
    [SerializeField] Camera cam;

    public override void Use()
    {
        Shoot();
    }

    void Shoot()
    {
        // Crear el disparo que sale desde el centro de nuestra pantalla
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        // Ponemos el principio del rayo en la posicion de la camara
        ray.origin = cam.transform.position;

        // Disparo del rayo
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Comprobamos si el objeto que hemos golpeado tiene la interfaz, y si la tiene usamos el metodo TakeDamage()
            // Tenemos que castear a `GunInfo` porque la clase `ItemInfo` no tiene la variable `damage`
            hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
        }
    }
}
