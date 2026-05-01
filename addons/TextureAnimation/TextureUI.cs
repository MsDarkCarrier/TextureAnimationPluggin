#if TOOLS
using Godot;
using System;

[Tool]
public partial class TextureUI : EditorPlugin
{
    public override void _EnterTree() => AddCustomType("TextureAnimation", "TextureRect", GD.Load<CSharpScript>("res://addons/TextureAnimation/TextureAnimation.cs"), GD.Load<Texture2D>("res://addons/TextureAnimation/LogoPluggin.png"));
    public override void _ExitTree() => RemoveCustomType("TextureAnimation");

}
#endif
