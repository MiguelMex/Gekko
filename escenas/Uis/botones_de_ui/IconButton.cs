using Godot;
using System;

public partial class IconButton : MarginContainer
{
	// Called when the node enters the scene tree for the first time.
	[Signal] public delegate void ButtonPressedEventHandler();
	[Export] public string idButon;
	[Export] public Texture2D IconRight { get; set; }
	[Export] public Texture2D IconLeft { get; set; }
	[Export] public string TextButton { get; set; } = "Boton";

	[Export] public int marginValue = 10;

	private TextureRect _iconoIzquierdo;
	private TextureRect _iconoDerecho;
	private Label _texto;
	private Button _button;
	public override void _Ready()
	{
		// This code sample assumes the current script is extending MarginContainer.
		AddThemeConstantOverride("margin_top", marginValue);
		AddThemeConstantOverride("margin_left", marginValue);
		AddThemeConstantOverride("margin_bottom", marginValue);
		AddThemeConstantOverride("margin_right", marginValue);


		//La información del botón
		_iconoIzquierdo = GetNode<TextureRect>("Button/Contenido/IconoIzquierdo");
		_iconoDerecho = GetNode<TextureRect>("Button/Contenido/IconoDerecho");
		_texto = GetNode<Label>("Button/Contenido/Texto");

		if(IconLeft != null) _iconoIzquierdo.Texture = IconLeft;
		if(IconRight != null) _iconoDerecho.Texture = IconRight;
		_texto.Text = TextButton;

		_button =  GetNode<Button>("Button");
		_button.Pressed += () => EmitSignal(SignalName.ButtonPressed, idButon);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
