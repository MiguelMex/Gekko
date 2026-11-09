
using Godot;

public partial class TransitionManager : CanvasLayer
{
	private AnimatedSprite2D _efecto;
	private bool _isTransitioning = false;

	private const string AnimationName = "cerrar";

	public override void _Ready()
	{
		Layer = 100;

		_efecto = GetNode<AnimatedSprite2D>("EfectoTransicion");

		_efecto.Visible = false;

		AjustarEfecto();
	}

	private void AjustarEfecto()
	{
		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;

		_efecto.Position = viewportSize / 2f;

		Texture2D textura = _efecto.SpriteFrames
			.GetFrameTexture(AnimationName, 0);

		Vector2 frameSize = textura.GetSize();

		float diagonal = Mathf.Sqrt(
			viewportSize.X * viewportSize.X +
			viewportSize.Y * viewportSize.Y
		);

		float escala = diagonal / frameSize.Y * 0.5f;

		_efecto.Scale = Vector2.One * escala;
	}

	public async void ChangeScene(string scenePath)
	{
		if (_isTransitioning)
			return;

		_isTransitioning = true;

		if (!ResourceLoader.Exists(scenePath))
		{
			GD.PrintErr($"No se encontró la escena: {scenePath}");
			_isTransitioning = false;
			return;
		}

		_efecto.Visible = true;

		// 1. CERRAR: el círculo crece y cubre la pantalla.
		_efecto.Frame = 0;
		_efecto.Play(AnimationName);

		await ToSignal(
			_efecto,
			AnimatedSprite2D.SignalName.AnimationFinished
		);

		// 2. CAMBIAR EL MAPA.
		Error error = GetTree().ChangeSceneToFile(scenePath);

		if (error != Error.Ok)
		{
			GD.PrintErr($"Error al cambiar de escena: {error}");
			await AbrirPantalla();
			_isTransitioning = false;
			return;
		}

		// Esperar a que se cargue la nueva escena.
		await ToSignal(
			GetTree(),
			SceneTree.SignalName.SceneChanged
		);

		// 3. BUSCAR AL JUGADOR Y EL SPAWNPOINT.
		Jugador jugador = GetTree()
			.GetFirstNodeInGroup("jugador") as Jugador;

		Marker2D spawnPoint = GetTree()
 		   .CurrentScene
		   .FindChild("SpawnPoint", true, false) as Marker2D;

		if (jugador != null && spawnPoint != null)
		{
			jugador.GlobalPosition = spawnPoint.GlobalPosition;
			jugador.Velocity = Vector2.Zero;

			jugador.ActualizarCheckpoint(spawnPoint.GlobalPosition);

			GD.Print("Gekko apareció en el nuevo mapa.");
		}
		else
		{
			GD.PrintErr(
                "No se encontró al jugador o al SpawnPoint."
			);
		}

		// 4. ABRIR: el círculo se reduce y revela el nuevo mapa.
		await AbrirPantalla();

		_isTransitioning = false;

		GD.Print("¡Transición completada!");
	}

	private async System.Threading.Tasks.Task AbrirPantalla()
	{
		int totalFrames = _efecto.SpriteFrames.GetFrameCount(
			AnimationName
		);

		_efecto.Frame = totalFrames - 1;
		_efecto.PlayBackwards(AnimationName);

		await ToSignal(
			_efecto,
			AnimatedSprite2D.SignalName.AnimationFinished
		);

		_efecto.Visible = false;
	}
}
