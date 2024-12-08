global using Vector2 = Microsoft.Xna.Framework.Vector2;
global using Color = Microsoft.Xna.Framework.Color;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Origins.Gores.NPCs;
using Origins.Items.Accessories;
using Origins.Items.Armor.Felnum;
using Origins.Items.Armor.Bleeding;
using Origins.Items.Armor.Vanity.Dev.PlagueTexan;
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
using static Origins.OriginExtensions;
using MC = Terraria.ModLoader.ModContent;
using Origins.Walls;
using Origins.Tiles.Other;
using AltLibrary.Common.AltBiomes;
using Origins.NPCs;
using Origins.Tiles.Banners;
using Origins.Graphics;
using MonoMod.Cil;
using Origins.Items.Tools;
using Microsoft.Xna.Framework.Audio;

namespace Origins {
	public partial class Origins : Mod {
		public static Origins instance;

		public static Dictionary<int, int> ExplosiveBaseDamage { get; private set; }
		public static bool[] DamageModOnHit;
		public static ushort[] VanillaElements { get; private set; }
		static bool[] forceFelnumShockOnShoot;
		public static bool[] ForceFelnumShockOnShoot { get => forceFelnumShockOnShoot; }
		static float[] flatDamageMultiplier;
		public static float[] FlatDamageMultiplier { get => flatDamageMultiplier; }
		static int[] wallHammerRequirement;
		public static int[] WallHammerRequirement { get => wallHammerRequirement; }
		public static Dictionary<int, (int maxLevel, float accelerationFactor, float velocityFactor)> RasterizeAdjustment { get; private set; }
		static bool[] artifactMinion;
		public static bool[] ArtifactMinion { get => artifactMinion; }
		static float[] homingEffectivenessMultiplier;
		public static float[] HomingEffectivenessMultiplier { get => homingEffectivenessMultiplier; }
		static int[] magicTripwireRange;
		public static int[] MagicTripwireRange { get => magicTripwireRange; }
		static int[] magicTripwireDetonationStyle;
		public static int[] MagicTripwireDetonationStyle { get => magicTripwireDetonationStyle; }
		static bool[] itemsThatAllowRemoteRightClick;
		public static bool[] ItemsThatAllowRemoteRightClick { get => itemsThatAllowRemoteRightClick; }
		public static short[] itemGlowmasks = [];
		public static Dictionary<int, ModBiome> NPCOnlyTargetInBiome { get; private set; } = [];
		public static Dictionary<int, (ushort potType, int minStyle, int maxStyle)> PotType { get; private set; }
		public static Dictionary<int, (ushort pileType, int minStyle, int maxStyle)> PileType { get; private set; }
		public static ModKeybind SetBonusTriggerKey { get; private set; }
		public static ModKeybind InspectItemKey { get; private set; }
		#region Armor IDs
		public static int FelnumHeadArmorID { get; private set; }
		public static int FelnumBodyArmorID { get; private set; }
		public static int FelnumLegsArmorID { get; private set; }

		public static int PlagueTexanJacketID { get; private set; }

		public static int RiftHeadArmorID { get; private set; }
		public static int RiftBodyArmorID { get; private set; }
		public static int RiftLegsArmorID { get; private set; }
		public static Dictionary<int, AutoCastingAsset<Texture2D>> HelmetGlowMasks { get; private set; }
		public static Dictionary<int, AutoCastingAsset<Texture2D>> BreastplateGlowMasks { get; private set; }
		public static Dictionary<int, AutoCastingAsset<Texture2D>> LeggingGlowMasks { get; private set; }
		public static Dictionary<int, AutoCastingAsset<Texture2D>> TorsoLegLayers { get; private set; }

		#endregion Armor IDs
		public static int[] celestineBoosters;

		public static MiscShaderData perlinFade0;
		public static MiscShaderData blackHoleShade;
		public static ArmorShaderData solventShader;
		public static MiscShaderData rasterizeShader;
		public static ArmorShaderData amebicProtectionShader;
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
			this.MusicAutoloadingEnabled = true;
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
			PlagueTexanJacketID = ModContent.GetInstance<Plague_Texan_Jacket>().Item.bodySlot;
			RiftHeadArmorID = ModContent.GetInstance<Bleeding_Obsidian_Helmet>().Item.headSlot;
			RiftBodyArmorID = ModContent.GetInstance<Bleeding_Obsidian_Breastplate>().Item.bodySlot;
			RiftLegsArmorID = ModContent.GetInstance<Bleeding_Obsidian_Greaves>().Item.legSlot;
			#endregion
			Logger.Info("fixing tilemerge for " + OriginTile.IDs.Count + " tiles");
			Main.tileMerge[TileID.Sand][TileID.Sandstone] = true;
			Main.tileMerge[TileID.Sand][TileID.HardenedSand] = true;
			Main.tileMerge[TileID.Sandstone][TileID.HardenedSand] = true;
			for (int oID = 0; oID < OriginTile.IDs.Count; oID++) {
				OriginTile oT = OriginTile.IDs[oID];
				if (oT.mergeID == oT.Type) continue;
				Logger.Info("fixing tilemerge for " + oT.GetType());
				//Main.tileMergeDirt[oT.Type] = Main.tileMergeDirt[oT.mergeID];
				Main.tileMerge[oT.Type] = Main.tileMerge[oT.mergeID];
				Main.tileMerge[oT.mergeID][oT.Type] = true;
				Main.tileMerge[oT.Type][oT.mergeID] = true;
				for (int i = 0; i < TileLoader.TileCount; i++) {
					if (Main.tileMerge[oT.mergeID][i]) {
						Main.tileMerge[i][oT.Type] = true;
						Main.tileMerge[oT.Type][i] = true;
					} else if (Main.tileMerge[i][oT.mergeID]) {
						Main.tileMerge[oT.Type][i] = true;
						Main.tileMerge[i][oT.Type] = true;
					}
				}
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
			OriginsModIntegrations.LateLoad();
			_ = OriginExtensions.StrikethroughFont;
		}
		public override void Load() {
			LocalizationMethods.BindArgs(Language.GetOrRegister("Riven", () => "{0}"), Language.GetTextValue("Mods.Origins.Generic.Riven"));
			LocalizationMethods.BindArgs(Language.GetOrRegister("Dusk", () => "{0}"), Language.GetTextValue("Mods.Origins.Generic.Dusk"));
			LocalizationMethods.BindArgs(Language.GetOrRegister("Defiled", () => "{0}"), Language.GetTextValue("Mods.Origins.Generic.Defiled"));
			LocalizationMethods.BindArgs(Language.GetOrRegister("Defiled_Wastelands", () => "{0}"), Language.GetTextValue("Mods.Origins.Generic.Defiled_Wastelands"));
			LocalizationMethods.BindArgs(Language.GetOrRegister("The_Defiled_Wastelands", () => "the {0}"), Language.GetTextValue("Mods.Origins.Generic.Defiled_Wastelands")); 

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
			forceFelnumShockOnShoot = new bool[ProjectileLoader.ProjectileCount];
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
			homingEffectivenessMultiplier = ProjectileID.Sets.Factory.CreateFloatSet(
				1f,
				ProjectileID.ScarabBomb, 0f,
				ProjectileID.StyngerShrapnel, 0f,
				ProjectileID.ClusterFragmentsI, 0f,
				ProjectileID.ClusterFragmentsII, 0f,
				ProjectileID.BloodCloudMoving, 0f,
				ProjectileID.BloodCloudRaining, 0f,
				ProjectileID.RainCloudMoving, 0f,
				ProjectileID.RainCloudRaining, 0f,
				ProjectileID.PrincessWeapon, 0f,
				ProjectileID.ClingerStaff, 0f,
				ProjectileID.VilethornBase, 0f,
				ProjectileID.VilethornTip, 0f,
				ProjectileID.CrystalVileShardShaft, 0f,
				ProjectileID.CrystalVileShardHead, 0f,
				ProjectileID.NettleBurstLeft, 0f,
				ProjectileID.NettleBurstRight, 0f,
				ProjectileID.NettleBurstEnd, 0f,
				ProjectileID.MedusaHead, 0f,
				ProjectileID.PrincessWeapon, 0f,
				ProjectileID.MagnetSphereBolt, 0f,
				ProjectileID.InfernoFriendlyBlast, 0f,
				ProjectileID.LastPrism, 2f,
				ProjectileID.LastPrismLaser, 2f
			);
			PotType = new();
			PileType = new();

			HelmetGlowMasks = new();
			BreastplateGlowMasks = new();
			LeggingGlowMasks = new();
			TorsoLegLayers = new();

			OriginExtensions.initExt();
			if (!Main.dedServ) {
				//OriginExtensions.drawPlayerItemPos = (Func<float, int, Vector2>)typeof(Main).GetMethod("DrawPlayerItemPos", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate(typeof(Func<float, int, Vector2>), Main.instance);
				perlinFade0 = new MiscShaderData(Assets.Request<Effect>("Effects/PerlinFade", AssetRequestMode.ImmediateLoad), "RedFade");
				//perlinFade0.UseImage("Images/Misc/Perlin");
				perlinFade0.Shader.Parameters["uThreshold0"].SetValue(0.6f);
				perlinFade0.Shader.Parameters["uThreshold1"].SetValue(0.6f);
				blackHoleShade = new MiscShaderData(Assets.Request<Effect>("Effects/BlackHole"), "BlackHole");

				Filters.Scene["Origins:ZoneDusk"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/BiomeShade"), "VoidShade"), EffectPriority.High);
				if (Filters.Scene["Origins:ZoneDefiled"]?.IsActive() ?? false) {
					Filters.Scene["Origins:ZoneDefiled"].Deactivate();
				}
				Filters.Scene["Origins:ZoneDefiled"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/BiomeShade"), "DefiledShade")
					.UseImage(MC.Request<Texture2D>("Terraria/Images/Misc/noise"), 1),
				EffectPriority.High);
				Filters.Scene["Origins:MaskedRasterizeFilter"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/MaskedRasterizeFilter"), "MaskedRasterizeFilter"), EffectPriority.VeryHigh);
				Filters.Scene["Origins:VolatileGelatinFilter"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/MaskedPurpleJellyFilter"), "MaskedPurpleJellyFilter"), EffectPriority.VeryHigh);
				Filters.Scene["Origins:RivenBloodCoating"] = new Filter(new ScreenShaderData(Assets.Request<Effect>("Effects/RivenBloodCoating"), "RivenBloodCoating"), EffectPriority.VeryHigh);
				Filters.Scene["Origins:RivenBloodCoating"].GetShader().UseImage(Assets.Request<Texture2D>("Textures/Riven_Blood_Map"), 0, SamplerState.PointWrap);
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

				GameShaders.Misc["Origins:SapphireAura"] = new MiscShaderData(Assets.Request<Effect>("Effects/Strip"), "SapphireAura");
				GameShaders.Misc["Origins:SapphireAura"].UseImage0(TextureAssets.Extra[194]);

				GameShaders.Misc["Origins:Framed"] = new MiscShaderData(Assets.Request<Effect>("Effects/Strip"), "Framed");
				GameShaders.Misc["Origins:AnimatedTrail"] = new MiscShaderData(Assets.Request<Effect>("Effects/Strip"), "AnimatedTrail").UseSamplerState(SamplerState.PointWrap);
				GameShaders.Misc["Origins:LaserBlade"] = new MiscShaderData(Assets.Request<Effect>("Effects/Strip"), "LaserBlade").UseSamplerState(SamplerState.PointWrap);

				GameShaders.Misc["Origins:Beam"] = new MiscShaderData(Assets.Request<Effect>("Effects/Beam"), "Beam");
				GameShaders.Misc["Origins:Beam"].UseSamplerState(SamplerState.PointClamp);

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
				IEnumerable<string> files = GetFileNames();
				IEnumerable<string> goreFiles = files.Where(f => f.EndsWith(".rawimg") && f.Contains("Gores") && !files.Contains(f.Replace(".rawimg", ".cs")));
				foreach (string gore in goreFiles) {
					AutoLoadGores.AddGore("Origins/" + gore.Replace(".rawimg", null), this);
				}
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
			SetBonusTriggerKey = KeybindLoader.RegisterKeybind(this, "Trigger Set Bonus", Keys.Q.ToString());
			InspectItemKey = KeybindLoader.RegisterKeybind(this, "Inspect Item", "Mouse3");
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
				MaxInstances = 0,
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
			//OriginExtensions.initClone();
			Music.LoadMusic();

			Main.OnPostDraw += IncrementFrameCount;
			ApplyPatches();
#if DEBUG
			MonoModHooks.Add(typeof(Logging).GetMethod("FirstChanceExceptionHandler", BindingFlags.NonPublic | BindingFlags.Static), FCEH);
#endif
		}
		public override void Unload() {
			ExplosiveBaseDamage = null;
			DamageModOnHit = null;
			VanillaElements = null;
			forceFelnumShockOnShoot = null;
			flatDamageMultiplier = null;
			RasterizeAdjustment = null;
			homingEffectivenessMultiplier = null;
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
			HelmetGlowMasks = null;
			BreastplateGlowMasks = null;
			LeggingGlowMasks = null;
			TorsoLegLayers = null;
			instance = null;
			Petrified_Tree.Unload();
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
			BiomeNPCGlobals.assimilationProviders.Clear();
			BiomeNPCGlobals.assimilationProviders.TrimExcess();
			foreach (IUnloadable unloadable in unloadables) {
				unloadable.Unload();
			}
			unloadables.Clear();
			tickers.Clear();
			Main.OnPostDraw -= IncrementFrameCount;
			Array.Resize(ref TextureAssets.GlowMask, GlowMaskID.Count);
			loggedErrors.Clear();
			List<LocalizedText> loadingWarnings = [];
		}
		public static uint gameFrameCount = 0;
		static void IncrementFrameCount(GameTime gameTime) {
			unchecked {
				gameFrameCount++;
			}
			currentScreenTarget = null;
		}
		public override void PostSetupContent() {
			for (int i = 0; i < NPCID.Sets.SpecificDebuffImmunity.Length; i++) {
				bool?[] immunityData = NPCID.Sets.SpecificDebuffImmunity[i];
				if (immunityData is not null && (immunityData[BuffID.Confused] ?? false) && !RasterizeAdjustment.ContainsKey(i)) {
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
		internal static void AddHelmetGlowmask(ModItem modItem) => AddHelmetGlowmask(modItem.Item.headSlot, $"{modItem.Texture}_{EquipType.Head}_Glow");
		internal static void AddBreastplateGlowmask(ModItem modItem) => AddBreastplateGlowmask(modItem.Item.bodySlot, $"{modItem.Texture}_{EquipType.Body}_Glow");
		internal static void AddLeggingGlowMask(ModItem modItem) => AddLeggingGlowMask(modItem.Item.legSlot, $"{modItem.Texture}_{EquipType.Legs}_Glow");
		internal static void AddHelmetGlowmask(int armorID, string texture) {
			if (Main.netMode != NetmodeID.Server && MC.RequestIfExists(texture, out Asset<Texture2D> asset)) {
				HelmetGlowMasks.Add(armorID, asset);
			}
		}
		internal static void AddBreastplateGlowmask(int armorID, string texture) {
			if (Main.netMode != NetmodeID.Server && MC.RequestIfExists(texture, out Asset<Texture2D> asset)) {
				BreastplateGlowMasks.Add(armorID, asset);
			}
		}
		internal static void AddLeggingGlowMask(int armorID, string texture) {
			if (Main.netMode != NetmodeID.Server && MC.RequestIfExists(texture, out Asset<Texture2D> asset)) {
				LeggingGlowMasks.Add(armorID, asset);
			}
		}
		internal static void ResizeArrays() {
			Array.Resize(ref DamageModOnHit, ProjectileLoader.ProjectileCount);
			Array.Resize(ref forceFelnumShockOnShoot, ProjectileLoader.ProjectileCount);
			Array.Resize(ref wallHammerRequirement, WallLoader.WallCount);
			flatDamageMultiplier = new SetFactory(ItemLoader.ItemCount).CreateFloatSet(1f,
				ItemID.Minishark, 3f / 8f
			);
			Array.Resize(ref artifactMinion, ProjectileLoader.ProjectileCount);
			int oldCanGainHomingLength = homingEffectivenessMultiplier.Length;
			Array.Resize(ref homingEffectivenessMultiplier, ProjectileLoader.ProjectileCount);
			for (int i = oldCanGainHomingLength; i < ProjectileLoader.ProjectileCount; i++) {
				homingEffectivenessMultiplier[i] = 1f;
			}
			Array.Resize(ref Mitosis_P.aiVariableResets, ProjectileLoader.ProjectileCount);
			for (int i = 0; i < Mitosis_P.aiVariableResets.Length; i++) {
				Mitosis_P.aiVariableResets[i] = new bool[3];
			}
			magicTripwireRange = ProjectileID.Sets.Factory.CreateIntSet(0);
			magicTripwireDetonationStyle = ProjectileID.Sets.Factory.CreateIntSet(0);
			ExplosiveGlobalProjectile.SetupMagicTripwireRanges(magicTripwireRange, magicTripwireDetonationStyle);
			itemsThatAllowRemoteRightClick = ItemID.Sets.Factory.CreateBoolSet();
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
			public static int Fiberglass;

			public static int BrinePool;

			public static int Dusk;

			public static int Defiled;
			public static int UndergroundDefiled;
			public static int DefiledBoss;
			public static int AncientDefiled;

			public static int Riven;
			public static int UndergroundRiven;
			public static int RivenBoss;
			public static int RivenOcean;
			public static int AncientRiven;
			internal static void LoadMusic() {
				static int GetMusicSlot(string path) {
					MusicLoader.AddMusic(instance, path);
					return MusicLoader.GetMusicSlot(instance, path);
				}
				Fiberglass = GetMusicSlot("Music/The_Room_Before");
				BrinePool = GetMusicSlot("Music/Only_the_Brave");
				Dusk = GetMusicSlot("Music/Dancing_With_Ghosts");
				Defiled = GetMusicSlot("Music/Stolen_Memories");
				UndergroundDefiled = GetMusicSlot("Music/Heart_Of_The_Beast");
				DefiledBoss = GetMusicSlot("Music/ADJUDICATE");
				Riven = GetMusicSlot("Music/Pereunt_Unum_Scindendum");
				UndergroundRiven = GetMusicSlot("Music/Festering_Hives");
				RivenBoss = GetMusicSlot("Music/Ad_Laceratur");
				RivenOcean = GetMusicSlot("Music/Sizzling_Waves");
				AncientDefiled = GetMusicSlot("Music/Shattered_Topography_Old");
				AncientRiven = UndergroundRiven;//GetMusicSlot("Music/Festering_Hives");
			}
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

			public static class LaserTag {
				public static SoundStyle Hurt = SoundID.DSTMaleHurt;
				public static SoundStyle Death = SoundID.DD2_KoboldDeath;
				public static SoundStyle Score = SoundID.DrumTamaSnare;
			}
		}
		public override object Call(params object[] args) {
			return args[0] switch {
				"get_explosive_classes_dict" or "GetExplosiveClassesDict" => DamageClasses.ExplosiveVersion,
				_ => base.Call(args),
			};
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
			instance.Logger.Warn(message.Value);
			loadingWarnings.Add(message);
		}
	}
}
