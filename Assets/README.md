videos: https://www.youtube.com/@davegamedevelopment/videos


Explicacion de:

private void SpeedControl()
{
    // Obtener velocidad horizontal (Solo nos interesa XZ)
    Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

    // La magnitud del vector es su longitud, si queremos saber si vamos demasiado rapido
    // comparamos la magnitud con la velocidad. Ej si la magnitud es 12 y mi moveSpeed es 7, tendremos que frenar
    if(flatVel.magnitude > moveSpeed) {
        // 
        Vector3 limitedVel = flatVel.normalized * moveSpeed;
        rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
    }
}

```
Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
```
Este vector representa la velocidad del objeto en el plano XZ, ignoramos el plano Y porque no queremos limitar su velocidad si esta cayendo o saltando

```
if(flatVel.magnitude > moveSpeed)
```

`flatVel.magnitude` es la magnitud del vector de velocidad horizontal. La magnitud de un vector es básicamente su longitud.

Para un vector 3d la magnitud se calcula con el teorema de pitagoras en 3 dimensiones: √x^2 + y^2 + z^2

Si nuestro vector fuese (3,0,4): √3^2 + 0^2 + 4^2 = √9 + 0 + 16 = √25 = 5

flatVel.magnitude representa la velocidad horizontal real del objeto, combinando al velocidad en x y la velocidad en z.

`flatVel.magnitude`:

Cuando tienes este vector:

`Vector 3 velocity = rb.velocity`

Este vector indica:

- Direccion del movimiento
- Magnitud del vector (velocidad escalar)

Supon que el objeto se mueve asi:

velocity = new Vector3(3, 0, 4)

- 3u/s en X
- 0u/s en Y
- 4u/s en Z

No nos interesa como se reparte el movimiento, sino que tan rapido se desplaza en total. Al hacer magnitude 
aplicamos el teorema de pitagoras en 3 dimensiones:

√x^2 + y^2 + z^2

En nuestro caso:

√3^2 + 0^2 + 4^2 = √9 + 0 + 16 = √25 = 5u/s

No decimos "Mi personaje se mueve a 3u/s en el eje X y a 4u/s en el eje Z".
Queremos decir algo como "Mi personaje se mueve a 5u/s".

Luego de compararlo con nuestra velocidad, si es mayor creamos un nuevo `Vector3 limitedVel`

```
Vector3 limitedVel = flatVel.normalized * moveSpeed;
```

Ahora normalizamos el vector porque ya no nos interesa su longitud, sino la direccion. Lo multiplicamos por nuestra
velocidad maxima y lo aplicamos a la velocidad del `Rigidody`

```
rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
```

