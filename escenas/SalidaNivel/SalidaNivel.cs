using Godot;

public partial class SalidaNivel : Area2D
{
	[Export]
	public string NextScenePath { get; set; } = "";

	private bool _activated = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (_activated || body is not Jugador)
			return;

		if (string.IsNullOrWhiteSpace(NextScenePath))
		{
			GD.PrintErr("No se ha asignado el siguiente mapa.");
			return;
		}

		_activated = true;

		TransitionManager transition =
			GetNode<TransitionManager>("/root/TransitionManager");

		transition.ChangeScene(NextScenePath);
	}
}
