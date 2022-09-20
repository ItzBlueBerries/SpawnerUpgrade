using HarmonyLib;
using SRML;
using SRML.Config.Attributes;
using SRML.SR;
using SRML.Utils;
using SRML.Utils.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static SRML.Console.Console;

[ConfigFile("SPAWNER_CONFIG")]
internal class Configuration
{
    public static string[] SLIMES_TO_SPAWN = new string[] {
        "PINK_SLIME", 
        "ROCK_SLIME", 
        "TABBY_SLIME", 
        "PHOSPHOR_SLIME", 
        "HONEY_SLIME", 
        "PUDDLE_SLIME",
        "HUNTER_SLIME", 
        "QUANTUM_SLIME", 
        "DERVISH_SLIME", 
        "TANGLE_SLIME",
        "SABER_SLIME",
        "RAD_SLIME",
        "BOOM_SLIME",
        "CRYSTAL_SLIME", 
        "FIRE_SLIME",
        "MOSAIC_SLIME", 
        "GOLD_SLIME",
        "LUCKY_SLIME"
    };

    public static string CUSTOM_ZONE_TARGET = "zoneNONE";

    public static int CUSTOM_TARGET_SLIME_COUNT = 12;

    public static int RANCH_TARGET_SLIME_COUNT = 12;

    public static int MOCHI_TARGET_SLIME_COUNT = 12;

    public static int VIKTOR_TARGET_SLIME_COUNT = 12;

    public static int OGDEN_TARGET_SLIME_COUNT = 12;
}

[EnumHolder]
internal class Enums
{
#pragma warning disable CS0649
    public static readonly LandPlot.Upgrade SPAWNER_UPGRADE;
#pragma warning restore CS0649
}

namespace SpawnerUpgrade
{
    public class Main : ModEntryPoint
    {
        public static SlimeSet.Member[] slimeMembers;

        [HarmonyPatch(typeof(SRModLoader), "PostLoadMods")]
        internal static class PostLoadPatch
        {
            public static void Postfix()
            {
                foreach (string toSpawn in Configuration.SLIMES_TO_SPAWN)
                    Enum.Parse(typeof(Identifiable.Id), toSpawn);

                List<SlimeSet.Member> members = new List<SlimeSet.Member>();
                foreach (string id in Configuration.SLIMES_TO_SPAWN)
                {
                    if (GameContext.Instance.LookupDirector.GetPrefab((Identifiable.Id)Enum.Parse(typeof(Identifiable.Id), id)) != null)
                        members.Add(new SlimeSet.Member() { prefab = GameContext.Instance.LookupDirector.GetPrefab((Identifiable.Id)Enum.Parse(typeof(Identifiable.Id), id)), weight = 0.5f });
                }

                slimeMembers = members.ToArray();
            }
        }

        public static Texture2D LoadImage(string filename) // thanks aidan or whoever created this at first- lol
        {
            var a = Assembly.GetExecutingAssembly();
            var spriteData = a.GetManifestResourceStream(a.GetName().Name + "." + filename);
            var rawData = new byte[spriteData.Length];
            spriteData.Read(rawData, 0, rawData.Length);
            var tex = new Texture2D(1, 1);
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Bilinear;
            return tex;
        }
        public static Sprite CreateSprite(Texture2D texture) => Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1);

        // Called before GameContext.Awake
        // You want to register new things and enum values here, as well as do all your harmony patching
        public override void PreLoad()
        {
            HarmonyInstance.PatchAll();
            TranslationPatcher.AddPediaTranslation("m.upgrade.name.corral.spawner_upgrade","Spawner Upgrade");
            TranslationPatcher.AddPediaTranslation("m.upgrade.desc.corral.spawner_upgrade", "Places a slime spawner within your Corral.");
            SRCallbacks.PreSaveGameLoad += delegate (SceneContext sceneContext)
            {
                foreach (CellDirector component in GameObject.Find("zoneRANCH").GetComponentsInChildren<CellDirector>())
                {
                    component.targetSlimeCount = Configuration.RANCH_TARGET_SLIME_COUNT;
                }
                foreach (CellDirector component in GameObject.Find("zoneVIKTOR").GetComponentsInChildren<CellDirector>())
                {
                    component.targetSlimeCount = Configuration.VIKTOR_TARGET_SLIME_COUNT;
                }
                foreach (CellDirector component in GameObject.Find("zoneMOCHI").GetComponentsInChildren<CellDirector>())
                {
                    component.targetSlimeCount = Configuration.MOCHI_TARGET_SLIME_COUNT;
                }
                foreach (CellDirector component in GameObject.Find("zoneOGDEN").GetComponentsInChildren<CellDirector>())
                {
                    component.targetSlimeCount = Configuration.OGDEN_TARGET_SLIME_COUNT;
                }

                if (GameObject.Find(Configuration.CUSTOM_ZONE_TARGET) != null)
                {
                    foreach (CellDirector component in GameObject.Find(Configuration.CUSTOM_ZONE_TARGET).GetComponentsInChildren<CellDirector>())
                    {
                        component.targetSlimeCount = Configuration.CUSTOM_TARGET_SLIME_COUNT;
                    }
                }
            };
        }

        internal class SpawnerPlotUpgrader : PlotUpgrader
        {
            public override void Apply(LandPlot.Upgrade upgrade)
            {
                if (upgrade == Enums.SPAWNER_UPGRADE)
                {
                    this.gameObject.AddComponent<SlimeSpawnerRunner>();
                }
            }
        }

        // Called before GameContext.Start
        // Used for registering things that require a loaded gamecontext
        public override void Load()
        {
            GameContext.Instance.LookupDirector.GetPlotPrefab(LandPlot.Id.CORRAL).AddComponent<SpawnerPlotUpgrader>();

            LandPlotUpgradeRegistry.UpgradeShopEntry entry = new LandPlotUpgradeRegistry.UpgradeShopEntry();
            entry.cost = 10000;
            entry.icon = CreateSprite(LoadImage("upgrade_icon.png"));
            entry.LandPlotName = "corral";
            entry.landplotPediaId = PediaDirector.Id.CORRAL;
            entry.isAvailable = ((LandPlot x) => !x.HasUpgrade(Enums.SPAWNER_UPGRADE));
            entry.mainImg = CreateSprite(LoadImage("upgrade_icon.png"));
            entry.upgrade = Enums.SPAWNER_UPGRADE;
            LandPlotUpgradeRegistry.RegisterPurchasableUpgrade<CorralUI>(entry);
        }

        // Called after all mods Load's have been called
        // Used for editing existing assets in the game, not a registry step
        public override void PostLoad()
        {

        }

    }
}
