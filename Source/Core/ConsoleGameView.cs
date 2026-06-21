using System;
using Source.Core.Interfaces;
using Source.Core.Contracts;

namespace Source.Core;

public class ConsoleGameView : IEngineFacade, IGameView
{
	public void DrawMesh(int id, Source.Core.Math.Transform2D transform) { }
	public bool IsActionPressed(string action) => false;
	public void Render(in CharacterSheetDto data)
	{
#if GODOT
		// Inside Godot Editor/Game
		Godot.GD.Print($"--- Character Sheet: {data.Name} ---");
		Godot.GD.Print($"Weapon: {data.Weapon}");
		// ... etc
#else
		// Inside pure Console App
		Console.WriteLine($"--- Character Sheet: {data.Name} ---");
		Console.WriteLine($"Weapon: {data.Weapon}");
		Console.WriteLine($"Skill: {data.Skill}");
		Console.WriteLine($"Health: {data.Health} | Mana: {data.Mana}");
		Console.WriteLine($"Strength: {data.Strength} | Intelligence: {data.Intelligence}");
#endif
	}
}
