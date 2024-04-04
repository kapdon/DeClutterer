using Aki.Reflection.Patching;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using System.Reflection;

namespace Framesaver
{
    public class PhysicsUpdatePatch : ModulePatch
    {
        public static bool everyOtherFixedUpdate = false;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass649), "Update");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            everyOtherFixedUpdate = !everyOtherFixedUpdate;
            if (everyOtherFixedUpdate)
            {
                GClass649.GClass650.Update();
                GClass649.GClass651.Update();
            }
            return false;
        }
    }
    public class PhysicsFixedUpdatePatch : ModulePatch
    {
        public static bool everyOtherFixedUpdate = false;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass649), "FixedUpdate");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            everyOtherFixedUpdate = !everyOtherFixedUpdate;
            if (everyOtherFixedUpdate)
            {
                GClass649.GClass650.FixedUpdate();
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
            everyOtherFixedUpdate = !everyOtherFixedUpdate;
            if (everyOtherFixedUpdate)
            {
                GClass649.SyncTransforms();
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
            return false;
        }
    }
}