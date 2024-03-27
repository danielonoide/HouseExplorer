using Godot;
using System;

public partial class node_2d : Node2D
{
    Sprite2D center, arrow;
    public override void _Ready()
    {
        center = GetNode<Sprite2D>("Center");
        arrow = GetNode<Sprite2D>("Arrow");


        Vector2 pos = arrow.Position;
        Vector2 centerPos = center.Position;

        float degrees = 180f;

        GD.Print("Distancia antes: ", pos.DistanceTo(centerPos));
        pos = pos.Rotated(Mathf.DegToRad(degrees));
        GD.Print("Distancia después: ", pos.DistanceTo(centerPos));

        arrow.Position = pos;
        arrow.Rotate(Mathf.DegToRad(degrees));
    }



}
