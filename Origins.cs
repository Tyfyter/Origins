using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Origins.Buffs;
using Origins.Gores.NPCs;
using Origins.Items.Accessories;
using Origins.Items.Armor.Felnum;
using Origins.Items.Armor.Rift;
using Origins.Items.Armor.Vanity.Dev.PlagueTexan;
using Origins.Items.Other.Dyes;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Projectiles;
using Origins.Tiles;
using Origins.Tiles.Defiled;
using Origins.UI;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.UI;
using Terraria.UI.Chat;
using static Origins.OriginExtensions;
using MC = Terraria.ModLoader.ModContent;

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
		public static Dictionary<int, (int maxLevel, float velDiffMult)> RasterizeAdjustment { get; private set; }
		static bool[] artifactMinion;
		public static bool[] ArtifactMinion { get => artifactMinion; }
		static float[] homingEffectivenessMultiplier;
		public static float[] HomingEffectivenessMultiplier { get => homingEffectivenessMultiplier; }
		static int[] magicTripwireRange;
		public static int[] MagicTripwireRange { get => magicTripwireRange; }
		static int[] magicTripwireDetonationStyle;
		public static int[] MagicTripwireDetonationStyle { get => magicTripwireDetonationStyle; }
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
		public static MiscShaderData solventShader;
		public static MiscShaderData rasterizeShader;
		public static ArmorShaderData amebicProtectionShader;
		public static HairShaderData amebicProtectionHairShader;
		public static ArmorShaderData tileOutlineShader;
		public static int amebicProtectionShaderID;
		public static int amebicProtectionHairShaderID;
		public static AutoCastingAsset<Texture2D> cellNoiseTexture;
		public static AutoCastingAsset<Texture2D> eyndumCoreUITexture;
		public static AutoCastingAsset<Texture2D> eyndumCoreTexture;
		public override uint ExtraPlayerBuffSlots => 4;
		public Origins() {
			instance = this;
			celestineBoosters = new int[3];
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
			RiftHeadArmorID = ModContent.GetInstance<Rift_Helmet>().Item.headSlot;
			RiftBodyArmorID = ModContent.GetInstance<Rift_Breastplate>().Item.bodySlot;
			RiftLegsArmorID = ModContent.GetInstance<Rift_Greaves>().Item.legSlot;
			#endregion
			Logger.Info("fixing tilemerge for " + OriginTile.IDs.Count + " tiles");
			Main.tileMerge[TileID.Sand][TileID.Sandstone] = true;
			Main.tileMerge[TileID.Sand][TileID.HardenedSand] = true;
			Main.tileMerge[TileID.Sandstone][TileID.HardenedSand] = true;
			for (int oID = 0; oID < OriginTile.IDs.Count; oID++) {
				OriginTile oT = OriginTile.IDs[oID];
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
			OriginsModIntegrations.LateLoad();
		}
		public override void Load() {
			LocalizedText newTranslation = Language.GetOrRegister("Riven", 
				() => OriginsModIntegrations.CheckAprilFools() ? "{$Mods.Origins.April_Fools.Generic.Riven}" : "{$Mods.Origins.Generic.Riven}"
			);
			newTranslation = Language.GetOrRegister("Dusk",
				() => OriginsModIntegrations.CheckAprilFools() ? "{$Mods.Origins.April_Fools.Generic.Dusk}" : "{$Mods.Origins.Generic.Dusk}"
			);
			newTranslation = Language.GetOrRegister("Defiled",
				() => OriginsModIntegrations.CheckAprilFools() ? "{$Mods.Origins.April_Fools.Generic.Defiled}" : "{$Mods.Origins.Generic.Defiled}"
			);
			newTranslation = Language.GetOrRegister("Defiled_Wastelands",
				() => OriginsModIntegrations.CheckAprilFools() ? "{$Mods.Origins.April_Fools.Generic.Defiled_Wastelands}" : "{$Mods.Origins.Generic.Defiled_Wastelands}"
			);
			newTranslation = Language.GetOrRegister("The_Defiled_Wastelands",
				() => OriginsModIntegrations.CheckAprilFools() ? "{$Mods.Origins.April_Fools.Generic.Defiled_Wastelands}" : "the {$Mods.Origins.Generic.Defiled_Wastelands}"
			); 

			RasterizeAdjustment = new Dictionary<int, (int, float)>();
			ExplosiveBaseDamage = new Dictionary<int, int>();
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

			if (!Main.dedServ) {
				//OriginExtensions.drawPlayerItemPos = (Func<float, int, Vector2>)typeof(Main).GetMethod("DrawPlayerItemPos", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate(typeof(Func<float, int, Vector2>), Main.instance);
				perlinFade0 = new MiscShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/PerlinFade", AssetRequestMode.ImmediateLoad).Value), "RedFade");
				//perlinFade0.UseImage("Images/Misc/Perlin");
				perlinFade0.Shader.Parameters["uThreshold0"].SetValue(0.6f);
				perlinFade0.Shader.Parameters["uThreshold1"].SetValue(0.6f);
				blackHoleShade = new MiscShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/BlackHole", AssetRequestMode.ImmediateLoad).Value), "BlackHole");

				Filters.Scene["Origins:ZoneDusk"] = new Filter(new ScreenShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/BiomeShade", AssetRequestMode.ImmediateLoad).Value), "VoidShade"), EffectPriority.High);
				Filters.Scene["Origins:ZoneDefiled"] = new Filter(new ScreenShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/BiomeShade", AssetRequestMode.ImmediateLoad).Value), "DefiledShade"), EffectPriority.High);
				Filters.Scene["Origins:MaskedRasterizeFilter"] = new Filter(new ScreenShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/MaskedRasterizeFilter", AssetRequestMode.ImmediateLoad).Value), "MaskedRasterizeFilter"), EffectPriority.VeryHigh);
				
				//Filters.Scene["Origins:ZoneRiven"] = new Filter(new ScreenShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/BiomeShade", AssetRequestMode.ImmediateLoad).Value), "RivenShade"), EffectPriority.High);

				solventShader = new MiscShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/Solvent", AssetRequestMode.ImmediateLoad).Value), "Dissolve");
				GameShaders.Misc["Origins:Solvent"] = solventShader;
				cellNoiseTexture = Assets.Request<Texture2D>("Textures/Cell_Noise_Pixel");
				Filters.Scene["Origins:MaskedRasterizeFilter"].GetShader().UseImage(cellNoiseTexture, 2);

				rasterizeShader = new MiscShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/Rasterize", AssetRequestMode.ImmediateLoad).Value), "Rasterize");
				GameShaders.Misc["Origins:Rasterize"] = rasterizeShader;

				amebicProtectionShader = new ArmorShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/AmebicProtection", AssetRequestMode.ImmediateLoad).Value), "AmebicProtection");
				GameShaders.Armor.BindShader(MC.ItemType<Amebic_Vial>(), amebicProtectionShader);
				amebicProtectionShaderID = GameShaders.Armor.GetShaderIdFromItemId(MC.ItemType<Amebic_Vial>());

				amebicProtectionHairShader = new HairShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/AmebicProtection", AssetRequestMode.ImmediateLoad).Value), "AmebicProtection");
				GameShaders.Hair.BindShader(MC.ItemType<Amebic_Vial>(), amebicProtectionHairShader);
				amebicProtectionHairShaderID = GameShaders.Hair.GetShaderIdFromItemId(MC.ItemType<Amebic_Vial>());

				tileOutlineShader = new ArmorShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/TileOutline", AssetRequestMode.ImmediateLoad).Value), "TileOutline");
				GameShaders.Armor.BindShader(MC.ItemType<High_Contrast_Dye>(), tileOutlineShader);
				tileOutlineShader.Shader.Parameters["uImageSize0"].SetValue(Main.ScreenSize.ToVector2());
				tileOutlineShader.Shader.Parameters["uScale"].SetValue(2);
				tileOutlineShader.Shader.Parameters["uColor"].SetValue(new Vector3(1f, 1f, 1f));
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
				Terraria.On_Player.KeyDoubleTap += (Terraria.On_Player.orig_KeyDoubleTap orig, Player self, int keyDir) => {
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
			}
			ChatManager.Register<Journal_Link_Handler>(new string[]{
				"journal",
				"j"
			});
			ChatManager.Register<Quest_Link_Handler>(new string[]{
				"quest",
				"q"
			});
			ChatManager.Register<Quest_Stage_Snippet_Handler>(new string[]{
				"queststage",
				"qs"
			});
			ChatManager.Register<Header_Snippet_Handler>(new string[]{
				"header"
			});
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
			Sounds.DefiledKill = new SoundStyle("Origins/Sounds/Custom/Defiled_Kill1", SoundType.Sound) {
				MaxInstances = 0,
				Volume = 0.4f,
				PitchRange = (0.9f, 1.1f)
			};
			Sounds.RivenBass = new SoundStyle("Origins/Sounds/Custom/RivenBass", SoundType.Sound) {
				MaxInstances = 0
			};
			//OriginExtensions.initClone();
			Music.Dusk = MusicID.Eerie;
			Music.Defiled = MusicID.Corruption;
			Music.UndergroundDefiled = MusicID.UndergroundCorruption;
			ApplyPatches();
		}
		public override void PostSetupContent() {
			foreach (KeyValuePair<int, NPCDebuffImmunityData> item in NPCID.Sets.DebuffImmunitySets) {
				NPCDebuffImmunityData immunityData = item.Value;
				if (immunityData is not null && immunityData.SpecificallyImmuneTo is not null && immunityData.SpecificallyImmuneTo.Contains(BuffID.Confused)) {
					Array.Resize(ref immunityData.SpecificallyImmuneTo, immunityData.SpecificallyImmuneTo.Length + 2);
					immunityData.SpecificallyImmuneTo[^2] = Stunned_Debuff.ID;
					immunityData.SpecificallyImmuneTo[^1] = Toxic_Shock_Debuff.ID;
					//immunityData.SpecificallyImmuneTo[^1] = Rasterized_Debuff.ID;
					switch (item.Key) {
						case NPCID.KingSlime:
						case NPCID.QueenSlimeBoss:
						RasterizeAdjustment.Add(item.Key, (0, 1));
						break;

						case NPCID.QueenBee:
						RasterizeAdjustment.Add(item.Key, (16, 0.95f));
						break;

						case NPCID.EaterofWorldsHead:
						RasterizeAdjustment.Add(item.Key, (8, 0.5f));
						break;

						case NPCID.Deerclops:
						RasterizeAdjustment.Add(item.Key, (8, 0));
						break;

						default:
						RasterizeAdjustment.Add(item.Key, (8, 0.95f));
						break;
					}

				}
			}
		}

		private static void FixedDrawBreath(Terraria.On_Main.orig_DrawInterface_Resources_Breath orig) {
			Player localPlayer = Main.LocalPlayer;
			int breath = localPlayer.breath;
			int breathMax = localPlayer.breathMax;
			if (breathMax > 400) {
				localPlayer.breathMax = 400;
				localPlayer.breath = breath == breathMax ? 400 : (int)(breath / (breathMax / 400f));
			}
			orig();
			localPlayer.breath = breath;
			localPlayer.breathMax = breathMax;
		}

		private void NPC_GetMeleeCollisionData(Terraria.On_NPC.orig_GetMeleeCollisionData orig, Rectangle victimHitbox, int enemyIndex, ref int specialHitSetter, ref float damageMultiplier, ref Rectangle npcRect) {
			NPC self = Main.npc[enemyIndex];
			MeleeCollisionNPCData.knockbackMult = 1f;
			if (self.ModNPC is IMeleeCollisionDataNPC meleeNPC) {
				meleeNPC.GetMeleeCollisionData(victimHitbox, enemyIndex, ref specialHitSetter, ref damageMultiplier, ref npcRect, ref MeleeCollisionNPCData.knockbackMult);
			} else {
				orig(victimHitbox, enemyIndex, ref specialHitSetter, ref damageMultiplier, ref npcRect);
			}
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
			Defiled_Tree.Unload();
			OriginExtensions.unInitExt();
			OriginTile.IDs = null;
			OriginTile.DefiledTiles = null;
			OriginSystem worldInstance = ModContent.GetInstance<OriginSystem>();
			if (!(worldInstance is null)) {
				worldInstance.defiledResurgenceTiles = null;
				worldInstance.defiledAltResurgenceTiles = null;
			}
			eyndumCoreUITexture = null;
			eyndumCoreTexture = null;
		}
		public static void SetEyndumCoreUI() {
			UserInterface setBonusUI = OriginSystem.Instance.setBonusUI;
			if (setBonusUI.CurrentState is not Eyndum_Core_UI) {
				setBonusUI.SetState(new Eyndum_Core_UI());
			}
		}
		public static void SetMimicSetUI() {
			UserInterface setBonusUI = OriginSystem.Instance.setBonusUI;
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
		internal static short AddGlowMask(string name) {
			if (Main.netMode != NetmodeID.Server) {
				Asset<Texture2D>[] glowMasks = new Asset<Texture2D>[TextureAssets.GlowMask.Length + 1];
				for (int i = 0; i < TextureAssets.GlowMask.Length; i++) {
					glowMasks[i] = TextureAssets.GlowMask[i];
				}
				glowMasks[^1] = instance.Assets.Request<Texture2D>("Items/" + name);
				TextureAssets.GlowMask = glowMasks;
				return (short)(glowMasks.Length - 1);
			} else return -1;
		}
		public static short AddGlowMask(ModItem item, string suffix = "_Glow") {
			if (Main.netMode != NetmodeID.Server) {
				string name = item.Texture + suffix;
				if (ModContent.RequestIfExists<Texture2D>(name, out Asset<Texture2D> asset)) {
					Asset<Texture2D>[] glowMasks = new Asset<Texture2D>[TextureAssets.GlowMask.Length + 1];
					for (int i = 0; i < TextureAssets.GlowMask.Length; i++) {
						glowMasks[i] = TextureAssets.GlowMask[i];
					}
					glowMasks[^1] = asset;
					TextureAssets.GlowMask = glowMasks;
					return (short)(glowMasks.Length - 1);
				}
			}
			return -1;
		}
		internal static void AddHelmetGlowmask(int armorID, string texture) {
			if (Main.netMode != NetmodeID.Server && instance.RequestAssetIfExists(texture, out Asset<Texture2D> asset)) {
				HelmetGlowMasks.Add(armorID, asset);
			}
		}
		internal static void AddBreastplateGlowmask(int armorID, string texture) {
			if (Main.netMode != NetmodeID.Server && instance.RequestAssetIfExists(texture, out Asset<Texture2D> asset)) {
				BreastplateGlowMasks.Add(armorID, asset);
			}
		}
		internal static void AddLeggingGlowMask(int armorID, string texture) {
			if (Main.netMode != NetmodeID.Server && instance.RequestAssetIfExists(texture, out Asset<Texture2D> asset)) {
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
			magicTripwireRange = ProjectileID.Sets.Factory.CreateIntSet(0);
			magicTripwireDetonationStyle = ProjectileID.Sets.Factory.CreateIntSet(0);
			ExplosiveGlobalProjectile.SetupMagicTripwireRanges(magicTripwireRange, magicTripwireDetonationStyle);
		}
		public static class Music {
			public static int Dusk = MusicID.PumpkinMoon;
			public static int Defiled = MusicID.Corruption;
			public static int DefiledBoss = MusicID.OtherworldlyBoss1;
			public static int UndergroundDefiled = MusicID.UndergroundCorruption;
			public static int Riven = MusicID.Crimson;
			public static int RivenBoss = MusicID.OtherworldlyBoss1;
			public static int UndergroundRiven = MusicID.UndergroundCrimson;
		}
		public static class Sounds {
			public static SoundStyle MultiWhip = SoundID.Item153;
			public static SoundStyle Krunch = SoundID.Item36;
			public static SoundStyle HeavyCannon = SoundID.Item36;
			public static SoundStyle EnergyRipple = SoundID.Item8;
			public static SoundStyle DeepBoom = SoundID.Item14;
			public static SoundStyle DefiledIdle = SoundID.Zombie1;
			public static SoundStyle DefiledHurt = SoundID.DD2_SkeletonHurt;
			public static SoundStyle DefiledKill = SoundID.NPCDeath1;
			public static SoundStyle PowerUp = SoundID.Item4;
			public static SoundStyle RivenBass = SoundID.Item4;
		}
		public override object Call(params object[] args) {
			return args[0] switch {
				"get_explosive_classes_dict" or "GetExplosiveClassesDict" => DamageClasses.ExplosiveVersion,
				_ => base.Call(args),
			};
		}
	}
}
