# FPS Multiplayer

Repo para la entrega de FPS en Motores

## Arquitectura Multiplayer

Vamos a tener dos prefabs

**Player Manager:**

- Se encarga de la persistencia de datos del jugador
- Respawn y muerte
- Recibir informacion de otros jugadores

**Player Controller:**

- Movimiento del jugador
- Disparo

Esta estructura es necesaria para que pueda haber un intercambio de datos entre los jugadores y la partida. Por ejemplo si no tuviesemos esta
estructura separada en un Manager y un Controller, si yo elimino a otro jugador, el otro jugador debe saber que ha sido eliminado por mi, y yo quiero ver
que he eliminado a ese jugador. Esta comunicación no sería posible, porque si destruimos el GameObject del enemigo, no podremos enviar la información.

Estos dos prefabs van a ser generados por un `RoomManager`, un empty que tendremos en la escena. El `RoomManager` se va a encargar de crear el `PlayerManager` cuando
nos unimos a la sala, y el `PlayerManager` se va a encargar de crear el `PlayerController`. Cuando salgamos de la escena, el `PlayerManager` y el `PlayerController` seran 
destruidos automaticamente por Photon.

```
RoomManager -> PlayerManager -> PlayerController
```