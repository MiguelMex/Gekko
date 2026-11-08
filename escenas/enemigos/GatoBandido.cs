using Godot;

#nullable enable

/// <summary>Enemigo terrestre que busca un ángulo despejado y dispara en cualquier dirección.</summary>
public partial class GatoBandido : CharacterBody2D
{
    private enum Estado { Patrulla, Combate }

    [ExportGroup("Percepción")]
    [Export] public float DistanciaDeteccion { get; set; } = 1400.0f;
    [Export] public float TiempoPerderVista { get; set; } = 1.2f;

    [ExportGroup("Posicionamiento")]
    [Export] public float VelocidadPatrulla { get; set; } = 65.0f;
    [Export] public float VelocidadPersecucion { get; set; } = 240.0f;
    [Export] public float DistanciaPatrulla { get; set; } = 110.0f;
    [Export] public float DistanciaMinimaDisparo { get; set; } = 185.0f;
    [Export] public float DistanciaMaximaDisparo { get; set; } = 900.0f;
    [Export] public float AltoRaycastSuelo { get; set; } = 74.0f;

    [ExportGroup("Arma")]
    [Export] public float CadenciaDisparo { get; set; } = 0.72f;
    [Export] public float VelocidadBala { get; set; } = 950.0f;
    [Export] public float AnticipacionBala { get; set; } = 0.16f;
    [Export] public float ToleranciaDisparoHorizontal { get; set; } = 24.0f;
    [Export] public PackedScene EscenaBala { get; set; } = null!;

    private AnimatedSprite2D _sprite = null!;
    private Marker2D _bocaArma = null!;
    private Jugador? _jugador;
    private Estado _estado = Estado.Patrulla;
    private Vector2 _posicionInicial;
    private Vector2 _ultimaPosicionJugador;
    private float _direccionPatrulla = 1.0f;
    private float _tiempoSinVision;
    private float _tiempoParaDisparo;
    private float _tiempoAnimacionDisparo;
    private string _animacionDisparoActual = "disparo_x";
    private int _frameDisparoActual = 4;
    private Vector2 _origenDisparoPendiente;
    private Vector2 _direccionDisparoPendiente;
    private bool _disparoPendiente;
    private Rect2 _limitesMapa;
    private bool _tieneLimitesMapa;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("sprite");
        _sprite.FrameChanged += OnSpriteFrameChanged;
        _bocaArma = GetNode<Marker2D>("Muzzle");
        GetNode<Area2D>("DamageArea").BodyEntered += OnDamageAreaBodyEntered;
        _posicionInicial = GlobalPosition;
        _sprite.Play("caminar");
        _tiempoParaDisparo = 0.25f;
        CalcularLimitesMapa();
        BuscarJugador();
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        BuscarJugador();
        _tiempoParaDisparo = Mathf.Max(0.0f, _tiempoParaDisparo - dt);
        _tiempoAnimacionDisparo = Mathf.Max(0.0f, _tiempoAnimacionDisparo - dt);

        Vector2 velocidad = Velocity;
        velocidad.Y = IsOnFloor() ? 0.0f : velocidad.Y + GetGravity().Y * dt;

        switch (_estado)
        {
            case Estado.Patrulla:
                ProcesarPatrulla(ref velocidad);
                DetectarJugador();
                break;
            case Estado.Combate:
                ProcesarCombate(ref velocidad, dt);
                break;
        }

        Velocity = velocidad;
        MoveAndSlide();
        ActualizarAnimacion(velocidad.X);
    }

    private void BuscarJugador()
    {
        if (_jugador != null && GodotObject.IsInstanceValid(_jugador)) return;
        _jugador = GetTree().GetFirstNodeInGroup("player") as Jugador;
    }

    private void DetectarJugador()
    {
        if (_jugador == null || GlobalPosition.DistanceTo(_jugador.GlobalPosition) > DistanciaDeteccion)
            return;
        if (!HayLineaDeVision(_jugador)) return;

        _estado = Estado.Combate;
        _tiempoSinVision = 0.0f;
        _ultimaPosicionJugador = _jugador.GlobalPosition;
        _tiempoParaDisparo = 0.0f;
    }

    private void ProcesarPatrulla(ref Vector2 velocidad)
    {
        float limite = _posicionInicial.X + _direccionPatrulla * DistanciaPatrulla;
        if (Mathf.Abs(GlobalPosition.X - limite) < 5.0f || !HaySueloDelante(_direccionPatrulla))
            _direccionPatrulla *= -1.0f;

        velocidad.X = _direccionPatrulla * VelocidadPatrulla;
    }

    private void ProcesarCombate(ref Vector2 velocidad, float delta)
    {
        if (_jugador == null)
        {
            VolverAPatrulla();
            velocidad.X = 0.0f;
            return;
        }

        Vector2 diferencia = _jugador.GlobalPosition - GlobalPosition;
        bool visible = diferencia.Length() <= DistanciaDeteccion && HayLineaDeVision(_jugador);
        if (visible)
        {
            _tiempoSinVision = 0.0f;
            _ultimaPosicionJugador = _jugador.GlobalPosition;
        }
        else
        {
            _tiempoSinVision += delta;
            if (_tiempoSinVision >= TiempoPerderVista)
            {
                VolverAPatrulla();
                velocidad.X = 0.0f;
                return;
            }
        }

        Vector2 origenBala = ObtenerOrigenBala();
        Vector2 objetivo = ObtenerCentroJugador(_jugador);
        float distancia = origenBala.DistanceTo(objetivo);
        bool enRango = distancia <= DistanciaMaximaDisparo;
        bool demasiadoCerca = distancia < DistanciaMinimaDisparo;
        bool trayectoriaDespejada = visible && HayTrayectoriaDeDisparo(_jugador, origenBala);

        if (enRango && !demasiadoCerca && trayectoriaDespejada)
        {
            // Puede disparar arriba, abajo o en diagonal; solo necesita un trayecto libre hasta Gekko.
            velocidad.X = 0.0f;
            ActualizarOrientacion(diferencia.X);
            if (_tiempoParaDisparo <= 0.0f)
                Disparar(_jugador, origenBala);
            return;
        }

        Vector2 destinoMovimiento = visible ? _jugador.GlobalPosition : _ultimaPosicionJugador;
        float diferenciaX = destinoMovimiento.X - GlobalPosition.X;
        float direccion;
        if (demasiadoCerca)
            direccion = Mathf.Abs(diferenciaX) > 20.0f ? -Mathf.Sign(diferenciaX) : (_sprite.FlipH ? 1.0f : -1.0f);
        else if (Mathf.Abs(diferenciaX) > 28.0f)
            direccion = Mathf.Sign(diferenciaX);
        else
            // Si Gekko está justo encima, el gato se desplaza lateralmente para abrir un ángulo diagonal.
            direccion = _sprite.FlipH ? -1.0f : 1.0f;

        if (HaySueloDelante(direccion))
        {
            velocidad.X = direccion * VelocidadPersecucion;
            ActualizarOrientacion(direccion);
        }
        else
        {
            velocidad.X = 0.0f;
            // Prueba el otro lado si el borde o un desnivel bloquea el avance.
            float alternativa = -direccion;
            if (HaySueloDelante(alternativa))
            {
                velocidad.X = alternativa * VelocidadPersecucion;
                ActualizarOrientacion(alternativa);
            }
        }
    }

    private bool HayLineaDeVision(Jugador jugador)
    {
        Vector2 desde = GlobalPosition + new Vector2(0.0f, -92.0f);
        Vector2 hasta = ObtenerCentroJugador(jugador);
        var excluir = new Godot.Collections.Array<Rid> { GetRid() };
        var consulta = PhysicsRayQueryParameters2D.Create(desde, hasta, CollisionMask, excluir);
        var resultado = GetWorld2D().DirectSpaceState.IntersectRay(consulta);
        return resultado.Count == 0 || resultado["collider"].AsGodotObject() == jugador;
    }

    private bool HayTrayectoriaDeDisparo(Jugador jugador, Vector2 origen)
    {
        var excluir = new Godot.Collections.Array<Rid> { GetRid() };
        var consulta = PhysicsRayQueryParameters2D.Create(origen, ObtenerCentroJugador(jugador), CollisionMask, excluir);
        var resultado = GetWorld2D().DirectSpaceState.IntersectRay(consulta);
        return resultado.Count == 0 || resultado["collider"].AsGodotObject() == jugador;
    }

    private bool HaySueloDelante(float direccion)
    {
        if (direccion == 0.0f) return true;
        float ancho = 28.0f;
        var hitbox = GetNodeOrNull<CollisionShape2D>("hitbox");
        if (hitbox?.Shape is RectangleShape2D rect)
            ancho = rect.Size.X * hitbox.GlobalScale.X * 0.5f + 5.0f;

        Vector2 origen = GlobalPosition + new Vector2(direccion * ancho, -4.0f);
        Vector2 destino = origen + new Vector2(0.0f, AltoRaycastSuelo);
        var consulta = PhysicsRayQueryParameters2D.Create(origen, destino, CollisionMask, new Godot.Collections.Array<Rid> { GetRid() });
        return GetWorld2D().DirectSpaceState.IntersectRay(consulta).Count > 0;
    }

    private Vector2 ObtenerOrigenBala()
    {
        float x = _sprite.FlipH ? -Mathf.Abs(_bocaArma.Position.X) : Mathf.Abs(_bocaArma.Position.X);
        return GlobalPosition + new Vector2(x, _bocaArma.Position.Y);
    }

    private static Vector2 ObtenerCentroJugador(Jugador jugador)
    {
        var hitbox = jugador.GetNodeOrNull<CollisionShape2D>("hitbox");
        return hitbox?.GlobalPosition ?? jugador.GlobalPosition;
    }

    private void Disparar(Jugador jugador, Vector2 origen)
    {
        if (EscenaBala == null) return;

        Vector2 centroJugador = ObtenerCentroJugador(jugador);
        Vector2 objetivo = Mathf.Abs(centroJugador.Y - origen.Y) <= ToleranciaDisparoHorizontal
            ? new Vector2(centroJugador.X + jugador.Velocity.X * AnticipacionBala, origen.Y)
            : centroJugador + jugador.Velocity * AnticipacionBala;
        Vector2 direccion = (objetivo - origen).Normalized();
        if (direccion.LengthSquared() < 0.01f)
            direccion = _sprite.FlipH ? Vector2.Left : Vector2.Right;

        var bala = EscenaBala.Instantiate<BalaGato>();
        bala.Velocidad = VelocidadBala;
        bala.Configurar(direccion, _limitesMapa, _tieneLimitesMapa);
        GetTree().CurrentScene.AddChild(bala);
        bala.GlobalPosition = origen;

        ActualizarOrientacion(direccion.X);
        float absX = Mathf.Abs(direccion.X);
        float absY = Mathf.Abs(direccion.Y);
        _animacionDisparoActual = absY < 0.35f
            ? "disparo_x"
            : absX < 0.35f ? "disparo_y" : "disparo_xy";
        _frameDisparoActual = _animacionDisparoActual == "disparo_xy" ? 3 : 4;
        _origenDisparoPendiente = origen;
        _direccionDisparoPendiente = direccion;
        _disparoPendiente = true;
        _tiempoParaDisparo = CadenciaDisparo;
        _tiempoAnimacionDisparo = 0.58f;
        _sprite.Play(_animacionDisparoActual);
    }

    private void OnSpriteFrameChanged()
    {
        if (!_disparoPendiente || _sprite.Frame != _frameDisparoActual) return;

        var bala = EscenaBala.Instantiate<BalaGato>();
        bala.Velocidad = VelocidadBala;
        bala.Configurar(_direccionDisparoPendiente, _limitesMapa, _tieneLimitesMapa);
        GetTree().CurrentScene.AddChild(bala);
        bala.GlobalPosition = _origenDisparoPendiente;
        _disparoPendiente = false;
    }

    private void CalcularLimitesMapa()
    {
        Node? escena = GetTree().CurrentScene;
        if (escena == null) return;

        foreach (Node nodo in escena.FindChildren("*", "TileMapLayer", true, false))
        {
            if (nodo is not TileMapLayer capa || capa.TileSet == null) continue;
            Rect2I usadas = capa.GetUsedRect();
            if (usadas.Size.X <= 0 || usadas.Size.Y <= 0) continue;

            Vector2 tamanoCelda = new(capa.TileSet.TileSize.X, capa.TileSet.TileSize.Y);
            Vector2 minLocal = capa.MapToLocal(usadas.Position) - tamanoCelda * 0.5f;
            Vector2 maxLocal = capa.MapToLocal(usadas.End) - tamanoCelda * 0.5f;
            Vector2[] esquinas =
            {
                capa.ToGlobal(minLocal),
                capa.ToGlobal(new Vector2(maxLocal.X, minLocal.Y)),
                capa.ToGlobal(maxLocal),
                capa.ToGlobal(new Vector2(minLocal.X, maxLocal.Y))
            };

            float minX = esquinas[0].X, maxX = esquinas[0].X;
            float minY = esquinas[0].Y, maxY = esquinas[0].Y;
            foreach (Vector2 punto in esquinas)
            {
                minX = Mathf.Min(minX, punto.X);
                maxX = Mathf.Max(maxX, punto.X);
                minY = Mathf.Min(minY, punto.Y);
                maxY = Mathf.Max(maxY, punto.Y);
            }

            var rectangulo = new Rect2(minX, minY, maxX - minX, maxY - minY);
            _limitesMapa = _tieneLimitesMapa ? _limitesMapa.Merge(rectangulo) : rectangulo;
            _tieneLimitesMapa = true;
        }
    }

    private void OnDamageAreaBodyEntered(Node2D cuerpo)
    {
        if (cuerpo is Jugador jugador)
            jugador.RecibirDanioEnemigo();
    }

    private void VolverAPatrulla()
    {
        _estado = Estado.Patrulla;
        _tiempoSinVision = 0.0f;
        _direccionPatrulla = Mathf.Sign(_posicionInicial.X - GlobalPosition.X);
        if (_direccionPatrulla == 0.0f) _direccionPatrulla = 1.0f;
    }

    private void ActualizarOrientacion(float direccion)
    {
        if (Mathf.Abs(direccion) > 0.05f)
            _sprite.FlipH = direccion < 0.0f;
    }

    private void ActualizarAnimacion(float velocidadX)
    {
        if (_tiempoAnimacionDisparo > 0.0f)
        {
            if (_sprite.Animation != _animacionDisparoActual)
                _sprite.Play(_animacionDisparoActual);
            return;
        }

        string animacion = Mathf.Abs(velocidadX) > 3.0f ? "caminar" : "apuntar";
        if (_sprite.Animation != animacion || !_sprite.IsPlaying())
            _sprite.Play(animacion);
    }
}
