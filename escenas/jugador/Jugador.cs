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

    private Vector2 previousPosition;

    private int _consecutiveJump = 0;

	/**
	La velocidad con la que el jugador salta
	*/
	[Export]
	public float JumpVelocity = -400.0f;

    // NUEVO: Variable para guardar la posición de reaparición actual
    private Vector2 _respawnPosition;

    //Velocidad acumulada
    [Export]
    public float speedGrowRate = 0.01f;
    [Export]
    public float maxSpeedStored = 4.0f;
    [Export]
    public float speedStored = 1;
    [Export]
    public PackedScene shockwave;

    AnimatedSprite2D _sprite;
    double _timeIdle;

    //TODO: Haz que la velocidad se manteenga sin importar la dirección en la que se mueve el jugador
    //TODO: Que el jugador salga impulsado hacia adelante tras una explosión
    //TODO: Que el jugador se deslize hacia adelante si sale disparado mientras se agacha

    public float facing;
    private float _knockbackTime = 0f;
    [Export]
    public float knockbackDuration = 0.25f;

    public bool is_crouching = false;

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

    public void _manageCruch()
    {
        if (is_crouching)
        {
            _sprite.Play("crouch");
        } else
        {
            _sprite.Play("stand_up");
        }
    }

    public void _animateWalk(bool sprinting)
    {
        if(facing < 0){
            _sprite.FlipH = true;
        } else if (facing > 0)
        {
            _sprite.FlipH = false;
        }

        if(IsOnFloor()) _sprite.Play("run");
        _sprite.SpeedScale = sprinting ? 2 : 1;
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

        if (Input.IsActionPressed("down") && IsOnFloor())
        {
            is_crouching = true;
            _manageCruch();
        }
        if(Input.IsActionJustReleased("down") && is_crouching)
        {
            is_crouching = false;
            _manageCruch();
        }

        //No se detecta ningún input horizontal durante el knocback
        if(_knockbackTime > 0)
        {
            _knockbackTime -= (float)delta;
            Velocity = velocity;
            MoveAndSlide();
            return;
        }

		/**
		Detecta el imput enviado por el jugador, se definen en el motor grafico
		Solo se está usando izquierda y derecha así que no hay necesidad de asignar las otras dos
		*/
        facing = Input.GetAxis("left","right");
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
            speedStored = Mathf.Min(speedStored + speedGrowRate, maxSpeedStored);
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
		}

        GD.Print("Velocidad acumulada: "+speedStored);
        Velocity = velocity;
        MoveAndSlide();
        _detectHorizontalCollision(direction);
    }

    public void _applyKnockback(Vector2 direction, float force)
    {
        Velocity =  new Vector2(
            direction.X * force,
            -force * 0.5f
        );
        _knockbackTime = knockbackDuration;
    }

    //Metodo para detectar colisiones con paredes
    public void _detectHorizontalCollision(Vector2 inputDirection)
    {
        if(inputDirection.X == 0) return;

        if(!IsOnWall()) return;

        var collision = GetLastSlideCollision();
        if(collision == null) return;

        Vector2 normal = collision.GetNormal();

        //La normal apunta en direccion a la pared
        //Si el jugador se mueve a la derecha (x>0) y la pared empuja a la izquiera (normal.X<0), chocan de frene
        if(Mathf.Sign(normal.X) == -Math.Sign(inputDirection.X))
        {
            //Calculo del daño y area de la onda expansiva
            var radious = 50 * speedStored;
            var damage  = 2 * speedStored;
            GD.Print("Daño: "+damage+", radio: "+radious);
            if (shockwave != null && speedStored >= 1)
            {
                var wave = shockwave.Instantiate<Shockwave>();
                GetParent().AddChild(wave);
                wave.GlobalPosition = GlobalPosition;
                wave._setAttributes(radious, damage);
            }
            speedStored = 1;
        }
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