using Godot;
using System.Text.Json; // O puedes usar Godot.Collections para Variant
using System.IO;


public partial class SaveManager : Node
{
    public static SaveManager.GameData CurrentGameData { get; set; }
    private const string SavePath = "user://savegame.json";

    // Estructura de los datos que quieres guardar
    public class GameData
    {
        public float PlayerPosX { get; set; }
        public float PlayerPosY { get; set; }
        public int CurrentLevel { get; set; }
        public bool HasKey { get; set; }
        // Ruta de la escena del nivel donde se guardó (para cargar el nivel correcto)
        public string ScenePath { get; set; } = "";
    }

    // Reinicia todo el estado de partida para empezar desde cero
    public static void NuevaPartida()
    {
        CurrentGameData = null;
        LoadingScreen.targetPlayerSpawnPosition = null;
        LoadingScreen.levelSelected = false;
        Jugador.ContadorMuertesTotales = 0;
    }

    // Método para guardar datos
    public static void SaveGame(Vector2 playerPosition, string scenePath, bool hasKey)
    {
        var data = new GameData()
        {
            PlayerPosX = playerPosition.X,
            PlayerPosY = playerPosition.Y,
            CurrentLevel = Niveles.IndiceDe(scenePath),
            HasKey = hasKey,
            ScenePath = scenePath
        };

        // Convertimos los datos a texto JSON
        string jsonString = JsonSerializer.Serialize(data);
        
        // Guardamos el archivo en la ruta segura de usuario de Godot ("user://")
        using var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Write);
        if (file == null)
        {
            GD.PrintErr($"No se pudo escribir el guardado: {Godot.FileAccess.GetOpenError()}");
            return;
        }
        file.StoreString(jsonString);
        
        GD.Print("¡Juego guardado exitosamente!");
    }

    // Método para cargar datos
    public static GameData LoadGame()
    {
        if (!Godot.FileAccess.FileExists(SavePath))
        {
            GD.Print("No se encontró archivo de guardado.");
            return null;
        }

        using var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Read);
        if (file == null) return null;
        string jsonString = file.GetAsText();

        try
        {
            // Recuperamos los datos desde el JSON
            var data = JsonSerializer.Deserialize<GameData>(jsonString);
            if (data == null) return null;

            // Guardados antiguos sin ruta: empiezan en el primer nivel
            if (string.IsNullOrEmpty(data.ScenePath) || Niveles.IndiceDe(data.ScenePath) < 0)
                data.ScenePath = Niveles.RutaInicial;
            return data;
        }
        catch (JsonException e)
        {
            GD.PrintErr($"Guardado corrupto: {e.Message}");
            return null;
        }
    }
}