using System;
using System.Collections.Generic;
using Godot;

#nullable enable

/// <summary>Enemigo volador que detecta a Gekko, lo rastrea con A* y se lanza contra él.</summary>
public partial class Dron : CharacterBody2D
{
    private enum Estado { Flotando, Seguimiento, Embestida, Regreso, Explosion, Rearmado }

    [ExportGroup("Detección y A*")]
    [Export] public float DistanciaDeteccion { get; set; } = 900.0f;
    [Export] public float DuracionPersecucion { get; set; } = 8.0f;
    [Export] public float TamanoCelda { get; set; } = 32.0f;
    [Export] public float TamanoDronParaNavegacion { get; set; } = 58.0f;
    [Export] public float MargenCuadricula { get; set; } = 96.0f;
    [Export] public float IntervaloRecalculoRuta { get; set; } = 0.15f;

    [ExportGroup("Movimiento kamikaze")]
    [Export] public float VelocidadBase { get; set; } = 300.0f;
    [Export] public float DistanciaParaEmbestir { get; set; } = 270.0f;
    [Export] public float DemoraEmbestida { get; set; } = 0.35f;
    [Export] public float VelocidadMaximaEmbestida { get; set; } = 900.0f;
    [Export] public float AceleracionEmbestida { get; set; } = 900.0f;
    [Export] public float VelocidadRegreso { get; set; } = 250.0f;

    [ExportGroup("Choque y reaparición")]
    [Export] public float RadioExplosion { get; set; } = 88.0f;
    [Export] public float DuracionExplosion { get; set; } = 0.35f;
    [Export] public bool ReaparecerDespuesDelChoque { get; set; } = true;
    [Export] public float TiempoReaparicion { get; set; } = 2.0f;

    private AnimatedSprite2D _sprite = null!;
    private CollisionShape2D _hitbox = null!;
    private Area2D _areaExplosion = null!;
    private Jugador? _jugador;
    private Vector2 _posicionInicial;
    private Estado _estado = Estado.Flotando;
    private float _tiempoPersecucion;
    private float _tiempoEmbestida;
    private float _velocidadActual;
    private float _tiempoRecalculoRuta;
    private float _tiempoExplosion;
    private float _tiempoFlotacion;
    private AStarGrid2D? _cuadricula;
    private Vector2 _origenCuadricula;
    private Vector2[] _ruta = Array.Empty<Vector2>();
    private int _indiceRuta;
    private readonly HashSet<ulong> _jugadoresDanados = new();

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("sprite");
        _hitbox = GetNode<CollisionShape2D>("hitbox");
        _areaExplosion = GetNode<Area2D>("ExplosionArea");
        _posicionInicial = GlobalPosition;
        _sprite.Play("volar");
        if (_areaExplosion.GetNode<CollisionShape2D>("CollisionShape2D").Shape is CircleShape2D radio)
            radio.Radius = RadioExplosion;
        BuscarJugador();
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        BuscarJugador();

        switch (_estado)
        {
            case Estado.Flotando:
                ProcesarFlotacion(dt);
                DetectarJugador();
                break;
            case Estado.Seguimiento:
                ProcesarSeguimiento(dt);
                break;
            case Estado.Embestida:
                ProcesarEmbestida(dt);
                break;
            case Estado.Regreso:
                ProcesarRegreso(dt);
                break;
            case Estado.Explosion:
                ProcesarExplosion(dt);
                break;
            case Estado.Rearmado:
                ProcesarReaparicion(dt);
                break;
        }
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
        if (diferencia.Length() > DistanciaDeteccion || !HayLineaDeVision(_jugador)) return;

        _estado = Estado.Seguimiento;
        _tiempoPersecucion = DuracionPersecucion;
        _tiempoRecalculoRuta = 0.0f;
        _cuadricula = null;
        _sprite.Modulate = Colors.White;
        RecalcularRuta(_jugador.GlobalPosition);
    }

    private bool HayLineaDeVision(Jugador jugador)
    {
        var excluir = new Godot.Collections.Array<Rid> { GetRid() };
        var consulta = PhysicsRayQueryParameters2D.Create(GlobalPosition, jugador.GlobalPosition, CollisionMask, excluir);
        var resultado = GetWorld2D().DirectSpaceState.IntersectRay(consulta);
        return resultado.Count == 0 || resultado["collider"].AsGodotObject() == jugador;
    }

    private void ProcesarFlotacion(float delta)
    {
        _tiempoFlotacion += delta;
        Velocity = new Vector2(0, Mathf.Sin(_tiempoFlotacion * 2.0f) * 18.0f);
        MoveAndCollide(Velocity * delta);
    }

    private void ProcesarSeguimiento(float delta)
    {
        if (_jugador == null)
        {
            IniciarRegreso();
            return;
        }

        _tiempoPersecucion -= delta;
        if (_tiempoPersecucion <= 0.0f)
        {
            IniciarRegreso();
            return;
        }

        if (GlobalPosition.DistanceTo(_jugador.GlobalPosition) <= DistanciaParaEmbestir)
        {
            IniciarEmbestida();
            return;
        }

        SeguirRutaHacia(_jugador.GlobalPosition, VelocidadBase, delta);
    }

    private void IniciarEmbestida()
    {
        _estado = Estado.Embestida;
        _tiempoEmbestida = DemoraEmbestida;
        _velocidadActual = VelocidadBase;
        _tiempoRecalculoRuta = 0.0f;
        _sprite.Modulate = new Color(1.0f, 0.55f, 0.35f, 1.0f);
        _sprite.Play("embestida");
    }

    private void ProcesarEmbestida(float delta)
    {
        if (_jugador == null)
        {
            IniciarRegreso();
            return;
        }

        _tiempoPersecucion -= delta;
        if (_tiempoPersecucion <= 0.0f)
        {
            IniciarRegreso();
            return;
        }

        _tiempoEmbestida -= delta;
        _velocidadActual = _tiempoEmbestida > 0.0f
            ? VelocidadBase
            : Mathf.MoveToward(_velocidadActual, VelocidadMaximaEmbestida, AceleracionEmbestida * delta);

        Vector2 objetivo = _jugador.GlobalPosition + _jugador.Velocity * 0.12f;
        SeguirRutaHacia(objetivo, _velocidadActual, delta);
    }

    private void SeguirRutaHacia(Vector2 objetivo, float velocidad, float delta)
    {
        _tiempoRecalculoRuta -= delta;
        if (_tiempoRecalculoRuta <= 0.0f || _cuadricula == null || !DentroDeCuadricula(objetivo))
            RecalcularRuta(objetivo);

        Vector2 punto = ObtenerSiguientePunto(objetivo);
        Vector2 direccion = punto - GlobalPosition;
        if (direccion.LengthSquared() < 1.0f)
        {
            Velocity = Vector2.Zero;
            return;
        }

        direccion = direccion.Normalized();
        Velocity = direccion * velocidad;
        if (Mathf.Abs(direccion.X) > 0.05f)
            _sprite.FlipH = direccion.X < 0.0f;

        KinematicCollision2D? choque = MoveAndCollide(Velocity * delta);
        if (choque != null)
        {
            if (_estado == Estado.Seguimiento || _estado == Estado.Embestida)
                Detonar(choque.GetCollider() as Jugador);
            else
            {
                _cuadricula = null;
                _tiempoRecalculoRuta = 0.0f;
                Velocity = Vector2.Zero;
            }
        }
    }

    private void RecalcularRuta(Vector2 objetivo)
    {
        _tiempoRecalculoRuta = IntervaloRecalculoRuta;
        if (_cuadricula == null || !DentroDeCuadricula(objetivo))
            CrearCuadricula(objetivo);
        if (_cuadricula == null) return;

        Vector2I inicio = PosicionACelda(GlobalPosition);
        Vector2I destinoIdeal = PosicionACelda(objetivo);
        if (!_cuadricula.IsInBoundsv(inicio)) return;

        Vector2I destino = BuscarCeldaTransitable(destinoIdeal);
        if (destino.X < 0 || destino.Y < 0) return;

        // El dron puede estar parcialmente junto a una pared; permite salir de su celda actual.
        bool inicioEraSolido = _cuadricula.IsPointSolid(inicio);
        _cuadricula.SetPointSolid(inicio, false);
        _ruta = _cuadricula.GetPointPath(inicio, destino);
        _cuadricula.SetPointSolid(inicio, inicioEraSolido);
        _indiceRuta = _ruta.Length > 1 ? 1 : _ruta.Length;
    }

    private void CrearCuadricula(Vector2 objetivo)
    {
        float celda = Mathf.Max(16.0f, TamanoCelda);
        Vector2 minimo = new Vector2(
            Mathf.Min(GlobalPosition.X, objetivo.X) - MargenCuadricula,
            Mathf.Min(GlobalPosition.Y, objetivo.Y) - MargenCuadricula);
        Vector2 maximo = new Vector2(
            Mathf.Max(GlobalPosition.X, objetivo.X) + MargenCuadricula,
            Mathf.Max(GlobalPosition.Y, objetivo.Y) + MargenCuadricula);

        _origenCuadricula = new Vector2(Mathf.Floor(minimo.X / celda) * celda, Mathf.Floor(minimo.Y / celda) * celda);
        int ancho = Mathf.Max(2, Mathf.CeilToInt((maximo.X - _origenCuadricula.X) / celda));
        int alto = Mathf.Max(2, Mathf.CeilToInt((maximo.Y - _origenCuadricula.Y) / celda));

        _cuadricula = new AStarGrid2D
        {
            Region = new Rect2I(0, 0, ancho, alto),
            CellSize = new Vector2(celda, celda),
            Offset = _origenCuadricula + new Vector2(celda * 0.5f, celda * 0.5f),
            DiagonalMode = AStarGrid2D.DiagonalModeEnum.OnlyIfNoObstacles
        };
        _cuadricula.Update();

        // Marca celdas sólidas consultando la física con el volumen completo del dron.
        var forma = new RectangleShape2D
        {
            Size = new Vector2(TamanoDronParaNavegacion, TamanoDronParaNavegacion)
        };
        var consulta = new PhysicsShapeQueryParameters2D
        {
            Shape = forma,
            CollisionMask = CollisionMask,
            CollideWithAreas = false,
            CollideWithBodies = true,
            Exclude = new Godot.Collections.Array<Rid> { GetRid() }
        };
        if (_jugador != null)
            consulta.Exclude.Add(_jugador.GetRid());

        var espacio = GetWorld2D().DirectSpaceState;
        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                Vector2 centro = _origenCuadricula + new Vector2((x + 0.5f) * celda, (y + 0.5f) * celda);
                consulta.Transform = new Transform2D(0.0f, centro);
                if (espacio.IntersectShape(consulta, 1).Count > 0)
                    _cuadricula.SetPointSolid(new Vector2I(x, y), true);
            }
        }
    }

    private Vector2I PosicionACelda(Vector2 posicion)
    {
        float celda = Mathf.Max(16.0f, TamanoCelda);
        return new Vector2I(
            Mathf.FloorToInt((posicion.X - _origenCuadricula.X) / celda),
            Mathf.FloorToInt((posicion.Y - _origenCuadricula.Y) / celda));
    }

    private bool DentroDeCuadricula(Vector2 posicion)
    {
        if (_cuadricula == null) return false;
        return _cuadricula.IsInBoundsv(PosicionACelda(posicion));
    }

    private Vector2I BuscarCeldaTransitable(Vector2I ideal)
    {
        if (_cuadricula == null) return new Vector2I(-1, -1);
        int maximo = Mathf.CeilToInt(DistanciaParaEmbestir / Mathf.Max(16.0f, TamanoCelda));
        for (int radio = 0; radio <= maximo; radio++)
        {
            Vector2I mejor = new Vector2I(-1, -1);
            float mejorDistancia = float.MaxValue;
            for (int y = -radio; y <= radio; y++)
            {
                for (int x = -radio; x <= radio; x++)
                {
                    if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) != radio) continue;
                    var candidato = ideal + new Vector2I(x, y);
                    if (!_cuadricula.IsInBoundsv(candidato) || _cuadricula.IsPointSolid(candidato)) continue;
                    float distancia = new Vector2(x, y).LengthSquared();
                    if (distancia < mejorDistancia)
                    {
                        mejor = candidato;
                        mejorDistancia = distancia;
                    }
                }
            }

            if (mejor.X >= 0) return mejor;
        }

        return new Vector2I(-1, -1);
    }

    private Vector2 ObtenerSiguientePunto(Vector2 objetivo)
    {
        if (_ruta.Length == 0)
            return GlobalPosition;

        while (_indiceRuta < _ruta.Length && GlobalPosition.DistanceTo(_ruta[_indiceRuta]) < TamanoCelda * 0.3f)
            _indiceRuta++;

        // En el último tramo se apunta al jugador para que la colisión física concrete la embestida.
        return _indiceRuta < _ruta.Length ? _ruta[_indiceRuta] : objetivo;
    }

    private void IniciarRegreso()
    {
        if (_estado == Estado.Regreso || _estado == Estado.Explosion) return;
        _estado = Estado.Regreso;
        _sprite.Modulate = Colors.White;
        _sprite.Play("volar");
        _tiempoRecalculoRuta = 0.0f;
    }

    private void ProcesarRegreso(float delta)
    {
        if (GlobalPosition.DistanceTo(_posicionInicial) <= TamanoCelda * 0.5f)
        {
            _estado = Estado.Flotando;
            _cuadricula = null;
            _ruta = Array.Empty<Vector2>();
            Velocity = Vector2.Zero;
            return;
        }

        SeguirRutaHacia(_posicionInicial, VelocidadRegreso, delta);
    }

    private void Detonar(Jugador? jugadorImpactado)
    {
        if (_estado == Estado.Explosion) return;
        _estado = Estado.Explosion;
        Velocity = Vector2.Zero;
        _tiempoExplosion = DuracionExplosion;
        _jugadoresDanados.Clear();
        _sprite.Visible = true;
        _sprite.Modulate = Colors.White;
        _sprite.Play("explosion");
        _hitbox.SetDeferred("disabled", true);
        _areaExplosion.SetDeferred("monitoring", true);

        if (jugadorImpactado != null)
            AplicarDanio(jugadorImpactado);
    }

    private void ProcesarExplosion(float delta)
    {
        foreach (Node2D cuerpo in _areaExplosion.GetOverlappingBodies())
        {
            if (cuerpo is Jugador jugador)
                AplicarDanio(jugador);
        }

        _tiempoExplosion -= delta;
        if (_tiempoExplosion > 0.0f) return;

        if (ReaparecerDespuesDelChoque)
        {
            _areaExplosion.SetDeferred("monitoring", false);
            _sprite.Visible = false;
            GlobalPosition = _posicionInicial;
            _cuadricula = null;
            _ruta = Array.Empty<Vector2>();
            _tiempoExplosion = TiempoReaparicion;
            _estado = Estado.Rearmado;
        }
        else
        {
            QueueFree();
        }
    }

    private void ProcesarReaparicion(float delta)
    {
        _tiempoExplosion -= delta;
        if (_tiempoExplosion > 0.0f) return;

        _hitbox.SetDeferred("disabled", false);
        _sprite.Visible = true;
        _sprite.Modulate = Colors.White;
        _sprite.Play("volar");
        _tiempoFlotacion = 0.0f;
        _estado = Estado.Flotando;
    }

    private void AplicarDanio(Jugador jugador)
    {
        ulong id = jugador.GetInstanceId();
        if (_jugadoresDanados.Add(id))
            jugador.RecibirDanioEnemigo();
    }
}
