using Godot;
using System;

public partial class KillZone : Area2D
{
    // Este método se conecta automáticamente a la señal de Godot cuando algo entra al Area2D
    public override void _Ready()
    {
        // Conectamos la señal BodyEntered por código de forma limpia y segura
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        // Verificamos si el objeto que entró al área es nuestro Jugador
        if (body is Jugador jugador)
        {
            // Llamamos al método Morir que creamos antes en el Jugador
            jugador.Morir();
        }
    }
}