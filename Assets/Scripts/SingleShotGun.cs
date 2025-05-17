using Photon.Pun;
using UnityEngine;

public class SingleShotGun : Gun
{
    [SerializeField] Camera cam;

    PhotonView pv;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

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
            pv.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
        }
    }

    // ** Nota: Esta implementacion para las marcas de los disparos no es optima/correcta. Lo correcto seria hacer
    // una pool con los impactos ya creados, activarlos y desactivarlos segun los disparos que se hagan.
    //
    // Dado el tiempo que queda hasta la entrega y mi conocimiento en pools, en conjunto con tener que sincronizar esto con
    // el resto de jugadores, voy a dejar esta implementacion.
    [PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            // `hitPosition + hitNormal * 0.001f` --> Esto es para evitar que los efectos de impactos se superpongan con el GameObject
            // en el que colisionan, y Unity pueda renderizarlos bien.
            //
            // `Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation` --> Esto es para hacer que
            // el impacto de la bala siempre este mirando recto a la superficie a la que impacta. No queremos disparar al suelo y que la 
            // marca del disparo este mirando hacia un lateral.
            GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
            Destroy(bulletImpactObj, 2f);
            // Con esta linea añadimos el impacto de la bala como hijo de lo que sea que haya golpeado. Esto
            // es util cuando le disparamos a los enemigos y que los efectos de bala no se queden flotando 
            // cuando los enemigos se mueven
            bulletImpactObj.transform.SetParent(colliders[0].transform);
        }
        
    }
}
