using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MetalRecharging.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class InteractPerformedPatch
    {
        private static bool _swappingTwoHandedValue = false;

        [HarmonyPatch("Interact_performed")]
        [HarmonyPrefix]
        internal static void InteractPerformedPrefix(PlayerControllerB __instance)
        {
            if (__instance.hoveringOverTrigger == null) return;
            // Not holding a two handed item
            if (!__instance.twoHanded) return;

            // Error check, also make sure interact is on Charge Station
            var triggerParent = __instance.hoveringOverTrigger.transform.parent;
            if (triggerParent == null || triggerParent.gameObject.name != "ChargeStationTrigger") return;

            // If we get here, just change flag so we're not two handed anymore for this interaction
            _swappingTwoHandedValue = true;
            __instance.twoHanded = false;
        }

        [HarmonyPatch("Interact_performed")]
        [HarmonyPostfix]
        internal static void InteractPerformedPostfix(PlayerControllerB __instance)
        {
            if (!_swappingTwoHandedValue) return;
            _swappingTwoHandedValue = false;
            __instance.twoHanded = true;
        }
    }
}
