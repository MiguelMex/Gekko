using Godot;
using System;

public partial class PantallaPrincipal : Control
{
	private Button BtnCargar;
	private Button BtnNuevaPartida;
	private Button BtnSalir;
	private Button BtnAjustes;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BtnNuevaPartida = GetNode<Button>("Texture/CenterContainer/VContainer/BtnNuevaPartida/Container/Button");
		BtnCargar = GetNode<Button>("Texture/CenterContainer/VContainer/BtnCargar/Container/Button");
		BtnAjustes = GetNode<Button>("Texture/CenterContainer/VContainer/BtnAjustes/Container/Button");
		BtnSalir = GetNode<Button>("Texture/CenterContainer/VContainer/BtnSalir/Container/Button");

		BtnNuevaPartida.Pressed += _NuevaPartida;
		BtnCargar.Pressed += _Cargar;
		BtnAjustes.Pressed += _Ajustes;
		BtnSalir.Pressed += _Salir;
	}

	private void _NuevaPartida(){}

	private void _Cargar(){}

	private void _Ajustes(){}

	private void _Salir(){}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
