using Godot;
using System;

public partial class ArenaMovedisa : Node2D
{
	[Export] public bool top;
	[Export] public int pull = 50;
	[Export] public float SuffocateTime = 5.0f;

	private AnimatedSprite2D _sprite;
	private Timer _suffocateTimer;
	private Jugador _jugador;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("sprite");
		//Define que animación va a reproducirse
		_sprite.Play(top ? "top" : "bottom");

		//Time
		_suffocateTimer = new Timer();
		this.AddChild(_suffocateTimer);
		_suffocateTimer.OneShot = true;
		_suffocateTimer.Autostart = false;
		_suffocateTimer.WaitTime = SuffocateTime;

		_suffocateTimer.Timeout += OnSuffocateTimeout;
	}

	private void OnSuffocateTimeout()
	{
		if (_jugador ==  null || !IsInstanceValid(_jugador)) return;
		_jugador.Morir();
	}

	private void _BodyEntered(Node2D node)
	{
		if (node is not CharacterBody2D character) return;

		if (character is Jugador jugador)
		{
			jugador.EnArenasMovedisas(pull, true);
			_jugador = jugador;
		}
		
	}

	private void _AreaEntered(Area2D area)
	{
		if (!_CheckForHead(area)) return;
		
		_suffocateTimer.Start();
		GD.Print("Cabeza entró en la arena");
	}

	private void _BodyExited(Node2D node)
	{
		if (node is not CharacterBody2D character) return;

		if (character is Jugador jugador)
		{
			jugador.EnArenasMovedisas(0, false);
			_jugador = null;
		}
	}

	private void _AreaExited(Area2D area)
	{
		if (!_CheckForHead(area)) return;
		GD.Print("Cabeza salió de la arena");
		_suffocateTimer.Stop();
	}

	private bool _CheckForHead(Area2D area)
	{
		if (area.Name != "head") return false;
		if (area.GetParent() is not Jugador) return false;
		return true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
