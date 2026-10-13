using Godot;
using System;

public partial class FileToLoad : Control
{
	private SaveManager.GameData gameData;

	public void setGameData(SaveManager.GameData data)
	{
		gameData = data;
		label = GetNode<Label>("Label");
		label.Text = getGameDataString(gameData);
	}

	public string getGameDataString(SaveManager.GameData data)
	{
		if (data == null) return "";
		String currentLevel = data.CurrentLevel.ToString();
		String positionX = data.PlayerPosX.ToString();
		String positionY = data.PlayerPosY.ToString();
		String hasKey = data.HasKey.ToString();

		return currentLevel + "\n" + positionX + " " + positionY + "\n" + hasKey;
	}

	//Nodos
	private Label label;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionPressed("accept_ui"))
		{
			//Placeholder, la ruta del nivel debe depender de que nivel es
			_goToScene("res://testing_player.tscn");
		}
	}

	private void _goToScene(string path)
	{
		LoadingScreen.targetScenePath = path;
		GetTree().ChangeSceneToFile("res://escenas/Uis/loading_screen/loading_screen.tscn");
	}
}
