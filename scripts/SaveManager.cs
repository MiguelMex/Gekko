using Godot;
using System.Text.Json; // O puedes usar Godot.Collections para Variant
using System.IO;

public partial class SaveManager : Node
{
    private const string SavePath = "user://savegame.json";

    // Estructura de los datos que quieres guardar
    public class GameData
    {
        public float PlayerPosX { get; set; }
        public float PlayerPosY { get; set; }
        public int CurrentLevel { get; set; }
        public bool HasKey { get; set; }
    }

    // Método para guardar datos
    public static void SaveGame(Vector2 playerPosition, int level, bool hasKey)
    {
        var data = new GameData()
        {
            PlayerPosX = playerPosition.X,
            PlayerPosY = playerPosition.Y,
            CurrentLevel = level,
            HasKey = hasKey
        };

        // Convertimos los datos a texto JSON
        string jsonString = JsonSerializer.Serialize(data);
        
        // Guardamos el archivo en la ruta segura de usuario de Godot ("user://")
        using var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Write);
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
        string jsonString = file.GetAsText();

        // Recuperamos los datos desde el JSON
        var data = JsonSerializer.Deserialize<GameData>(jsonString);
        return data;
    }
}