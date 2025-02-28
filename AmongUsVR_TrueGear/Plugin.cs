using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Fusion;
using HarmonyLib;
using Il2CppSystem;
using SG.Airlock;
using SG.Airlock.Cutscenes;
using SG.Airlock.Interaction;
using SG.Airlock.Minigames;
using SG.Airlock.Minimap;
using SG.Airlock.Network;
using SG.Airlock.Venting;
using SG.Airlock.XR;
using SG.Platform;
using System;
using UnityEngine.Events;
using MyTrueGear;
using System.Threading;

namespace AmongUsVR_TrueGear;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;
    private static int localPlayerID = -1;
    private static TrueGearMod _TrueGear = null;
    private static bool canHaptic = true;

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;

        Harmony.CreateAndPatchAll(typeof(Plugin));
        _TrueGear = new TrueGearMod();
        _TrueGear.Play("LevelStarted");
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    [HarmonyPostfix, HarmonyPatch(typeof(PlayerState), "PlayerKilledLocation")]
    private static void PlayerState_PlayerKilledLocation_Postfix(PlayerState __instance)
    {
        Log.LogInfo("---------------------------------------------");
        Log.LogInfo("PlayerKilledLocation");
        if (localPlayerID == __instance._playerId)
        {
            Log.LogInfo("PlayerDeath");
            _TrueGear.Play("PlayerDeath");
        }
        Log.LogInfo(__instance._playerId);

    }

    [HarmonyPostfix, HarmonyPatch(typeof(CutsceneManager), "ShowDeath")]
    private static void CutsceneManager_ShowDeath_Postfix(CutsceneManager __instance, PlayerRef killedPlayer, PlayerRef killer)
    {

        Log.LogInfo("---------------------------------------------");
        Log.LogInfo("ShowDeath");
        if (killedPlayer.PlayerId == localPlayerID)
        {
            Log.LogInfo("PlayerDeath");
            _TrueGear.Play("PlayerDeath");
        }
        Log.LogInfo(killedPlayer.PlayerId);
        Log.LogInfo(killer.PlayerId);
    }
    private static bool canChangeVent = false;
    [HarmonyPostfix, HarmonyPatch(typeof(XRRig), "EnterVent")]
    private static void XRRig_EnterVent_Postfix(XRRig __instance)
    {
        Log.LogInfo("---------------------------------------------");
        Log.LogInfo("XRRigEnterVent");
        if (localPlayerID == __instance.PState._playerId)
        {
            Log.LogInfo("PlayerEnterVent");
            _TrueGear.Play("PlayerEnterVent");
            canChangeVent = false;
        }
        Log.LogInfo(__instance.PState._playerId);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(Vent), "EnterVent")]
    private static void Vent_EnterVent_Postfix(Vent __instance, XRRig rig)
    {

        Log.LogInfo("---------------------------------------------");
        Log.LogInfo("VentEnterVent");
        if (localPlayerID == rig.PState._playerId)
        {
            if (!canChangeVent)
            {
                canChangeVent = true;
                return;
            }
            Log.LogInfo("PlayerChangeVent");
            _TrueGear.Play("PlayerChangeVent");
        }
        Log.LogInfo(rig.PState._playerId);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(XRRig), "ExitVent")]
    private static void XRRig_ExitVent_Postfix(XRRig __instance, bool teleport, bool playVentAnimation, bool forceExit, bool playVentPoof)
    {
        Log.LogInfo("---------------------------------------------");
        Log.LogInfo("XRRigExitVent");
        if (localPlayerID == __instance.PState._playerId)
        {
            Log.LogInfo("PlayerExitVent");
            _TrueGear.Play("PlayerExitVent");
        }
        Log.LogInfo(__instance.PState._playerId);
        Log.LogInfo(teleport);
        Log.LogInfo(playVentAnimation);
        Log.LogInfo(forceExit);
        Log.LogInfo(playVentPoof);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MinigameButton), "OnInteractionStart")]
    private static void MinigameButton_OnInteractionStart_Postfix(MinigameButton __instance, XRHand hand)
    {
        Log.LogInfo("---------------------------------------------");
        Log.LogInfo("OnInteractionStart");
        if (localPlayerID == hand.xrRig.PState._playerId && __instance._isPhysicallyPressable && __instance.name != "ButtonCollider")
        {
            if (hand.Hand == SG.Platform.Hand.Left)
            {
                Log.LogInfo("LeftHandPressButton");
                _TrueGear.Play("LeftHandPressButton");
            }
            else if (hand.Hand == SG.Platform.Hand.Right)
            {
                Log.LogInfo("RightHandPressButton");
                _TrueGear.Play("RightHandPressButton");
            }

        }
        else if (localPlayerID == hand.xrRig.PState._playerId && __instance.name.Contains("BodyCollider"))
        {
            if (hand.Hand == SG.Platform.Hand.Left)
            {
                Log.LogInfo("LeftHandReport");
                _TrueGear.Play("LeftHandReport");
            }
            else if (hand.Hand == SG.Platform.Hand.Right)
            {
                Log.LogInfo("RightHandReport");
                _TrueGear.Play("RightHandReport");
            }
        }
        else if (!__instance._isPhysicallyPressable)
        {
            canHaptic = false;
            Timer hapticTimr = new Timer(HapticTimerCallBack, null, 100, Timeout.Infinite);
        }
        Log.LogInfo(hand.Hand);
        Log.LogInfo(hand.xrRig.PState._playerId);
        Log.LogInfo(__instance.name);
        Log.LogInfo(__instance._isPhysicallyPressable);
    }

    private static void HapticTimerCallBack(object o)
    {
        canHaptic = true;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(SpawnManager), "OnPlayerJoined")]
    private static void SpawnManager_OnPlayerJoined_Postfix(SpawnManager __instance, PlayerRef player)
    {
        Log.LogInfo("---------------------------------------------");
        Log.LogInfo("OnPlayerJoined");
        if (player.PlayerId == __instance.GetLocalPlayerState()._playerId)
        {
            Log.LogInfo("LevelStarted");
            _TrueGear.Play("LevelStarted");
        }
        Log.LogInfo(__instance.GetLocalPlayerState()._playerId);
        localPlayerID = __instance.GetLocalPlayerState()._playerId;
        Log.LogInfo(player.PlayerId);
        Log.LogInfo($"LocalPlayerID :{localPlayerID}");
    }

    //[HarmonyPostfix, HarmonyPatch(typeof(SpawnManager), "Spawned")]
    //private static void SpawnManager_Spawned_Postfix(SpawnManager __instance)
    //{
    //    Log.LogInfo("---------------------------------------------");
    //    Log.LogInfo("Spawned");
    //    Log.LogInfo(__instance.GetLocalPlayerState()._playerId);
    //    localPlayerID = __instance.GetLocalPlayerState()._playerId;
    //    Log.LogInfo($"LocalPlayerID :{localPlayerID}");
    //}

    [HarmonyPostfix, HarmonyPatch(typeof(SpawnManager), "OnBodySpawn")]
    private static void SpawnManager_OnBodySpawn_Postfix(SpawnManager __instance, PlayerRef playerRef)
    {
        Log.LogInfo("---------------------------------------------");
        Log.LogInfo("OnBodySpawn");
        if (playerRef.PlayerId == __instance.GetLocalPlayerState()._playerId)
        {
            Log.LogInfo("LevelStarted");
            _TrueGear.Play("LevelStarted");
        }
        Log.LogInfo(__instance.GetLocalPlayerState()._playerId);
        localPlayerID = __instance.GetLocalPlayerState()._playerId;
        Log.LogInfo(playerRef.PlayerId);
        Log.LogInfo($"LocalPlayerID :{localPlayerID}");
    }

    [HarmonyPostfix, HarmonyPatch(typeof(NetworkedLocomotionPlayer), "PlayKillerFX")]
    private static void NetworkedLocomotionPlayer_PlayKillerFX_Postfix(NetworkedLocomotionPlayer __instance)
    {

        Log.LogInfo("---------------------------------------------");
        Log.LogInfo("PlayKillerFX");
        if (localPlayerID == __instance.PState._playerId)
        {
            Log.LogInfo("PlayerKillPlayer");
            _TrueGear.Play("PlayerKillPlayer");
        }
        Log.LogInfo(__instance.PState._playerId);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(GrabbableInteractableBase), "OnInteractionStart")]
    private static void GrabbableInteractableBase_OnInteractionStart_Postfix(GrabbableInteractableBase __instance, XRHand hand)
    {
        Log.LogInfo("---------------------------------------------");
        Log.LogInfo("GrabbableInteractableBaseOnInteractionStart");
        if (localPlayerID == hand.xrRig.PState._playerId)
        {
            if (hand.Hand == Hand.Left)
            {
                Log.LogInfo("LeftHandGrabItem");
                _TrueGear.Play("LeftHandGrabItem");
            }
            else if (hand.Hand == Hand.Right)
            {
                Log.LogInfo("RightHandGrabItem");
                _TrueGear.Play("RightHandGrabItem");
            }
        }
        Log.LogInfo(hand.Hand);
        Log.LogInfo(hand.xrRig.PState._playerId);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(Haptics), "Trigger", new System.Type[] { typeof(Hand), typeof(float), typeof(float), typeof(float) })]
    private static void Haptics_Trigger4_Postfix(Haptics __instance, Hand hand, float duration, float frequency, float amplitude)
    {
        Log.LogInfo("---------------------------------------------");
        Log.LogInfo("Trigger4");
        if (amplitude >= 0.1 && canHaptic)
        {
            int power = (int)(amplitude * 10);
            if (power > 5) power = 5;
            if (hand == Hand.Left)
            {
                Log.LogInfo("LeftHandHaptic" + power);
                _TrueGear.Play("LeftHandHaptic" + power);
            }
            else if (hand == Hand.Right)
            {
                Log.LogInfo("RightHandHaptic" + power);
                _TrueGear.Play("RightHandHaptic" + power);
            }
        }
        Log.LogInfo(hand);
        Log.LogInfo(duration);
        Log.LogInfo(frequency);
        Log.LogInfo(amplitude);
    }


}
