using Godot;

#nullable enable

/// <summary>Proyectil del Gato Bandido: solo desaparece al impactar o salir del mapa.</summary>
public partial class BalaGato : CharacterBody2D
{
    [Export] public float Velocidad { get; set; } = 950.0f;

    private Vector2 _direccion = Vector2.Right;
    private Rect2 _limitesMapa;
    private bool _tieneLimitesMapa;
    private Sprite2D _sprite = null!;

    public void Configurar(Vector2 direccion, Rect2 limitesMapa, bool tieneLimitesMapa)
    {
        _direccion = direccion.Normalized();
        _limitesMapa = limitesMapa;
        _tieneLimitesMapa = tieneLimitesMapa;
    }

    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("sprite");
        _sprite.Rotation = _direccion.Angle();
    }

    public override void _PhysicsProcess(double delta)
    {
        KinematicCollision2D? impacto = MoveAndCollide(_direccion * Velocidad * (float)delta);
        if (impacto != null)
        {
            if (impacto.GetCollider() is Jugador jugador)
                jugador.RecibirDanioEnemigo();
            QueueFree();
            return;
        }

        if (_tieneLimitesMapa && !_limitesMapa.HasPoint(GlobalPosition))
            QueueFree();
    }
}
