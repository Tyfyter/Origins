global using Microsoft.Xna.Framework;
global using Vector2 = Microsoft.Xna.Framework.Vector2;
global using Vector3 = Microsoft.Xna.Framework.Vector3;
global using Vector4 = Microsoft.Xna.Framework.Vector4;
global using Color = Microsoft.Xna.Framework.Color;
global using Rectangle = Microsoft.Xna.Framework.Rectangle;
global using ALRecipeGroups = AltLibrary.Common.Systems.RecipeGroups;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Armor.Felnum;
using Origins.Items.Armor.Bleeding;
using Origins.Items.Other.Dyes;
using Origins.Items.Weapons.Ranged;
using Origins.Projectiles;
using Origins.Reflection;
using Origins.Tiles;
using Origins.Tiles.Defiled;
using Origins.UI;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using MC = Terraria.ModLoader.ModContent;
using Origins.Walls;
using Origins.Tiles.Other;
using AltLibrary.Common.AltBiomes;
using Origins.Tiles.Banners;
using Origins.Graphics;
using MonoMod.Cil;
using Origins.Buffs;
using PegasusLib;
using Origins.Items;
using Origins.Items.Other;
using Origins.NPCs.TownNPCs;
using Origins.Items.Other.Testing;
using Origins.Journal;
using Origins.Backgrounds;
using Origins.NPCs.MiscB.Shimmer_Construct;
using static Origins.OriginsSets.Items;
using Origins.World.BiomeData;
using Origins.Layers;
using Origins.Items.Vanity.Dev.PlagueTexan;
using System.Text.RegularExpressions;
using System.Text;

namespace Origins {
	public partial class Origins : Mod {
		public static Origins instance;

		public static Dictionary<int, int> ExplosiveBaseDamage { get; private set; }
		#region sets
		public static bool[] DamageModOnHit;
		public static ushort[] VanillaElements { get; private set; }
		static float[] flatDamageMultiplier;
		public static float[] FlatDamageMultiplier { get => flatDamageMultiplier; }
		static int[] wallHammerRequirement;
		public static int[] WallHammerRequirement { get => wallHammerRequirement; }
		static bool[] artifactMinion;
		public static bool[] ArtifactMinion { get => artifactMinion; }
		static bool[] brothBuffs;
		public static bool[] BrothBuffs { get => brothBuffs; }
		static bool[] isFineWithCrowdedParties;
		public static bool[] IsFineWithCrowdedParties { get => isFineWithCrowdedParties; }
		static bool[] tileTransformsOnKill;
		public static bool[] TileTransformsOnKill { get => tileTransformsOnKill; }
		static bool[] tileBlocksMinecartTracks;
		public static bool[] TileBlocksMinecartTracks { get => tileBlocksMinecartTracks; }
		static bool[] wallBlocksMinecartTracks;
		public static bool[] WallBlocksMinecartTracks { get => wallBlocksMinecartTracks; }
		public static bool[] ForceFelnumShockOnShoot => OriginsSets.Projectiles.ForceFelnumShockOnShoot;
		public static float[] HomingEffectivenessMultiplier => OriginsSets.Projectiles.HomingEffectivenessMultiplier;
		public static int[] MagicTripwireRange => OriginsSets.Projectiles.MagicTripwireRange;
		public static int[] MagicTripwireDetonationStyle => OriginsSets.Projectiles.MagicTripwireDetonationStyle;
		public static bool[] ItemsThatAllowRemoteRightClick => OriginsSets.Items.ItemsThatAllowRemoteRightClick;
		public static float[] DamageBonusScale => OriginsSets.Items.DamageBonusScale;
		public static bool[] SpecialPrefix => OriginsSets.Prefixes.SpecialPrefix;
		#endregion
		public static short[] itemGlowmasks = [];
		public static Dictionary<int, (int maxLevel, float accelerationFactor, float velocityFactor)> RasterizeAdjustment { get; private set; }
		public static Dictionary<int, ModBiome> NPCOnlyTargetInBiome { get; private set; } = [];
		public static Dictionary<int, (ushort potType, int minStyle, int maxStyle)> PotType { get; private set; }
		public static Dictionary<int, (ushort pileType, int minStyle, int maxStyle)> PileType { get; private set; }
		#region Armor IDs
		public static int FelnumHeadArmorID { get; private set; }
		public static int FelnumBodyArmorID { get; private set; }
		public static int FelnumLegsArmorID { get; private set; }
		public static int AncientFelnumHeadArmorID { get; private set; }
		public static int AncientFelnumBodyArmorID { get; private set; }
		public static int AncientFelnumLegsArmorID { get; private set; }

		public static int PlagueTexanJacketID { get; private set; }

		public static int RiftHeadArmorID { get; private set; }
		public static int RiftBodyArmorID { get; private set; }
		public static int RiftLegsArmorID { get; private set; }
		public static Dictionary<int, AutoCastingAsset<Texture2D>> TorsoLegLayers { get; private set; }

		#endregion Armor IDs
		public static int[] celestineBoosters;

		public static MiscShaderData perlinFade0;
		public static MiscShaderData blackHoleShade;
		public static ArmorShaderData solventShader;
		public static MiscShaderData rasterizeShader;
		public static ArmorShaderData amebicProtectionShader;
		public static ArmorShaderData journalDrawingShader;
		public static ArmorShaderData journalTransparentShader;
		public static HairShaderData amebicProtectionHairShader;
		public static ArmorShaderData coordinateMaskFilter;
		public static ArmorShaderData tileOutlineShader;
		public static int amebicProtectionShaderID;
		public static int amebicProtectionHairShaderID;
		public static int coordinateMaskFilterID;
		public static int transparencyFilterID;
		public static AutoCastingAsset<Texture2D> cellNoiseTexture;
		public static AutoCastingAsset<Texture2D> eyndumCoreUITexture;
		public static AutoCastingAsset<Texture2D> eyndumCoreTexture;
		public static Texture2D[] CloudBottoms;
		public static List<IUnloadable> unloadables = [];
		public static List<ITicker> tickers = [];
		public static List<object> loggedErrors = [];
		public static List<LocalizedText> loadingWarnings = [];
		public override uint ExtraPlayerBuffSlots => 4;
		public Origins() {
			instance = this;
			celestineBoosters = new int[3];
			List<LocalizedText> loadingWarnings = [];
			this.MusicAutoloadingEnabled = false;
#if DEBUG
			try {
				MethodInfo meth = typeof(ModType).GetMethod(nameof(ModType.PrettyPrintName));
				if (meth is not null) {
					MonoModHooks.Modify(meth, il => {
						ILCursor c = new(il);
						c.GotoNext(MoveType.Before, ILPatternMatchingExt.MatchRet);
						c.EmitLdstr("_ ");
						c.EmitLdstr(" ");
						c.EmitCall(typeof(String).GetMethod(nameof(String.Replace), [typeof(string), typeof(string)]));
					});
				}
			} catch (Exception e) {
				_ = e;
			}
#endif
		}
		internal void LateLoad() {
			#region vanilla explosive base damage registry
			ExplosiveBaseDamage.Add(ProjectileID.Bomb, 70);
			ExplosiveBaseDamage.Add(ProjectileID.StickyBomb, 70);
			ExplosiveBaseDamage.Add(ProjectileID.BouncyBomb, 70);
			ExplosiveBaseDamage.Add(ProjectileID.BombFish, 70);
			ExplosiveBaseDamage.Add(ProjectileID.Dynamite, 175);
			ExplosiveBaseDamage.Add(ProjectileID.StickyDynamite, 175);
			ExplosiveBaseDamage.Add(ProjectileID.BouncyDynamite, 175);
			ExplosiveBaseDamage.Add(ProjectileID.DryBomb, 70);
			ExplosiveBaseDamage.Add(ProjectileID.WetBomb, 70);
			ExplosiveBaseDamage.Add(ProjectileID.LavaBomb, 70);
			ExplosiveBaseDamage.Add(ProjectileID.HoneyBomb, 70);
			ExplosiveBaseDamage.Add(ProjectileID.ScarabBomb, 100);
			#endregion vanilla explosive base damage registry
			#region armor slot ids
			FelnumHeadArmorID = ModContent.GetInstance<Felnum_Helmet>().Item.headSlot;
			FelnumBodyArmorID = ModContent.GetInstance<Felnum_Breastplate>().Item.bodySlot;
			FelnumLegsArmorID = ModContent.GetInstance<Felnum_Greaves>().Item.legSlot;
			AncientFelnumHeadArmorID = ModContent.GetInstance<Ancient_Felnum_Helmet>().Item.headSlot;
			AncientFelnumBodyArmorID = ModContent.GetInstance<Ancient_Felnum_Breastplate>().Item.bodySlot;
			AncientFelnumLegsArmorID = ModContent.GetInstance<Ancient_Felnum_Greaves>().Item.legSlot;
			PlagueTexanJacketID = ModContent.GetInstance<Plague_Texan_Jacket>().Item.bodySlot;
			RiftHeadArmorID = ModContent.GetInstance<Bleeding_Obsidian_Helmet>().Item.headSlot;
			RiftBodyArmorID = ModContent.GetInstance<Bleeding_Obsidian_Breastplate>().Item.bodySlot;
			RiftLegsArmorID = ModContent.GetInstance<Bleeding_Obsidian_Greaves>().Item.legSlot;
			#endregion
			//Logger.Info("fixing tilemerge for " + OriginTile.IDs.Count + " tiles");
			//Main.tileMerge[TileID.Sand][TileID.Sandstone] = true;
			//Main.tileMerge[TileID.Sand][TileID.HardenedSand] = true;
			//Main.tileMerge[TileID.Sandstone][TileID.HardenedSand] = true;
			for (int oID = 0; oID < OriginTile.IDs.Count; oID++) {
				OriginTile oT = OriginTile.IDs[oID];
				if (oT.mergeID == oT.Type || oT.mergeID == -1) continue;
				//Main.tileMergeDirt[oT.Type] = Main.tileMergeDirt[oT.mergeID];
				Main.tileMerge[oT.Type] = Main.tileMerge[oT.mergeID];
				Main.tileMerge[oT.mergeID][oT.Type] = true;
				Main.tileMerge[oT.Type][oT.mergeID] = true;
				for (int i = 0; i < TileLoader.TileCount; i++) {
					if (Main.tileMerge[oT.mergeID][i]) {
						Main.tileMerge[i][oT.Type] = true;
					}
					if (Main.tileMerge[i][oT.mergeID]) {
						Main.tileMerge[i][oT.Type] = true;
					}
				}
			}
			for (int i = 0; i < OriginTile.LateSetupActions.Count; i++) {
				OriginTile.LateSetupActions[i]();
			}
			if (OriginConfig.Instance.GrassMerge) {
				List<int> grasses = new List<int>() { };
				for (int i = 0; i < TileLoader.TileCount; i++) {
					if (TileID.Sets.Grass[i] || TileID.Sets.GrassSpecial[i]) {
						grasses.Add(i);
					}
				}
				IEnumerable<(int, int)> pairs = grasses.SelectMany(
					(val, index) => grasses.Skip(index + 1),
					(a, b) => (a, b)
				);
				foreach ((int, int) pair in pairs) {
					Main.tileMerge[pair.Item1][pair.Item2] = true;
					Main.tileMerge[pair.Item2][pair.Item1] = true;
				}
			}
			MC.GetInstance<CorruptionAltBiome>().AddChambersiteConversions(MC.TileType<Chambersite_Ore_Ebonstone>(), MC.WallType<Chambersite_Ebonstone_Wall>());
			MC.GetInstance<CrimsonAltBiome>().AddChambersiteConversions(MC.TileType<Chambersite_Ore_Crimstone>(), MC.WallType<Chambersite_Crimstone_Wall>());
			UnstableHooking.IL_Main_DoDraw += Defiled_Wastelands_Mod_Menu.EnableShaderOnMenu;
			OriginsModIntegrations.LateLoad();
			_ = OriginExtensions.StrikethroughFont;
			for (int k = 0; k < ItemLoader.ItemCount; k++) {
				Item item = ContentSamples.ItemsByType[k];
				if (item.createTile > -1 && OriginsSets.Tiles.PlacementItem[item.createTile] == -1 && (!Main.tileFrameImportant[item.createTile] || TileID.Sets.Torch[item.createTile])) {
					OriginsSets.Tiles.PlacementItem[item.createTile] = item.type;
				}
			}
			for (int i = 0; i < OriginsSets.Tiles.PlacementItem.Length; i++) {
				int placementItem = OriginsSets.Tiles.PlacementItem[i];
				if (placementItem < 0) continue;
				if (ItemID.Sets.ShimmerCountsAsItem[placementItem] >= 0) placementItem = ItemID.Sets.ShimmerCountsAsItem[placementItem];
				if (ItemID.Sets.ShimmerTransformToItem[placementItem] < 0) continue;
				Item item = ContentSamples.ItemsByType[ItemID.Sets.ShimmerTransformToItem[placementItem]];
				if (item.createTile > -1) {
					OriginsSets.Tiles.ShimmerTransformToTile[i] = item.createTile;
				}
			}
			OriginsSets.Misc.SetupDyes();
		}
		public override void Load() {
			AssimilationLoader.Load();
			LocalizationMethods.BindArgs(Language.GetOrRegister("Riven", () => "{0}"), Language.GetTextValue("Mods.Origins.Generic.Riven"));
			LocalizationMethods.BindArgs(Language.GetOrRegister("Dusk", () => "{0}"), Language.GetTextValue("Mods.Origins.Generic.Dusk"));
			LocalizationMethods.BindArgs(Language.GetOrRegister("Defiled", () => "{0}"), Language.GetTextValue("Mods.Origins.Generic.Defiled"));
			LocalizationMethods.BindArgs(Language.GetOrRegister("Defiled_Wastelands", () => "{0}"), Language.GetTextValue("Mods.Origins.Generic.Defiled_Wastelands"));
			LocalizationMethods.BindArgs(Language.GetOrRegister("The_Defiled_Wastelands", () => "{0}"), Language.GetTextValue("Mods.Origins.Generic.The_Defiled_Wastelands")); 
			LocalizationMethods.BindArgs(Language.GetOrRegister("the_Defiled_Wastelands", () => "{0}"), Language.GetTextValue("Mods.Origins.Generic.the_Defiled_Wastelands")); 

			RasterizeAdjustment = [];
			ExplosiveBaseDamage = [];
			DamageModOnHit = new bool[ProjectileLoader.ProjectileCount];
			DamageModOnHit[ProjectileID.Bomb] = true;
			DamageModOnHit[ProjectileID.StickyBomb] = true;
			DamageModOnHit[ProjectileID.BouncyBomb] = true;
			DamageModOnHit[ProjectileID.BombFish] = true;
			DamageModOnHit[ProjectileID.Dynamite] = true;
			DamageModOnHit[ProjectileID.StickyDynamite] = true;
			DamageModOnHit[ProjectileID.BouncyDynamite] = true;
			DamageModOnHit[ProjectileID.DryBomb] = true;
			DamageModOnHit[ProjectileID.WetBomb] = true;
			DamageModOnHit[ProjectileID.LavaBomb] = true;
			DamageModOnHit[ProjectileID.HoneyBomb] = true;
			DamageModOnHit[ProjectileID.ScarabBomb] = true;
			#region vanilla weapon elements
			VanillaElements = ItemID.Sets.Factory.CreateUshortSet(0,
			#region fire
				(ushort)ItemID.FlamingArrow, Elements.Fire,
				(ushort)ItemID.FlareGun, Elements.Fire,
				(ushort)ItemID.WandofSparking, Elements.Fire,
				(ushort)ItemID.FieryGreatsword, Elements.Fire,
				(ushort)ItemID.MoltenPickaxe, Elements.Fire,
				(ushort)ItemID.MoltenHamaxe, Elements.Fire,
				(ushort)ItemID.ImpStaff, Elements.Fire,
				(ushort)ItemID.FlowerofFire, Elements.Fire,
				(ushort)ItemID.Flamelash, Elements.Fire,
				(ushort)ItemID.Sunfury, Elements.Fire,
				(ushort)ItemID.Flamethrower, Elements.Fire,
				(ushort)ItemID.ElfMelter, Elements.Fire,
				(ushort)ItemID.InfernoFork, Elements.Fire,
				(ushort)ItemID.Cascade, Elements.Fire,
				(ushort)ItemID.HelFire, Elements.Fire,
				(ushort)ItemID.HellwingBow, Elements.Fire,
				(ushort)ItemID.PhoenixBlaster, Elements.Fire,
				(ushort)ItemID.MoltenFury, Elements.Fire,
				(ushort)ItemID.DD2FlameburstTowerT1Popper, Elements.Fire,
				(ushort)ItemID.DD2FlameburstTowerT2Popper, Elements.Fire,
				(ushort)ItemID.DD2FlameburstTowerT3Popper, Elements.Fire,
				(ushort)ItemID.DD2PhoenixBow, Elements.Fire,
				(ushort)ItemID.FrostburnArrow, Elements.Fire | Elements.Ice,
				(ushort)ItemID.FlowerofFrost, Elements.Fire | Elements.Ice,
				(ushort)ItemID.Amarok, Elements.Fire | Elements.Ice,
				(ushort)ItemID.CursedArrow, Elements.Fire | Elements.Acid,
				(ushort)ItemID.CursedBullet, Elements.Fire | Elements.Acid,
				(ushort)ItemID.CursedFlames, Elements.Fire | Elements.Acid,
				(ushort)ItemID.CursedDart, Elements.Fire | Elements.Acid,
				(ushort)ItemID.ClingerStaff, Elements.Fire | Elements.Acid,
				(ushort)ItemID.ShadowFlameBow, Elements.Fire,
				(ushort)ItemID.ShadowFlameHexDoll, Elements.Fire,
				(ushort)ItemID.ShadowFlameKnife, Elements.Fire,
				(ushort)ItemID.SolarFlareAxe, Elements.Fire,
				(ushort)ItemID.SolarFlareChainsaw, Elements.Fire,
				(ushort)ItemID.SolarFlareDrill, Elements.Fire,
				(ushort)ItemID.SolarFlareHammer, Elements.Fire,
				(ushort)ItemID.SolarFlarePickaxe, Elements.Fire,
				(ushort)ItemID.DayBreak, Elements.Fire,
				(ushort)ItemID.SolarEruption, Elements.Fire,
			#endregion fire
			#region ice
				(ushort)ItemID.IceBlade, Elements.Ice,
				(ushort)ItemID.IceBoomerang, Elements.Ice,
				(ushort)ItemID.IceRod, Elements.Ice,
				(ushort)ItemID.IceBow, Elements.Ice,
				(ushort)ItemID.IceSickle, Elements.Ice,
				(ushort)ItemID.FrostDaggerfish, Elements.Ice,
				(ushort)ItemID.FrostStaff, Elements.Ice,
				(ushort)ItemID.Frostbrand, Elements.Ice,
				(ushort)ItemID.StaffoftheFrostHydra, Elements.Ice,
				(ushort)ItemID.NorthPole, Elements.Ice,
				(ushort)ItemID.BlizzardStaff, Elements.Ice,
				(ushort)ItemID.SnowballCannon, Elements.Ice,
				(ushort)ItemID.SnowmanCannon, Elements.Ice,
			#endregion ice
			#region earth
				(ushort)ItemID.CrystalBullet, Elements.Earth,
				(ushort)ItemID.CrystalDart, Elements.Earth,
				(ushort)ItemID.CrystalSerpent, Elements.Earth,
				(ushort)ItemID.CrystalStorm, Elements.Earth,
				(ushort)ItemID.CrystalVileShard, Elements.Earth,
				(ushort)ItemID.MeteorStaff, Elements.Earth,
				(ushort)ItemID.Seedler, Elements.Earth,
				(ushort)ItemID.MushroomSpear, Elements.Earth,
				(ushort)ItemID.Hammush, Elements.Earth,
				(ushort)ItemID.StaffofEarth, Elements.Earth,
				(ushort)ItemID.BladeofGrass, Elements.Earth,
				(ushort)ItemID.ThornChakram, Elements.Earth,
				(ushort)ItemID.PoisonStaff, Elements.Earth,
				(ushort)ItemID.Toxikarp, Elements.Earth,
				(ushort)ItemID.VenomArrow, Elements.Earth,
				(ushort)ItemID.VenomBullet, Elements.Earth,
				(ushort)ItemID.VenomStaff, Elements.Earth,
				(ushort)ItemID.SpiderStaff, Elements.Earth,
				(ushort)ItemID.QueenSpiderStaff, Elements.Earth,
				(ushort)ItemID.ChlorophyteArrow, Elements.Earth,
				(ushort)ItemID.ChlorophyteBullet, Elements.Earth,
				(ushort)ItemID.ChlorophyteChainsaw, Elements.Earth,
				(ushort)ItemID.ChlorophyteClaymore, Elements.Earth,
				(ushort)ItemID.ChlorophyteDrill, Elements.Earth,
				(ushort)ItemID.ChlorophyteGreataxe, Elements.Earth,
				(ushort)ItemID.ChlorophyteJackhammer, Elements.Earth,
				(ushort)ItemID.ChlorophytePartisan, Elements.Earth,
				(ushort)ItemID.ChlorophytePickaxe, Elements.Earth,
				(ushort)ItemID.ChlorophyteSaber, Elements.Earth,
				(ushort)ItemID.ChlorophyteShotbow, Elements.Earth,
				(ushort)ItemID.ChlorophyteWarhammer, Elements.Earth);
			#endregion earth
			#endregion vanilla weapon elements
			PotType = new();
			PileType = new();

			TorsoLegLayers = new();

			OriginExtensions.initExt();
			if (!Main.dedServ) {
				//OriginExtensions.drawPlayerItemPos = (Func<float, int, Vector2>)typeof(Main).GetMethod("DrawPlayerItemPos", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate(typeof(Func<float, int, Vector2>), Main.instance);
				perlinFade0 = new MiscShaderData(Assets.Request<Effect>("Effects/PerlinFade", AssetRequestMode.ImmediateLoad), "RedFade");
				//perlinFade0.UseImage("Images/Misc/Perlin");
				perlinFade0.Shader.Parameters["uThreshold0"].SetValue(0.6f);
				perlinFade0.Shader.Parameters["uThreshold1"].SetValue(0.6f);
				blackHoleShade = new MiscShaderData(Assets.Request<Effect>("Effects/BlackHole"), "BlackHole");

				Filters.Scene["Origins:TestingShader"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/TestingShader"), "TestingShader"), EffectPriority.VeryHigh);
				Filters.Scene["Origins:ZoneDusk"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/BiomeShade"), "VoidShade"), EffectPriority.High);
				if (Filters.Scene["Origins:ZoneDefiled"]?.IsActive() ?? false) {
					Filters.Scene["Origins:ZoneDefiled"].Deactivate();
				}
				Filters.Scene["Origins:ZoneDefiled"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/BiomeShade"), "DefiledShade")
					.UseImage(MC.Request<Texture2D>("Terraria/Images/Misc/noise"), 0),
				EffectPriority.High);
				Overlays.Scene["Origins:ZoneDefiled"] = new Tangela_Resaturate_Overlay();
				Overlays.Scene["Origins:MaskedRasterizeFilter"] = new Tangela_Resaturate_Overlay();
				Filters.Scene["Origins:ZoneFiberglassUndergrowth"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/Misc"), "NoScreenShader"));
				Overlays.Scene["Origins:ZoneFiberglassUndergrowth"] = new Fiberglass_Background();

				Filters.Scene["Origins:ShimmerConstructPhase3Underlay"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/Misc"), "NoScreenShader"));
				Filters.Scene["Origins:ShimmerConstructPhase3Midlay"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/Misc"), "NoScreenShader"));
				Filters.Scene["Origins:ShimmerConstructPhase3"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/Misc"), "NoScreenShader"));
				Filters.Scene["Origins:ShimmerConstructPhase3Cheap"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/Misc"), "NoScreenShader"));
				Overlays.Scene["Origins:ShimmerConstructPhase3Underlay"] = ModContent.GetInstance<SC_Phase_Three_Underlay>();
				Overlays.Scene["Origins:ShimmerConstructPhase3Midlay"] = ModContent.GetInstance<SC_Phase_Three_Midlay>();
				Overlays.Scene["Origins:ShimmerConstructPhase3"] = ModContent.GetInstance<SC_Phase_Three_Overlay>();
				Overlays.Scene["Origins:ShimmerConstructPhase3Cheap"] = new Cheap_SC_Phase_Three_Underlay();

				Filters.Scene["Origins:MaskedRasterizeFilter"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/MaskedRasterizeFilter"), "MaskedRasterizeFilter"), EffectPriority.VeryHigh);
				Filters.Scene["Origins:VolatileGelatinFilter"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/MaskedPurpleJellyFilter"), "MaskedPurpleJellyFilter"), EffectPriority.VeryHigh);
				Filters.Scene["Origins:RivenBloodCoating"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/RivenBloodCoating"), "RivenBloodCoating"), EffectPriority.VeryHigh);
				Filters.Scene["Origins:RivenBloodCoating"].GetShader().UseImage(Assets.Request<Texture2D>("Textures/Riven_Blood_Map"), 0, SamplerState.PointWrap);

				Filters.Scene["Origins:ChineseRivenBloodCoating"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/RivenBloodCoating"), "ChineseRivenBloodCoating"), EffectPriority.VeryHigh);

				Filters.Scene["Origins:MaskedTornFilter"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/MaskedTornFilter"), "MaskedTornFilter"), EffectPriority.VeryHigh);
				Filters.Scene["Origins:MaskedTornFilter"].GetShader().UseImage(Assets.Request<Texture2D>("Textures/Torn_Texture"), 0, SamplerState.PointWrap);
				//Filters.Scene["Origins:ZoneRiven"] = new Filter(new ScreenShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/BiomeShade").Value), "RivenShade"), EffectPriority.High);

				solventShader = new ArmorShaderData(Assets.Request<Effect>("Effects/Solvent"), "Dissolve");
				cellNoiseTexture = Assets.Request<Texture2D>("Textures/Cell_Noise_Pixel");
				Filters.Scene["Origins:MaskedRasterizeFilter"].GetShader().UseImage(cellNoiseTexture, 2);
				Filters.Scene["Origins:VolatileGelatinFilter"].GetShader().UseImage(cellNoiseTexture, 2);

				rasterizeShader = new MiscShaderData(Assets.Request<Effect>("Effects/Rasterize"), "Rasterize");
				GameShaders.Misc["Origins:Rasterize"] = rasterizeShader;


				amebicProtectionShader = new ArmorShaderData(Assets.Request<Effect>("Effects/AmebicProtection"), "AmebicProtection");
				GameShaders.Armor.BindShader(MC.ItemType<Amebic_Vial>(), amebicProtectionShader);
				amebicProtectionShaderID = GameShaders.Armor.GetShaderIdFromItemId(MC.ItemType<Amebic_Vial>());

				amebicProtectionHairShader = new HairShaderData(Assets.Request<Effect>("Effects/AmebicProtection"), "AmebicProtection");
				GameShaders.Hair.BindShader(MC.ItemType<Amebic_Vial>(), amebicProtectionHairShader);
				amebicProtectionHairShaderID = GameShaders.Hair.GetShaderIdFromItemId(MC.ItemType<Amebic_Vial>());

				coordinateMaskFilter = new ArmorShaderData(Assets.Request<Effect>("Effects/CoordinateMaskFilter"), "CoordinateMask");
				GameShaders.Armor.BindShader(MC.ItemType<Tainted_Flesh>(), coordinateMaskFilter);
				coordinateMaskFilterID = GameShaders.Armor.GetShaderIdFromItemId(MC.ItemType<Tainted_Flesh>());

				ArmorShaderData transparencyFilter = new ArmorShaderData(Assets.Request<Effect>("Effects/CoordinateMaskFilter"), "Transparency");
				GameShaders.Armor.BindShader(MC.ItemType<Defiled_Altar_Item>(), transparencyFilter);
				transparencyFilterID = GameShaders.Armor.GetShaderIdFromItemId(MC.ItemType<Defiled_Altar_Item>());

				tileOutlineShader = new ArmorShaderData(Assets.Request<Effect>("Effects/TileOutline", AssetRequestMode.ImmediateLoad), "TileOutline");
				GameShaders.Armor.BindShader(MC.ItemType<High_Contrast_Dye>(), tileOutlineShader);
				tileOutlineShader.Shader.Parameters["uImageSize0"].SetValue(Main.ScreenSize.ToVector2());
				tileOutlineShader.Shader.Parameters["uScale"].SetValue(2);
				tileOutlineShader.Shader.Parameters["uColor"].SetValue(new Vector3(1f, 1f, 1f));

				GameShaders.Misc["Origins:SapphireAura"] = new MiscShaderData(Assets.Request<Effect>("Effects/Strip"), "SapphireAura")
				.UseImage0(TextureAssets.Extra[194]);

				GameShaders.Misc["Origins:Framed"] = new MiscShaderData(Assets.Request<Effect>("Effects/Strip"), "Framed");
				GameShaders.Misc["Origins:AnimatedTrail"] = new MiscShaderData(Assets.Request<Effect>("Effects/Strip"), "AnimatedTrail").UseSamplerState(SamplerState.PointWrap);
				GameShaders.Misc["Origins:LaserBlade"] = new MiscShaderData(Assets.Request<Effect>("Effects/Strip"), "LaserBlade").UseSamplerState(SamplerState.PointWrap);
				GameShaders.Misc["Origins:Identity"] = new MiscShaderData(Assets.Request<Effect>("Effects/Strip"), "Identity")
				.UseSamplerState(SamplerState.PointClamp);
				GameShaders.Misc["Origins:DefiledIndicator"] = new MiscShaderData(Assets.Request<Effect>("Effects/DefiledIndicator"), "DefiledIndicator");
				GameShaders.Misc["Origins:DefiledSpike"] = new MiscShaderData(Assets.Request<Effect>("Effects/DefiledSpike"), "DefiledSpike");
				GameShaders.Misc["Origins:DefiledPortal"] = new MiscShaderData(Assets.Request<Effect>("Effects/DefiledPortal"), "DefiledPortal");
				GameShaders.Misc["Origins:DefiledLaser"] = new MiscShaderData(Assets.Request<Effect>("Effects/DefiledLaser"), "DefiledLaser");
				GameShaders.Misc["Origins:DefiledLaser2"] = new MiscShaderData(Assets.Request<Effect>("Effects/Tangela"), "DefiledLaser")
				.UseImage2(ModContent.Request<Texture2D>("Terraria/Images/Misc/Perlin"))
				.UseImage1(ModContent.Request<Texture2D>("Terraria/Images/Misc/noise"));
				GameShaders.Misc["Origins:SilhouetteShader"] = new MiscShaderData(Assets.Request<Effect>("Effects/SilhouetteShader"), "SilhouetteShader");
				GameShaders.Misc["Origins:ShimmerConstructSDF"] = new MiscShaderData(Assets.Request<Effect>("Effects/ShimmerConstructSDF"), "ShimmerConstructSDF");
				GameShaders.Misc["Origins:SC_DustEffect"] = new MiscShaderData(Assets.Request<Effect>("Effects/SC_DustEffect"), "SC_DustEffect");

				GameShaders.Misc["Origins:Beam"] = new MiscShaderData(Assets.Request<Effect>("Effects/Beam"), "Beam")
				.UseSamplerState(SamplerState.PointClamp);

				journalDrawingShader = new ArmorShaderData(Assets.Request<Effect>("Effects/Journal"), "Drawing");
				GameShaders.Armor.BindShader(MC.ItemType<Journal_Item>(), journalDrawingShader);

				journalTransparentShader = new ArmorShaderData(Assets.Request<Effect>("Effects/Journal"), "LightnessToTransparency");
				GameShaders.Armor.BindShader(MC.ItemType<Framing_Tester>(), journalTransparentShader);

				GameShaders.Misc["Origins:Constellation"] = new MiscShaderData(Assets.Request<Effect>("Effects/ConstellationMod"), "ConstellationMod")
				.UseImage1(ModContent.Request<Texture2D>("Terraria/Images/Misc/Perlin"));

				TangelaVisual.LoadShader();

				//amebicProtectionShaderID = GameShaders.Armor.GetShaderIdFromItemId(MC.ItemType<Amebic_Vial>());
				//Filters.Scene["Origins:ZoneDusk"].GetShader().UseOpacity(0.35f);
				//Ref<Effect> screenRef = new Ref<Effect>(GetEffect("Effects/ScreenDistort")); // The path to the compiled shader file.
				//Filters.Scene["BlackHole"] = new Filter(new ScreenShaderData(screenRef, "BlackHole"), EffectPriority.VeryHigh);
				//Filters.Scene["BlackHole"].Load();
				eyndumCoreUITexture = Assets.Request<Texture2D>("UI/CoreSlot");
				eyndumCoreTexture = Assets.Request<Texture2D>("Items/Armor/Eyndum/Eyndum_Breastplate_Body_Core");
				Journal_UI_Button.Texture = Assets.Request<Texture2D>("UI/Lore/Journal");
				Journal_UI_Open.BackTexture = Assets.Request<Texture2D>("UI/Lore/Journal_Use_Back");
				Journal_UI_Open.PageTexture = Assets.Request<Texture2D>("UI/Lore/Journal_Use");
				Journal_UI_Open.TabsTexture = Assets.Request<Texture2D>("UI/Lore/Journal_Tabs");
				On_Player.KeyDoubleTap += (On_Player.orig_KeyDoubleTap orig, Player self, int keyDir) => {
					orig(self, keyDir);
					if (OriginClientConfig.Instance.SetBonusDoubleTap) {
						if (keyDir == (Main.ReversedUpDownArmorSetBonuses ? 1 : 0)) {
							self.GetModPlayer<OriginPlayer>().TriggerSetBonus();
						}
					}
				};
				/*List<string> files = GetFileNames();
				for (int i = 0; i < files.Count; i++) {
					string file = files[i];
					string name = file.Split('/')[^1];
					string[] nameParts = name.Split('.');
					string pureName = string.Join('.', nameParts[0..^1]);
					string extension = nameParts[^1];
					if (extension == "rawimg" && file.Contains("Gores") && !files.Contains(file.Replace(".rawimg", ".cs"))) {
						AutoLoadGores.AddGore("Origins/" + file.Replace(".rawimg", null), this);
					}
				}*/

				/*Task.Run(async () => {
					await Task.Yield();
					lock (CloudBottoms) {
						for (int i = 0; i < TextureAssets.Cloud.Length; i++) {
							TextureAssets.Cloud[i].Wait();
							Texture2D baseCloud = TextureAssets.Cloud[i].Value;
							Texture2D bottom = new Texture2D(baseCloud.GraphicsDevice, baseCloud.Width, baseCloud.Height) {
								Name = baseCloud.Name + "_Bottom"
							};
							Color[] colorData = new Color[baseCloud.Width * baseCloud.Height];
							baseCloud.GetData(colorData);
							bottom.SetData(colorData.Select(c => new Color(255 - c.B, 255 - c.B, 255 - c.B, 0)).ToArray());
							CloudBottoms[i] = bottom;
						}
					}
				});*/
				FontAssets.MouseText.Value.SpriteCharacters.TryAdd('\u200C'/*zero-width non-joiner*/, new(Asset<Texture2D>.DefaultValue, default, default, Vector3.Zero));
			}
			ChatManager.Register<Journal_Control_Handler>([
				"jctrl"
			]);
			ChatManager.Register<Journal_Link_Handler>([
				"journal",
				"j"
			]);
			ChatManager.Register<Journal_Series_Header_Handler>([
				"jsh"
			]);
			ChatManager.Register<Journal_Entry_Handler>([
				"jentry"
			]);
			ChatManager.Register<Quest_Link_Handler>([
				"quest",
				"q"
			]);
			ChatManager.Register<Quest_Stage_Snippet_Handler>([
				"queststage",
				"qs"
			]);
			ChatManager.Register<Header_Snippet_Handler>([
				"header"
			]);
			ChatManager.Register<NPC_Name_Handler>([
				"npcname"
			]);
			ChatManager.Register<Item_Name_Handler>([
				"itemname"
			]);
			ChatManager.Register<Imperfect_Item_Name_Handler>([
				"imperfect"
			]);
			ChatManager.Register<Player_Name_Handler>([
				"player",
				"you"
			]);
			ChatManager.Register<Player_Head_Handler>([
				"head"
			]);
			ChatManager.Register<Evil_Handler>([
				"evil"
			]);
			ChatManager.Register<Journal_Portrait_Handler>([
				"jportrait"
			]);
			ChatManager.Register<Image_Handler>([
				"jimage"
			]);
			ChatManager.Register<Item_Hint_Handler>([
				"itemhint"
			]);
			ChatManager.Register<Wiggle_Handler>([
				"wiggle"
			]);
			ChatManager.Register<Word_Snippet_Handler>([
				"word"
			]);
			ChatManager.Register<Centered_Snippet_Handler>([
				"centered"
			]);
			ChatManager.Register<Glitch_Handler>([
				"glitch"
			]);
			ChatManager.Register<Quest_Reward_Item_List_Handler>([
				"qian"
			]);
			ChatManager.Register<Italics_Snippet_Handler>([
				"italic",
				"italics",
				"italicize"
			]);
			ChatManager.Register<AF_Alt_Handler>([
				"fool"
			]);
			Sounds.MultiWhip = new SoundStyle("Terraria/Sounds/Item_153", SoundType.Sound) {
				MaxInstances = 0,
				SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
				PitchVariance = 0f
			};
			Sounds.Krunch = new SoundStyle("Origins/Sounds/Custom/BurstCannon", SoundType.Sound);
			Sounds.HeavyCannon = new SoundStyle("Origins/Sounds/Custom/HeavyCannon", SoundType.Sound);
			Sounds.PowerUp = new SoundStyle("Origins/Sounds/Custom/PowerUp", SoundType.Sound);
			Sounds.EnergyRipple = new SoundStyle("Origins/Sounds/Custom/EnergyRipple", SoundType.Sound);
			Sounds.PhaserCrash = new SoundStyle("Origins/Sounds/Custom/PhaserCrash", SoundType.Sound);
			Sounds.DeepBoom = new SoundStyle("Origins/Sounds/Custom/DeepBoom", SoundType.Sound) {
				MaxInstances = 0
			};
			Sounds.DefiledIdle = new SoundStyle("Origins/Sounds/Custom/Defiled_Idle", new int[] { 2, 3 }, SoundType.Sound) {
				MaxInstances = 0,
				Volume = 0.4f,
				PitchRange = (0.9f, 1.1f)
			};
			Sounds.DefiledHurt = new SoundStyle("Origins/Sounds/Custom/Defiled_Hurt", new int[] { 1, 2 }, SoundType.Sound) {
				MaxInstances = 0,
				Volume = 0.4f,
				PitchRange = (0.9f, 1.1f)
			};
			Sounds.defiledKill = new SoundStyle("Origins/Sounds/Custom/Defiled_Kill1", SoundType.Sound) {
				MaxInstances = 0,
				Volume = 0.4f,
				PitchRange = (0.9f, 1.1f)
			};
			Sounds.defiledKillAF = new SoundStyle("Origins/Sounds/Custom/BlockusLand", SoundType.Sound) {
				MaxInstances = 0,
				Volume = 1.0f,
				PitchRange = (0.9f, 1.1f)
			};
			Sounds.Amalgamation = new SoundStyle("Origins/Sounds/Custom/ChorusRoar", SoundType.Sound) {
				MaxInstances = 0,
				Volume = 0.5f,
				PitchRange = (0.04f, 0.15f)
			};
			Sounds.BeckoningRoar = new SoundStyle("Origins/Sounds/Custom/BeckoningRoar", SoundType.Sound) {
				MaxInstances = 0,
				//Volume = 0.75f,
				PitchRange = (0.2f, 0.3f)
			};
			Sounds.IMustScream = new SoundStyle("Origins/Sounds/Custom/IMustScream", SoundType.Sound) {
				MaxInstances = 0,
				PitchRange = (0.2f, 0.3f)
			};
			Sounds.RivenBass = new SoundStyle("Origins/Sounds/Custom/RivenBass", SoundType.Sound) {
				MaxInstances = 0
			};
			Sounds.ShrapnelFest = new SoundStyle("Origins/Sounds/Custom/ShrapnelFest", SoundType.Sound) {
				MaxInstances = 5,
				SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
				Volume = 0.75f
			};
			Sounds.LaserTag.Hurt = new SoundStyle("Origins/Sounds/Custom/LaserTag/Hurt", SoundType.Sound) {
				MaxInstances = 0
			};
			Sounds.LaserTag.Death = new SoundStyle("Origins/Sounds/Custom/LaserTag/Death", SoundType.Sound) {
				MaxInstances = 0
			};
			Sounds.LaserTag.Score = new SoundStyle("Origins/Sounds/Custom/LaserTag/Score", SoundType.Sound) {
				MaxInstances = 0
			};
			Sounds.Lightning = new SoundStyle("Origins/Sounds/Custom/ThunderShot", SoundType.Sound) {
				MaxInstances = 0
			}.WithPitchRange(-0.1f, 0.1f).WithVolume(0.75f);
			Sounds.LightningSounds = [
				Sounds.Lightning,
				new SoundStyle($"Terraria/Sounds/Thunder_0", SoundType.Sound).WithPitchRange(-0.1f, 0.1f).WithVolume(0.75f)
			];
			Sounds.LightningCharging = new SoundStyle("Origins/Sounds/Custom/ChargedUp", SoundType.Sound) {
				MaxInstances = 0,
				IsLooped = true
			};
			Sounds.LightningChargingSoft = new SoundStyle("Origins/Sounds/Custom/ChargedUpSmol", SoundType.Sound) {
				MaxInstances = 0,
				IsLooped = true
			};
			Sounds.LittleZap = new SoundStyle("Origins/Sounds/Custom/ChainZap", SoundType.Sound) {
				MaxInstances = 5,
				SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
				PitchVariance = 0.4f
			};
			Sounds.Bonk = new SoundStyle("Origins/Sounds/Custom/GrandSlam", SoundType.Sound) {
				MaxInstances = 0,
				PitchVariance = 0.4f
			};
			Sounds.Journal = new SoundStyle("Origins/Sounds/Custom/WritingFire", SoundType.Sound) {
				MaxInstances = 1,
				SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				PitchVariance = 0.4f
			};
			Sounds.ShinedownLoop = new SoundStyle("Origins/Sounds/Custom/BeckoningRoar", SoundType.Sound) {
				MaxInstances = 0,
				IsLooped = true,
				//Volume = 0.75f,
				PitchRange = (0.2f, 0.3f)
			};
			Sounds.WCHit = new SoundStyle("Origins/Sounds/Custom/WCHit", SoundType.Sound) {
				PitchVariance = 0.3f
			};
			Sounds.WCIdle = new SoundStyle("Origins/Sounds/Custom/WCIdle", SoundType.Sound) {
				PitchVariance = 0.3f
			};
			Sounds.WCScream = new SoundStyle("Origins/Sounds/Custom/WCScream", SoundType.Sound) {
				PitchVariance = 0.3f
			};
			//OriginExtensions.initClone();

			Main.OnPostDraw += IncrementFrameCount;
			PegasusLib.PegasusLib.Require(this, LibFeature.IDrawNPCEffect, LibFeature.IComplexMineDamageTile_Hammer, LibFeature.WrappingTextSnippet);
			ApplyPatches();

			for (int i = 1372; i < 1376; i++) PaintingsNotFromVendor[i] = true;
			for (int i = 1433; i < 1444; i++) PaintingsNotFromVendor[i] = true;
			for (int i = 1474; i < 1481; i++) PaintingsNotFromVendor[i] = true;
			for (int i = 1495; i < 1503; i++) PaintingsNotFromVendor[i] = true;
			for (int i = 1538; i < 1543; i++) PaintingsNotFromVendor[i] = true;
			for (int i = 1573; i < 1578; i++) PaintingsNotFromVendor[i] = true;
			for (int i = 1846; i < 1851; i++) PaintingsNotFromVendor[i] = true;
			PaintingsNotFromVendor[ItemID.PillaginMePixels] = true;
			PaintingsNotFromVendor[ItemID.SparkyPainting] = true;
			for (int i = 4626; i < 4640; i++) PaintingsNotFromVendor[i] = true;
			for (int i = 5218; i < 5275; i++) {
				if (i != 5222 && i != 5225 && i != 5228 && i != 5229 && (i !>= 5231 && i !<= 5233) && (i !>= 5241 && i !<= 5245) && i != 5251 && i != 5253 && i != 5257 && i != 5266) PaintingsNotFromVendor[i] = true;
			}
			PaintingsNotFromVendor[ItemID.SunOrnament] = true;

			if (ModLoader.TryGetMod("Fargowiltas", out Mod FargosMutant)) {
				FargosMutant.Call("AddCaughtNPC", "Brine_Fiend_Item", MC.NPCType<Brine_Fiend>(), Name);
				FargosMutant.Call("AddCaughtNPC", "Defiled_Effigy_Item", MC.NPCType<Defiled_Effigy>(), Name);
				// FargosMutant.Call("AddCaughtNPC", "Cubekon_Tinkerer_Item", MC.NPCType<Cubekon_Tinkerer>(), Name); // for future
			}
			AltLibrary.AltLibrary.Instance.Call("AddInvalidRangeHandler", "Origins:BrinePool", Brine_Pool.Gen.JungleAvoider, 6);
#if DEBUG
			for (int i = 0; i < ItemID.Count; i++) OriginGlobalItem.AddVanillaTooltips(i, [], true);
			MonoModHooks.Modify(typeof(Logging).GetMethod("FirstChanceExceptionHandler", BindingFlags.NonPublic | BindingFlags.Static), FCEH);
#endif
		}
		public override void Unload() {
			AssimilationLoader.Unload();
			ExplosiveBaseDamage = null;
			DamageModOnHit = null;
			VanillaElements = null;
			flatDamageMultiplier = null;
			RasterizeAdjustment = null;
			brothBuffs = null;
			isFineWithCrowdedParties = null;
			PotType = null;
			PileType = null;
			artifactMinion = null;
			celestineBoosters = null;
			perlinFade0 = null;
			blackHoleShade = null;
			solventShader = null;
			rasterizeShader = null;
			amebicProtectionShader = null;
			amebicProtectionHairShader = null;
			tileOutlineShader = null;
			cellNoiseTexture = null;
			Journal_UI_Button.Texture = null;
			Journal_UI_Open.BackTexture = null;
			Journal_UI_Open.PageTexture = null;
			Journal_UI_Open.TabsTexture = null;
			OriginExtensions.drawPlayerItemPos = null;
			Tolruk.glowmasks = null;
			TorsoLegLayers = null;
			instance = null;
			OriginExtensions.unInitExt();
			OriginTile.IDs = null;
			OriginTile.DefiledTiles = null;
			OriginSystem worldInstance = ModContent.GetInstance<OriginSystem>();
			if (worldInstance is not null) {
				worldInstance.defiledResurgenceTiles = null;
				worldInstance.defiledAltResurgenceTiles = null;
			}
			eyndumCoreUITexture = null;
			eyndumCoreTexture = null;
			CloudBottoms = null;
			foreach (IUnloadable unloadable in unloadables) {
				unloadable.Unload();
			}
			unloadables.Clear();
			tickers.Clear();
			Main.OnPostDraw -= IncrementFrameCount;
			Array.Resize(ref TextureAssets.GlowMask, GlowMaskID.Count);
			loggedErrors.Clear();
			List<LocalizedText> loadingWarnings = [];
			FastFieldInfo<OverlayManager, LinkedList<Overlay>[]> _activeOverlays = "_activeOverlays";
			LinkedList<Overlay>[] activeOverlays = _activeOverlays.GetValue(Overlays.Scene);
			for (int i = 0; i < activeOverlays.Length; i++) {
				LinkedList<Overlay> overlayer = activeOverlays[i];
				LinkedListNode<Overlay> node = overlayer.First;
				while (node is not null) {
					LinkedListNode<Overlay> next = node.Next;
					if (node.Value.GetType().Assembly.FullName.Contains(nameof(Origins))) overlayer.Remove(node);
					node = next;
				}
			}
		}
		public static uint gameFrameCount = 0;
		static void IncrementFrameCount(GameTime gameTime) {
			unchecked {
				gameFrameCount++;
			}
			currentScreenTarget = null;
		}
		public override void PostSetupContent() {
			Regex safeGoreRegex = new("^(DF|FG|Felnum|Shimmer)", RegexOptions.Compiled);
			foreach (SimpleModGore gore in GetContent<SimpleModGore>()) {
				if (safeGoreRegex.IsMatch(gore.Name)) ChildSafety.SafeGore[gore.Type] = true;
			}
			Journal_Registry.SetupContent();
			OriginsModIntegrations.PostSetupContent(this);
			for (int i = 0; i < NPCID.Sets.SpecificDebuffImmunity.Length; i++) {
				bool?[] immunityData = NPCID.Sets.SpecificDebuffImmunity[i];
				if (immunityData is not null) {
					if (NPCID.Sets.ShouldBeCountedAsBoss[i] || ContentSamples.NpcsByNetId[i].boss) immunityData[Blind_Debuff.ID] = true;
					if ((immunityData[BuffID.Confused] ?? false) && !RasterizeAdjustment.ContainsKey(i)) {
						switch (i) {
							case NPCID.KingSlime:
							RasterizeAdjustment.Add(i, (12, 1f, 1f));
							break;

							case NPCID.QueenSlimeBoss:
							RasterizeAdjustment.Add(i, (12, 0.05f, 0.5f));
							break;

							case NPCID.QueenBee:
							RasterizeAdjustment.Add(i, (16, 0.05f, 1f));
							break;

							case NPCID.EaterofWorldsHead:
							RasterizeAdjustment.Add(i, (8, 0.5f, 1f));
							break;

							case NPCID.Deerclops:
							RasterizeAdjustment.Add(i, (8, 1f, 1f));
							break;

							case NPCID.Golem:
							RasterizeAdjustment.Add(i, (16, 0f, 1f));
							break;

							default:
							RasterizeAdjustment.Add(i, (8, 0.05f, 1f));
							break;
						}
					}
				}
			}
			foreach (ModItem item in MC.GetContent<ModItem>()) {
				if (item is not IJournalEntrySource source) continue;
				JournalEntry.AddJournalEntry(ref OriginsSets.Items.JournalEntries[item.Type], source.EntryName);
			}
			foreach (ModNPC npc in MC.GetContent<ModNPC>()) {
				if (npc is IJournalEntrySource source) JournalEntry.AddJournalEntry(ref OriginsSets.NPCs.JournalEntries[npc.Type], source.EntryName);
				if (npc is IMinions minions) NPCID.Sets.BossBestiaryPriority.InsertRange(NPCID.Sets.BossBestiaryPriority.IndexOf(npc.Type) + 1, minions.BossMinions);
			}
			MC.GetInstance<Explosive_Weapons_Entry>().AddEntryToItems();
			ForcedDialectCompatibility.PostSetupContent();
		}
		private static void FixedDrawBreath(On_Main.orig_DrawInterface_Resources_Breath orig) {
			Player localPlayer = Main.LocalPlayer;
			int breath = localPlayer.breath;
			int breathMax = localPlayer.breathMax;
			try {
				if (breathMax > 400) {
					localPlayer.breathMax = 400;
					localPlayer.breath = breath == breathMax ? 400 : (int)((breath / (float)breathMax) * 400f);
				}
				orig();
			} finally {
				localPlayer.breath = breath;
				localPlayer.breathMax = breathMax;
			}
		}

		private void NPC_GetMeleeCollisionData(On_NPC.orig_GetMeleeCollisionData orig, Rectangle victimHitbox, int enemyIndex, ref int specialHitSetter, ref float damageMultiplier, ref Rectangle npcRect) {
			NPC self = Main.npc[enemyIndex];
			MeleeCollisionNPCData.knockbackMult = 1f;
			if (self.ModNPC is IMeleeCollisionDataNPC meleeNPC) {
				orig(victimHitbox, enemyIndex, ref specialHitSetter, ref damageMultiplier, ref npcRect);
				meleeNPC.GetMeleeCollisionData(victimHitbox, enemyIndex, ref specialHitSetter, ref damageMultiplier, ref npcRect, ref MeleeCollisionNPCData.knockbackMult);
			} else {
				orig(victimHitbox, enemyIndex, ref specialHitSetter, ref damageMultiplier, ref npcRect);
			}
		}

		public static void SetEyndumCoreUI() {
			UserInterface setBonusUI = OriginSystem.Instance.setBonusInventoryUI;
			if (setBonusUI.CurrentState is not Eyndum_Core_UI) {
				setBonusUI.SetState(new Eyndum_Core_UI());
			}
		}
		public static void SetMimicSetUI() {
			UserInterface setBonusUI = OriginSystem.Instance.setBonusInventoryUI;
			if (setBonusUI.CurrentState is not Mimic_Selection_UI) {
				setBonusUI.SetState(new Mimic_Selection_UI());
			}
		}
		public static void OpenJournalEntry(string key) {
			bool canRetry = true;
			retry:
			if (Main.InGameUI.CurrentState is Journal_UI_Open journalUI) {
				journalUI.SwitchMode(Journal_UI_Mode.Normal_Page, key);
			} else if (canRetry) {
				IngameFancyUI.OpenUIState(new Journal_UI_Open());
				canRetry = false;
				goto retry;
			}
		}
		public static void OpenJournalQuest(string key) {
			bool canRetry = true;
			retry:
			if (Main.InGameUI.CurrentState is Journal_UI_Open journalUI) {
				journalUI.SwitchMode(Journal_UI_Mode.Quest_Page, key);
			} else if (canRetry) {
				IngameFancyUI.OpenUIState(new Journal_UI_Open());
				canRetry = false;
				goto retry;
			}
		}
		public static short AddGlowMask(string texture) {
			if (Main.netMode != NetmodeID.Server) {
				string name = texture;
				if (MC.RequestIfExists(name, out Asset<Texture2D> asset)) {
					int index = TextureAssets.GlowMask.Length;
					Array.Resize(ref TextureAssets.GlowMask, index + 1);
					TextureAssets.GlowMask[^1] = asset;
					return (short)index;
				}
			}
			return -1;
		}
		public static short AddGlowMask(ModItem item, string suffix = "_Glow") {
			short slot = AddGlowMask(item.Texture + suffix);
			itemGlowmasks[item.Type] = slot;
			return slot;
		}
		public static short AddGlowMask(ModTexturedType content, string suffix = "_Glow") {
			return AddGlowMask(content.Texture + suffix);
		}
		internal static void AddHelmetGlowmask(ModItem modItem, string suffix = "_Glow") => AddHelmetGlowmask(modItem.Item.headSlot, $"{modItem.Texture}_{EquipType.Head}{suffix}");
		internal static void AddBreastplateGlowmask(ModItem modItem, string suffix = "_Glow") => AddBreastplateGlowmask(modItem.Item.bodySlot, $"{modItem.Texture}_{EquipType.Body}{suffix}");
		internal static void AddLeggingGlowMask(ModItem modItem, string suffix = "_Glow") => AddLeggingGlowMask(modItem.Item.legSlot, $"{modItem.Texture}_{EquipType.Legs}{suffix}");
		internal static void AddHelmetGlowmask(int armorID, string texture) {
			if (Main.netMode != NetmodeID.Server && MC.HasAsset(texture)) {
				Accessory_Glow_Layer.AddGlowMask(EquipType.Head, armorID, texture);
			}
		}
		internal static void AddBreastplateGlowmask(int armorID, string texture) {
			if (Main.netMode != NetmodeID.Server && MC.RequestIfExists(texture, out Asset<Texture2D> asset)) {
				Accessory_Glow_Layer.AddGlowMask(EquipType.Body, armorID, texture);
			}
		}
		internal static void AddLeggingGlowMask(int armorID, string texture) {
			if (Main.netMode != NetmodeID.Server && MC.RequestIfExists(texture, out Asset<Texture2D> asset)) {
				Accessory_Glow_Layer.AddGlowMask(EquipType.Legs, armorID, texture);
			}
		}
		internal static void ResizeArrays() {
			Array.Resize(ref DamageModOnHit, ProjectileLoader.ProjectileCount);
			Array.Resize(ref wallHammerRequirement, WallLoader.WallCount);
			flatDamageMultiplier = ItemID.Sets.Factory.CreateFloatSet(1f,
				ItemID.Minishark, 3f / 8f,
				ItemID.BeeGun, 3f / 8f
			);
			Array.Resize(ref artifactMinion, ProjectileLoader.ProjectileCount);
			brothBuffs = BuffID.Sets.Factory.CreateBoolSet();
			isFineWithCrowdedParties = NPCID.Sets.Factory.CreateBoolSet(false, NPCID.PartyGirl, NPCID.DD2Bartender, NPCID.Steampunker, NPCID.Pirate, NPCID.Princess, NPCID.PantlessSkeleton);
			tileTransformsOnKill = TileID.Sets.Factory.CreateBoolSet(false);
			tileBlocksMinecartTracks = TileID.Sets.Factory.CreateBoolSet(false);
			wallBlocksMinecartTracks = WallID.Sets.Factory.CreateBoolSet(false);
			MeleeGlobalProjectile.applyScaleToProjectile = ItemID.Sets.Factory.CreateBoolSet();
			BannerGlobalNPC.BuildBannerCache();
			Array.Resize(ref itemGlowmasks, ItemLoader.ItemCount);
		}
		static void LoadCloudBottoms() {
			CloudBottoms = new Texture2D[TextureAssets.Cloud.Length];
			for (int i = 0; i < TextureAssets.Cloud.Length; i++) {
				Texture2D baseCloud = TextureAssets.Cloud[i].Value;
				Texture2D bottom = new RenderTarget2D(baseCloud.GraphicsDevice, baseCloud.Width, baseCloud.Height) {
					Name = baseCloud.Name + "_Bottom"
				};
				Color[] colorData = new Color[baseCloud.Width * baseCloud.Height];
				baseCloud.GetData(colorData);
				const float minValue = 0.25f;
				const float maxValue = 1;
				Color[] bottomData = new Color[colorData.Length];
				for (int j = 0; j < colorData.Length; j++) {
					Color c = colorData[j];
					if (c.A == 0) continue;
					float value = Math.Max(maxValue - (c.R / 255f), 0) / (maxValue - minValue);
					if (j / baseCloud.Width < 2 || colorData[j - baseCloud.Width * 2].A == 0) {
						value *= 0.5f;
					} else {

					}
					bottomData[j] = new Color(value, value, value, 1f) * (c.A / 255f);//
				}
				bottom.SetData(bottomData);
				CloudBottoms[i] = bottom;
			}
		}
		public static class Music {
			public static int Fiberglass = MusicID.OtherworldlyJungle;

			public static int BrinePool = MusicID.OtherworldlyEerie;
			public static int LostDiver = MusicID.OtherworldlyInvasion;
			public static int MildewCarrion = MusicID.OtherworldlyPlantera;
			public static int CrownJewel = MusicID.OtherworldlySpace;
			public static int AncientBrinePool;

			public static int ShimmerConstruct = MusicID.OtherworldlyBoss2;
			public static int ShimmerConstructPhase3 = 0;

			public static int Dusk;

			public static int Defiled = MusicID.OtherworldlyMushrooms;
			public static int OtherworldlyDefiled = MusicID.OtherworldlyCorruption;
			public static int UndergroundDefiled = MusicID.OtherworldlyUGHallow;
			public static int DefiledBoss = MusicID.OtherworldlyBoss1;
			public static int AncientDefiled;

			public static int Riven = MusicID.OtherworldlyCrimson;
			public static int UndergroundRiven = MusicID.OtherworldlyIce;
			public static int RivenBoss = MusicID.Boss5;
			public static int RivenOcean = MusicID.OtherworldlyRain;
			public static int AncientRiven;

			public static int TheDive;
		}
		public static class Sounds {
			public static SoundStyle MultiWhip = SoundID.Item153;
			public static SoundStyle Krunch = SoundID.Item36;
			public static SoundStyle HeavyCannon = SoundID.Item36;
			public static SoundStyle EnergyRipple = SoundID.Item8;
			public static SoundStyle PhaserCrash = SoundID.Item12;
			public static SoundStyle DeepBoom = SoundID.Item14;
			public static SoundStyle DefiledIdle = SoundID.Zombie1;
			public static SoundStyle DefiledHurt = SoundID.DD2_SkeletonHurt;
			public static SoundStyle DefiledKill => OriginsModIntegrations.CheckAprilFools() ? defiledKillAF : defiledKill;
			public static SoundStyle defiledKill = SoundID.NPCDeath1;
			public static SoundStyle defiledKillAF = SoundID.NPCDeath1;
			public static SoundStyle Amalgamation = SoundID.Zombie1;
			public static SoundStyle BeckoningRoar = SoundID.ForceRoar;
			public static SoundStyle PowerUp = SoundID.Item4;
			public static SoundStyle RivenBass = SoundID.Item4;
			public static SoundStyle ShrapnelFest = SoundID.Item144;
			public static SoundStyle IMustScream = SoundID.Roar;
			public static SoundStyle ShinedownLoop = SoundID.ForceRoar;
			public static SoundStyle WCHit = SoundID.ForceRoar;
			public static SoundStyle WCIdle = SoundID.ForceRoar;
			public static SoundStyle WCScream = SoundID.ForceRoar;

			public static SoundStyle Lightning = SoundID.Roar;
			public static SoundStyle LightningCharging = SoundID.Roar;
			public static SoundStyle LightningChargingSoft = SoundID.Roar;
			public static SoundStyle[] LightningSounds = [];
			public static SoundStyle LittleZap = SoundID.Roar;

			public static SoundStyle ShimmershotCharging = new("Origins/Sounds/Custom/SoftCharge", SoundType.Sound) {
				IsLooped = true
			};
			public static SoundStyle ShimmerConstructAmbienceIntro = new("Origins/Sounds/Custom/Ambience/SCP3_Ambience_Start", SoundType.Ambient);
			public static SoundStyle ShimmerConstructAmbienceLoop = new("Origins/Sounds/Custom/Ambience/SCP3_Ambience_Mid", SoundType.Ambient);
			public static SoundStyle ShimmerConstructAmbienceOutro = new("Origins/Sounds/Custom/Ambience/SCP3_Ambience_End", SoundType.Ambient);

			public static SoundStyle Bonk = SoundID.Roar;
			public static SoundStyle BikeHorn = new("Origins/Sounds/Custom/BikeHorn", SoundType.Sound) {
				PitchVariance = 0.5f,
				MaxInstances = 0
			};
			public static SoundStyle BoneBreakBySoundEffectsFactory = new("Origins/Sounds/Custom/Bone_Break_By_SoundEffectsFactory", SoundType.Sound) {
				PitchVariance = 0.5f,
				MaxInstances = 0
			};
			public static SoundStyle Journal = SoundID.Roar;
			public static void Unload() {
				LightningSounds = null;
			}
			public static class LaserTag {
				public static SoundStyle Hurt = SoundID.DSTMaleHurt;
				public static SoundStyle Death = SoundID.DD2_KoboldDeath;
				public static SoundStyle Score = SoundID.DrumTamaSnare;
			}
		}
		public enum CallType {
			GetExplosiveClassesDict,
			AddBasicColorDyeShaderPass,
		}
		public override object Call(params object[] args) {
			if (!Enum.TryParse(args[0].ToString().Replace("_", ""), true, out CallType callType)) return null;
			switch (callType) {
				case CallType.GetExplosiveClassesDict:
				return DamageClasses.ExplosiveVersion;

				case CallType.AddBasicColorDyeShaderPass:
				try {
					return OriginsSets.Misc.BasicColorDyeShaderPasses.Add(((Effect)args[1], (string)args[2]));
				} catch (NullReferenceException) {
					throw new Exception("Cannot add Basic Color Dye Shader Pass after AddRecipes");
				}
			}
			return null;
		}
		public static void LogError(object message) {
			instance.Logger.Error(message);
			loggedErrors.Add(message);
		}
		public static void LogError(object message, Exception exception) {
			instance.Logger.Error(message, exception);
			loggedErrors.Add((message, exception));
		}
		public static void LogLoadingWarning(LocalizedText message) {
			PegasusLib.PegasusLib.LogLoadingWarning(message);
		}
		public static bool LogLoadingILError(string methodName, Exception exception) => LogLoadingILError(methodName, exception, []);
		public static bool LogLoadingILError(string methodName, Exception exception, params (Type exceptionType, string modName, Version modVersion)[] expect) {
#if DEBUG
			for (int i = 0; i < expect.Length; i++) {
				if (exception.GetType() == expect[i].exceptionType && ModLoader.TryGetMod(expect[i].modName, out Mod mod) && mod.Version == expect[i].modVersion) goto expected;
			}
			return true;
#endif
			expected:
			Origins.LogLoadingWarning(Language.GetOrRegister("Mods.Origins.Warnings.ILEditException").WithFormatArgs(methodName));
			return false;
		}
		// for DevHelper
		static string DevHelpBrokenReason {
			get {
				string reason = null;
				void AddReason(string text) {
					if (reason is not null) reason += "\n";
					reason += text;
				}
				foreach (ModItem item in instance.GetContent<ModItem>()) {
					if (item.DisplayName.Value.Contains("<PH>")) AddReason($"{item.Name} has placeholder name");
				}
				foreach (IBrokenContent item in instance.GetContent<IBrokenContent>()) {
					AddReason($"{item.GetType().Name}: {item.BrokenReason}");
				}
				const string completion_list = "completionList.txt";
				if ((instance?.FileExists(completion_list) ?? false)) {
					Regex clIssueRegex = new("^\\([^)]*(@|#|\\$|%)[^)]*\\).*!!!.*$", RegexOptions.Multiline | RegexOptions.Compiled);
					string text = Encoding.UTF8.GetString(instance.GetFileBytes(completion_list));
					foreach (Match item in (IEnumerable<Match>)clIssueRegex.Matches(text)) {
						AddReason(item.Value);
					}
				}
#if DEBUG
				AddReason("Mod was last built in DEBUG configuration");
#endif
				return reason;
			}
		}
	}
	internal interface IBrokenContent : ILoadable {
		public string BrokenReason { get; }
	}
}
