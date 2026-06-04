namespace BetterOreDetector;

using HarmonyLib;
using UnityEngine;

using Objects.Items;
using TerrainSystem;
using Assets.Scripts;

// Show angle distance in lights instead of actual distance
// the beeps are unchanged
// change the color of unlit status lights to black for better contrast with the red lit ones

[HarmonyPatch]
static class PatchOreDetector
{
    [HarmonyPatch(typeof(OreDetector)), HarmonyPatch(nameof(OreDetector.UpdateMaterials)), HarmonyPrefix]
    private static void PrefixUpdateMaterials(OreDetector __instance, ref float distance)
    {
        __instance.SignalInactiveMaterial.color = Color.black;

        var human = __instance.RootParentHuman;
        if (human == null) return;

        var forward = CameraController.CurrentCamera.transform.forward;
        var position = CameraController.CurrentCamera.transform.position + 0.5f * forward;

        var range = __instance._range;
        Vein nearestVeinOfType = Vein.GetNearestVeinOfType(position, range, __instance.TrackedMinableType);
        if (nearestVeinOfType == null)
            return;

        var veinPos = Vein.GetClosestMinablePosition(position, nearestVeinOfType);
        float num = Vector3.Distance(position, veinPos);

        if (num > range)
            return;
        __instance._audioPitch = Mathf.Lerp(OreDetector.MinAudioPitch, OreDetector.MaxAudioPitch, Mathf.Max(range - num, 0f) / range);

        Vector3 toVein = (veinPos - position).normalized;
        float angle = Vector3.Angle(forward, toVein);
        float angDist = Mathf.Clamp(angle / 90.0f, 0.0f, 1.0f - 0.4f / __instance.signalStrengthIndicators.Length); // show at least one light when a vein is in range
        distance = range * angDist;
    }
}