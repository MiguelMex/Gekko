extends Node2D
## Attached by YATI through the map's godot_script property.
## Tiles remain editable in Tiled; only their runtime copies become moving bodies.

const MOVIMIENTO = preload("res://scripts/PlataformaRecorrido.gd")

var tiempo_sincronizado: float = 0.0
var _duracion_ida_por_direccion: Dictionary = {}

func _physics_process(delta: float) -> void:
	tiempo_sincronizado += delta

func _ready() -> void:
	var terreno := get_node("Terreno") as TileMapLayer
	var recorridos := get_node("RecorridoPlataformas").get_children()
	# Share a clock per travel direction so paths of different lengths keep
	# the same normalized phase. The player-triggered platform stays independent.
	for nodo in recorridos:
		if not nodo is Path2D or not nodo.has_meta("platform_cells"):
			continue
		if str(nodo.get_meta("platform_mode", "ping_pong")) == "on_board_once":
			continue
		var grupo := _grupo_de_direccion(nodo as Path2D)
		var longitud := (nodo as Path2D).curve.get_baked_length()
		var velocidad := float(nodo.get_meta("platform_speed", 48.0))
		if velocidad > 0.0:
			_duracion_ida_por_direccion[grupo] = maxf(
				float(_duracion_ida_por_direccion.get(grupo, 0.0)), longitud / velocidad)
	for nodo in recorridos:
		if nodo is Path2D and nodo.has_meta("platform_cells"):
			_crear_plataforma(terreno, nodo)
	# Commit removed static collisions before the first physics tick.
	terreno.update_internals()

func _grupo_de_direccion(recorrido: Path2D) -> String:
	var longitud := recorrido.curve.get_baked_length()
	if longitud <= 0.0:
		return "sin_movimiento"
	var desplazamiento := recorrido.curve.sample_baked(longitud) - recorrido.curve.sample_baked(0.0)
	if absf(desplazamiento.y) >= absf(desplazamiento.x):
		return "arriba" if desplazamiento.y < 0.0 else "abajo"
	return "izquierda" if desplazamiento.x < 0.0 else "derecha"

func _crear_plataforma(terreno: TileMapLayer, recorrido: Path2D) -> void:
	var celdas: Array[Vector2i] = []
	for par in str(recorrido.get_meta("platform_cells")).split(";"):
		var xy := par.split(",")
		celdas.append(Vector2i(int(xy[0]), int(xy[1])))
	for celda in celdas:
		if terreno.get_cell_source_id(celda) == -1:
			push_error("Falta un tile de la plataforma: %s, %s" % [recorrido.name, celda])
			return

	var cuerpo := AnimatableBody2D.new()
	cuerpo.set_script(MOVIMIENTO)
	cuerpo.name = "Plataforma_" + str(recorrido.name)
	cuerpo.recorrido = recorrido
	cuerpo.velocidad = float(recorrido.get_meta("platform_speed", 48.0))
	cuerpo.solo_ida = str(recorrido.get_meta("platform_mode", "ping_pong")) == "on_board_once"
	if not cuerpo.solo_ida:
		var grupo := _grupo_de_direccion(recorrido)
		cuerpo.reloj_sincronizacion = self
		cuerpo.duracion_ida_sincronizada = float(_duracion_ida_por_direccion.get(grupo, 0.0))
	cuerpo.sync_to_physics = false
	var ancla := terreno.map_to_local(celdas[0])
	cuerpo.position = terreno.position + ancla
	cuerpo.collision_layer = 1
	cuerpo.collision_mask = 0

	var dibujo := TileMapLayer.new()
	dibujo.name = "TilesOriginales"
	dibujo.tile_set = terreno.tile_set
	dibujo.collision_enabled = false
	dibujo.navigation_enabled = false
	dibujo.position = -ancla
	dibujo.texture_filter = terreno.texture_filter
	dibujo.modulate = terreno.modulate
	dibujo.material = terreno.material
	cuerpo.z_index = terreno.z_index
	cuerpo.add_child(dibujo)

	for celda in celdas:
		dibujo.set_cell(celda, terreno.get_cell_source_id(celda),
			terreno.get_cell_atlas_coords(celda), terreno.get_cell_alternative_tile(celda))
		var datos := terreno.get_cell_tile_data(celda)
		var centro := terreno.map_to_local(celda) - ancla
		var tiene_colision := false
		for capa in range(terreno.tile_set.get_physics_layers_count()):
			for indice in range(datos.get_collision_polygons_count(capa)):
				var forma := CollisionPolygon2D.new()
				forma.polygon = datos.get_collision_polygon_points(capa, indice)
				forma.position = centro
				forma.one_way_collision = datos.is_collision_polygon_one_way(capa, indice)
				forma.one_way_collision_margin = datos.get_collision_polygon_one_way_margin(capa, indice)
				cuerpo.add_child(forma)
				tiene_colision = true
		# The red platform tiles (51-53) have no collision in the source tileset.
		if not tiene_colision:
			var forma := CollisionShape2D.new()
			var rectangulo := RectangleShape2D.new()
			rectangulo.size = Vector2(terreno.tile_set.tile_size)
			forma.shape = rectangulo
			forma.position = centro
			cuerpo.add_child(forma)
		terreno.erase_cell(celda)
	add_child(cuerpo)
