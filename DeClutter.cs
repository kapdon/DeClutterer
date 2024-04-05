using BepInEx;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using EFT.Ballistics;
using EFT.Interactive;
using Koenigz.PerfectCulling.EFT;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TYR_DeClutterer.Patches;
using TYR_DeClutterer.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TYR_DeClutterer
{
    [BepInPlugin("com.TYR.DeClutter", "TYR_DeClutter", "1.2.0")]
    public class DeClutter : BaseUnityPlugin
    {
        private static string PluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private static GameWorld gameWorld;

        public static bool MapLoaded() => Singleton<GameWorld>.Instantiated;

        internal static ClutterNameStruct CleanUpNames = new ClutterNameStruct();

        private List<GameObject> allGameObjectsList = new List<GameObject>();
        public static List<GameObject> savedClutterObjects = new List<GameObject>();
        public static Player Player;
        private static bool deCluttered = false;

        public static bool applyDeclutter = false;
        public static bool defaultsoftParticles = false;
        public static int defaultparticleRaycastBudget = 0;
        public static bool defaultsoftVegetation = false;
        public static bool defaultrealtimeReflectionProbes = false;
        public static int defaultpixelLightCount = 0;
        public static ShadowQuality defaultShadows;
        public static int defaultshadowCascades = 0;
        public static int defaultmasterTextureLimit = 0;
        public static float defaultlodBias = 0f;

        private Dictionary<string, bool> dontDisableDictionary = new Dictionary<string, bool>
        {
            { "item_", true },
            { "weapon_", true },
            { "barter_", true },
            { "mod_", true },
            { "audio", true },
            { "container", true },
            { "trigger", true },
            { "culling", true },
            { "collider", true },
            { "colider", true },
            { "group", true },
            { "manager", true },
            { "scene", true },
            { "player", true },
            { "portal", true },
            { "bakelod", true },
            { "door", true },
            { "shadow", true },
            { "mine", true }
        };

        private void Awake()
        {
            new PhysicsUpdatePatch().Enable();
            new PhysicsFixedUpdatePatch().Enable();
            new RagdollPhysicsLateUpdatePatch().Enable();

            new AmbientLightOptimizeRenderingPatch().Enable();
            new AmbientLightDisableUpdatesPatch().Enable();
            new AmbientLightDisableLateUpdatesPatch().Enable();

            new DontSpawnShellsFiringPatch().Enable();
            new DontSpawnShellsJamPatch().Enable();
            new DontSpawnShellsAtAllReallyPatch().Enable();

            new SkyDelayUpdatesPatch().Enable();
            new WeatherLateUpdatePatch().Enable();
            new CloudsControllerDelayUpdatesPatch().Enable();
            new WeatherEventControllerDelayUpdatesPatch().Enable();
        }

        private Dictionary<string, bool> clutterNameDictionary = new Dictionary<string, bool>
        {
        };

        private void Start()
        {
            InitializeClutterNameDictionary();

            SceneManager.sceneUnloaded += OnSceneUnloaded;

            Configuration.Bind(Config);
            SubScribeConfig();

            applyDeclutter = Configuration.declutterEnabledConfig.Value;

            defaultsoftParticles = QualitySettings.softParticles;
            defaultparticleRaycastBudget = QualitySettings.particleRaycastBudget;
            defaultsoftVegetation = QualitySettings.softVegetation;
            defaultrealtimeReflectionProbes = QualitySettings.realtimeReflectionProbes;
            defaultpixelLightCount = QualitySettings.pixelLightCount;
            defaultShadows = QualitySettings.shadows;
            defaultshadowCascades = QualitySettings.shadowCascades;
            defaultmasterTextureLimit = QualitySettings.masterTextureLimit;
            defaultlodBias = QualitySettings.lodBias;
        }

        private void Update()
        {
            if (!MapLoaded() || deCluttered || !Configuration.declutterEnabledConfig.Value)
                return;

            gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.MainPlayer == null || IsInHideout())
                return;

            deCluttered = true;

            DeClutterScene();
            DeClutterVisuals();
        }

        private void InitializeClutterNameDictionary()
        {
            var cleanUpJsonText = File.ReadAllText(Path.Combine(PluginFolder, "CleanUpNames.json"));
            CleanUpNames = JsonConvert.DeserializeObject<ClutterNameStruct>(cleanUpJsonText);

            clutterNameDictionary = clutterNameDictionary.Concat(CleanUpNames.Garbage)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            clutterNameDictionary = clutterNameDictionary.Concat(CleanUpNames.Heaps)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            clutterNameDictionary = clutterNameDictionary.Concat(CleanUpNames.SpentCartridges)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            clutterNameDictionary = clutterNameDictionary.Concat(CleanUpNames.FoodDrink)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            clutterNameDictionary = clutterNameDictionary.Concat(CleanUpNames.Decals)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            clutterNameDictionary = clutterNameDictionary.Concat(CleanUpNames.Puddles)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            clutterNameDictionary = clutterNameDictionary.Concat(CleanUpNames.Shards)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private void SubScribeConfig()
        {
            Configuration.declutterEnabledConfig.SettingChanged += OnApplyDeclutterSettingChanged;
        }

        // Framesaver information and patches brought to you by Ari.
        private void DeClutterVisuals()
        {
            if (Configuration.framesaverEnabledConfig.Value)
            {
                if (Configuration.framesaverParticlesEnabledConfig.Value)
                {
                    QualitySettings.softParticles = false;
                    if (Configuration.framesaverParticleBudgetDividerConfig.Value > 1)
                    {
                        QualitySettings.particleRaycastBudget = defaultparticleRaycastBudget / Configuration.framesaverParticleBudgetDividerConfig.Value;
                    }
                }
                else
                {
                    QualitySettings.softParticles = defaultsoftParticles;
                    QualitySettings.particleRaycastBudget = defaultparticleRaycastBudget;
                }

                if (Configuration.framesaverSoftVegetationEnabledConfig.Value)
                {
                    QualitySettings.softVegetation = false;
                }
                else
                {
                    QualitySettings.softVegetation = defaultsoftVegetation;
                }

                if (Configuration.framesaverReflectionsEnabledConfig.Value)
                {
                    QualitySettings.realtimeReflectionProbes = false;
                }
                else
                {
                    QualitySettings.realtimeReflectionProbes = defaultrealtimeReflectionProbes;
                }

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
                    QualitySettings.pixelLightCount = defaultpixelLightCount;
                    QualitySettings.shadows = defaultShadows;
                    QualitySettings.shadowCascades = defaultshadowCascades;
                }

                if (Configuration.framesaverTexturesEnabledConfig.Value)
                {
                    if (Configuration.framesaverTextureSizeConfig.Value > 1)
                    {
                        QualitySettings.masterTextureLimit = 0 + Configuration.framesaverTextureSizeConfig.Value;
                    }
                }
                else
                {
                    QualitySettings.masterTextureLimit = defaultmasterTextureLimit;
                }

                if (Configuration.framesaverLODEnabledConfig.Value)
                {
                    if (Configuration.framesaverLODBiasConfig.Value > 1.0f)
                    {
                        QualitySettings.lodBias = 2.0f / Configuration.framesaverLODBiasConfig.Value;
                    }
                }
                else
                {
                    QualitySettings.lodBias = defaultlodBias;
                }
            }
            else
            {
                QualitySettings.softParticles = defaultsoftParticles;
                QualitySettings.particleRaycastBudget = defaultparticleRaycastBudget;
                QualitySettings.softVegetation = defaultsoftVegetation;
                QualitySettings.realtimeReflectionProbes = defaultrealtimeReflectionProbes;
                QualitySettings.pixelLightCount = defaultpixelLightCount;
                QualitySettings.shadows = defaultShadows;
                QualitySettings.shadowCascades = defaultshadowCascades;
                QualitySettings.masterTextureLimit = defaultmasterTextureLimit;
                QualitySettings.lodBias = defaultlodBias;
            }
        }

        private void OnApplyDeclutterSettingChanged(object sender, EventArgs e)
        {
            applyDeclutter = Configuration.declutterEnabledConfig.Value;
            if (deCluttered)
            {
                if (applyDeclutter)
                {
                    DeClutterEnabled();
                }
                else
                {
                    ReClutterEnabled();
                }
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
            allGameObjectsList.Clear();
            savedClutterObjects.Clear();
            deCluttered = false;
        }

        private bool IsInHideout()
        {
            // Check if "bunker_2" is one of the active scene names
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == "bunker_2")
                {
                    //EFT.UI.ConsoleScreen.LogError("bunker_2 loaded, not running de-cluttering.");
                    return true;
                }
            }
            //EFT.UI.ConsoleScreen.LogError("bunker_2 not loaded, de-cluttering.");
            return false;
        }

        private void DeClutterEnabled()
        {
            foreach (GameObject obj in savedClutterObjects)
            {
                if (obj.activeSelf == true)
                {
                    obj.SetActive(false);
                }
            }
        }

        private void ReClutterEnabled()
        {
            foreach (GameObject obj in savedClutterObjects)
            {
                if (obj.activeSelf == false)
                {
                    obj.SetActive(true);
                }
            }
        }

        private void DeClutterScene()
        {
            StaticManager.BeginCoroutine(GetAllGameObjectsInSceneCoroutine());
            StaticManager.BeginCoroutine(DeClutterGameObjects());
        }

        private IEnumerator DeClutterGameObjects()
        {
            // Loop until the coroutine has finished
            while (true)
            {
                if (allGameObjectsList != null && allGameObjectsList.Count > 0)
                {
                    // Coroutine has finished, and allGameObjectsList is populated
                    GameObject[] allGameObjectsArray = allGameObjectsList.ToArray();
                    foreach (GameObject obj in allGameObjectsArray)
                    {
                        if (obj != null && ShouldDisableObject(obj))
                        {
                            obj.SetActive(false);
                            //Logger.LogInfo("Clutter Removed " + obj.name);
                            //EFT.UI.ConsoleScreen.LogError("Clutter Removed " + obj.name);
                        }
                    }
                }
                yield break;
            }
        }

        private IEnumerator GetAllGameObjectsInSceneCoroutine()
        {
            GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();

            foreach (GameObject obj in gameObjects)
            {
                bool isLODGroup = obj.GetComponent<LODGroup>() != null;
                bool isStaticDeferredDecal = obj.GetComponent<StaticDeferredDecal>() != null;
                bool isParticleSystem = obj.GetComponent<ParticleSystem>() != null;
                bool isGoodThing = isLODGroup || isStaticDeferredDecal || isParticleSystem;

                if (Configuration.framesaverFireAndSmokeEnabledConfig.Value)
                {
                    if (Configuration.declutterDecalsEnabledConfig.Value)
                    {
                        isGoodThing = isLODGroup || isStaticDeferredDecal || isParticleSystem;
                    }
                    else
                    {
                        isGoodThing = isLODGroup || isParticleSystem;
                    }
                }
                else
                {
                    if (Configuration.declutterDecalsEnabledConfig.Value)
                    {
                        isGoodThing = isLODGroup || isStaticDeferredDecal;
                    }
                    else
                    {
                        isGoodThing = isLODGroup;
                    }
                }

                bool isTarkovContainer = obj.GetComponent<LootableContainer>() != null;
                bool isTarkovContainerGroup = obj.GetComponent<LootableContainersGroup>() != null;
                bool isTarkovObservedItem = obj.GetComponent<ObservedLootItem>() != null;
                bool isTarkovItem = obj.GetComponent<LootItem>() != null;
                bool isTarkovWeaponMod = obj.GetComponent<WeaponModPoolObject>() != null;
                bool hasRainCondensator = obj.GetComponent<RainCondensator>() != null;
                bool isLocalPlayer = obj.GetComponent<LocalPlayer>() != null;
                bool isPlayer = obj.GetComponent<Player>() != null;
                bool isBotOwner = obj.GetComponent<BotOwner>() != null;
                bool isCullingObject = obj.GetComponent<CullingObject>() != null;
                bool isCullingLightObject = obj.GetComponent<CullingLightObject>() != null;
                bool isCullingGroup = obj.GetComponent<CullingGroup>() != null;
                bool isDisablerCullingObject = obj.GetComponent<DisablerCullingObject>() != null;
                bool isObservedCullingManager = obj.GetComponent<ObservedCullingManager>() != null;
                bool isPerfectCullingCrossSceneGroup = obj.GetComponent<PerfectCullingCrossSceneGroup>() != null;
                bool isScreenDistanceSwitcher = obj.GetComponent<ScreenDistanceSwitcher>() != null;
                bool isBakedLodContent = obj.GetComponent<BakedLodContent>() != null;
                bool isGuidComponent = obj.GetComponent<GuidComponent>() != null;
                bool isOcclusionPortal = obj.GetComponent<OcclusionPortal>() != null;
                bool isMultisceneSharedOccluder = obj.GetComponent<MultisceneSharedOccluder>() != null;
                bool isWindowBreaker = obj.GetComponent<WindowBreaker>() != null;
                bool isBallisticCollider = obj.GetComponent<BallisticCollider>() != null;
                bool isBotSpawner = obj.GetComponent<BotSpawner>() != null;
                bool isBadThing = isTarkovContainer || isTarkovContainerGroup || isTarkovObservedItem || isTarkovItem || isTarkovWeaponMod ||
                                  hasRainCondensator || isLocalPlayer || isPlayer || isBotOwner || isCullingObject || isCullingLightObject ||
                                  isCullingGroup || isDisablerCullingObject || isObservedCullingManager || isPerfectCullingCrossSceneGroup ||
                                  isBakedLodContent || isScreenDistanceSwitcher || isGuidComponent || isOcclusionPortal || isBotSpawner ||
                                  isMultisceneSharedOccluder || isWindowBreaker || isBallisticCollider;

                if (isGoodThing && !isBadThing)
                {
                    allGameObjectsList.Add(obj);
                }
            }

            yield break;
        }

        private bool ShouldDisableObject(GameObject obj)
        {
            if (obj == null)
            {
                // Handle the case when obj is null for whatever reason.
                return false;
            }

            bool isStaticDeferredDecal = obj.GetComponent<StaticDeferredDecal>() != null;
            bool isParticleSystem = obj.GetComponent<ParticleSystem>() != null;
            bool isGoodThing = isStaticDeferredDecal || isParticleSystem;
            GameObject childGameMeshObject = null;
            GameObject childGameColliderObject = null;
            bool childHasMesh = false;
            float sizeOnY = 3f;
            bool childHasCollider = false;
            bool foundClutterName = false;
            bool dontDisableName = dontDisableDictionary.Keys.Any(key => obj.name.ToLower().Contains(key.ToLower()));

            //EFT.UI.ConsoleScreen.LogError("Found Lod Group " + obj.name);
            if (Configuration.declutterUnscrutinizedEnabledConfig.Value == true)
            {
                foundClutterName = true;
            }
            else
            {
                foundClutterName = clutterNameDictionary.Keys.Any(key => obj.name.ToLower().Contains(key.ToLower()));
            }

            if (foundClutterName && !dontDisableName)
            {
                //EFT.UI.ConsoleScreen.LogError("Found Clutter Name" + obj.name);
                foreach (Transform child in obj.transform)
                {
                    childGameMeshObject = child.gameObject;
                    bool isTarkovContainer = childGameMeshObject.GetComponent<LootableContainer>() != null;
                    bool isTarkovContainerGroup = childGameMeshObject.GetComponent<LootableContainersGroup>() != null;
                    bool isTarkovObservedItem = childGameMeshObject.GetComponent<ObservedLootItem>() != null;
                    bool isTarkovItem = childGameMeshObject.GetComponent<LootItem>() != null;
                    bool isTarkovWeaponMod = childGameMeshObject.GetComponent<WeaponModPoolObject>() != null;
                    bool hasRainCondensator = childGameMeshObject.GetComponent<RainCondensator>() != null;
                    bool isLocalPlayer = childGameMeshObject.GetComponent<LocalPlayer>() != null;
                    bool isPlayer = childGameMeshObject.GetComponent<Player>() != null;
                    bool isBotOwner = childGameMeshObject.GetComponent<BotOwner>() != null;
                    bool isCullingObject = childGameMeshObject.GetComponent<CullingObject>() != null;
                    bool isCullingLightObject = childGameMeshObject.GetComponent<CullingLightObject>() != null;
                    bool isCullingGroup = childGameMeshObject.GetComponent<CullingGroup>() != null;
                    bool isDisablerCullingObject = childGameMeshObject.GetComponent<DisablerCullingObject>() != null;
                    bool isObservedCullingManager = childGameMeshObject.GetComponent<ObservedCullingManager>() != null;
                    bool isPerfectCullingCrossSceneGroup = childGameMeshObject.GetComponent<PerfectCullingCrossSceneGroup>() != null;
                    bool isScreenDistanceSwitcher = childGameMeshObject.GetComponent<ScreenDistanceSwitcher>() != null;
                    bool isBakedLodContent = childGameMeshObject.GetComponent<BakedLodContent>() != null;
                    bool isGuidComponent = childGameMeshObject.GetComponent<GuidComponent>() != null;
                    bool isOcclusionPortal = childGameMeshObject.GetComponent<OcclusionPortal>() != null;
                    bool isMultisceneSharedOccluder = childGameMeshObject.GetComponent<MultisceneSharedOccluder>() != null;
                    bool isWindowBreaker = childGameMeshObject.GetComponent<WindowBreaker>() != null;
                    bool isBotSpawner = childGameMeshObject.GetComponent<BotSpawner>() != null;
                    bool isBadThing = isTarkovContainer || isTarkovContainerGroup || isTarkovObservedItem || isTarkovItem || isTarkovWeaponMod ||
                                      hasRainCondensator || isLocalPlayer || isPlayer || isBotOwner || isCullingObject || isCullingLightObject ||
                                      isCullingGroup || isDisablerCullingObject || isObservedCullingManager || isPerfectCullingCrossSceneGroup ||
                                      isBakedLodContent || isScreenDistanceSwitcher || isGuidComponent || isOcclusionPortal || isBotSpawner ||
                                      isMultisceneSharedOccluder || isWindowBreaker;
                    if (isBadThing)
                    {
                        return false;
                    }
                }
                foreach (Transform child in obj.transform)
                {
                    childGameMeshObject = child.gameObject;
                    if (child.GetComponent<MeshRenderer>() != null && !childGameMeshObject.name.ToLower().Contains("shadow") && !childGameMeshObject.name.ToLower().StartsWith("col") && !childGameMeshObject.name.ToLower().EndsWith("der"))
                    {
                        childHasMesh = true;
                        // Exit the loop since we've found what we need
                        break;
                    }
                }
                if (!childHasMesh && !isGoodThing)
                {
                    return false;
                }
                foreach (Transform child in obj.transform)
                {
                    if ((child.GetComponent<MeshCollider>() != null || child.GetComponent<BoxCollider>() != null) && child.GetComponent<BallisticCollider>() == null)
                    {
                        childGameColliderObject = child.gameObject;
                        if (childGameColliderObject != null && childGameColliderObject.activeSelf)
                        {
                            childHasCollider = true;
                            // Exit the loop since we've found what we need
                            break;
                        }
                    }
                }
                if (isGoodThing)
                {
                    sizeOnY = 0.1f;
                }
                else if (childHasMesh)
                {
                    sizeOnY = GetMeshSizeOnY(childGameMeshObject);
                }
                else
                {
                    return false;
                }
                if ((childHasMesh || isGoodThing) && (!childHasCollider || isGoodThing) && sizeOnY <= 2f * Configuration.declutterScaleOffsetConfig.Value)
                {
                    savedClutterObjects.Add(obj);
                    return true;
                }
            }
            return false;
        }

        private float GetMeshSizeOnY(GameObject childGameObject)
        {
            MeshRenderer meshRenderer = childGameObject?.GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.enabled)
            {
                Bounds bounds = meshRenderer.bounds;
                return bounds.size.y;
            }
            return 0.0f;
        }
    }
}