using HarmonyLib;
using System.Reflection;

namespace Marioalexsan.AtlyssDiscordRichPresence.HarmonyPatches;

[HarmonyPatch]
static class Creep_Handle_AggroedNetObj
{
    static MethodInfo TargetMethod() => AccessTools.GetDeclaredMethods(typeof(Creep)).First(x => x.Name.Contains("Handle_AggroedNetObj"));

    static void Prefix(Creep __instance)
    {
        if (__instance)
            TrackedAggroCreeps.List.Add(__instance);
    }
}

public static class TrackedAggroCreeps
{
    public static List<Creep> List { get; } = [];
}