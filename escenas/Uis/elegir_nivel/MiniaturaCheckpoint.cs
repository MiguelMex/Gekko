using Godot;
using System;

public partial class MiniaturaCheckpoint : Control
{
	// Called when the node enters the scene tree for the first time.
	[Export] public string checkpointName;
	[Export] public Vector2 position;

	public Node levelPath;
	public string levelScenePath;
	public override void _Ready()
	{
		var label = GetNode<Label>("Button/Name");

	}

	public void _goToCheckpoint()
	{
		if (string.IsNullOrEmpty(levelScenePath))
		{
			GD.PushError("MiniaturaCheckpoint: falta levelScenePath.");
            return;
		}

		LoadingScreen.targetScenePath = levelScenePath;
		LoadingScreen.targetPlayerSpawnPosition = position;
		GetTree().ChangeSceneToFile("res://escenas/Uis/loading_screen/loading_screen.tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
