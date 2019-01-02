﻿using Harmony;
using RimWorld;
using System;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TechAdvancing
{
    /// <summary>
    /// Class storing all the detours and the detour call.
    /// </summary>
    class HarmonyDetours
    {
        /// <summary>
        /// Method for performing all the detours via Harmony.
        /// </summary>
        public static void Setup()
        {
            var harmony = HarmonyInstance.Create("com.ghxx.rimworld.techadvancing");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    /// <summary>
    /// Prefix for adding the button below the progressbar of the research window. The button is used for opening the config screen.
    /// </summary>
    [HarmonyPatch(typeof(RimWorld.MainTabWindow_Research))]
    [HarmonyPatch("DrawLeftRect")]
    [HarmonyPatch(new Type[] { typeof(Rect) })]
    class TA_Research_Menu_Patch
    {
        static ResearchProjectDef selectedProject = null;
        static void Prefix(Rect leftOutRect)
        {
            Rect TA_Cfgrect = new Rect(0f, 0f, 180f, 20f);
            TA_Cfgrect.x = (leftOutRect.width - TA_Cfgrect.width) / 2f;
            TA_Cfgrect.y = leftOutRect.height - 20f;

            if (Widgets.ButtonText(TA_Cfgrect, "TAcfgmenulabel".Translate(), true, false, true))
            {
                SoundDef.Named("ResearchStart").PlayOneShotOnCamera();
                Find.WindowStack.Add((Window)new TechAdvancing_Config_Tab());
            }
        }
    }

    /// <summary>
    /// Replace research cost calc method to be able to remove cost cap, like in A18
    /// </summary>
    [HarmonyPatch(typeof(Verse.ResearchProjectDef))]
    [HarmonyPatch("CostFactor")]
    [HarmonyPatch(typeof(TechLevel))]
    class TA_ReplaceResearchProjectDef
    {
        static void Postfix(Verse.ResearchProjectDef __instance,ref float __result,TechLevel researcherTechLevel)
        {
            TechLevel TA_POSTFIXED_techLevel = TechAdvancing_Config_Tab.configCheckboxDisableCostMultiplicatorCap == 1 ? __instance.techLevel : (TechLevel)Mathf.Min((int)__instance.techLevel, 4);

            //TechLevel techLevel = (TechLevel)Mathf.Min((int)__instance.techLevel, 4);
            if ((int)researcherTechLevel >= (int)TA_POSTFIXED_techLevel)
            {
                __result = 1f;
            }
            else
            {
                int num = TA_POSTFIXED_techLevel - researcherTechLevel;
                __result = 1f + (float)num * 0.5f;
            }
        }
    }

    /// <summary>
    /// Patch for having a method called when a pawn dies.
    /// </summary>
    [HarmonyPatch(typeof(Verse.Pawn))]
    [HarmonyPatch("Kill")]
    [HarmonyPatch(new Type[] { typeof(DamageInfo?), typeof(Hediff) })]
    class TA_OnKill_Event
    {
        static void Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            TechAdvancing.Event.OnKill(__instance);
        }
    }

    /// <summary>
    /// Patch for getting notified about faction changes. E.g.: when a pawn joins the colony.
    /// </summary>
    [HarmonyPatch(typeof(Verse.Pawn))]
    [HarmonyPatch("SetFaction")]
    [HarmonyPatch(new Type[] { typeof(Faction), typeof(Pawn) })]
    class TA_OnNewPawn_Event
    {
        static void Prefix(Pawn __instance, Faction newFaction, Pawn recruiter = null)
        {
            TechAdvancing.Event.OnNewPawn(__instance);
        }
    }

    /// <summary>
    /// Postfix Patch for getting to know the new faction.
    /// </summary>
    [HarmonyPatch(typeof(Verse.Pawn))]
    [HarmonyPatch("SetFaction")]
    [HarmonyPatch(new Type[] { typeof(Faction), typeof(Pawn) })]
    class TA_PostOnNewPawn_Event
    {
        static void Postfix(Faction newFaction, Pawn recruiter = null)
        {
            TechAdvancing.Event.PostOnNewPawn();
        }
    }

}
