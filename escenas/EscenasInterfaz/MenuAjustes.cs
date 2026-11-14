using Godot;
using System.Collections.Generic;

public partial class MenuAjustes : CanvasLayer
{
	private const string RutaConfig = "user://ajustes.cfg";
	private static readonly string[] Acciones = { "left", "right", "jump", "pause" };

	private readonly Dictionary<string, Button> _botones = new();
	private BaseButton _pantallaCompleta;
	private string _accionEsperando = null;
	private Control _focoPrevio;
	[Signal] public delegate void CerradoEventHandler();

	public override void _Ready()
	{
		Visible = false;
		ProcessMode = ProcessModeEnum.Always;

		_pantallaCompleta = GetNode<BaseButton>("VBoxContainer/PantallaCompletaCheck");
		_pantallaCompleta.Toggled += OnPantallaCompleta;

		foreach (string accion in Acciones)
		{
			var boton = GetNode<Button>($"VBoxContainer/GridContainer/Boton_{accion}");
			_botones[accion] = boton;
			boton.Pressed += () => EmpezarRemapeo(accion);
		}

		GetNode<Button>("VBoxContainer/RestablecerButton").Pressed += Restablecer;
		GetNode<Button>("VBoxContainer/VolverButton").Pressed += Cerrar;

		Cargar();            // aplica lo guardado al arrancar el juego
		ActualizarTextos();
	}

	// ---------- Abrir / cerrar ----------

	public void Abrir()
	{
		_focoPrevio = GetViewport().GuiGetFocusOwner();  // para devolver el foco al cerrar
		ActualizarTextos();
		Visible = true;
		_pantallaCompleta.GrabFocus();
	}

	public void Cerrar()
{
	CancelarRemapeo();
	Guardar();
	Visible = false;
	EmitSignal(SignalName.Cerrado);

	// Solo devuelve el foco si ese control ya es visible otra vez
	if (IsInstanceValid(_focoPrevio) && _focoPrevio.IsVisibleInTree())
		_focoPrevio.GrabFocus();
}

	// ---------- Pantalla ----------

	private void OnPantallaCompleta(bool activada)
	{
		DisplayServer.WindowSetMode(activada
			? DisplayServer.WindowMode.Fullscreen
			: DisplayServer.WindowMode.Windowed);
	}

	// ---------- Remapeo de teclas ----------

	private void EmpezarRemapeo(string accion)
	{
		CancelarRemapeo();
		_accionEsperando = accion;
		_botones[accion].Text = "Presiona una tecla...";
	}

	private void CancelarRemapeo()
	{
		_accionEsperando = null;
		ActualizarTextos();
	}

	// Borra solo las teclas de la acción; conserva los botones y sticks del gamepad.
	private static void BorrarTeclas(string accion)
	{
		foreach (InputEvent e in InputMap.ActionGetEvents(accion))
		{
			if (e is InputEventKey)
				InputMap.ActionEraseEvent(accion, e);
		}
	}

	private void AsignarTecla(string accion, Key codigo)
	{
		BorrarTeclas(accion);
		InputMap.ActionAddEvent(accion, new InputEventKey { PhysicalKeycode = codigo });
		CancelarRemapeo();
	}

	public override void _Input(InputEvent @event)
	{
		if (!Visible) return;

		// Esperando una tecla nueva
		if (_accionEsperando != null)
		{
			if (@event is InputEventKey tecla && tecla.Pressed && !tecla.Echo)
			{
				if (tecla.PhysicalKeycode == Key.Escape)
					CancelarRemapeo();
				else
					AsignarTecla(_accionEsperando, tecla.PhysicalKeycode);

				GetViewport().SetInputAsHandled();
			}
			return;
		}

		// Escape cierra los ajustes (y evita que el menú de pausa también reaccione)
		if (@event.IsActionPressed("ui_cancel"))
		{
			Cerrar();
			GetViewport().SetInputAsHandled();
		}
	}

	private void Restablecer()
	{
		InputMap.LoadFromProjectSettings();   // vuelve a las teclas definidas en el Mapa de Entrada
		ActualizarTextos();
	}

	private void ActualizarTextos()
	{
		foreach (string accion in Acciones)
			_botones[accion].Text = TextoTecla(accion);
	}

	private string TextoTecla(string accion)
	{
		foreach (InputEvent e in InputMap.ActionGetEvents(accion))
		{
			if (e is InputEventKey k)
				return OS.GetKeycodeString(k.PhysicalKeycode != Key.None ? k.PhysicalKeycode : k.Keycode);
		}
		return "—";
	}

	// ---------- Guardar / cargar ----------

	private void Guardar()
	{
		var cfg = new ConfigFile();
		cfg.SetValue("pantalla", "completa", _pantallaCompleta.ButtonPressed);

		foreach (string accion in Acciones)
		{
			foreach (InputEvent e in InputMap.ActionGetEvents(accion))
			{
				if (e is InputEventKey k)
				{
					Key codigo = k.PhysicalKeycode != Key.None ? k.PhysicalKeycode : k.Keycode;
					cfg.SetValue("controles", accion, (long)codigo);
					break;
				}
			}
		}
		cfg.Save(RutaConfig);
	}

	private void Cargar()
	{
		var cfg = new ConfigFile();
		if (cfg.Load(RutaConfig) != Error.Ok) return;   // primera vez: no hay archivo

		_pantallaCompleta.ButtonPressed = (bool)cfg.GetValue("pantalla", "completa", false);

		foreach (string accion in Acciones)
		{
			if (!cfg.HasSectionKey("controles", accion)) continue;
			long codigo = (long)cfg.GetValue("controles", accion);
			BorrarTeclas(accion);
			InputMap.ActionAddEvent(accion, new InputEventKey { PhysicalKeycode = (Key)codigo });
		}
	}
}
