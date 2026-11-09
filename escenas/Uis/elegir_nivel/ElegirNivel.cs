using Godot;
using System;
using System.Collections.Generic;

public partial class ElegirNivel : Control
{
	[Export] public string levelsDirectory = "res://mapas/niveles/";
	[Export] public PackedScene packedLevel;

	[Export] public float slideShowInterval = 5.0f;

	private GridContainer gridContainer;
	private ScrollContainer scrollBar;
	private TextureRect previewRect;

	//Para el cambio de imagenes
	private readonly List<Texture2D> _miniatures = new();
    private int _currentIndex = 0;
    private Timer _slideTimer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		scrollBar = GetNode<ScrollContainer>("Fondo/Scroll");
		gridContainer = scrollBar.GetNode<GridContainer>("Container");
		previewRect = GetNode<TextureRect>("Fondo/Background");

		gridContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		gridContainer.SizeFlagsVertical = Control.SizeFlags.ShrinkBegin;

		List<string> levelList = _loadAllLevels();

		foreach(var level in levelList)
		{
			var instance = packedLevel.Instantiate<MiniaturaNivel>();
			instance.name = level;
			instance.imageRoute = levelsDirectory.PathJoin(level).PathJoin("miniature.jpg");
			instance.levelRoute = levelsDirectory.PathJoin(level);
			gridContainer.AddChild(instance);
			if (ResourceLoader.Exists(instance.imageRoute))
			{
				_miniatures.Add(GD.Load<Texture2D>(instance.imageRoute));
			} else
			{
				GD.PushWarning($"Falta miniatura: {instance.imageRoute}");
			}
		}

		//Para el cambio de imagenes en el fondo
		if (previewRect != null && _miniatures.Count > 0)
		{
			previewRect.Texture = _miniatures[0];
		}

		_slideTimer = new Timer
		{
			WaitTime = slideShowInterval,
			OneShot = false,
			Autostart = true
		};

		_slideTimer.Timeout += _OnSlideTimeout;
		AddChild(_slideTimer);
	}

	private async void _OnSlideTimeout()
	{
		if(previewRect == null || _miniatures.Count < 2) return;

		var fadeOut = CreateTween();
		fadeOut.TweenProperty(previewRect, "modulate:a", 0.0f, 0.25f);
		await ToSignal(fadeOut, Tween.SignalName.Finished);

		_currentIndex = (_currentIndex + 1) % _miniatures.Count;
		previewRect.Texture = _miniatures[_currentIndex];

		var fadeIn = CreateTween();
		fadeIn.TweenProperty(previewRect, "modulate:a", 1.0f, 0.25f);
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
