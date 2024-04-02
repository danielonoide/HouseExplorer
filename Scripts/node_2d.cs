using Godot;
using System;

public partial class node_2d : Node2D
{
    Sprite2D center, arrow;
    HSlider slider;
    Label sliderLabel;
    public override void _Ready()
    {
        center = GetNode<Sprite2D>("Center");
        arrow = GetNode<Sprite2D>("Arrow");
        slider = GetNode<HSlider>("Control/HSlider");
        sliderLabel = slider.GetChild<Label>(0);

        float timeDegrees = Mathf.RadToDeg(arrow.Rotation);
        timeDegrees += 270;

        float currTime = timeDegrees/360 *24;
        slider.Value = currTime;

        GD.Print("Degrees: ", timeDegrees);


/*         Vector2 pos = arrow.Position;
        Vector2 centerPos = center.Position;

        float degrees = 165-180f;

        pos = pos.Rotated(Mathf.DegToRad(degrees));
        arrow.Position = pos;
        arrow.Rotate(Mathf.DegToRad(degrees)); */


/*         degrees = 180-165f;

        pos = pos.Rotated(Mathf.DegToRad(degrees));
        arrow.Position = pos;
        arrow.Rotate(Mathf.DegToRad(degrees)); */


    }

    private void _on_h_slider_value_changed(float militaryTime)
    {

        float time = militaryTime > 12 ? militaryTime % 12 : militaryTime;

        string abbrev = militaryTime >= 12 ? "pm" : "am";

        if(time == 0)
        {
            time = 12;
        }

        sliderLabel.Text = $"{time} {abbrev}";  
        float degrees = (militaryTime/24) * 360;
        degrees -= 270;
        GD.Print("Degrees: ", degrees);
        ChangeTime(degrees);
    }

    private void ChangeTime(float degrees)
    {
/*         float currRotation = arrow.Rotation;
        float diff = radians - currRotation; */

        float currRotation = arrow.RotationDegrees;
        float diff = degrees - currRotation;

        Vector2 newPos = arrow.Position.Rotated(Mathf.DegToRad(diff));
        arrow.Position = newPos;

        //arrow.Rotate(Mat);
        arrow.RotationDegrees = degrees; 

        GD.Print("New pos: ", newPos);
        GD.Print("Light rotation: ", Mathf.RadToDeg(arrow.Rotation));
    }


    

}
