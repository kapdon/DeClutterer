using StayInTarkov;
using EFT.Weather;
using HarmonyLib;
using System.Reflection;
using TYR_DeClutterer.Utils;

namespace TYR_DeClutterer.Patches
{
    internal class AmbientLightOptimizeRenderingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AmbientLight).GetMethod("method_8", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            if (!Configuration.framesaverLightingShadowsEnabledConfig.Value || !Configuration.framesaverEnabledConfig.Value)
                return true;

            return false;
        }
    }

    internal class AmbientLightDisableUpdatesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AmbientLight).GetMethod("Update", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            if (!Configuration.framesaverLightingShadowsEnabledConfig.Value || !Configuration.framesaverEnabledConfig.Value)
                return true;

            return false;
        }
    }

    internal class AmbientLightDisableLateUpdatesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AmbientLight).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            if (!Configuration.framesaverLightingShadowsEnabledConfig.Value || !Configuration.framesaverEnabledConfig.Value)
                return true;

            return false;
        }
    }

    internal class CloudsControllerDelayUpdatesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(CloudsController).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            if (!Configuration.framesaverWeatherUpdatesEnabledConfig.Value || !Configuration.framesaverEnabledConfig.Value)
                return true;

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
        public static bool PatchPrefix(WeatherController __instance, Class1803 ___class1803_0, ToDController ___TimeOfDayController)
        {
            if (!Configuration.framesaverWeatherUpdatesEnabledConfig.Value || !Configuration.framesaverEnabledConfig.Value)
                return true;

            everyOtherLateUpdate = !everyOtherLateUpdate;

            if (everyOtherLateUpdate)
            {
                ___TimeOfDayController.Update();
                ___class1803_0.Update();
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
            if (!Configuration.framesaverWeatherUpdatesEnabledConfig.Value || !Configuration.framesaverEnabledConfig.Value)
                return true;

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

    internal class WeatherEventControllerDelayUpdatesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(WeatherEventController).GetMethod("Update", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            if (!Configuration.framesaverWeatherUpdatesEnabledConfig.Value || !Configuration.framesaverEnabledConfig.Value)
                return true;

            return false;
        }
    }
}