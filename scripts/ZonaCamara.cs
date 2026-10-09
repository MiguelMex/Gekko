using Godot;

public partial class ZonaCamara : Area2D
{
    // Exportamos los nuevos límites que tendrá la cámara al cruzar esta zona
    [Export] private int nuevoLimiteIzquierda = 11000;
    [Export] private int nuevoLimiteDerecha = 37200;
    [Export] private int nuevoLimiteArriba = -700;
    [Export] private int nuevoLimiteAbajo = 3900;	

    public override void _Ready()
    {
        // Conectamos la señal de cuando el jugador entra a esta zona
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        // Verificamos si quien entró es el jugador
        if (body is Jugador jugador)
        {
            // 1. DISPARAR EL GUARDADO AUTOMÁTICO
            // Puedes guardar su posición actual o la de esta zona de sección
            SaveManager.SaveGame(jugador.GlobalPosition, level: 1, hasKey: false);
            GD.Print("¡Autoguardado por sección de mapa realizado!");

            // 2. CAMBIAR LOS LÍMITES DE LA CÁMARA
            // Buscamos la cámara que tiene el jugador (asumiendo que cuelga de él)
            var camara = jugador.GetNodeOrNull<Camera2D>("Camera2D");
            if (camara != null)
            {
                camara.LimitLeft = nuevoLimiteIzquierda;
                camara.LimitRight = nuevoLimiteDerecha;
                camara.LimitTop = nuevoLimiteArriba;
                camara.LimitBottom = nuevoLimiteAbajo;
                
                GD.Print("¡Límites de la cámara actualizados para la nueva sección!");
            }
            
            // Opcional: Desactivar el área para que solo guarde y mueva la cámara la primera vez que pasa
            // QueueFree(); 
        }
    }
}