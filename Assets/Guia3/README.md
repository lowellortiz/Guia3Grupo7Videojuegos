# Guía 3 · Semana 03 — Interacción móvil: mover y apilar 4 cubos

Prototipo 3D en primera persona para el reto de la sección 8 de la guía. El usuario mueve cuatro
cubos del punto A al punto B y los apila, mientras el prototipo cronometra y registra errores.

## Escenas

| Escena | Índice en Build Settings | Contenido |
|---|---|---|
| `Assets/Scenes/Guia3_Menu.unity` | 0 (arranca aquí) | Menú **GRUPO 7** con **JUGAR** y **SALIR**. |
| `Assets/Scenes/Guia3_Apilado.unity` | 1 | El reto: punto A, punto B, 4 cubos y UI táctil. |

## Cómo abrir y probar

1. Abre `Assets/Scenes/Guia3_Menu.unity` y pulsa **Play**. **JUGAR** carga el reto.
2. Dentro del reto:
   - **Editor (teclado y mouse):** `WASD` mueve, mouse mira, `E` toma, `Espacio` suelta, `R` reinicia.
   - **Móvil:** stick virtual abajo a la izquierda para moverse, arrastre en la mitad derecha de la
     pantalla para mirar, botón **TOMAR/SOLTAR** abajo a la derecha.
   - **Gamepad:** stick izquierdo mueve, stick derecho mira, gatillo derecho toma, botón sur suelta,
     Start reinicia.
3. Al apilar el cuarto cubo aparece el panel de resultados con **REINICIAR** y **MENÚ**.
4. Para probar la interacción táctil en PC: `Window ▸ General ▸ Device Simulator`, elige un teléfono
   en **vertical** (el proyecto está fijado en Portrait).

**Reiniciar recarga la escena completa** (botón REINICIAR, tecla `R` o Start del gamepad). Es la
forma más segura de dejar el escenario idéntico para el siguiente usuario: no arrastra cubos
desplazados, velocidades residuales ni contadores a medias. El número de intento sigue avanzando
porque se guarda en `PlayerPrefs`, no en la escena.

## Menús disponibles

| Menú | Qué hace |
|---|---|
| `Guia3 ▸ Construir todo (menu + escena)` | Regenera las dos escenas y las registra en Build Settings. |
| `Guia3 ▸ Construir escena de juego` | Solo la escena del reto. |
| `Guia3 ▸ Construir menu principal` | Solo el menú. |
| `Guia3 ▸ Generar icono de la app` | Regenera el icono provisional y lo asigna a Android e iOS. |
| `Guia3 ▸ Datos ▸ Abrir carpeta de resultados` | Abre la carpeta donde se escribe el CSV. |
| `Guia3 ▸ Datos ▸ Ver resumen en consola` | Vuelca la tabla de datos en la consola. |
| `Guia3 ▸ Datos ▸ Borrar CSV antes de las pruebas` | Deja la tabla vacía antes de la sesión con los 10 usuarios. |

## Esquema de interacción elegido y por qué

**Metáfora: zonas táctiles + retícula + botón explícito de tomar/soltar.**

| Pregunta de la guía (8.1) | Respuesta del diseño |
|---|---|
| ¿Qué entrada **inicia** la manipulación? | Botón táctil TOMAR, activo solo cuando la retícula resalta un cubo. |
| ¿Qué entrada la **mantiene**? | Ninguna: el cubo queda anclado frente a la cámara y el usuario camina libre. No se exige sostener el dedo, que cansa y bloquea la otra mano. |
| ¿Qué entrada **confirma** la colocación? | El mismo botón, que ha cambiado su texto a SOLTAR. |

Argumentos para el informe:

- **Comprensión sin instrucciones.** La retícula cambia de color y tamaño al apuntar un cubo, y el
  botón pasa de gris a azul con el texto TOMAR. El usuario ve qué puede hacer antes de tocar nada,
  que es la restricción de evaluación de la sección 8.4.
- **Separación de zonas.** El dedo de mover (stick, mitad izquierda) y el de mirar (arrastre, mitad
  derecha) nunca compiten por el mismo contacto, así que se puede caminar y girar la cámara a la vez.
- **Errores contables.** Cada error es un evento discreto: pulsar TOMAR sin objetivo (error de
  agarre), soltar fuera de la zona verde (error de colocación) y retirar un cubo de la base
  derribando los de arriba (derrumbe). Eso es exactamente lo que pide el formato de observación
  de la sección 11.
- **Corrección de errores.** Un cubo mal colocado se puede volver a tomar; la pila se recalcula sola.
- **Silueta de destino.** El cubo translúcido sobre el punto B indica dónde va el siguiente nivel,
  sin convertir la prueba en una instrucción paso a paso.

### Desviación deliberada respecto a la tabla de la sección 6

La guía sugiere `Player/Move ← <Touchscreen>/delta` y `Player/Look ← <Touchscreen>/primaryTouch/delta`.
Esos dos bindings leen el **mismo dedo**, así que mover y mirar se pelean por el contacto. Aquí se
implementa el equivalente correcto, que es el patrón que la propia guía describe en 5.4:

- `Move` ← `<Gamepad>/leftStick`, escrito por el **On-Screen Stick** y también por un gamepad real.
- `Look` táctil ← componente `TouchLookZone`, restringido a la mitad derecha y a dedos que no
  empezaron sobre un botón de UI.

Vale la pena mencionar esta decisión en el informe: es un hallazgo de diseño, no un atajo.

## Arquitectura

Organización de carpetas:

```
Assets/Scenes/          Guia3_Menu.unity y Guia3_Apilado.unity
Assets/Scripts/Runtime  logica de juego y menu
Assets/Scripts/Editor   generadores de escena, icono y utilidades de datos
Assets/Guia3/Input      Guia3Controls.inputactions
Assets/Guia3/Materials  materiales generados
Assets/Guia3/Icon       icono de la app
```

`Assets/Guia3/Input/Guia3Controls.inputactions` — mapas `Player` (Move, Look, LookDelta, Grab, Place,
Reset) y `UI`, con schemes Touch / Gamepad / Keyboard&Mouse.

Nota: `Look` y `LookDelta` están separadas a propósito. El stick del gamepad entrega una **tasa**
(-1..1, se multiplica por `Time.deltaTime`) mientras que mouse y dedo entregan un **desplazamiento
por frame** en píxeles. Mezclarlas en una sola acción haría el mouse hipersensible o el stick inútil.

| Script | Responsabilidad |
|---|---|
| `Guia3Input` | Única lectura del Input Actions Asset; expone valores y eventos. |
| `TouchLookZone` | Mirada táctil por zona derecha, rastreando el dedo por identidad. |
| `FirstPersonController` | Movimiento con `CharacterController`, yaw en el cuerpo y pitch en la cámara. |
| `AimReticle` | Raycast central; publica y resalta el cubo apuntado. |
| `Grabbable` | Cubo manipulable: resaltado, estados de física y posición inicial. |
| `CarryController` | Tomar, transportar y colocar. El botón móvil alterna. |
| `StackZone` | Punto B: valida la colocación, hace snap por niveles y gestiona derrumbes. |
| `SessionMetrics` | Cronómetro (arranca al primer input), contadores y export a CSV. |
| `HudController` | Estados de retícula, botón, contadores y panel final. |
| `AttemptController` | Reinicia el intento recargando la escena, y vuelve al menú. |
| `MainMenuController` | Menú principal: JUGAR carga el reto, SALIR cierra la aplicación. |

## Datos para el informe

Al apilar el cuarto cubo se agrega una fila a `guia3_datos.csv` en `Application.persistentDataPath`:

```
intento,fecha_hora,tiempo_segundos,cubos_apilados,errores_agarre,errores_colocacion,derrumbes
```

- **En el editor:** `C:\Users\<usuario>\AppData\LocalLow\UMNG\Guia3Grupo7Videojuegos\`
- **En el celular:** `/storage/emulated/0/Android/data/com.UMNG.Guia3Grupo7Videojuegos/files/`
  (sácalo con `adb pull`, o léelo desde el panel de resultados).

Antes de la sesión con los 10 usuarios, ejecuta `Guia3 ▸ Datos ▸ Borrar CSV antes de las pruebas`
para que la numeración de intentos empiece en 1.

## Configuración del proyecto aplicada

Según la guía de configuración del curso:

| Ajuste | Valor |
|---|---|
| Company Name | `UMNG` |
| Product Name | `Guia3Grupo7Videojuegos` |
| Package name / Bundle Identifier (Android e iOS) | `com.UMNG.Guia3Grupo7Videojuegos` |
| Minimum API Level | Android 7.1 Nougat (API 25) |
| Install Location | Prefer External + permiso de escritura externa |
| Target minimum iOS Version | 15.0 |
| Icon | `Assets/Guia3/Icon/Guia3_Icon.png` (provisional, regenerable) |
| Run without focus | Desactivado |
| Fullscreen Mode | Fullscreen Window |
| Aspect Ratio Mode | Native Aspect Ratio |
| Default Orientation | Portrait, sin autorrotación |
| Color Space | Linear |
| MSAA Fallback | Downgrade |
| Scripting Backend / Arquitectura | IL2CPP / ARM64 |

**El Canvas está diseñado para pantalla vertical** (referencia 1080 × 1920): HUD apilado en dos filas
arriba, stick abajo a la izquierda y botón de acción abajo a la derecha. El FOV de la cámara se subió
a 75° porque en vertical el campo horizontal se estrecha mucho. Si el equipo decidiera volver a
horizontal, hay que revisar `Guia3SceneBuilder.BuildCanvas` y `ConfigureProjectSettings`.

## Build de Android

`File ▸ Build and Run` con el celular conectado y la depuración USB activada. La plataforma activa
ya es Android y la única escena en Build Settings es `Guia3_Apilado`.

## Pendiente para completar la guía

Este prototipo cubre el reto y la medición. Falta la parte de campo:

- Probar con 10 usuarios externos siguiendo el protocolo de la sección 9 (brief neutral, sin pistas).
- Aplicar la encuesta de la sección 10 y el formato de observación de la sección 11.
- Análisis de la sección 12 y las 3 propuestas de rediseño priorizadas.
- Video corto de la interacción y de una prueba real.
