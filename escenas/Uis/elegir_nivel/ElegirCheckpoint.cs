using Godot;
using System;
using System.Collections.Generic;

public partial class ElegirCheckpoint : Control
{
	// Called when the node enters the scene tree for the first time.
	[Export] public PackedScene packedCheckpoint;
	private Label label;
	private string _levelPath;
	private VBoxContainer vBoxContainer;
	public override void _Ready()
	{
		vBoxContainer = GetNode<VBoxContainer>("Background/Container");
		label = GetNode<Label>("Background/Label");

		label.Text = LoadingScreen.sceneToLoadName;

		_levelPath = LoadingScreen.targetLevelPath;
		if (string.IsNullOrEmpty(_levelPath))
		{
			GD.PushError("ElegirCheckpoint: no se recibió ninguna ruta de nivel.");
            return;
		}
		
		foreach (var info in _loadCheckpoints())
		{
			var item = packedCheckpoint.Instantiate<MiniaturaCheckpoint>();
			item.checkpointName = info.Name;
			item.position = info.GlobalPosition;
			item.levelScenePath = _levelPath;
			vBoxContainer.AddChild(item);
		}

	}

	private List<Checkpoint> _loadCheckpoints()
	{
		var result = new List<Checkpoint>();

		var packed = GD.Load<PackedScene>(_levelPath);
		if(packed == null)
		{
			GD.PushError($"No se pudo ccargar la escena: {_levelPath}");
			return result;
		}

		var root = packed.Instantiate();
		sortCheckpoints(root, result);
		root.QueueFree();
		return result;

	}

	private void sortCheckpoints(Node node, List<Checkpoint> list)
	{
		if(node is Checkpoint checkpoint) list.Add(checkpoint);

		foreach(var child in node.GetChildren()) sortCheckpoints(child, list);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
