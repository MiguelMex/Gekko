using System.Collections.Generic;

/// <summary>
/// Secuencia oficial de niveles del juego:
/// Nivel_Inicio (.tmx envuelto en nivelInicio.tscn) -> testing_player (.tscn) -> cueva_1 (.tmx envuelto en test_cueva.tscn).
/// Cualquier cosa que necesite saber "qué nivel es" o "cuál sigue" debe usar esta clase.
/// </summary>
public static class Niveles
{
	public record Nivel(string Nombre, string RutaEscena);

	public static readonly IReadOnlyList<Nivel> Secuencia = new List<Nivel>
	{
		new("Nivel_Inicio",   "res://escenas/niveles/nivelInicio.tscn"),
		new("testing_player", "res://escenas/niveles/testing_player.tscn"),
		new("cueva_1",        "res://escenas/niveles/test_cueva.tscn"),
	};

	// Escena a la que se pasa al terminar el último nivel.
	public const string EscenaFinal = "res://escenas/Uis/creditos/creditos.tscn";

	public static string RutaInicial => Secuencia[0].RutaEscena;

	/// <summary>Índice del nivel dentro de la secuencia (-1 si la ruta no es un nivel).</summary>
	public static int IndiceDe(string rutaEscena)
	{
		for (int i = 0; i < Secuencia.Count; i++)
			if (Secuencia[i].RutaEscena == rutaEscena) return i;
		return -1;
	}

	public static string NombreDe(string rutaEscena)
	{
		int i = IndiceDe(rutaEscena);
		return i >= 0 ? Secuencia[i].Nombre : "Desconocido";
	}

	/// <summary>Escena del siguiente nivel; si ya es el último, la escena final.</summary>
	public static string SiguienteDe(string rutaEscena)
	{
		int i = IndiceDe(rutaEscena);
		return (i >= 0 && i + 1 < Secuencia.Count) ? Secuencia[i + 1].RutaEscena : EscenaFinal;
	}

	/// <summary>
	/// Escena jugable para una carpeta de mapas/niveles/ (selector de niveles).
	/// Las carpetas con un .tmx suelto no tienen jugador, así que se usa la escena envoltorio.
	/// </summary>
	public static string RutaPorCarpeta(string carpeta) => carpeta switch
	{
		"Desierto" => Secuencia[0].RutaEscena,
		"Nivel 2"  => Secuencia[1].RutaEscena,
		"Cueva"    => Secuencia[2].RutaEscena,
		_ => null,
	};
}
