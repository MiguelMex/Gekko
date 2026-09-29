using Godot;
using System;

public partial class Shockwave : Area2D
{
	[Export]
	public float radious = 0;
	[Export]
	public float damage = 0;
	[Export]
	public float baseRadious = 10f;

	//The nodes
	private CollisionShape2D hitbox;
	private AnimatedSprite2D sprite;

	public void _setAttributes(float r, float d)
	{
		radious = r;
		damage = d;

		if(hitbox == null) hitbox = hitbox = GetNode<CollisionShape2D>("CollisionShape2D");
		if(sprite == null) sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		if(hitbox.Shape != null)
		{
			hitbox.Shape = hitbox.Shape.Duplicate() as Shape2D;
		}

		if(hitbox.Shape is CircleShape2D circle)
		{
			circle.Radius = radious;
		}

		float scaleFactor = radious / baseRadious;
		sprite.Scale = Vector2.One * scaleFactor;
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		hitbox = GetNode<CollisionShape2D>("CollisionShape2D");
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		this.sprite.Play("explosion");

		sprite.AnimationFinished += () => QueueFree();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
