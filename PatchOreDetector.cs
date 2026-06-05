namespace BetterOreDetector;

using HarmonyLib;
using UnityEngine;

using Objects.Items;
using TerrainSystem;
using Assets.Scripts.Objects.Items;

// Show angle distance in lights instead of actual distance
// the beeps are unchanged
// change the color of unlit status lights to black for better contrast with the red lit ones

[HarmonyPatch]
static class PatchOreDetector
{
    static void ResetMaterials(OreDetector detector)
    {
        var newMat = new Material(detector.SignalInactiveMaterial);
        newMat.color = Color.black;
        detector.SignalInactiveMaterial = newMat;
        for (int i = 0; i < detector.signalStrengthIndicators.Length; i++)
            detector.indicatorStates[i] = OreDetector.IndicatorState.On;
        detector.ResetSignalStrength();
    }

    static void DimBrightnessIfHelmetLight(OreDetector detector)
    {
        MeshRenderer screen = detector.Screen;
        var mat = new Material(screen.material);

        var human = detector.RootParentHuman;
        var factor = 1.0f;
        if (!human.IsUnresponsive && !human.IsSleeping && human.HelmetSlot.Contains<IWearableLight>(out var light) && light.OnOff)
            factor = 0.5f;
        screen.material.color = factor * Color.white;
    }

    [HarmonyPatch(typeof(OreDetector)), HarmonyPatch(nameof(OreDetector.UpdateMaterials)), HarmonyPrefix]
    private static void PrefixUpdateMaterials(OreDetector __instance, ref float distance)
    {
        if (__instance == null)
            return;

        if (__instance.SignalInactiveMaterial.color != Color.black)
            ResetMaterials(__instance);

        DimBrightnessIfHelmetLight(__instance);

        var human = __instance.RootParentHuman;
        if (human == null)
            return;

        if (!BetterOreDetectorPlugin.EnableOreCompass.Value)
            return;

        var position = human.HeadBone.position;
        var forward = human.AimIk.position - position;

        var range = __instance._range;
        Vein nearestVeinOfType = Vein.GetNearestVeinOfType(position, range, __instance.TrackedMinableType);
        if (nearestVeinOfType == null)
            return;

        var veinIndex = nearestVeinOfType.GetClosestActiveIndex(position);
        var minable = nearestVeinOfType._minables[veinIndex];
        var veinPos = minable.WorldPositionInt(nearestVeinOfType.VeinWorldPosition);
        float num = Vector3.Distance(position, veinPos);

        if (num > range)
            return;

        Vector3 toVein = (veinPos - position).normalized;
        float angle = Mathf.Min(Vector3.Angle(forward, toVein), 120f) / 120f;
        float dist = 1.0f - Mathf.Pow(1.0f - angle, 1.5f); // put a bit more accuracy into close angles
        float angDist = Mathf.Clamp(dist, 0.0f, 1.0f - 0.4f / __instance.signalStrengthIndicators.Length); // show at least one (blinking) light when a vein is in range
        distance = range * angDist;
    }
}