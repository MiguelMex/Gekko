# Niveles del Gekko

Cinco mapas de plataformas 2D para Tiled y Godot con C#. Son ortogonales, finitos y usan tiles de **16 × 16 px**. Requieren una prueba con el personaje real para ajustar la dificultad.

## Archivos necesarios

Conservar `mapas/` con los cinco `.tmx` y `assets/` con los tilesets `.tsx` e imágenes `.png`, manteniendo sus rutas relativas. Abrir los TMX en Tiled; `niveles.tiled-project` es opcional para organizarlos.

Godot necesita importar o convertir capas, colisiones, objetos y propiedades. Copiar los archivos no implementa sus comportamientos. `tools/` y `vistas/` no son dependencias del juego. Los generadores Python pueden sobrescribir ediciones manuales: no ejecutarlos para abrir o jugar los mapas.

## Los cinco mapas

| Archivo en `mapas/` | Tiles | Píxeles | Dificultad y recorrido | Peligros y mecanismos |
|---|---|---|---|---|
| `01_dunas_del_amanecer.tmx` | 100 × 24 | 1600 × 384 | Fácil; horizontal, con recuperación inicial. | Huecos, 1 grupo de pinchos y 2 checkpoints. |
| `02_ruinas_del_viento.tmx` | 72 × 48 | 1152 × 768 | Media; ascenso en zigzag. | Huecos, 3 grupos de pinchos y 3 checkpoints. |
| `03_bajo_la_piramide.tmx` | 56 × 72 | 896 × 1152 | Alta; descenso izquierdo y ascenso derecho. | 9 grupos de pinchos, fondo peligroso y 6 checkpoints. |
| `04_caravana_de_los_pinchos.tmx` | 128 × 30 | 2048 × 480 | Media; travesía horizontal. | Huecos, 4 grupos de pinchos, 2 plataformas horizontales y 3 checkpoints. |
| `05_maquinaria_de_la_piramide.tmx` | 88 × 54 | 1408 × 864 | Alta; ascenso por varias alturas. | Huecos, 5 grupos de pinchos, 1 plataforma horizontal, 2 ascensores y 7 checkpoints. |

Las salidas definen **dos secuencias: 01 → 02 → 03 y 04 → 05**. El 03 termina con `completes_game=true`; el 05, con `completes_sequence=true`. Para unir los cinco, cambiar la salida del 03 para enlazar al 04 y desactivar su finalización.

Bandera verde: inicio. Bandera turquesa: checkpoint. Puerta iluminada: final. Cactus, antorchas y escombros son decoración sin daño.

## Física de referencia

| Parámetro | Valor |
|---|---|
| Velocidad horizontal máxima | 160 px/s |
| Impulso vertical de salto | −320 px/s |
| Gravedad constante | 1066.6667 px/s² |
| Altura máxima desde los pies | 48 px / 3 tiles |
| Vuelo hasta regresar a la misma altura | 0.6 s |
| Alcance a igual altura y velocidad máxima | 96 px / 6 tiles |
| Caja usada para comprobar los recorridos | 12 × 14 px, provisional |

No requieren doble salto, dash, planeo ni salto en pared. Los 96 px son desplazamiento del personaje, no ancho seguro de un hueco. Una plataforma elevada reduce el alcance. La aceleración, el salto variable y una caja distinta requieren revisar los saltos con el personaje real.

## Capas

| Capa | Función |
|---|---|
| `Fondo` | Paisaje sin colisión. |
| `Terreno` | Suelo y plataformas estáticas sólidas; usar las colisiones del TSX. |
| `Decoracion` | Elementos visuales, incluidos los pinchos; sin colisión de terreno. |
| `PrimerPlano` | Capa visual disponible en 01–03. |
| `Gameplay` | Inicio, salida, checkpoints y zonas de muerte. |
| `Pinchos_de_recorrido` / `Trampas` | Objetos de daño: la primera en 01–03, la segunda en 04–05. |
| `Plataformas_moviles` | Objetos para crear mecanismos reales en 04–05. |
| `Vista_previa_moviles` | Dibujo inicial de móviles para Tiled; excluir del juego. |
| `Rutas_moviles` | Guías de movimiento; sin dibujo ni colisión en el juego. |
| `Camara` | Leer sus límites aunque esté oculta en Tiled. |
| `Ruta_de_diseno` | Guía opcional del recorrido; excluir del juego. |

Los bloques sólidos tienen colisión completa, no son atravesables desde abajo. El dibujo de los pinchos y sus áreas de daño están separados. Los móviles no se animan en Tiled.

## Objetos y coordenadas

Las posiciones están en píxeles desde la esquina superior izquierda del mapa, con Y positiva hacia abajo. El atributo `type` identifica el comportamiento. Los rectángulos se posicionan por su esquina superior izquierda; los puntos de aparición, por el centro de los pies. Si el mapa se instancia desplazado, transformar sus posiciones al espacio de la escena.

| Tipo | Implementación necesaria |
|---|---|
| `Spawn` | Colocar los pies del personaje en el punto y aplicar `facing`; ajustar según el origen de su escena. |
| `Checkpoint` | Guardar `respawn_x`, `respawn_y` y `facing` al entrar. Usar Spawn hasta activar uno. |
| `SpikeTrap` | Área de daño del tamaño del objeto; `always_active=true`, `damage_mode=respawn`. |
| `KillZone` | Reaparición al caer o salir lateralmente. |
| `Exit` | Cargar `next_map`, relativo a `mapas/`, o completar la secuencia según sus propiedades. |
| `CameraBounds` | Límites de cámara; no crea un muro. |
| `MovingPlatform` | Plataforma sólida móvil según sus propiedades. |
| `Guide`, `PlatformGuide`, `MotionPath` | Datos de autoría; no son objetos jugables. |

Usar `fallback_death_y` como protección adicional contra caídas. Al reaparecer, limpiar la velocidad del personaje. Ajustar el encuadre si la vista de la cámara es mayor que el mapa.

## Plataformas móviles

Los objetos `MovingPlatform` miden 64 × 16 px. Su posición inicial y `target_x`/`target_y` indican la esquina superior izquierda en coordenadas del mapa.

- `mode=ping_pong`: ir y volver entre los extremos.
- `speed_px_s`: velocidad propia del mecanismo, entre 32 y 40 px/s.
- `wait_seconds=1`: pausa en ambos extremos, incluida la posición inicial al comenzar.
- `carry_player=true`: transportar al personaje apoyado encima.
- `solid=true`: colisión completa.
- `reset_on_respawn=true`: volver al inicio y reiniciar la pausa al reaparecer el personaje; no al activar un checkpoint.
- `path_name`: guía de Tiled asociada al mecanismo.
- `surface_tile_gid=25`: tile turquesa repetido para dibujarlo; resolverlo contra el tileset correspondiente.

Los checkpoints de 04–05 tienen `reset_mechanisms=true` para la lógica de reaparición. Excluir `Vista_previa_moviles` evita dejar una plataforma fija duplicada en el inicio del trayecto.

## Prueba de integración

1. Cargar el mapa 01 y comprobar escala, colisiones, aparición y física del personaje.
2. Comprobar daño, caídas, checkpoints y reaparición sin velocidad residual.
3. Conectar la salida al siguiente mapa y aplicar los límites de cámara.
4. Probar giros y saltos elevados en 02–03.
5. En 04–05, probar abordaje, transporte, salida y reinicio de cada móvil; esperar otro ciclo siempre debe permitir continuar.

Las comprobaciones geométricas existentes no sustituyen estas pruebas dentro de Godot.
