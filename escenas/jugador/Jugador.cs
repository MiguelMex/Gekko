using Godot;
using System;

public partial class Jugador : CharacterBody2D
{
    [Export] public float Speed = 300.0f;
    [Export] public float JumpVelocity = -400.0f;

    // NUEVO: Variable para guardar la posición de reaparición actual
    private Vector2 _respawnPosition;

    public override void _Ready()
    {
        // NUEVO: Al iniciar el juego, guardamos su posición inicial como el primer respawn por defecto
        _respawnPosition = GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        Vector2 direction = Input.GetVector("left", "right", "void", "void");
        if (direction != Vector2.Zero && IsOnFloor())
        {
            velocity.X = direction.X * Speed;
        }
        else
        {
            velocity.X = 0;
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