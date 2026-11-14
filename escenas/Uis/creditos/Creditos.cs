using Godot;
using System;

public partial class Creditos : Control
{
	// Integrantes del equipo y su rol. Editar aquí para cambiar la lista.
	private static readonly (string Nombre, string Rol)[] Equipo =
	{
		("Miguel Ángel Diaz Méndez", "Desarrollador y Scrum Master"),
		("Karla Jael Macías Contreras", "Desarrolladora y Product Owner"),
		("Miguel Ortiz Ibarra", "Desarrollador y Product Owner"),
		("Tomás Noe Espíndola Najera", "Desarrollador"),
		("Carlos Arturo Retana", "Desarrollador"),
		("Eliers Rodolfo García Martínez", "Desarrollador"),
		("Octavio Piña", "Docente invitado"),
	};

	private const string MenuPrincipal = "res://escenas/Uis/pantalla_principal/pantalla_principal.tscn";

	private VBoxContainer _lista;
	private HBoxContainer _btnVolver;

	public override void _Ready()
	{
		_lista = GetNode<VBoxContainer>("Margen/Contenido/Scroll/Lista");
		_btnVolver = GetNode<HBoxContainer>("Margen/Contenido/Pie/BtnVolver");

		// Clic sobre "Volver"
		_btnVolver.GuiInput += (@event) =>
		{
			if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
				_Volver();
		};

		var fuenteNombre = GD.Load<FontFile>("res://recursos/fonts/PixeloidSans-Bold.ttf");
		var fuenteRol = GD.Load<FontFile>("res://recursos/fonts/PixeloidSans.ttf");

		foreach (var (nombre, rol) in Equipo)
		{
			_lista.AddChild(CrearLabel(nombre, fuenteNombre, 30));
			_lista.AddChild(CrearLabel(rol, fuenteRol, 20, new Color(0.85f, 0.85f, 0.85f)));

			// Separación entre integrantes
			_lista.AddChild(new Control { CustomMinimumSize = new Vector2(0, 18) });
		}
	}

	private static Label CrearLabel(string texto, Font fuente, int tamano, Color? color = null)
	{
		return new Label
		{
			Text = texto,
			HorizontalAlignment = HorizontalAlignment.Center,
			LabelSettings = new LabelSettings
			{
				Font = fuente,
				FontSize = tamano,
				FontColor = color ?? Colors.White,
				ShadowSize = 8,
				ShadowColor = new Color(0, 0, 0, 0.67f),
				ShadowOffset = new Vector2(1, 3),
			},
		};
	}

	private void _Volver()
	{
		GetTree().ChangeSceneToFile(MenuPrincipal);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		// Esc (acción "pause" o "ui_cancel") regresa al menú principal
		if (@event.IsActionPressed("ui_cancel") || @event.IsActionPressed("pause"))
		{
			GetViewport().SetInputAsHandled();
			_Volver();
		}
	}
}
