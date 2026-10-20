using Godot;
using System;

public partial class MiniaturaCheckpoint : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public void _goToCheckpoint()
	{
		_goToScene("res://testing_player.tscn");
	}

	private void _goToScene(string path)
	{
		LoadingScreen.targetScenePath = path;
		GetTree().ChangeSceneToFile("res://escenas/Uis/loading_screen/loading_screen.tscn");

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
