using Godot;
using System;

public partial class Jugador : CharacterBody2D
{
	// Variable estática para llevar el registro de muertes durante la partida
	public static int ContadorMuertesTotales = 0;

	//Variable de control de la velocidad acumulada
	private Label _labelVelocidad; // NUEVO: Referencia para el texto de velocidad

	// Referencia al Label de la UI (asegúrate de ajustar la ruta si es diferente)
	private Label _labelMuertes;

	/**
	[Export] cumple la función de mostrar un valor del script en el motor grafico de GODOT
	Speed es la velocidad a la que se moverá el jugador
	*/
	[Export] public float Speed = 400.0f;
	[Export] public float SprintMultiplier = 2.0f;
	[Export] public double DoubleTapWindow = 0.3; //Segundos para considerar el doble input
	private const float DuracionInvulnerabilidadEnemigos = 1.5f;


	[Export] public float StepUpHeight = 50f;   // antes 8f
	private const float RayReach = 135f;
	private RayCast2D _wallCheck;
	private RayCast2D _stepCheck;



	private double _timeSinceLastPress = 999.0;
	private int _lastTapDirection = 0;
	private bool _isSprinting = false;
	private float _tiempoInvulnerable;

	private int _consecutiveJump = 0;
	private Vector2 previousPosition;


	/**
	La velocidad con la que el jugador salta
	*/
	[Export]
	public float JumpVelocity = -1500.0f;

	// NUEVO: Variable para guardar la posición de reaparición actual
	private Vector2 _respawnPosition;

	//Valores para la hailidad de velocidad acumulada
	[Export]
	public float speedGrowRate = 0.5f;
	[Export]
	public float maxSpeedStored = 4f;
	[Export]
	public float speedStored = 1;
	[Export]
	public PackedScene shockwave;

	AnimatedSprite2D _sprite;
	private AudioStreamPlayer _jumpSound;
	private AudioStreamPlayer _gameOverSound;
	private AudioStreamPlayer _winSound;
	private AudioStreamPlayer _afkSound;
	private AudioStreamPlayer _fallSound;
	private AudioStreamPlayer _checkpointSound;
	private AudioStreamPlayer _sprintSound;
	private AudioStreamPlayer _impactSound;
	private const float ImpactSoundThreshold = 260.0f;

	double _timeIdle;

	public float facing;
	private float _knockbackTime = 0f;
	[Export]
	public float knockbackDuration = 0.25f;

	public bool is_crouching = false;

	//Si la habiidad esta activada
	private bool _abilityState = false;

	//Para la arena movedisa
	private int _arenaOverlapCount = 0;
	private bool _dragedDown => _arenaOverlapCount > 0;
	private float _dragForce = 500.0f;

	public override void _Ready()
	{
		// Permite al administrador encontrar al Gekko después de cargar el nuevo mapa
		AddToGroup("jugador");

		// 1. Verificamos si venimos de cargar una partida guardada
		if (SaveManager.CurrentGameData != null)
		{
			GlobalPosition = new Vector2(
				SaveManager.CurrentGameData.PlayerPosX,
				SaveManager.CurrentGameData.PlayerPosY
			);

			SaveManager.CurrentGameData = null;

			GD.Print("¡Jugador posicionado en la partida cargada!");
		}
		else if (LoadingScreen.targetPlayerSpawnPosition.HasValue)
		{
			GlobalPosition = LoadingScreen.targetPlayerSpawnPosition.Value;
			LoadingScreen.targetPlayerSpawnPosition = null;
		}

		// 2. Asignamos la posición de reaparición
		_respawnPosition = GlobalPosition;

		// 3. Inicializamos el jugador
		_sprite = GetNode<AnimatedSprite2D>("sprite");
		_sprite.Play("idle");

		// Inicializamos los sonidos
		_jumpSound = GetNode<AudioStreamPlayer>("SonidoSalto");
		_gameOverSound = GetNode<AudioStreamPlayer>("SonidoGameOver");
		_winSound = GetNode<AudioStreamPlayer>("SonidoWin");
		_afkSound = GetNode<AudioStreamPlayer>("SonidoAFK");
		_fallSound = GetNode<AudioStreamPlayer>("SonidoCaida");
		_checkpointSound = GetNode<AudioStreamPlayer>("SonidoCheckpoint");
		_sprintSound = GetNode<AudioStreamPlayer>("SonidoSprint");
		_impactSound = GetNode<AudioStreamPlayer>("SonidoChoque");

		if (_sprintSound.Stream is AudioStreamWav sprintWav)
			sprintWav.LoopMode = AudioStreamWav.LoopModeEnum.Forward;

		_wallCheck = GetNode<RayCast2D>("wallcheck");
		_stepCheck = GetNode<RayCast2D>("stepcheck");

		GetNode<Area2D>("DetectorPeligros").BodyEntered += OnPeligroDetectado;

		// Inicializamos los contadores
		_labelMuertes = GetNodeOrNull<Label>("%ContadorMuertes");
		ActualizarTextoMuertes();

		_labelVelocidad = GetNodeOrNull<Label>("%ContadorVelocidad");
	}
	private void ActualizarTextoMuertes()
	{
		if (_labelMuertes != null)
		{
			_labelMuertes.Text = $"Muertes: {ContadorMuertesTotales}";
		}
	}

	private void ActualizarTextoVelocidad()
	{
		if (_labelVelocidad != null)
		{
			// Mostramos la velocidad actual redondeada (puedes usar Velocity.X o Velocity.Length())
			int velocidadActual = Mathf.RoundToInt(Mathf.Abs(Velocity.X));
			_labelVelocidad.Text = $"Vel: {velocidadActual}";
		}
	}
	private void OnPeligroDetectado(Node2D body)
	{
		// Verificamos si el objeto detectado es una capa de tiles
		if (body is TileMapLayer)
		{
			// Reutilizamos el método de muerte que ya tenía el jugador
			Morir();
		}
	}

	public void _playerIsIdle()
	{
		//GD.Print("Time idle is: "+_timeIdle);
		if (_timeIdle > 10)
		{
			_sprite.Play("tongue");
		}
		else
		{
			_sprite.Play("idle");
		}

	}

	//Maneja la animación de agacharse del personaje
	public void _manageCruch()
	{
		if (is_crouching)
		{
			_sprite.Play("crouch");
		}
		else
		{
			_sprite.Play("stand_up");
		}
	}

	//Maneja la animación de cmainata
	public void _animateWalk(bool sprinting)
	{
		if (facing < 0)
		{
			_sprite.FlipH = true;
		}
		else if (facing > 0)
		{
			_sprite.FlipH = false;
		}

		if (IsOnFloor()) _sprite.Play("run");
		_sprite.SpeedScale = sprinting ? 2 : 1;
		if (sprinting)
		{
			if (!_sprintSound.Playing) _sprintSound.Play();
		}
		else if (_sprintSound.Playing) _sprintSound.Stop();
	}

	//Metodo de prueba para probar las mecanicas que se activan tras una explosión de velocidad
	public void _testExplosion()
	{
		var radious = 50 * 4;
		var damage = 2 * 4;
		//GD.Print("Daño: "+damage+", radio: "+radious);
		if (shockwave != null)
		{
			var wave = shockwave.Instantiate<Shockwave>();
			GetParent().AddChild(wave);
			wave.GlobalPosition = GlobalPosition;
			wave._setAttributes(radious, damage);
		}
	}

	//Metodo que maneja la pausa
	public void _ManagePause()
	{
		if (GetTree().Paused)
		{
			//Despausar
			//GetTree().Paused = false;
			_sprite.Pause();
		}
		else
		{
			//Pausar
			//GetTree().Paused = true;
			_sprite.Play();
		}
		//GD.Print("Pauses: "+(GetTree().Paused ? "ON" : "OFF"));
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		//Si se detecta unna pausa se detiene toda la escena
		if (@event.IsActionPressed("pause"))
		{
			_ManagePause();
		}
	}

	// Se activa cuando la tecla está siendo presionada
	public void _storeSpeedStatus()
	{
		//Establece el estado
		speedStored *= -1;
	}

	public override void _PhysicsProcess(double delta)
	{
		// No avanza si el juego está pausado
		if (GetTree().Paused) return;

		// Activación de la habilidad de velocidad con Z
		if (Input.IsActionJustPressed("ability"))
		{
			if (_abilityState)
			{
				// La habilidad está activada: libera la explosión
				_createExplosion();
			}
			else
			{
				// Activa la habilidad y comienza a acumular velocidad
				_abilityState = true;
			}
		}

		_tiempoInvulnerable = Mathf.Max(
			0.0f,
			_tiempoInvulnerable - (float)delta
		);

		_ManagePause();
		bool wasOnFloor = IsOnFloor();

		Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			if (!_dragedDown) velocity += GetGravity() * (float)delta;
			_sprite.Stop();
			_sprite.Play("fall");
		}
		else
		{
			_consecutiveJump = 0;
		}
		// Manejar el salto.
		if (Input.IsActionJustPressed("jump") && _consecutiveJump < 2)
		{
			if (_afkSound.Playing) _afkSound.Stop();
			_sprite.Play("jump");
			_jumpSound.Play();

			velocity.Y = _dragedDown ? JumpVelocity / 4 : JumpVelocity;

			_timeIdle = 0;
			_consecutiveJump++;
		}

		//Solo se agacha si está en el suelo y se mantiene la tecla presionada
		if (Input.IsActionPressed("down") && IsOnFloor())
		{
			is_crouching = true;
			_manageCruch();
			//_testExplosion();
		}
		if (Input.IsActionJustReleased("down") && is_crouching)
		{
			is_crouching = false;
			_manageCruch();
		}

		//No se detecta ningún input horizontal durante el knocback
		if (_knockbackTime > 0)
		{
			_knockbackTime -= (float)delta;
			Velocity = velocity;
			MoveAndSlide();
			PlayLandingSound(wasOnFloor, velocity.Y);
			DetectHardTerrainImpact(velocity);
			ActivarPlataformaBajoJugador();
			return;
		}

		/**
		Detecta el imput enviado por el jugador, se definen en el motor grafico
		Solo se está usando izquierda y derecha así que no hay necesidad de asignar las otras dos
		*/
		facing = Input.GetAxis("left", "right");
		Vector2 direction = new Vector2(facing, 0);
		//Deteccción de doble input
		if (Input.IsActionJustPressed("left") || Input.IsActionJustPressed("right"))
		{
			int pressDir = Input.IsActionJustPressed("left") ? -1 : 1;

			//Misma dirección y dentro de la ventana del tiempo
			if (_timeSinceLastPress <= DoubleTapWindow && pressDir == _lastTapDirection)
			{
				_isSprinting = true;
				//GD.Print("¡Sprint activado!");
			}
			else
			{
				_isSprinting = false;
				//GD.Print("No hay sprint");
			}

			_lastTapDirection = pressDir;
			_timeSinceLastPress = 0;
		}
		_timeSinceLastPress += delta;
		//Detecta si hubo algun input, no puede caminar mientras está agachado
		if (direction != Vector2.Zero && !is_crouching)
		{
			//Detecta si esta acumulando velocidad
			if (_abilityState)
			{
				//Solo acumula velocidad si la habilidad esta activada
				speedStored = Mathf.Min(speedStored + speedGrowRate * (float)delta, maxSpeedStored);
				GD.Print("Velocidad acumulada: " + speedStored);
			}
			float currentSpeed = _isSprinting ? Speed * SprintMultiplier : Speed;
			//Cambio en la velocidad del jugador
			velocity.X = direction.X * currentSpeed * speedStored;
			_animateWalk(_isSprinting);
			_timeIdle = 0;
		}
		else
		{
			//Que frene en seco si no hay input
			//speedStored = 1;
			velocity.X = 0;
			_timeIdle += delta;
			_isSprinting = false;
			if (_sprintSound.Playing) _sprintSound.Stop();
			if (_timeIdle > 10 && !_afkSound.Playing) _afkSound.Play();
		}
		if (direction != Vector2.Zero && _afkSound.Playing) _afkSound.Stop();

		//Para arenas movedisas
		if (_dragedDown)
		{
			velocity.Y += _dragForce * (float)delta;

			//Para no acelerar infinitamente
			float maxSinkSpeed = _dragForce * 0.5f;
			velocity.Y = Math.Min(velocity.Y, maxSinkSpeed);

			//frenar el mocimiento horizontal
			velocity.X *= 0.01f;
		}

		//GD.Print("Velocidad acumulada: "+speedStored);
		Velocity = velocity;
		MoveAndSlide();
		PlayLandingSound(wasOnFloor, velocity.Y);

		DetectHardTerrainImpact(velocity);
		ActivarPlataformaBajoJugador();

		//Detecta si hubo una colisión en este frame
		_detectHorizontalCollision(direction);

		// Actualizar el texto de la velocidad en pantalla
		ActualizarTextoVelocidad();
	}

	private void PlayLandingSound(bool wasOnFloor, float incomingVerticalSpeed)
	{
		// El sonido se reproduce al tocar el piso tras caer, nunca al empezar a descender.
		if (!wasOnFloor && IsOnFloor() && incomingVerticalSpeed >= 200.0f)
			_fallSound.Play();
	}

	// Aplica el knockback de una explosión
	public void _applyKnockback(Vector2 direction, float force)
	{
		// Si se está agachando no hay movimiento vertical
		if (is_crouching)
		{
			Velocity = new Vector2(
				direction.X * force,
				Velocity.Y
			);
		}
		else
		{
			Velocity = new Vector2(
				direction.X * force,

				-force * 0.1f
			);
		}

		_knockbackTime = knockbackDuration;
	}


	// Detecta colisiones con paredes
	public void _detectHorizontalCollision(Vector2 inputDirection)
	{
		if (inputDirection.X == 0) return;
		if (!IsOnWall()) return;

		var collision = GetLastSlideCollision();
		if (collision == null) return;

		Vector2 normal = collision.GetNormal();

		if (Mathf.Sign(normal.X) == -Math.Sign(inputDirection.X))
		{
			_createExplosion();
		}
	}


	// Detecta impactos fuertes contra superficies
	private void DetectHardTerrainImpact(Vector2 incomingVelocity)
	{
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			var collision = GetSlideCollision(i);

			// Ignora el suelo
			if (collision.GetNormal().Y < -0.7f) continue;

			float impactSpeed = -incomingVelocity.Dot(collision.GetNormal());

			if (impactSpeed >= ImpactSoundThreshold && !_impactSound.Playing)
			{
				_impactSound.Play();
				return;
			}
		}
	}


	// Activa plataformas especiales al pisarlas
	private void ActivarPlataformaBajoJugador()
	{
		if (!IsOnFloor()) return;

		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			Node? cuerpo = GetSlideCollision(i).GetCollider() as Node;

			if (cuerpo == null || !cuerpo.HasMethod("activar_por_jugador"))
				continue;

			cuerpo.Call("activar_por_jugador");
			return;
		}
	}
	//Crea una explosión y resetea la velocidad acumulada
	private void _createExplosion()
	{
		//Calculo del daño y area de la onda expansiva
		var radious = speedStored * 2;
		var damage = speedStored * 3;
		//GD.Print("Daño: "+damage+", radio: "+radious);
		if (shockwave != null && speedStored > 2)
		{
			GD.Print("Crear explosión");
			var wave = shockwave.Instantiate<Shockwave>();
			GetParent().AddChild(wave);
			wave.GlobalPosition = GlobalPosition;
			wave._setAttributes(radious, damage);
		}
		speedStored = 1;
		_abilityState = false;
	}

	//Para las arenas movedisas
	public void EnArenasMovedisas(float dragForce, bool inside)
	{
		if (inside)
		{
			_arenaOverlapCount++;
			_dragForce = dragForce;
			GD.Print($"Esta en arena, total: ${_arenaOverlapCount}");
		}
		else
		{
			_arenaOverlapCount = Mathf.Max(0, _arenaOverlapCount - 1);
			GD.Print($"Sale de la arena, total: ${_arenaOverlapCount}");
		}

	}

	// NUEVO: Método público para actualizar el checkpoint
	public void ActualizarCheckpoint(Vector2 nuevaPosicion)
	{
		_respawnPosition = nuevaPosicion;
		_checkpointSound.Play();
		GD.Print("¡Checkpoint actualizado!");
	}

	// NUEVO: Método para "morir" y regresar al último checkpoint
	public void Morir()
	{
		// Se asegura de que la arena no quede con efectos en el jugador
		_arenaOverlapCount = 0;
		_dragForce = 0f;
		Respawn();
	}

	// La protección temporal solo ignora golpes de enemigos.
	public void RecibirDanioEnemigo()
	{

		// Evita muertes repetidas si reaparece junto al águila o a otro peligro.
		if (_tiempoInvulnerable > 0.0f)
			return;

		Respawn();
		_tiempoInvulnerable = DuracionInvulnerabilidadEnemigos;
	}

	private void Respawn()
	{
		_gameOverSound.Play();
		if (_sprintSound.Playing) _sprintSound.Stop();
		if (_afkSound.Playing) _afkSound.Stop();
		// Death can occur during a physics query; reset platforms after it finishes.
		GetTree().CallGroupFlags((uint)SceneTree.GroupCallFlags.Deferred,
			"plataformas_reiniciables", "reiniciar");
		GlobalPosition = _respawnPosition;
		Velocity = Vector2.Zero;


		speedStored = 1;
		_abilityState = false;
		// 1. Incrementamos el contador
		ContadorMuertesTotales++;
		ActualizarTextoMuertes();

		GD.Print($"El jugador ha muerto. Muertes totales: {ContadorMuertesTotales}");

	}

	// Conectar con la señal que complete el nivel.
	public void Ganar()
	{
		_winSound.Play();
		if (_sprintSound.Playing) _sprintSound.Stop();
	}



	private void UpdateRays()
	{
		if (Velocity.X == 0) return;
		float dir = Mathf.Sign(Velocity.X);
		_wallCheck.TargetPosition = new Vector2(RayReach * dir, 0);
		_stepCheck.TargetPosition = new Vector2(RayReach * dir, 0);

		// Aplica el cambio de dirección de inmediato, sin esperar al siguiente frame
		_wallCheck.ForceRaycastUpdate();
		_stepCheck.ForceRaycastUpdate();

	}

	private void TryStepUp()
	{
		if (Velocity.X != 0 && IsOnFloor() && Velocity.Y >= 0
			&& _wallCheck.IsColliding() && !_stepCheck.IsColliding())
		{
			GlobalPosition += new Vector2(0, -StepUpHeight);
		}

	}

}
