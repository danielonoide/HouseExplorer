using Godot;
using System;

public partial class Joystick : Area2D
{
	float radius;
	Sprite2D bigCircle, smallCircle;


	int fingerIndex = 0;
	public int FingerIndex {get=>fingerIndex;}

	public override void _Ready()
	{
		var circleShape2D = (CircleShape2D)GetNode<CollisionShape2D>("CollisionShape2D").Shape;
		radius = circleShape2D.Radius;
		bigCircle = GetNode<Sprite2D>("BigCircle");
		smallCircle = GetNode<Sprite2D>("BigCircle/SmallCircle");
	}

	public override void _Process(double delta)
	{
	}

	public Vector2 GetDirection()
	{
		return Vector2.Zero.DirectionTo(smallCircle.Position);
	}

	public void _on_input_event(Node viewport, InputEvent @event, int shape_idx)
	{
		if(@event is InputEventScreenDrag screenDrag)
		{
			smallCircle.Position = ToLocal(screenDrag.Position);
		}

		if(@event is InputEventScreenTouch screenTouch)
		{
			fingerIndex = screenTouch.Index;
			if(screenTouch.Pressed)
			{
				smallCircle.Position = ToLocal(screenTouch.Position);
			}
			else
			{
				smallCircle.Position = Vector2.Zero;
			}
		}

	}

	public void _on_mouse_exited()
	{
		smallCircle.Position = Vector2.Zero;
	}



	

}
