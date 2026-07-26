using EFT.UI;
using HarmonyLib;
using UnityEngine;

namespace CustomUISounds
{
    [HarmonyPatch(typeof(UISoundsWrapper), nameof(UISoundsWrapper.GetUIClip))]
    static class UISoundPatch
    {
        [HarmonyPostfix]
        static void Postfix(EUISoundType soundType, ref AudioClip __result)
        {
            AudioClip custom = SoundManager.GetClip(soundType);

            if (custom != null)
            {
                __result = custom;
            }
        }
    }
}
