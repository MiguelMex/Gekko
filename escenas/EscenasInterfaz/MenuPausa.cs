using Godot;
using System;

public partial class MenuPausa : CanvasLayer
{
	 public override void _Ready()
	{
		Visible = false;
		ProcessMode = ProcessModeEnum.Always;

		GetNode<Button>("VBoxContainer/ReanudarButton").Pressed += OnResume;
		GetNode<Button>("VBoxContainer/SalirEscButton").Pressed += OnQuit;
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

	private void OnQuit()
	{
		GetTree().Quit();
	}
}
