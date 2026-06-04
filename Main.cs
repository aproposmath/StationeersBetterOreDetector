namespace BetterOreDetector;

using System;

using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

[BepInPlugin(ThisModInfo.ModID, ThisModInfo.AssemblyName, ThisModInfo.Version)]
public class BetterOreDetectorPlugin : BaseUnityPlugin
{
    public const string PluginGuid = ThisModInfo.ModID;
    public const string PluginName = ThisModInfo.AssemblyName;
    public const string PluginVersion = ThisModInfo.Version;
    private Harmony _harmony;

    public static ConfigEntry<bool> OreCompass;

    private void Awake()
    {
        try
        {
            L.SetLogger(this.Logger);
            L.Info($"Awake {ThisModInfo.Info}");

            OreCompass = Config.Bind(
                "General",
                "OreCompass",
                true,
                "Directional indicators instead of distance based"
            );

            _harmony = new Harmony(ThisModInfo.ModID);
            _harmony.PatchAll();
        }
        catch (Exception ex)
        {
            L.Error($"Error during init of {ThisModInfo.Info}: {ex}");
        }
    }

    private void OnDestroy()
    {
#if DEBUG
        if (!ModUtils.IsLoadedByScriptEngine(typeof(BetterOreDetectorPlugin)))
            return;
        L.Info($"OnDestroy of ${ThisModInfo.Info}, cleaning up patches");
        _harmony.UnpatchSelf();
#endif
    }
}
