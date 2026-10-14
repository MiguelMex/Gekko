extends SceneTree
## Run: Godot --headless --path <project> --script res://scripts/VerificarPlataformas.gd

func _initialize() -> void:
	call_deferred("_verificar")

func _verificar() -> void:
	var mapa = load("res://mapas/niveles/Desierto/Desierto_vacio.tmx").instantiate()
	mapa.position = Vector2(9, 7)
	mapa.scale = Vector2(6, 6)
	var terreno: TileMapLayer = mapa.get_node("Terreno")
	var originales := {}
	for celda in terreno.get_used_cells():
		originales[celda] = [terreno.get_cell_source_id(celda), terreno.get_cell_atlas_coords(celda), terreno.get_cell_alternative_tile(celda)]
	root.add_child(mapa)
	var plataformas: Array[AnimatableBody2D] = []
	var posiciones := {}
	var total_tiles := 0
	assert(mapa.get_node("RecorridoPlataformas").get_child_count() == 15)
	for ruta in mapa.get_node("RecorridoPlataformas").get_children():
		assert(ruta is Path2D)
		assert(ruta.get_child_count() == 0, "Una ruta conserva colisiones")
	for nodo in mapa.get_children():
		if nodo is AnimatableBody2D:
			plataformas.append(nodo)
			posiciones[nodo] = nodo.position
			var dibujo: TileMapLayer = nodo.get_node("TilesOriginales")
			assert(not dibujo.collision_enabled)
			for celda in dibujo.get_used_cells():
				assert(terreno.get_cell_source_id(celda) == -1, "Queda suelo fijo bajo una plataforma")
				assert(originales[celda] == [dibujo.get_cell_source_id(celda), dibujo.get_cell_atlas_coords(celda), dibujo.get_cell_alternative_tile(celda)])
				assert(dibujo.to_global(dibujo.map_to_local(celda)).is_equal_approx(terreno.to_global(terreno.map_to_local(celda))), "Cambio de posicion visual inicial")
				total_tiles += 1
	assert(plataformas.size() == 15)
	assert(total_tiles == 53)
	assert(terreno.get_used_cells().size() == originales.size() - 53)
	# A real physics rider verifies that the moving collision carries a character.
	var plataforma := plataformas[6] # Horizontal route.
	var viajero := CharacterBody2D.new()
	var forma := CollisionShape2D.new()
	var rectangulo := RectangleShape2D.new()
	rectangulo.size = Vector2(48, 96)
	forma.shape = rectangulo
	viajero.add_child(forma)
	root.add_child(viajero)
	viajero.global_position = plataforma.global_position + Vector2(96, -96)
	var inicial_viajero := viajero.global_position
	var contacto := false
	for paso in range(240):
		await physics_frame
		viajero.velocity = Vector2(0, 980)
		viajero.move_and_slide()
		contacto = contacto or viajero.is_on_floor()
		await process_frame
	for cuerpo in plataformas:
		if cuerpo.solo_ida:
			assert(cuerpo.position.is_equal_approx(posiciones[cuerpo]), "La plataforma larga arranca sola")
		else:
			assert(cuerpo.position.distance_to(posiciones[cuerpo]) > 1.0, "Plataforma inmovil")
	assert(contacto, "El personaje no aterriza en la plataforma")
	assert(viajero.global_position.x > inicial_viajero.x + 100, "La plataforma no transporta al personaje")
	# Use the actual C# player to exercise boarding and the death/respawn hook.
	var larga = plataformas.filter(func(p): return p.solo_ida)[0]
	var jugador = load("res://escenas/jugador/jugador.tscn").instantiate()
	root.add_child(jugador)
	jugador.global_position = larga.global_position + Vector2(96, -180)
	for paso in range(120):
		await physics_frame
		await process_frame
	assert(larga._activada, "El jugador real no activa la plataforma al subir")
	jugador.set_physics_process(false)
	jugador.global_position = Vector2(-1000, -1000)
	var avance_al_bajar: float = larga._avance
	for paso in range(30):
		await physics_frame
		await process_frame
	assert(larga._avance > avance_al_bajar, "Se detiene al bajar del jugador")
	# Fast-forward through the endpoint, then restore the regular speed.
	larga.velocidad = larga._longitud * 120.0
	for paso in range(3):
		await physics_frame
		await process_frame
	assert(larga._avance > larga._longitud)
	var despues_del_final: Vector2 = larga.position
	await physics_frame
	await process_frame
	assert(larga.position.x > despues_del_final.x, "Se detiene o regresa al final")
	larga.velocidad = 48.0
	jugador.Morir()
	for paso in range(3):
		await physics_frame
		await process_frame
	assert(not larga._activada and larga._avance == 0.0)
	assert(larga.position.is_equal_approx(posiciones[larga]), "No se reinicia al morir")
	jugador.queue_free()
	print("PASS: apariencia y colisiones; transporte; espera al jugador; arranque al subir; avanza sin pasajero y sin regreso; reinicio al morir.")
	mapa.queue_free()
	viajero.queue_free()
	await process_frame
	quit()
