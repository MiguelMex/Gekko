
using Godot;
using System;

public partial class Checkpoint : Area2D
{
	private Sprite2D _banderaOFF;
	private Sprite2D _banderaON;
	private AudioStreamPlayer2D _sonidoCheckpoint;
	private Marker2D _spawnPoint;

	// Efecto de chispitas
	private AnimatedSprite2D _efecto;

	private bool _isActivated = false;

	public override void _Ready()
	{
		// Referencias a los nodos de la escena.
		_banderaOFF = GetNode<Sprite2D>("BanderaOFF");
		_banderaON = GetNode<Sprite2D>("BanderaON");
		_sonidoCheckpoint = GetNode<AudioStreamPlayer2D>("SonidoCheckpoint");
		_spawnPoint = GetNode<Marker2D>("SpawnPoint");

		// Referencia y configuración del efecto.
		_efecto = GetNode<AnimatedSprite2D>("Efecto");
		_efecto.Visible = false;
		_efecto.AnimationFinished += OnEfectoFinished;

		// Estado inicial del checkpoint.
		_banderaOFF.Visible = true;
		_banderaON.Visible = false;

		// Detectar cuando un cuerpo entra en el área.
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		// Solo activar cuando entra el jugador y todavía está apagado.
		if (body is Jugador jugador && !_isActivated)
		{
			_isActivated = true;

			// Reproducir las chispitas una sola vez.
			_efecto.Visible = true;
			_efecto.Play("activar");

			// Actualizar el punto de reaparición del jugador.
			jugador.ActualizarCheckpoint(_spawnPoint.GlobalPosition);

			// Cambiar la apariencia de OFF a ON.
			_banderaOFF.Visible = false;
			_banderaON.Visible = true;

			// Reproducir el sonido si hay un archivo asignado.
			if (_sonidoCheckpoint.Stream != null)
			{
				_sonidoCheckpoint.Play();
			}

			// Guardar el progreso del jugador.
			SaveManager.SaveGame(
				_spawnPoint.GlobalPosition,
				level: 1,
				hasKey: false
			);

			GD.Print("¡Checkpoint activado!");
		}
	}

	// Ocultar las chispitas cuando termina la animación.
	private void OnEfectoFinished()
	{
		_efecto.Visible = false;
	}
}
