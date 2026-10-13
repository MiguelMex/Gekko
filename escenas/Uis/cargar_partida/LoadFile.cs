using Godot;
using System;

public partial class LoadFile : Control
{
    private SaveManager.GameData gameData;
    private VBoxContainer container;

    [Export] PackedScene FileToLoadScene;

    public override void _Ready()
    {
        container = GetNode<VBoxContainer>("Background/Container");
        gameData = SaveManager.LoadGame(); //[cite: 1]

        // Si no hay datos de guardado, puedes mostrar un texto de "No hay partidas" o salir
        if (gameData == null)
        {
            GD.Print("No hay partidas guardadas para mostrar.");
            return;
        }

        if (FileToLoadScene == null) return;

        // Instanciamos el botón/tarjeta de la partida guardada
        // Dentro de tu LoadFile.cs, cuando instancias el slot:
        var slot = FileToLoadScene.Instantiate<FileToLoad>();
        container.AddChild(slot);

        // Le pasamos los datos leídos del JSON al slot individual
        slot.ConfigurarSlot(gameData);
        //slot.GetNode<Label>("Label").Text = "Partida guardada (Nivel " + gameData.CurrentLevel + ")";
        slot.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        slot.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;

        // AQUÍ CONECTAMOS EL CLIC DEL SLOT PARA QUE CARGUE LA PARTIDA
        // (Asumiendo que FileToLoad tiene algún evento de botón o Pressed, ajústalo según el tipo de nodo que sea)
        // slot.Pressed += () => CargarPartidaSeleccionada();
    }

    private void CargarPartidaSeleccionada()
    {
        if (gameData != null)
        {
            // Imprimimos en consola para verificar que los datos se leyeron bien del JSON
            GD.Print("Cargando posición X: " + gameData.PlayerPosX + ", Y: " + gameData.PlayerPosY);
        }

        // Redirigimos al nivel de juego usando tu pantalla de carga
        LoadingScreen.targetScenePath = "res://testing_player.tscn";
        GetTree().ChangeSceneToFile("res://escenas/Uis/loading_screen/loading_screen.tscn");
    }

    public override void _Process(double delta)
    {
    }
}