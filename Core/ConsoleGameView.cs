using System;
using Core.Interfaces;

namespace Core;

public class ConsoleGameView : IGameView
{
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
