﻿using HarmonyLib;
using ModLibrary;
using System.Collections.Generic;
using System.Reflection;

namespace InternalModBot
{
    [HarmonyPatch(typeof(EscMenu))]
    static class EscMenu_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Show")]
        static bool Show_Prefix()
        {
            return !GameUIRoot.Instance.GetPrivateField<bool>("_isEscMenuDisabled");
        }

        [HarmonyPrefix]
        [HarmonyPatch("Hide")]
        static bool Hide_Prefix()
        {
            return !GameUIRoot.Instance.GetPrivateField<bool>("_isEscMenuDisabled");
        }
    }
}