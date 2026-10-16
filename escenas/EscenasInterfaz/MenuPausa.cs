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
		 if (GetTree().CurrentScene.SceneFilePath == "res://escenas/Uis/pantalla_principal/pantalla_principal.tscn")
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
