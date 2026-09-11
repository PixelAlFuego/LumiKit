# Protocolo de verificación
Claude no ejecuta Unity. Nada pasa a ✅ sin que el usuario lo confirme en el editor.

## Estados
| Estado | Significado | Quién lo pone |
|---|---|---|
| ⬜ | Pendiente | cualquiera |
| 🟠 | En curso | Claude |
| 🟡 | Implementado, sin abrir Unity | Claude (su tope) |
| ✅ | Verificado en el editor | sólo el usuario |
| ⛔ | Bloqueado | cualquiera, con motivo en STATE.md |

## Al terminar de programar (Claude)
1. Marcar la tarea 🟡 en BACKLOG.md. Nunca ✅.
2. Añadir las filas nuevas a CODEMAP.md, sólo de archivos que existan en disco.
3. Copiar los criterios de aceptación de la spec a STATE.md > Pendiente de verificación,
   como checklist sin marcar.
4. Listar en STATE.md > Dudas abiertas cualquier API de Unity 6 / URP 17 usada sin certeza.
5. Commit `[LK-XX] descripción`.
6. Entregar al usuario: qué archivos se crearon, qué falta comprobar, qué no se hizo.

## Lo que Claude no puede afirmar
Prohibido escribir "funciona", "probado", "verificado", "compila", "sin errores".
Formulación correcta: "implementado, pendiente de verificar en el editor".

## Al verificar (usuario)
1. Abrir Unity y dejar que reimporte.
2. Consola sin errores ni warnings nuevos.
3. Recorrer los criterios de aceptación de `docs/specs/LK-XX_*.md`.
4. Entrar en Play Mode y repetir los criterios que lo requieran.
5. Comprobar que ningún `.mat` quedó modificado en disco tras salir de Play Mode (D-001).

## Cierre
El usuario escribe `verificado LK-XX`. Sólo entonces Claude:
- pone ✅ en BACKLOG.md,
- vacía esa checklist de STATE.md,
- mueve la sesión a la tabla de últimas 3 sesiones.

Si un criterio falla: el usuario describe el fallo, la tarea vuelve a 🟠 y se corrige
antes de tocar la siguiente tarea.

## Verificación del andamiaje (Sesión 00)
- [ ] Unity abre el proyecto sin errores de consola.
- [ ] Los dos `.asmdef` aparecen en el Project sin referencias en rojo.
- [ ] `Assets/LumiKit/` conserva todas sus subcarpetas tras el reimport.
- [ ] Version Control Mode = Visible Meta Files · Asset Serialization = Force Text.
