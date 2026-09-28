using Godot;
using System;

public partial class Checkpoint : Area2D
{
    private bool _isActivated = false;
    private AnimatedSprite2D _animSprite; // Cambiamos Sprite2D por AnimatedSprite2D

    public override void _Ready()
{
    // Buscamos el nodo AnimatedSprite2D
    _animSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");

    if (_animSprite != null)
    {
        // Ponemos opacidad inicial al 40%
        Color color = _animSprite.Modulate;
        color.A = 0.4f; 
        _animSprite.Modulate = color;

        // INICIA LA ANIMACIÓN AUTOMÁTICAMENTE
        _animSprite.Play("default"); // Asegúrate de que "default" sea el nombre de tu animación en el SpriteFrames
    }

    // Conectamos la señal de colisión
    BodyEntered += OnBodyEntered;
}

    private void OnBodyEntered(Node2D body)
    {
        if (body is Jugador jugador && !_isActivated)
        {
            _isActivated = true;
            jugador.ActualizarCheckpoint(GlobalPosition);

            if (_animSprite != null)
            {
                // Iluminamos por completo al 100% de opacidad
                Color color = _animSprite.Modulate;
                color.A = 1.0f;
                _animSprite.Modulate = color;

                // Opcional: Cambiar a la animación de movimiento si la creaste
                // _animSprite.Play("activo");
            }

            GD.Print("¡Checkpoint activado!");
        }
    }
}