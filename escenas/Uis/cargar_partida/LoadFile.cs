using Godot;
using System;

public partial class LoadFile : Control
{
	private SaveManager.GameData gameData;
	private VBoxContainer container;
	private FileToLoad loadFile;

	[Export] PackedScene FileToLoadScene;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		container = GetNode<VBoxContainer>("Background/Container");
		gameData = SaveManager.LoadGame();

		if(gameData == null) return;
		
		if(FileToLoadScene == null) return;

		var slot = FileToLoadScene.Instantiate<FileToLoad>();
		container.AddChild(slot);
		slot.GetNode<Label>("Label").Text = "Partida guardada";
		slot.SizeFlagsVertical = SizeFlags.ShrinkCenter;
		slot.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
