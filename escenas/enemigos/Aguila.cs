using Godot;

#nullable enable

/// <summary>Enemigo terrestre que patrulla, detecta al jugador y lo persigue.</summary>
public partial class Aguila : CharacterBody2D
{
    private enum Estado { Patrulla, Persecucion, Regreso }

    [ExportGroup("Movimiento")]
    [Export] public float VelocidadPatrulla { get; set; } = 75.0f;
    [Export] public float VelocidadPersecucion { get; set; } = 145.0f;
    [Export] public float DistanciaPatrulla { get; set; } = 230.0f;
    [Export] public float DistanciaDeteccion { get; set; } = 800.0f;
    [Export] public float DistanciaMaximaPersecucion { get; set; } = 900.0f;
    [Export] public float DistanciaPerdida { get; set; } = 1000.0f;

    [ExportGroup("Detección y retorno")]
    [Export] public float DiferenciaVerticalMaxima { get; set; } = 180.0f;
    [Export] public float TiempoSinAlcance { get; set; } = 2.5f;
    [Export] public float TiempoParaPerderVista { get; set; } = 0.65f;
    [Export] public float VelocidadRegreso { get; set; } = 95.0f;

    [ExportGroup("Colisión")]
    [Export] public float AltoRaycastSuelo { get; set; } = 48.0f;
    [Export] public float SeparacionRaycastFrente { get; set; } = 19.0f;

    private AnimatedSprite2D _sprite = null!;
    private Jugador? _jugador;
    private Vector2 _posicionInicial;
    private Estado _estado = Estado.Patrulla;
    private float _direccionPatrulla = 1.0f;
    private float _tiempoFueraDeAlcance;
    private float _tiempoSinVista;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("sprite");
        _posicionInicial = GlobalPosition;
        _sprite.Play("patrulla");
        BuscarJugador();
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        BuscarJugador();
        Vector2 velocidad = Velocity;
        if (!IsOnFloor())
            velocidad.Y += GetGravity().Y * dt;
        else
            velocidad.Y = 0;

        switch (_estado)
        {
            case Estado.Patrulla:
                ProcesarPatrulla(ref velocidad);
                DetectarJugador();
                break;
            case Estado.Persecucion:
                ProcesarPersecucion(ref velocidad, dt);
                break;
            case Estado.Regreso:
                ProcesarRegreso(ref velocidad);
                break;
        }

        Velocity = velocidad;
        MoveAndSlide();
    }

    private void BuscarJugador()
    {
        if (_jugador != null && GodotObject.IsInstanceValid(_jugador)) return;
        _jugador = GetTree().GetFirstNodeInGroup("player") as Jugador;
    }

    private void DetectarJugador()
    {
        if (_jugador == null) return;
        Vector2 diferencia = _jugador.GlobalPosition - GlobalPosition;
        if (Mathf.Abs(diferencia.X) > DistanciaDeteccion ||
            Mathf.Abs(ObtenerPiesJugador(_jugador) - GlobalPosition.Y) > DiferenciaVerticalMaxima ||
            !HayLineaDeVision(_jugador)) return;

        _estado = Estado.Persecucion;
        _tiempoFueraDeAlcance = 0;
        _tiempoSinVista = 0;
        _sprite.Play("deteccion");
    }

    private void ProcesarPersecucion(ref Vector2 velocidad, float delta)
    {
        if (_jugador == null)
        {
            IniciarRegreso();
            velocidad.X = 0;
            return;
        }

        float diferenciaX = _jugador.GlobalPosition.X - GlobalPosition.X;
        float diferenciaY = ObtenerPiesJugador(_jugador) - GlobalPosition.Y;
        bool puedeAlcanzar = Mathf.Abs(diferenciaY) <= DiferenciaVerticalMaxima &&
                             Mathf.Abs(diferenciaX) <= DistanciaMaximaPersecucion &&
                             HaySueloDelante(Mathf.Sign(diferenciaX));

        if (!puedeAlcanzar)
            _tiempoFueraDeAlcance += delta;
        else
            _tiempoFueraDeAlcance = 0;

        if (Mathf.Abs(diferenciaX) > DistanciaPerdida)
            _tiempoSinVista += delta;
        else if (HayLineaDeVision(_jugador))
            _tiempoSinVista = 0;
        else
            _tiempoSinVista += delta;

        if (_tiempoFueraDeAlcance >= TiempoSinAlcance || _tiempoSinVista >= TiempoParaPerderVista)
        {
            IniciarRegreso();
            velocidad.X = 0;
            return;
        }

        // Sin saltos: si el jugador está en otro nivel o detrás de un borde, espera un poco y regresa.
        if (!puedeAlcanzar || !HaySueloDelante(Mathf.Sign(diferenciaX)))
        {
            velocidad.X = 0;
            return;
        }

        _direccionPatrulla = Mathf.Sign(diferenciaX);
        velocidad.X = _direccionPatrulla * VelocidadPersecucion;
        ActualizarOrientacion(_direccionPatrulla);
        if (_sprite.Animation != "deteccion" || !_sprite.IsPlaying())
            _sprite.Play("correr");
    }

    private void ProcesarPatrulla(ref Vector2 velocidad)
    {
        float limite = _posicionInicial.X + _direccionPatrulla * DistanciaPatrulla;
        bool llegoAlLimite = Mathf.Abs(GlobalPosition.X - limite) < 4.0f;
        if (llegoAlLimite || !HaySueloDelante(_direccionPatrulla))
            _direccionPatrulla *= -1.0f;

        velocidad.X = _direccionPatrulla * VelocidadPatrulla;
        ActualizarOrientacion(_direccionPatrulla);
        if (_sprite.Animation != "patrulla") _sprite.Play("patrulla");
    }

    private void ProcesarRegreso(ref Vector2 velocidad)
    {
        float distancia = _posicionInicial.X - GlobalPosition.X;
        if (Mathf.Abs(distancia) < 7.0f || !HaySueloDelante(Mathf.Sign(distancia)))
        {
            _estado = Estado.Patrulla;
            _direccionPatrulla = distancia == 0 ? _direccionPatrulla : Mathf.Sign(distancia);
            velocidad.X = 0;
            _sprite.Play("patrulla");
            return;
        }

        float direccion = Mathf.Sign(distancia);
        velocidad.X = direccion * VelocidadRegreso;
        ActualizarOrientacion(direccion);
        if (_sprite.Animation != "correr") _sprite.Play("correr");
    }

    private float ObtenerPiesJugador(Jugador jugador)
    {
        var hitbox = jugador.GetNodeOrNull<CollisionShape2D>("hitbox");
        if (hitbox?.Shape is RectangleShape2D rect)
            return (hitbox.GlobalPosition + new Vector2(0, rect.Size.Y * 0.5f)).Y;

        return jugador.GlobalPosition.Y;
    }

    private bool HaySueloDelante(float direccion)
    {
        if (direccion == 0) return true;

        // Coloca la muestra fuera del ancho del enemigo y empieza por encima de sus pies.
        float distanciaFrontal = SeparacionRaycastFrente;
        var hitbox = GetNodeOrNull<CollisionShape2D>("hitbox");
        if (hitbox?.Shape is RectangleShape2D rect)
            distanciaFrontal = Mathf.Max(distanciaFrontal, rect.Size.X * hitbox.GlobalScale.X * 0.5f + 4.0f);

        Vector2 origen = GlobalPosition + new Vector2(direccion * distanciaFrontal, -4.0f);
        Vector2 destino = GlobalPosition + new Vector2(direccion * distanciaFrontal, AltoRaycastSuelo);
        var consulta = PhysicsRayQueryParameters2D.Create(origen, destino, CollisionMask, new Godot.Collections.Array<Rid> { GetRid() });
        return GetWorld2D().DirectSpaceState.IntersectRay(consulta).Count > 0;
    }

    private bool HayLineaDeVision(Jugador jugador)
    {
        Vector2 origen = GlobalPosition + new Vector2(0, -24);
        Vector2 destino = jugador.GlobalPosition + new Vector2(0, -20);
        var consulta = PhysicsRayQueryParameters2D.Create(origen, destino, CollisionMask, new Godot.Collections.Array<Rid> { GetRid() });
        var resultado = GetWorld2D().DirectSpaceState.IntersectRay(consulta);
        return resultado.Count == 0 || resultado["collider"].AsGodotObject() == jugador;
    }

    private void IniciarRegreso()
    {
        _estado = Estado.Regreso;
        _tiempoFueraDeAlcance = 0;
        _tiempoSinVista = 0;
    }

    private void ActualizarOrientacion(float direccion)
    {
        if (direccion != 0) _sprite.FlipH = direccion < 0;
    }
}
