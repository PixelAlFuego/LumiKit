// PreToolUse de UnityMCP: frena por acción lo que settings.json sólo puede frenar por herramienta.
// Reglas en CLAUDE.md > Unity MCP. Salida 2 = bloquear; stderr le llega a Claude.

const EDITOR_PERMITIDO = ['play', 'pause', 'stop', 'telemetry_status', 'telemetry_ping'];

let raw = '';
process.stdin.on('data', chunk => { raw += chunk; });
process.stdin.on('end', () => {
  let input;
  try { input = JSON.parse(raw); } catch { bloquear('entrada del hook ilegible'); }

  const tool = String(input.tool_name || '');
  const args = input.tool_input || {};
  const action = String(args.action ?? '').toLowerCase();

  if (tool.endsWith('__execute_menu_item')) {
    const menu = String(args.menu_path ?? '');
    if (!menu.startsWith('LumiKit/')) bloquear(`el menú "${menu}" no empieza por "LumiKit/"`);
  } else if (tool.endsWith('__manage_scene')) {
    // Guardar, crear y borrar escenas lo hace el usuario (Ctrl+S).
    if (/^(save|create)|delete/.test(action)) bloquear(`manage_scene no puede "${action}": las escenas las guarda, crea y borra el usuario`);
  } else if (tool.endsWith('__manage_editor')) {
    if (!EDITOR_PERMITIDO.includes(action)) bloquear(`manage_editor sólo permite ${EDITOR_PERMITIDO.join(', ')}; pediste "${action}"`);
  }
  process.exit(0);
});

function bloquear(motivo) {
  process.stderr.write(`Bloqueado por el hook (CLAUDE.md > Unity MCP): ${motivo}.\n`);
  process.exit(2);
}
