using System.Reflection;
using Aki.Reflection.Patching;
using EFT.Weather;
using HarmonyLib;

namespace Framesaver
{
    class AmbientLightOptimizeRenderingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AmbientLight).GetMethod("method_8", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
    class AmbientLightDisableUpdatesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AmbientLight).GetMethod("Update", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
    class AmbientLightDisableLateUpdatesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AmbientLight).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
    class CloudsControllerDelayUpdatesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(CloudsController).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.Public);
        }
        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
    public class WeatherLateUpdatePatch : ModulePatch
    {
        public static bool everyOtherLateUpdate = false;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(WeatherController), "LateUpdate");
        }

        [PatchPrefix]
        public static bool PatchPrefix(WeatherController __instance, Class1794 ___class1794_0, ToDController ___TimeOfDayController)
        {
            everyOtherLateUpdate = !everyOtherLateUpdate;

            if (everyOtherLateUpdate)
            {
                ___TimeOfDayController.Update();           
                ___class1794_0.Update();
                __instance.method_8();
            }
            return false;
        }
    }
    public class SkyDelayUpdatesPatch : ModulePatch
    {
        public static bool everyOtherLateUpdate = false;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TOD_Sky), "LateUpdate");
        }

        [PatchPrefix]
        public static bool PatchPrefix(TOD_Sky __instance)
        {
            everyOtherLateUpdate = !everyOtherLateUpdate;
            
            if (everyOtherLateUpdate)
            {
                __instance.method_17();
                __instance.method_18();
                __instance.method_0();
                __instance.method_1();
                __instance.method_2();
                __instance.method_3();
            }
            return false;
        }
    }
    class WeatherEventControllerDelayUpdatesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(WeatherEventController).GetMethod("Update", BindingFlags.Instance | BindingFlags.Public);
        }
        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
}