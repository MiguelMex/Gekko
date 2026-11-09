using Godot;
using System;

public partial class BloqueRompible : Node2D
{
	[Export] public int nivelDaño = 0;

	private int _dañoActual;

	private AnimatedSprite2D sprite;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("sprite");
		sprite.Stop();
		_dañoActual = nivelDaño <= 3 ? nivelDaño : 3; //Para no desbordar el sprite
		CambiarSprite(_dañoActual);
	}

	private void CambiarSprite(int indice)
	{
		sprite.SetFrameAndProgress(indice, 0);
		GD.Print("Vida restante: "+(4-indice));
	}

	private void EliminarNodo()
	{
		GD.Print("Nodo eliminado");
		this.QueueFree();
	}

	private void ImpactoRecibido(int fuerza)
	{
		//El valor maximo de daño es 12
		//Lo dividimos entre 3 para que sea compatible con los 4 estados de daño
		int fuerzaUsable = fuerza / 3;
		int dañoTotal = _dañoActual + fuerzaUsable;
		
		//Si el daño actual es >= 4 simplemente se elimina el nodo
		//Si el daño no es suficiente se revisa si debe pasar al siguiente estado
		if(dañoTotal >= 4)
		{
			sprite.Play("explosion");
		} 
		else if (fuerzaUsable > _dañoActual)
		{
			//Cambia al siguiente estado (0-4)
			_dañoActual = fuerzaUsable;
			GD.Print("Daño actual: "+_dañoActual);
			CambiarSprite(fuerzaUsable);
		}

	}

	// private void BodyEntered(Node2D body)
	// {
	// 	if(body is Shockwave shockwave)
	// 	{
	// 		GD.Print("Activado");
	// 		GD.Print("Cuerpo registrado: "+shockwave.damage);
	// 		ImpactoRecibido((int)shockwave.damage);
	// 	}
	// }

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
