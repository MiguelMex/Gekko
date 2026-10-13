using Godot;
using System;

public partial class FileToLoad : Button // Cambiamos Control por Button
{
    private SaveManager.GameData _gameData;

    public override void _Ready()
    {
        // Como la raíz ahora es un botón, nos conectamos a su propio evento Pressed
        Pressed += OnSlotPressed;
    }

    // Método para recibir los datos desde LoadFile.cs
    public void ConfigurarSlot(SaveManager.GameData data)
    {
        _gameData = data;
        
        var label = GetNodeOrNull<Label>("Label");
        if (label != null && _gameData != null)
        {
            // Puedes mostrar el nivel o la información que gustes en el texto del label
            label.Text = $"Nivel {_gameData.CurrentLevel} - Guardado";
        }
    }

    private void OnSlotPressed()
    {
        if (_gameData != null)
        {
            // 1. Guardamos temporalmente los datos para que el Jugador los lea al iniciar la escena
            SaveManager.CurrentGameData = _gameData;
            
            GD.Print($"Cargando partida guardada... Posición X: {_gameData.PlayerPosX}, Y: {_gameData.PlayerPosY}");
        }

        // 2. Cargamos la escena de juego usando la pantalla de carga
        LoadingScreen.targetScenePath = "res://testing_player.tscn";
        GetTree().ChangeSceneToFile("res://escenas/Uis/loading_screen/loading_screen.tscn");
    }

    public override void _Process(double delta)
    {
    }
}