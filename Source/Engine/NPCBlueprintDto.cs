namespace Source.Engine;

public class NPCBlueprintDto
{
    public int EntityId { get; set; }
    public required string Name { get; set; }
    public required string Race { get; set; }
    public required string Class { get; set; }
    public int EquippedItemId { get; set; }
}
