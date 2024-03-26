using Godot;
using System;
using System.Linq;

public partial class ModelOptions : Control
{
    enum SliderType
    {
        Position,
        Rotation,
        Scale
    }

    SliderType currentSliderType = SliderType.Position;

    bool eventDisabled = false;
    HSlider[] modelSliders;
    Label[] modelLabels, sliderLabels = new Label[3];
    Button[] restartButtons = new Button[3];
    Vector3 ChangingVector {get=>new((float)modelSliders[0].Value, (float)modelSliders[1].Value, (float)modelSliders[2].Value);}
	public override void _Ready()
    {
        modelSliders = GetTree().GetNodesInGroup("ModelSliders").Select(s => s as HSlider).ToArray();
        modelLabels = GetTree().GetNodesInGroup("ModelLabels").Select(s => s as Label).ToArray();
        

        for(int i=0; i<modelSliders.Length; i++)
        {
            int index = i;
            sliderLabels[i] =  modelSliders[i].GetChild<Label>(0);
            modelSliders[i].ValueChanged += value => {OnModelSliderValueChanged(value, index);};

            restartButtons[i] = modelSliders[i].GetParent().GetNode<Button>("Button");
            restartButtons[i].Pressed += () => OnRestartButtonPressed(index);
        }

        ChangeSliderType(currentSliderType);

    }

    private void ChangeSliderRange()
    {
        Vector2 halfFloorSize = World.FloorSize/2;
        double minValue = 0, maxValue = 0;

        switch(currentSliderType)
        {
            case SliderType.Position:
                modelSliders[0].MaxValue = halfFloorSize.X;
                modelSliders[0].MinValue = -halfFloorSize.X;

                modelSliders[1].MaxValue = 100;
                modelSliders[1].MinValue = 0;

                modelSliders[2].MaxValue = halfFloorSize.Y;
                modelSliders[2].MinValue = -halfFloorSize.Y;
            return;

            case SliderType.Rotation:
                minValue = -360;
                maxValue = 360;
            break;


            case SliderType.Scale:
                minValue = 0;
                maxValue = 10;
            break;
        }

        foreach(var slider in modelSliders)
        {
            slider.MinValue = minValue;
            slider.MaxValue = maxValue;
        }

    }

    private void ChangeLabels()
    {
        string firstWord = modelLabels[0].Text.Split(' ')[0];
        string newWord = currentSliderType == SliderType.Position ? "Posición" : currentSliderType == SliderType.Rotation ? "Rotación" : "Escala";

        foreach(Label label in modelLabels)
        {
            label.Text = label.Text.Replace(firstWord, newWord);
        }
    }

    private void ChangeSliderValues()
    {
        Vector3 values = currentSliderType == SliderType.Position ? World.GetModelPosition() : currentSliderType == SliderType.Rotation ? World.GetModelRotation() : World.GetModelScale();

        modelSliders[0].Value = values.X;
        modelSliders[1].Value = values.Y;
        modelSliders[2].Value = values.Z;

        sliderLabels[0].Text = values.X.ToString();
        sliderLabels[1].Text = values.Y.ToString();
        sliderLabels[2].Text = values.Z.ToString();
    }

    private void ChangeSliderStep()
    {
        double step = currentSliderType == SliderType.Scale ? 0.01 : 1;

        foreach(HSlider slider in modelSliders)
        {
            slider.Step = step;
        }
    }

    private void ChangeSliderType(SliderType newSliderType)
    {
        currentSliderType = newSliderType;

        eventDisabled = true;

        ChangeLabels();
        ChangeSliderStep();
        ChangeSliderRange();
        ChangeSliderValues();

        eventDisabled = false;
    }

    private void OnModelSliderValueChanged(double value, int index)
    {
        if(eventDisabled)
        {
            return;
        }

        sliderLabels[index].Text = value.ToString();

/*         switch(currentSliderType)
        {
            case SliderType.Position:
                World.SetModelPosition(ChangingVector);
            break;

            case SliderType.Rotation:
                World.SetModelRotation(ChangingVector);
            break;

            case SliderType.Scale:
                World.SetModelScale(ChangingVector);
            break;
        } */

        World world = new();
        world.Call($"SetModel{currentSliderType}", ChangingVector);

    }

    private void OnRestartButtonPressed(int index)
    {
        float value = currentSliderType == SliderType.Scale ? 1 : 0;

        modelSliders[index].Value = value;
    }

    private void _on_tab_bar_tab_changed(int tab)
    {
        ChangeSliderType((SliderType)tab);
    }
}
