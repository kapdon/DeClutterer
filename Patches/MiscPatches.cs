using StayInTarkov;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using System.Reflection;
using TYR_DeClutterer.Utils;

namespace TYR_DeClutterer.Patches
{
    public class PhysicsUpdatePatch : ModulePatch
    {
        public static bool everyOtherFixedUpdate = false;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass654), "Update");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            if (!Configuration.framesaverPhysicsEnabledConfig.Value || !Configuration.framesaverEnabledConfig.Value)
                return true;

            everyOtherFixedUpdate = !everyOtherFixedUpdate;
            if (everyOtherFixedUpdate)
            {
                GClass654.GClass655.Update();
                GClass654.GClass656.Update();
            }
            return false;
        }
    }

    public class PhysicsFixedUpdatePatch : ModulePatch
    {
        public static bool everyOtherFixedUpdate = false;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass654), "FixedUpdate");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            if (!Configuration.framesaverPhysicsEnabledConfig.Value || !Configuration.framesaverEnabledConfig.Value)
                return true;

            everyOtherFixedUpdate = !everyOtherFixedUpdate;
            if (everyOtherFixedUpdate)
            {
                GClass654.GClass655.FixedUpdate();
            }
            return false;
        }
    }

    public class RagdollPhysicsLateUpdatePatch : ModulePatch
    {
        public static bool everyOtherFixedUpdate = false;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(CorpseRagdollTestApplication), "LateUpdate");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            if (!Configuration.framesaverPhysicsEnabledConfig.Value || !Configuration.framesaverEnabledConfig.Value)
                return true;

            everyOtherFixedUpdate = !everyOtherFixedUpdate;
            if (everyOtherFixedUpdate)
            {
                GClass654.SyncTransforms();
            }
            return false;
        }
    }

    public class FlameDamageTriggerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(FlameDamageTrigger), "ProceedDamage");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            if (!Configuration.framesaverFireAndSmokeEnabledConfig.Value || !Configuration.framesaverEnabledConfig.Value)
                return true;

            return false;
        }
    }
}