using Godot;
using Godot.Collections;
using System;

public partial class LoadingScreen : Control
{
	//La escena que va a cargar
	public static String targetScenePath = "";
	private ResourceLoader.ThreadLoadStatus loading_status;
	private Godot.Collections.Array progress = new();
	private ProgressBar _progressBar;
	private AnimatedSprite2D _geckoSprite;

	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	_progressBar = GetNode<ProgressBar>("ProgressBar");
	_geckoSprite = GetNode<AnimatedSprite2D>("GeckoBailando");
	_geckoSprite.Play("default"); // <--- ESTA LÍNEA INICIA LA ANIMACIÓN

	_geckoSprite = GetNode<AnimatedSprite2D>("CactusBailando");
    _geckoSprite.Play("default"); // <--- ESTA LÍNEA INICIA LA ANIMACIÓN

	
		if (string.IsNullOrEmpty(targetScenePath))
		{
			GD.Print("Escena a cargar no definida");
			return;
		}
		ResourceLoader.LoadThreadedRequest(targetScenePath);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(string.IsNullOrEmpty(targetScenePath)) return;

		loading_status = ResourceLoader.LoadThreadedGetStatus(targetScenePath, progress);

		switch (loading_status)
		{
			case ResourceLoader.ThreadLoadStatus.InProgress:
				_progressBar.Value = progress[0].AsSingle() * 100;
				break;
			case ResourceLoader.ThreadLoadStatus.Loaded:
				var scene = (PackedScene)ResourceLoader.LoadThreadedGet(targetScenePath);
				//Limpiar string
				targetScenePath = "";
				GetTree().ChangeSceneToPacked(scene);
				break;
			case ResourceLoader.ThreadLoadStatus.Failed:
				GD.PrintErr("Error, resource not loaded");
				targetScenePath = "";
				break;
			case ResourceLoader.ThreadLoadStatus.InvalidResource:
				GD.PrintErr("Invalid resource");
				break;
		}
	}
}
