using Godot;
using System;

public partial class MenuPausa : CanvasLayer
{
	private Button _primerBoton;
	public override void _Ready()
	{
		Visible = false;
		ProcessMode = ProcessModeEnum.Always;

		GetNode<Button>("VBoxContainer/ReanudarButton").Pressed += OnResume;
		
		GetNode<Button>("VBoxContainer/ReiniciarNivelButton").Pressed += OnReiniciarNivel;
		
		GetNode<Button>("VBoxContainer/OpcionesButton").Pressed += OnOpciones;
		
		// Opción 1: Salir al menú principal
		GetNode<Button>("VBoxContainer/SalirMenuButton").Pressed += OnSalirAlMenu;
		
		// Opción 2: Salir por completo del juego (escritorio)
		GetNode<Button>("VBoxContainer/SalirEscButton").Pressed += OnSalirAlEscritorio;
		
		_primerBoton = GetNode<Button>("VBoxContainer/ReanudarButton");
		
		foreach (Node nodo in GetNode("VBoxContainer").GetChildren())
{
	if (nodo is Button boton)
		boton.MouseEntered += () => boton.GrabFocus();
}
		
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("pause"))
		{
			TogglePause();
		}
	}

	private void TogglePause()
{
	// No permitir pausar mientras los ajustes están abiertos
	if (GetNode<MenuAjustes>("/root/MenuAjustes").Visible)
		return;

	if (GetTree().CurrentScene.SceneFilePath == "res://escenas/Uis/pantalla_principal/pantalla_principal.tscn")
		return;

	if (GetTree().CurrentScene.SceneFilePath == "res://escenas/Uis/loading_screen/loading_screen.tscn")
		return;

	if (GetTree().CurrentScene.SceneFilePath == "res://escenas/Uis/cargar_partida/load_file.tscn")
		return;

	bool nowPaused = !GetTree().Paused;
	GetTree().Paused = nowPaused;
	Visible = nowPaused;

	if (nowPaused)
		_primerBoton.GrabFocus();
}

	private void OnResume()
	{
		GetTree().Paused = false;
		Visible = false;
	}
	
	private void OnReiniciarNivel()
{
	GetTree().Paused = false;   // quitar la pausa antes de recargar
	Visible = false;            // cerrar el menú
	GetTree().ReloadCurrentScene();
}

private async void OnOpciones()
{
	var ajustes = GetNode<MenuAjustes>("/root/MenuAjustes");

	ajustes.Abrir();      // primero: así guarda qué botón tenía el foco
	Visible = false;      // después: oculta el menú de pausa

	await ToSignal(ajustes, MenuAjustes.SignalName.Cerrado);

	if (GetTree().Paused)  // por si el juego se despausó mientras tanto
	{
		Visible = true;
		GetNode<Button>("VBoxContainer/OpcionesButton").GrabFocus();
	}
}

	// Método para regresar al menú principal
	private void OnSalirAlMenu()
	{
		// ¡Muy importante! Quitar la pausa antes de cambiar de escena
		GetTree().Paused = false; 
		
		Visible = false;
		
		// Cambia por la ruta exacta de tu pantalla principal
		GetTree().ChangeSceneToFile("res://escenas/Uis/pantalla_principal/pantalla_principal.tscn");
	}

	// Método para cerrar el juego e ir al escritorio
	private void OnSalirAlEscritorio()
	{
		GetTree().Quit();
	}
}
