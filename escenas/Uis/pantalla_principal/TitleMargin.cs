using Godot;
using System;
using System.Collections.Generic;

public partial class TitleMargin : MarginContainer
{
	[Export] public int marginValue = 100;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddThemeConstantOverride("margin_top", marginValue);
		AddThemeConstantOverride("margin_left", marginValue);
		AddThemeConstantOverride("margin_bottom", marginValue);
		AddThemeConstantOverride("margin_right", marginValue);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
