using System.Collections.Generic;
using Source.Core;

namespace Source.Engine
{
	// --- 1. Character Data Records ---
	public record RaceData(int RaceStr, int RaceInt);
	public record ClassData(int ClassHealth, int ClassMana, int ClassStr, int ClassInt, string PrimarySkill);
	public record SkillData(string AttributeScale);

	// --- 2. Context for FormulaProcessor ---
	public record FormulaContext(
		EntityStats Stats, 
		ClassData? Class = null, 
		RaceData? Race = null, 
		int WeaponDamage = 0,
		Dictionary<string, float>? ExtraParams = null
	);

	// --- 3. Attribute/Modifier Records ---
	public record ModifierDto(string Target, float Value);

	// --- 4. Component Data Structures ---
	public record GrantedComponentDto(
		string Tag, 
		List<ModifierDto>? Modifiers,
		Dictionary<string, string>? Properties
	);

	// --- 5. Main Item Record ---
	public record ItemData(
		string Name, 
		string? Slot, 
		List<GrantedComponentDto> GrantedComponents
	);

	// --- 6. Constants ---
	public static class EngineConfig
	{
		public const int MaxItemCapacity = 1024;
		public const int MaxEntities = 1024;

		// Grid settings
    public const int CellSize = 64;
    public const int ScreenWidth = 1920;
    public const int ScreenHeight = 1080;
    
    // Automatically calculate grid dimensions
    public const int GridWidth = ScreenWidth / CellSize;
    public const int GridHeight = ScreenHeight / CellSize;
	}

	public static class EntityMasks
  {
      // These values identify the bit-flags corresponding to your current pointer ranges
      public const int ITEM_MASK       = 0x000; // 0-255 (No high bits set)
      public const int NPC_MASK        = 0x100; // 256 (Bit 8 set)
      public const int PROJECTILE_MASK = 0x200; // 512 (Bit 9 set)
      
      // Mask to isolate only the type bits (ignoring the 0-255 index bits)
      public const int TYPE_MASK       = ITEM_MASK | NPC_MASK | PROJECTILE_MASK;
  }
}
