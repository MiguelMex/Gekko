using Godot;
using System;
using System.Collections.Generic;

public partial class PantallaPrincipal : Control
{
	private List<IconButton> _botones = new();
	private int _actualIndex = 0;

	private IconButton BtnCargar;
	private IconButton BtnNuevaPartida;
	private IconButton BtnSalir;
	private IconButton BtnAjustes;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BtnNuevaPartida = GetNode<IconButton>("Texture/Container/BtnContainer/BtnNuevaPartida");
		BtnCargar = GetNode<IconButton>("Texture/Container/BtnContainer/BtnCargar");
		BtnAjustes = GetNode<IconButton>("Texture/Container/BtnContainer/BtnAjustes");
		BtnSalir = GetNode<IconButton>("Texture/Container/BtnContainer/BtnSalir");

		// BtnNuevaPartida.ButtonPressed += _OnButtonPressed;
		// BtnCargar.ButtonPressed += _OnButtonPressed;
		// BtnAjustes.ButtonPressed += _OnButtonPressed;
		// BtnSalir.ButtonPressed += _OnButtonPressed;

		//Recoger todos los botones
		var contenedor = GetNode<VBoxContainer>("Texture/Container/BtnContainer");

		foreach (var button in contenedor.GetChildren())
		{
			if(button is IconButton iconButon)
			{
				_botones.Add(iconButon);
				iconButon.FocusMode = Control.FocusModeEnum.None;
				iconButon.ButtonPressed += () => _OnButtonPressed(iconButon);
			}
		}

		_ShowButton(0);

	}

	private void _ShowButton(int index)
	{
		if(index < 0) return;
		for(int i = 0; i < _botones.Count; i++)
		{
			_botones[i].Visible = (i == index);
		}

		_actualIndex = index;
	}

	private void _OnButtonPressed(IconButton button)
	{
		var id = button.idButon;
		switch(id)
		{
			case "nueva":
				_goToScene("res://03_bajo_la_piramide.tscn");
				break;
			case "cargar":
				//TODO: Ir al menu de carga
				break;
			case "ajustes":
				//TODO: Ir al menu de configuracoón
				break;
			case "salir":
				GetTree().Quit();
				break;
			default:
				GD.Print("Error, non recognized id");
				break;
		}
	}

	private void _goToScene(string path)
	{
		LoadingScreen.targetScenePath = path;
		GetTree().ChangeSceneToFile("res://escenas/Uis/loading_screen/loading_screen.tscn");
	}

	private void _nextButton()
	{
		int next = (_actualIndex + 1) % _botones.Count;
		_ShowButton(next);
		GetViewport().SetInputAsHandled();
	}

	private void _pastButton()
	{
		int previous = (_actualIndex - 1) % _botones.Count;
		_ShowButton(previous);
		GetViewport().SetInputAsHandled();
	}

    public override void _UnhandledInput(InputEvent @event)
    {
		if (@event.IsActionPressed("down_ui"))
		{
			_nextButton();
		}
		else if (@event.IsActionPressed("up_ui"))
		{
			_pastButton();
		}
		else if (@event.IsActionPressed("accept_ui"))
		{
			_botones[_actualIndex].EmitSignal(IconButton.SignalName.ButtonPressed);
			GetViewport().SetInputAsHandled();
		}
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
