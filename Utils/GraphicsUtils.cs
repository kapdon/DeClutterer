﻿using Comfort.Common;
using UnityEngine;

namespace TYR_DeClutterer.Utils
{
    internal static class GraphicsUtils
    {
        public static void SetParticlesQuality()
        {
            if (Configuration.framesaverParticlesEnabledConfig.Value)
            {
                QualitySettings.softParticles = false;
                if (Configuration.framesaverParticleBudgetDividerConfig.Value > 1)
                {
                    QualitySettings.particleRaycastBudget = DeClutter.DefaultparticleRaycastBudget / Configuration.framesaverParticleBudgetDividerConfig.Value;
                }
            }
            else
            {
                QualitySettings.softParticles = DeClutter.DefaultsoftParticles;
                QualitySettings.particleRaycastBudget = DeClutter.DefaultparticleRaycastBudget;
            }
        }

        public static void SetSoftVegetationQuality()
        {
            if (Configuration.framesaverSoftVegetationEnabledConfig.Value)
            {
                QualitySettings.softVegetation = false;
            }
            else
            {
                QualitySettings.softVegetation = DeClutter.DefaultsoftVegetation;
            }
        }

        public static void SetReflectionQuality()
        {
            if (Configuration.framesaverReflectionsEnabledConfig.Value)
            {
                QualitySettings.realtimeReflectionProbes = false;
            }
            else
            {
                QualitySettings.realtimeReflectionProbes = DeClutter.DefaultrealtimeReflectionProbes;
            }
        }

        public static void SetLightingShadowQuality()
        {
            if (Configuration.framesaverLightingShadowCascadesEnabledConfig.Value)
            {
                QualitySettings.shadows = ShadowQuality.HardOnly;
                if (Configuration.framesaverShadowDividerConfig.Value > 1)
                {
                    QualitySettings.pixelLightCount = 4 / Configuration.framesaverPixelLightDividerConfig.Value;
                    QualitySettings.shadowCascades = 4 / Configuration.framesaverShadowDividerConfig.Value;
                }
            }
            else
            {
                QualitySettings.pixelLightCount = DeClutter.DefaultpixelLightCount;
                QualitySettings.shadows = DeClutter.DefaultShadows;
                QualitySettings.shadowCascades = DeClutter.DefaultshadowCascades;
            }
        }

        public static void SetTextureQuality()
        {
            if (Configuration.framesaverTexturesEnabledConfig.Value)
            {
                if (Singleton<SettingsManager>.Instance.Graphics.Settings.TextureQuality.Value == 2)
                {
                    QualitySettings.masterTextureLimit = 0;
                }
                else
                {
                    QualitySettings.masterTextureLimit = Configuration.framesaverTextureSizeConfig.Value;
                }
            }
            else
            {
                if (Singleton<SettingsManager>.Instance.Graphics.Settings.TextureQuality.Value == 2)
                {
                    QualitySettings.masterTextureLimit = 0;
                }
                else
                {
                    QualitySettings.masterTextureLimit = DeClutter.DefaultmasterTextureLimit;
                }
            }
        }

        public static void SetLodBiasQuality()
        {
            if (Configuration.framesaverLODEnabledConfig.Value)
            {
                if (Configuration.framesaverLODBiasConfig.Value > 1.0f)
                {
                    QualitySettings.lodBias = 2.0f / Configuration.framesaverLODBiasConfig.Value;
                }
            }
            else
            {
                QualitySettings.lodBias = DeClutter.DefaultlodBias;
            }
        }

        public static void SetDefaultQualityForAll()
        {
            QualitySettings.softParticles = DeClutter.DefaultsoftParticles;
            QualitySettings.particleRaycastBudget = DeClutter.DefaultparticleRaycastBudget;
            QualitySettings.softVegetation = DeClutter.DefaultsoftVegetation;
            QualitySettings.realtimeReflectionProbes = DeClutter.DefaultrealtimeReflectionProbes;
            QualitySettings.pixelLightCount = DeClutter.DefaultpixelLightCount;
            QualitySettings.shadows = DeClutter.DefaultShadows;
            QualitySettings.shadowCascades = DeClutter.DefaultshadowCascades;

            if (Singleton<SettingsManager>.Instance.Graphics.Settings.TextureQuality.Value == 2)
            {
                QualitySettings.masterTextureLimit = 0;
            }
            else
            {
                QualitySettings.masterTextureLimit = DeClutter.DefaultmasterTextureLimit;
            }

            QualitySettings.lodBias = DeClutter.DefaultlodBias;
        }
    }
}