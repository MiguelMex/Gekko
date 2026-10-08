using Godot;
using System;

public partial class BajoLaPiramide : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SaveManager.SaveGame(new Vector2(0,0),1,false);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
