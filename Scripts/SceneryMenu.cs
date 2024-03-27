using System;
using Godot;
using Godot.Collections;

public partial class SceneryMenu : Control
{
    private enum Tab
    {
        Land,
        Background,
        Lighting
    }

    private Dictionary<Tab, Control> _tabMenus = new();
    private Tab _currentTab = Tab.Land;

    private LineEdit _widthEdit, _heightEdit;

    private CheckButton _triplanarButton;


    private Vector2 NewFloorSize => new((float)Convert.ToDouble(_widthEdit.Text), (float)Convert.ToDouble(_heightEdit.Text));

    public override void _Ready()
    {
        _tabMenus[Tab.Land] = GetNode<Control>("LandMenu");
        _tabMenus[Tab.Background] = GetNode<Control>("BgMenu");
        _tabMenus[Tab.Lighting] = GetNode<Control>("LightingMenu");

        _widthEdit = GetNode<LineEdit>("%WidthEdit");
        _heightEdit = GetNode<LineEdit>("%HeightEdit");

        _widthEdit.Text = World.FloorSize.X.ToString();
        _heightEdit.Text = World.FloorSize.Y.ToString();

        _triplanarButton = GetNode<CheckButton>("%TriplanarButton");
        _triplanarButton.ButtonPressed = World.TriplanarFloorTexture;

        ChangeTab(_currentTab);
    }

    private void ChangeTab(Tab tab)
    {
        foreach (var menu in _tabMenus.Values)
        {
            menu.Visible = false;
        }

        _tabMenus[tab].Visible = true;
        _currentTab = tab;
    }

    private void _on_tab_bar_tab_changed(int tab)
    {
        ChangeTab((Tab)tab);
    }

    private void _on_width_edit_text_changed(string newText)
    {
        World.FloorSize = NewFloorSize;
    }

    private void _on_height_edit_text_changed(string newText)
    {
        World.FloorSize = NewFloorSize;
    }

    private void _on_triplanar_button_toggled(bool toggled_on)
    {
        World.TriplanarFloorTexture = toggled_on;
    }
}
