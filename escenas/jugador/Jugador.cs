using Godot;
using System;

public partial class Jugador : CharacterBody2D
{

	/**
	[Export] cumple la función de mostrar un valor del script en el motor grafico de GODOT
	Speed es la velocidad a la que se moverá el jugador
	*/
	[Export] public float Speed = 300.0f;
    [Export] public float SprintMultiplier = 2.0f;
    [Export] public double DoubleTapWindow = 0.3; //Segundos para considerar el doble input
    private double _timeSinceLastPress = 999.0;
    private int _lastTapDirection = 0;
    private bool _isSprinting = false;

    private int _consecutiveJump = 0;

	/**
	La velocidad con la que el jugador salta
	*/
	[Export]
	public float JumpVelocity = -400.0f;

    // NUEVO: Variable para guardar la posición de reaparición actual
    private Vector2 _respawnPosition;
    AnimatedSprite2D _sprite;
    double _timeIdle;

    public override void _Ready()
    {
        // NUEVO: Al iniciar el juego, guardamos su posición inicial como el primer respawn por defecto
        _respawnPosition = GlobalPosition;
        _sprite = GetNode<AnimatedSprite2D>("sprite");
        _sprite.Play("idle");
    }

    public void _playerIsIdle()
    {
        GD.Print("Time idle is: "+_timeIdle);
        if(_timeIdle > 10)
        {
            _sprite.Play("tongue");
        } else
        {
            _sprite.Play("idle");
        }

    }

    public void _animateWalk(float direction, bool sprinting)
    {
        if(direction < 0){
            _sprite.FlipH = true;
        } else if (direction > 0)
        {
            _sprite.FlipH = false;
        }

        if(IsOnFloor()) _sprite.Play("run");
        if(sprinting) _sprite.SpeedScale = 2;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
            _sprite.Stop();
            _sprite.Play("fall");
        } else
        {
            _consecutiveJump = 0;
        }

		// Manejar el salto.
		if (Input.IsActionJustPressed("jump") && _consecutiveJump < 2)
		{
            _sprite.Play("jump");
			velocity.Y = JumpVelocity;
            _timeIdle = 0;
            _consecutiveJump ++;
		}

		/**
		Detecta el imput enviado por el jugador, se definen en el motor grafico
		Solo se está usando izquierda y derecha así que no hay necesidad de asignar las otras dos
		*/
        float facing = Input.GetAxis("left","right");
		Vector2 direction = new Vector2(facing, 0);
        //Deteccción de doble input
        if(Input.IsActionJustPressed("left") || Input.IsActionJustPressed("right"))
        {
            int pressDir = Input.IsActionJustPressed("left") ? -1 : 1;

            //Misma dirección y dentro de la ventana del tiempo
            if(_timeSinceLastPress <= DoubleTapWindow && pressDir == _lastTapDirection)
            {
                _isSprinting = true;
                GD.Print("¡Sprint activado!");
            } else
            {
                _isSprinting = false;
                GD.Print("No hay sprint");
            }

            _lastTapDirection = pressDir;
            _timeSinceLastPress = 0;
        }
        _timeSinceLastPress += delta;
		//Detecta si hubo algun input
		if (direction != Vector2.Zero)
		{
            float currentSpeed = _isSprinting ? Speed * SprintMultiplier : Speed;
			//Cambio en la velocidad del jugador
			velocity.X = direction.X * currentSpeed;
            _animateWalk(facing, _isSprinting);
            _timeIdle = 0;
		}
		else
		{
			//Que frene en seco si no hay input
			velocity.X = 0;
            _timeIdle += delta;
            _isSprinting = false;
            _sprite.SpeedScale = 1;
		}

        Velocity = velocity;
        MoveAndSlide();
    }

    // NUEVO: Método público para actualizar el checkpoint
    public void ActualizarCheckpoint(Vector2 nuevaPosicion)
    {
        _respawnPosition = nuevaPosicion;
        GD.Print("¡Checkpoint actualizado!");
    }

    // NUEVO: Método para "morir" y regresar al último checkpoint
    public void Morir()
    {
        // Regresamos al jugador a la posición guardada
        GlobalPosition = _respawnPosition;
        
        // Opcional pero recomendado: Frenar su velocidad para que no siga cayendo con inercia
        Velocity = Vector2.Zero;
        
        GD.Print("El jugador ha muerto y reaparecido.");
    }
}