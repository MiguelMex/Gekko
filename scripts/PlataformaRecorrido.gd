extends AnimatableBody2D
## Moves the entire body, including its original tile artwork and collision.

var recorrido: Path2D
var velocidad: float = 48.0 # Map pixels/second, before the level's scale.
var solo_ida: bool = false
var _activada: bool = false
var _reinicio_pendiente: bool = false
var _avance: float = 0.0
var _longitud: float
var _inicio: Vector2
var _origen_recorrido: Vector2

func _ready() -> void:
	_inicio = position
	_longitud = recorrido.curve.get_baked_length()
	_origen_recorrido = _punto_en_mapa(0.0)
	sync_to_physics = true
	if solo_ida:
		add_to_group("plataformas_reiniciables")

func activar_por_jugador() -> void:
	_activada = true

func reiniciar() -> void:
	_activada = false
	_avance = 0.0
	_reinicio_pendiente = true

func _punto_en_mapa(distancia: float) -> Vector2:
	return get_parent().to_local(recorrido.to_global(recorrido.curve.sample_baked(distancia)))

func _physics_process(delta: float) -> void:
	if _reinicio_pendiente:
		# Apply the respawn transform on the physics tick, after prior motion.
		position = _inicio
		reset_physics_interpolation()
		_reinicio_pendiente = false
		return
	if _longitud <= 0.0:
		return
	if solo_ida:
		if not _activada:
			return
		_avance += velocidad * delta
		var punto := _punto_en_mapa(minf(_avance, _longitud))
		if _avance > _longitud:
			# Continue in the final segment's direction: never stop or turn back.
			var fin := recorrido.curve.get_point_position(recorrido.curve.point_count - 1)
			var anterior := recorrido.curve.get_point_position(recorrido.curve.point_count - 2)
			var prolongacion := fin + (fin - anterior).normalized() * (_avance - _longitud)
			punto = get_parent().to_local(recorrido.to_global(prolongacion))
		position = _inicio + punto - _origen_recorrido
		return
	_avance = fposmod(_avance + velocidad * delta, 2.0 * _longitud)
	var distancia := _longitud - absf(_longitud - _avance)
	position = _inicio + _punto_en_mapa(distancia) - _origen_recorrido
