using Source.Core;
using Source.Engine;
using Source.Core.Interfaces;
using System.IO;
using Godot;

namespace DataDriven.Scripts;

public partial class CoreBootstrapper : Node
{
    private EngineDriver? _driver;

    public override void _Ready()
    {
        // Wiring the bridge: Inject the Godot implementation into the agnostic Source class
        DebugLog.Initialize(new GodotLogger());
				// DebugLog.SetEnabled(false);  // Uncomment to disable

        GD.Print("--- BOOTSTRAPPER READY ---");
        
        try
        {
            // 2. Setup Services
            GD.Print("CoreBootstrapper: Setting up services...");
            var itemDatabase = new ItemData[EngineConfig.MaxEntityCapacity];
            var service = new GodotService(GetViewport());
            EngineFacade.Implementation = service;

            // 3. Resolve Data Paths
            string dataPath = ResolveDataPath();
            GD.Print($"[DEBUG] Initializing Engine with Data Path: {dataPath}");

            // 4. Initialize Engine
            GD.Print("CoreBootstrapper: Attempting to instantiate EngineDriver...");
            _driver = new EngineDriver(service, itemDatabase, dataPath);
            GD.Print("CoreBootstrapper: EngineDriver instantiated successfully.");

            // 5. Load Data
            GD.Print("CoreBootstrapper: Attempting to load npc_blueprint.json...");
            _driver.LoadGameData("npc_blueprint.json");
            
            GD.Print("[SUCCESS] Engine Bootstrapped Successfully.");
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"[CRITICAL] Engine failed to initialize: {e.Message}");
            GD.PrintErr($"Stack Trace: {e.StackTrace}");
        }
    }

    public override void _Process(double delta)
    {
        _driver?.Tick((float)delta);
    }

    private string ResolveDataPath()
    {
        string godotRoot = ProjectSettings.GlobalizePath("res://");
        string repoRoot = Path.GetDirectoryName(godotRoot.TrimEnd('/'))!;
        return Path.Combine(repoRoot, "Source", "Data");
    }
}
