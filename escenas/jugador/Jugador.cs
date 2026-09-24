using Godot;
using System;

public partial class Jugador : CharacterBody2D
{
	public AnimatedSprite2D sprite;

    public override void _Ready()
    {
        base._Ready();
		sprite = GetNode<AnimatedSprite2D>("sprite");
		_on_no_animation_playing();
    }

	/**
	[Export] cumple la función de mostrar un valor del script en el motor grafico de GODOT
	Speed es la velocidad a la que se moverá el jugador
	*/
	[Export] public float Speed = 300.0f;

	/**
	La velocidad con la que el jugador salta
	*/
	[Export]
	public float JumpVelocity = -400.0f;

	public void _on_no_animation_playing()
	{
		sprite.Play("idle");
	}

	public void _play_walk_animation(String direction)
	{
		switch (direction)
		{
			case "left":
				sprite.Play("left");
				break;
			
			case "right":
				sprite.Play("right");
				break;

			default: return;
		}
	}

	/**
	Metodo de Godot que se ejecuta durante cada frame, sirve para manejar las fisicas
	delta: el tiempo que ah pasado desde el último frame, necesario sii trabajamos con aceleración  o para manter consistencia en tiempos incluso si el rendimiento es malo
	*/
	public override void _PhysicsProcess(double delta)
	{
		//La velocidad con la que se está movindo el jugador en cada frame
		Vector2 velocity = Velocity;

		// Añadir la gravedad
		/**
		IsOnFloor revisa si el nodo se encuentra en una superficie horizontal o si está flotando
		*/
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Manejar el salto.
		if (Input.IsActionJustPressed("jump"))
		{
			velocity.Y = JumpVelocity;
			sprite.Play("jump");
		}

		/**
		Detecta el imput enviado por el jugador, se definen en el motor grafico
		Solo se está usando izquierda y derecha así que no hay necesidad de asignar las otras dos
		*/
		Vector2 direction = Input.GetVector("left", "right","void", "void");
		if (Input.IsActionPressed("left"))
		{
			_play_walk_animation("left");
		} else if (Input.IsActionPressed("right"))
		{
			_play_walk_animation("right");
		}
		//Detecta si hubo algun input
		if (direction != Vector2.Zero)
		{
			//Cambio en la velocidad del jugador
			velocity.X = direction.X * Speed;			
		}
		else
		{
			//Que frene en seco si no hay input
			velocity.X = 0;
		}

		//Se asigna el cambio de velocidad
		Velocity = velocity;
		MoveAndSlide();
	}
}
