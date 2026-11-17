using Godot;

public partial class FondoDinamico : CanvasLayer
{
	[Export] public double DuracionTransicion = 0.6;
	[Export] public string TipoInicial = "";

	// Tipo de suelo -> imagen de fondo (se llena en el Inspector)
	[Export] public Godot.Collections.Dictionary<string, Texture2D> Fondos { get; set; } = new();

	private TextureRect _actual;
	private TextureRect _nuevo;
	private Tween _tween;
	private string _tipoActual = "";

	public override void _Ready()
	{
		AddToGroup("fondo");
		_actual = GetNode<TextureRect>("FondoActual");
		_nuevo = GetNode<TextureRect>("FondoNuevo");
		_nuevo.Modulate = new Color(1, 1, 1, 0);

		if (Fondos.TryGetValue(TipoInicial, out Texture2D inicial))
		{
			_actual.Texture = inicial;
			_tipoActual = TipoInicial;
		}
	}

	public void CambiarFondo(string tipo)
	{
		if (tipo == _tipoActual) return;
		if (!Fondos.TryGetValue(tipo, out Texture2D textura)) return;   // tipo sin fondo asignado
		_tipoActual = tipo;

		// Si había una transición a medias, la damos por terminada
		if (_tween != null && _tween.IsRunning())
		{
			_tween.Kill();
			_actual.Texture = _nuevo.Texture;
		}

		_nuevo.Texture = textura;
		_nuevo.Modulate = new Color(1, 1, 1, 0);

		_tween = CreateTween();
		_tween.TweenProperty(_nuevo, "modulate:a", 1.0f, DuracionTransicion);
		_tween.TweenCallback(Callable.From(() =>
		{
			_actual.Texture = textura;
			_nuevo.Modulate = new Color(1, 1, 1, 0);
		}));
	}
}
