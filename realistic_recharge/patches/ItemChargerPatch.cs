using HarmonyLib;
using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace MetalRecharging.Patches
{
    [HarmonyPatch(typeof(ItemCharger))]
    internal class ItemChargerPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static bool ItemChargerUpdate(ItemCharger __instance, ref float ___updateInterval, ref InteractTrigger ___triggerScript)
        {
            if (NetworkManager.Singleton == null) return false;
            if (___updateInterval > 1f)
            {
                ___updateInterval = 0;
                if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null)
                {
                    var heldObject = GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer;
                    if (heldObject == null || (!heldObject.itemProperties.isConductiveMetal && !heldObject.itemProperties.requiresBattery))
                    {
                        ___triggerScript.interactable = false;
                        ___triggerScript.disabledHoverTip = "(Requires battery-powered or metal item)";
                    }
                    else
                    {
                        ___triggerScript.interactable = true;
                    }
                    ___triggerScript.twoHandedItemAllowed = true;
                    return false;
                }
            }
            ___updateInterval += Time.deltaTime;
            return false;
        }

        [HarmonyPatch("ChargeItem")]
        [HarmonyPostfix]
        static void ItemChargerCharge(ItemCharger __instance, ref Coroutine ___chargeItemCoroutine)
        {
            var heldObject = GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer;
            if (heldObject != null && !heldObject.itemProperties.requiresBattery && heldObject.itemProperties.isConductiveMetal)
            {
                __instance.PlayChargeItemEffectServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
                if (___chargeItemCoroutine != null)
                {
                    __instance.StopCoroutine(___chargeItemCoroutine);
                }
                if (GameNetworkManager.Instance.localPlayerController.AllowPlayerDeath())
                {
                    ___chargeItemCoroutine = __instance.StartCoroutine(ItemChargerDelayed(__instance, heldObject));
                }
            }
        }

        static IEnumerator ItemChargerDelayed(ItemCharger __instance, GrabbableObject itemToCharge)
        {
            __instance.zapAudio.Play();
            yield return new WaitForSeconds(0.75f);
            __instance.chargeStationAnimator.SetTrigger("zap");
            GameNetworkManager.Instance.localPlayerController.KillPlayer(UnityEngine.Vector3.zero, true, CauseOfDeath.Electrocution);
        }
    }
}
