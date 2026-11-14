using Godot;
using System;

public partial class BloqueRompible : Node2D
{
	[Export] public int nivelDaño = 0;

	private int _dañoActual;
	private bool _destruido = false;
	private StaticBody2D _collision;
	private CollisionShape2D _shapeCollision;
	private Area2D _hitbox;
	private CollisionShape2D _shapeHitbox;

	private AnimatedSprite2D sprite;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_collision = GetNode<StaticBody2D>("collision");
		_shapeCollision = _collision.GetNode<CollisionShape2D>("CollisionShape2D");

		_hitbox = GetNode<Area2D>("hitbox");
        _shapeHitbox = _hitbox.GetNode<CollisionShape2D>("CollisionShape2D");

		sprite = GetNode<AnimatedSprite2D>("sprite");
		sprite.Stop();
		_dañoActual = nivelDaño <= 3 ? nivelDaño : 3; //Para no desbordar el sprite
		CambiarSprite(_dañoActual);
	}

	private void reiniciar()
	{
		_destruido = false;
		Visible = true;
		_shapeCollision.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		_shapeHitbox.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		_dañoActual = nivelDaño <= 3 ? nivelDaño : 3;
		sprite.Play("breaking");
		sprite.Pause();
		CambiarSprite(_dañoActual);
	}

	private void CambiarSprite(int indice)
	{
		sprite.SetFrameAndProgress(indice, 0);
		GD.Print("Vida restante: "+(4-indice));
	}

	private void EliminarNodo()
	{
		GD.Print("Nodo oculto");
		_destruido = true;
		Visible = false;

		_shapeCollision.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		_shapeHitbox.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
	}

	private void ImpactoRecibido(int fuerza)
	{
		if (_destruido) return;
		//El valor maximo de daño es 12
		//Lo dividimos entre 3 para que sea compatible con los 4 estados de daño
		int fuerzaUsable = fuerza / 3;
		int dañoTotal = _dañoActual + fuerzaUsable;
		
		//Si el daño actual es >= 4 simplemente se elimina el nodo
		//Si el daño no es suficiente se revisa si debe pasar al siguiente estado
		if(dañoTotal >= 4)
		{
			sprite.Play("explosion");
			sprite.AnimationFinished += OnExplosionFinished;
		} 
		else if (fuerzaUsable > _dañoActual)
		{
			//Cambia al siguiente estado (0-4)
			_dañoActual = fuerzaUsable;
			GD.Print("Daño actual: "+_dañoActual);
			CambiarSprite(fuerzaUsable);
		}

	}

	private void OnExplosionFinished()
	{
		sprite.AnimationFinished -= OnExplosionFinished; //Evitar doble conexión
		EliminarNodo();
	}

	private void AreaEntered(Area2D area)
	{
		if(area is Shockwave shockwave)
		{
			GD.Print("Activado");
			GD.Print("Area registrada: "+shockwave.damage);
			ImpactoRecibido((int)shockwave.damage);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
