using Godot;
using System;

public partial class MenuPausa : CanvasLayer
{
    public override void _Ready()
    {
        Visible = false;
        ProcessMode = ProcessModeEnum.Always;

        GetNode<Button>("VBoxContainer/ReanudarButton").Pressed += OnResume;
        
        // Opción 1: Salir al menú principal
        GetNode<Button>("VBoxContainer/SalirMenuButton").Pressed += OnSalirAlMenu;
        
        // Opción 2: Salir por completo del juego (escritorio)
        GetNode<Button>("VBoxContainer/SalirEscritorioButton").Pressed += OnSalirAlEscritorio;
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
        bool nowPaused = !GetTree().Paused;
        GetTree().Paused = nowPaused;
        Visible = nowPaused;
    }

    private void OnResume()
    {
        GetTree().Paused = false;
        Visible = false;
    }

    // Método para regresar al menú principal
    private void OnSalirAlMenu()
    {
        // ¡Muy importante! Quitar la pausa antes de cambiar de escena
        GetTree().Paused = false; 
        
        // Cambia por la ruta exacta de tu pantalla principal
        GetTree().ChangeSceneToFile("res://escenas/Uis/pantalla_principal/pantalla_principal.tscn");
    }

    // Método para cerrar el juego e ir al escritorio
    private void OnSalirAlEscritorio()
    {
        GetTree().Quit();
    }
}