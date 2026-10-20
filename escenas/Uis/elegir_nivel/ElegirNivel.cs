using Godot;
using System;
using System.Collections.Generic;

public partial class ElegirNivel : Control
{
	[Export] public string levelsDirectory = "res://mapas/niveles/";
	[Export] public PackedScene packedLevel;

	private GridContainer gridContainer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gridContainer = GetNode<GridContainer>("Fondo/Container");
		List<string> levelList = _loadAllLevels();

		foreach(var level in levelList)
		{
			var instance = packedLevel.Instantiate<MiniaturaNivel>();
			instance.name = level;
			instance.levelRoute = levelsDirectory.PathJoin(level);
			gridContainer.AddChild(instance);
		}
	}

	private List<string> _loadAllLevels()
	{
		var result = new List<string>();
		if(!DirAccess.DirExistsAbsolute(levelsDirectory)) return result;

		using var dir = DirAccess.Open(levelsDirectory);
		if (dir == null) return result;

		dir.ListDirBegin();
		string name;
		while ((name = dir.GetNext()) != "")
		{
			if (name == "." || name == "..") continue;
			if (dir.CurrentIsDir()) result.Add(name);
		}
		dir.ListDirEnd();
		return result;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
