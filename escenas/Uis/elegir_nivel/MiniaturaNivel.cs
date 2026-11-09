using Godot;
using System;

public partial class MiniaturaNivel : Control
{
	[Export] public string name = "";
	[Export] public string levelRoute = "";

	[Export] public string imageRoute = "";

	//La etiqueta con el nombre
	private Label label;
	private TextureRect textureRect;
	//La ruta para el nivel que va a cargar
	//No se usa aquí, se pasa a la seleección de checkpoints
	private string path;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		label = GetNode<Label>("Background/Name");
		
		//Cambiar el nombre
		label.Text = name;
		var texture = GD.Load<Texture2D>(imageRoute);
		textureRect = GetNode<TextureRect>("Background/Image");
		textureRect.Texture = texture;

		path = _findLevelScene(levelRoute);
		if (string.IsNullOrEmpty(path))
		{
			GD.PushWarning($"No se encontró escena dentro de {path}");
		}
	}

	private string _findLevelScene(string folderPath)
	{
		if(!DirAccess.DirExistsAbsolute(folderPath)) return null;

		using var dir = DirAccess.Open(folderPath);
		if (dir == null) return null;

		string tmxFallback = null;
		dir.ListDirBegin();
		string file;
		while((file = dir.GetNext()) != "")
		{
			if(file == "." || file == "..") continue;
			if(dir.CurrentIsDir()) continue;

			//Buscamos .tscn
			//Si no hay, aceptamos el .tmx
			if (file.EndsWith(".tscn"))
			{
				string full = folderPath.PathJoin(file);
				dir.ListDirEnd();
				return full;
			}

			if(file.EndsWith(".tmx") && tmxFallback == null)
			{
				tmxFallback = folderPath.PathJoin(file);
			}
		}
		dir.ListDirEnd();
		return tmxFallback;
	}

	public void _goToLevel()
	{
		if (string.IsNullOrEmpty(path))
		{
			GD.PushError("No hay escena de nivel que cargar.");
            return;
		}
		LoadingScreen.targetScenePath = "res://escenas/Uis/elegir_nivel/elegir_checkpoint.tscn";
		LoadingScreen.targetLevelPath = path;
		LoadingScreen.sceneToLoadName = name;
		LoadingScreen.levelImagePath = imageRoute;
		GetTree().ChangeSceneToFile("res://escenas/Uis/loading_screen/loading_screen.tscn");

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
