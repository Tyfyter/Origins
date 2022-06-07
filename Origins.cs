using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using Origins.Items;
using Origins.Items.Armor.Felnum;
using Origins.Items.Armor.Rift;
using Origins.Items.Armor.Vanity.Terlet.PlagueTexan;
using Origins.Items.Materials;
using Origins.Items.Weapons.Explosives;
using Origins.Items.Weapons.Felnum.Tier2;
using Origins.Tiles;
using Origins.Tiles.Defiled;
using Origins.UI;
using Origins.World;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using static Origins.OriginExtensions;
using MC = Terraria.ModLoader.ModContent;

namespace Origins {
    public class Origins : Mod {
        public static Origins instance;
        [Obsolete]
        public static bool[] ExplosiveProjectiles;
        [Obsolete]
        public static bool[] ExplosiveItems;
        [Obsolete]
        public static bool[] ExplosiveAmmo;

        public static Dictionary<int, int> ExplosiveBaseDamage;
        public static bool[] DamageModOnHit;
        public static ushort[] VanillaElements;

        public static ModKeybind SetBonusTriggerKey { get; private set; }
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

        #endregion Armor IDs
        public static int[] celestineBoosters;

        public static MiscShaderData perlinFade0;
        public static MiscShaderData blackHoleShade;
        public static MiscShaderData solventShader;
        public static MiscShaderData rasterizeShader;
        public static AutoCastingAsset<Texture2D> cellNoiseTexture;
        public static AutoCastingAsset<Texture2D> eyndumCoreUITexture;
        public static AutoCastingAsset<Texture2D> eyndumCoreTexture;

        public Origins() {
            instance = this;
            celestineBoosters = new int[3];
        }
        public override void AddRecipes() {
        #region vanilla explosive base damage registry
            ExplosiveBaseDamage.Add(ItemID.Bomb, 70);
            ExplosiveBaseDamage.Add(ItemID.StickyBomb, 70);
            ExplosiveBaseDamage.Add(ItemID.BouncyBomb, 70);
            ExplosiveBaseDamage.Add(ItemID.BombFish, 70);
            ExplosiveBaseDamage.Add(ItemID.Dynamite, 175);
            ExplosiveBaseDamage.Add(ItemID.StickyDynamite, 175);
            ExplosiveBaseDamage.Add(ItemID.BouncyDynamite, 175);
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
            Logger.Info("fixing tilemerge for "+OriginTile.IDs.Count+" tiles");
            Main.tileMerge[TileID.Sand][TileID.Sandstone] = true;
            Main.tileMerge[TileID.Sand][TileID.HardenedSand] = true;
            Main.tileMerge[TileID.Sandstone][TileID.HardenedSand] = true;
            for(int oID = 0; oID < OriginTile.IDs.Count; oID++) {
                OriginTile oT = OriginTile.IDs[oID];
                Logger.Info("fixing tilemerge for "+oT.GetType());
                //Main.tileMergeDirt[oT.Type] = Main.tileMergeDirt[oT.mergeID];
                Main.tileMerge[oT.Type] = Main.tileMerge[oT.mergeID];
                Main.tileMerge[oT.mergeID][oT.Type] = true;
                Main.tileMerge[oT.Type][oT.mergeID] = true;
                for(int i = 0; i < TileLoader.TileCount; i++) {
                    if(Main.tileMerge[oT.mergeID][i]) {
                        Main.tileMerge[i][oT.Type] = true;
                        Main.tileMerge[oT.Type][i] = true;
                    } else if(Main.tileMerge[i][oT.mergeID]) {
                        Main.tileMerge[oT.Type][i] = true;
                        Main.tileMerge[i][oT.Type] = true;
                    }
                }
            }
            if(OriginConfig.Instance.GrassMerge) {
                List<int> grasses = new List<int>() { };
                for(int i = 0; i<TileLoader.TileCount;  i++) {
                    if(TileID.Sets.Grass[i]||TileID.Sets.GrassSpecial[i]) {
                        grasses.Add(i);
                    }
                }
                IEnumerable<(int, int)> pairs = grasses.SelectMany(
                    (val, index) => grasses.Skip(index+1),
                    (a, b) => (a, b)
                );
                foreach((int,int) pair in pairs) {
                    Main.tileMerge[pair.Item1][pair.Item2] = true;
                    Main.tileMerge[pair.Item2][pair.Item1] = true;
                }
            }
        }
        public override void Load() {
            ExplosiveBaseDamage = new Dictionary<int, int>();
            //Explosive item types
            NonFishItem.ResizeItemArrays+=() => {
                ExplosiveItems = ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn.ToArray();
                ExplosiveItems[ItemID.BouncyBomb] = true;
                ExplosiveItems[ItemID.HellfireArrow] = true;
                ExplosiveItems[ItemID.BombFish] = true;
                ExplosiveItems[ItemID.PartyGirlGrenade] = true;
                ExplosiveItems[ItemID.Beenade] = true;
                ExplosiveItems[ItemID.MolotovCocktail] = true;
            };
            //Explosive projectile & ammo types
            NonFishItem.ResizeOtherArrays+=() => {
                ExplosiveProjectiles = new bool[ProjectileID.Sets.CanDistortWater.Length];
                ExplosiveProjectiles[ProjectileID.Grenade] = true;
                ExplosiveProjectiles[ProjectileID.StickyGrenade] = true;
                ExplosiveProjectiles[ProjectileID.BouncyGrenade] = true;
                ExplosiveProjectiles[ProjectileID.Bomb] = true;
                ExplosiveProjectiles[ProjectileID.StickyBomb] = true;
                ExplosiveProjectiles[ProjectileID.BouncyBomb] = true;
                ExplosiveProjectiles[ProjectileID.Dynamite] = true;
                ExplosiveProjectiles[ProjectileID.StickyDynamite] = true;
                ExplosiveProjectiles[ProjectileID.BouncyDynamite] = true;
                ExplosiveProjectiles[ProjectileID.HellfireArrow] = true;
                ExplosiveProjectiles[ProjectileID.BombFish] = true;
                ExplosiveProjectiles[ProjectileID.PartyGirlGrenade] = true;
                ExplosiveProjectiles[ProjectileID.Beenade] = true;
                ExplosiveProjectiles[ProjectileID.MolotovCocktail] = true;
                ExplosiveProjectiles[ProjectileID.RocketI] = true;
                ExplosiveProjectiles[ProjectileID.RocketII] = true;
                ExplosiveProjectiles[ProjectileID.RocketIII] = true;
                ExplosiveProjectiles[ProjectileID.RocketIV] = true;
                ExplosiveProjectiles[ProjectileID.GrenadeI] = true;
                ExplosiveProjectiles[ProjectileID.GrenadeII] = true;
                ExplosiveProjectiles[ProjectileID.GrenadeIII] = true;
                ExplosiveProjectiles[ProjectileID.GrenadeIV] = true;
                ExplosiveProjectiles[ProjectileID.ProximityMineI] = true;
                ExplosiveProjectiles[ProjectileID.ProximityMineII] = true;
                ExplosiveProjectiles[ProjectileID.ProximityMineIII] = true;
                ExplosiveProjectiles[ProjectileID.ProximityMineIV] = true;

                DamageModOnHit = new bool[ProjectileID.Sets.CanDistortWater.Length];
                DamageModOnHit[ProjectileID.Bomb] = true;
                DamageModOnHit[ProjectileID.StickyBomb] = true;
                DamageModOnHit[ProjectileID.BouncyBomb] = true;
                DamageModOnHit[ProjectileID.BombFish] = true;
                DamageModOnHit[ProjectileID.Dynamite] = true;
                DamageModOnHit[ProjectileID.StickyDynamite] = true;
                DamageModOnHit[ProjectileID.BouncyDynamite] = true;

                ExplosiveAmmo = ExplosiveItems.ToArray();
                ExplosiveAmmo[AmmoID.Rocket] = true;
                ExplosiveAmmo[AmmoID.StyngerBolt] = true;
            };
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
                (ushort)ItemID.EldMelter, Elements.Fire,
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
                (ushort)ItemID.FrostburnArrow, Elements.Fire|Elements.Ice,
                (ushort)ItemID.FlowerofFrost, Elements.Fire|Elements.Ice,
                (ushort)ItemID.Amarok, Elements.Fire|Elements.Ice,
                (ushort)ItemID.CursedArrow, Elements.Fire|Elements.Acid,
                (ushort)ItemID.CursedBullet, Elements.Fire|Elements.Acid,
                (ushort)ItemID.CursedFlames, Elements.Fire|Elements.Acid,
                (ushort)ItemID.CursedDart, Elements.Fire|Elements.Acid,
                (ushort)ItemID.ClingerStaff, Elements.Fire|Elements.Acid,
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
            HelmetGlowMasks = new();
            BreastplateGlowMasks = new();
            LeggingGlowMasks = new();
			if (!Main.dedServ) {
                OriginExtensions.drawPlayerItemPos = (Func<float, int, Vector2>)typeof(Main).GetMethod("DrawPlayerItemPos", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate(typeof(Func<float, int, Vector2>), Main.instance);
                perlinFade0 = new MiscShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/PerlinFade", AssetRequestMode.ImmediateLoad).Value), "RedFade");
                //perlinFade0.UseImage("Images/Misc/Perlin");
                perlinFade0.Shader.Parameters["uThreshold0"].SetValue(0.6f);
                perlinFade0.Shader.Parameters["uThreshold1"].SetValue(0.6f);
                blackHoleShade = new MiscShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/BlackHole", AssetRequestMode.ImmediateLoad).Value), "BlackHole");

				Filters.Scene["Origins:ZoneDusk"] = new Filter(new ScreenShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/BiomeShade", AssetRequestMode.ImmediateLoad).Value), "VoidShade"), EffectPriority.High);
				Filters.Scene["Origins:ZoneDefiled"] = new Filter(new ScreenShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/BiomeShade", AssetRequestMode.ImmediateLoad).Value), "DefiledShade"), EffectPriority.High);
				Filters.Scene["Origins:ZoneRiven"] = new Filter(new ScreenShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/BiomeShade", AssetRequestMode.ImmediateLoad).Value), "RivenShade"), EffectPriority.High);

                solventShader = new MiscShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/Solvent", AssetRequestMode.ImmediateLoad).Value), "Dissolve");
                GameShaders.Misc["Origins:Solvent"] = solventShader;
                cellNoiseTexture = Assets.Request<Texture2D>("Textures/Cell_Noise_Pixel");

                rasterizeShader = new MiscShaderData(new Ref<Effect>(Assets.Request<Effect>("Effects/Rasterize", AssetRequestMode.ImmediateLoad).Value), "Rasterize");
                GameShaders.Misc["Origins:Rasterize"] = rasterizeShader;
                //Filters.Scene["Origins:ZoneDusk"].GetShader().UseOpacity(0.35f);
                //Ref<Effect> screenRef = new Ref<Effect>(GetEffect("Effects/ScreenDistort")); // The path to the compiled shader file.
                //Filters.Scene["BlackHole"] = new Filter(new ScreenShaderData(screenRef, "BlackHole"), EffectPriority.VeryHigh);
                //Filters.Scene["BlackHole"].Load();
                eyndumCoreUITexture = Assets.Request<Texture2D>("UI/CoreSlot");
                eyndumCoreTexture = Assets.Request<Texture2D>("Items/Armor/Eyndum/Eyndum_Breastplate_Body_Core");
				if (OriginClientConfig.Instance.SetBonusDoubleTap) {
                    On.Terraria.Player.KeyDoubleTap += (On.Terraria.Player.orig_KeyDoubleTap orig, Player self, int keyDir) => {
                        orig(self, keyDir);
						if (keyDir == (Main.ReversedUpDownArmorSetBonuses ? 1 : 0)) {
                            self.GetModPlayer<OriginPlayer>().TriggerSetBonus();
                        }
                    };
				}
            }
            SetBonusTriggerKey = KeybindLoader.RegisterKeybind(this, "Trigger Set Bonus", Keys.Q.ToString());
            Sounds.Krunch = new SoundStyle("Origins/Sounds/Custom/BurstCannon", SoundType.Sound);
            Sounds.HeavyCannon = new SoundStyle("Origins/Sounds/Custom/HeavyCannon", SoundType.Sound);
            Sounds.EnergyRipple = new SoundStyle("Origins/Sounds/Custom/EnergyRipple", SoundType.Sound);
            Sounds.DeepBoom = new SoundStyle("Origins/Sounds/Custom/DeepBoom", SoundType.Sound);
            //OriginExtensions.initClone();
            Music.Dusk = MusicID.Eerie;
            Music.Defiled = MusicID.Corruption;
            Music.UndergroundDefiled = MusicID.UndergroundCorruption;
            On.Terraria.NPC.UpdateCollision+=(orig, self)=>{
                int realID = self.type;
                if (self.ModNPC is ISandsharkNPC shark) {
                    self.type = NPCID.SandShark;
                    try {
                        shark.PreUpdateCollision();
                        orig(self);
                    } finally {
                        shark.PostUpdateCollision();
                    }
                    self.type = realID;
                    return;
                }
                ITileCollideNPC tcnpc = self.ModNPC as ITileCollideNPC;
                self.type = tcnpc?.CollisionType??realID;
                orig(self);
                self.type = realID;
            };
            On.Terraria.NPC.GetMeleeCollisionData += NPC_GetMeleeCollisionData;
            On.Terraria.WorldGen.GERunner+=OriginSystem.GERunnerHook;
            On.Terraria.WorldGen.Convert+=OriginSystem.ConvertHook;
            On.Terraria.Item.NewItem_IEntitySource_int_int_int_int_int_int_bool_int_bool_bool+=OriginGlobalItem.NewItemHook;
            Mod blockSwap = ModLoader.GetMod("BlockSwap");
            if(!(blockSwap is null || blockSwap.Version>new Version(1,0,1)))On.Terraria.TileObject.CanPlace+=(On.Terraria.TileObject.orig_CanPlace orig, int x, int y, int type, int style, int dir, out TileObject objectData, bool onlyCheck, bool checkStay) => {
				if (type == 20){
					Tile soil = Main.tile[x, y + 1];
					if (soil.HasTile){
                        TileLoader.SaplingGrowthType(soil.TileType, ref type, ref style);
					}
				}
                return orig(x, y, type, style, dir, out objectData, onlyCheck, checkStay);
            };
            Defiled_Tree.Load();
            OriginSystem worldInstance = ModContent.GetInstance<OriginSystem>();
            if(!(worldInstance is null)) {
                worldInstance.defiledResurgenceTiles = new List<(int, int)> { };
                worldInstance.defiledAltResurgenceTiles = new List<(int, int, ushort)> { };
            }
            //IL.Terraria.WorldGen.GERunner+=OriginWorld.GERunnerHook;
            On.Terraria.Main.DrawInterface_Resources_Breath += FixedDrawBreath;
            On.Terraria.WorldGen.CountTiles += WorldGen_CountTiles;
            On.Terraria.WorldGen.AddUpAlignmentCounts += WorldGen_AddUpAlignmentCounts;
            Terraria.IO.WorldFile.OnWorldLoad += () => {
	            if (Main.netMode != NetmodeID.MultiplayerClient){
			        for(int i = 0; i < Main.maxTilesX; i++)WorldGen.CountTiles(i);
	            }
            };
            On.Terraria.Lang.GetDryadWorldStatusDialog += Lang_GetDryadWorldStatusDialog;
            HookEndpointManager.Add(typeof(TileLoader).GetMethod("MineDamage", BindingFlags.Public|BindingFlags.Static), (hook_MinePower)MineDamage);
			On.Terraria.Graphics.Renderers.LegacyPlayerRenderer.DrawPlayerInternal += LegacyPlayerRenderer_DrawPlayerInternal;
        }

		private void LegacyPlayerRenderer_DrawPlayerInternal(On.Terraria.Graphics.Renderers.LegacyPlayerRenderer.orig_DrawPlayerInternal orig, Terraria.Graphics.Renderers.LegacyPlayerRenderer self, Camera camera, Player drawPlayer, Vector2 position, float rotation, Vector2 rotationOrigin, float shadow, float alpha, float scale, bool headOnly) {
            bool shaded = false;
            try {
                int rasterizedTime = drawPlayer.GetModPlayer<OriginPlayer>().rasterizedTime;
                if (rasterizedTime > 0) {
                    shaded = true;
                    rasterizeShader.Shader.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
                    rasterizeShader.Shader.Parameters["uOffset"].SetValue(drawPlayer.velocity.WithMaxLength(4) * 0.0625f * rasterizedTime);
                    rasterizeShader.Shader.Parameters["uWorldPosition"].SetValue(drawPlayer.position);
                    rasterizeShader.Shader.Parameters["uSecondaryColor"].SetValue(new Vector3(40, 1120, 0));
                    Main.graphics.GraphicsDevice.Textures[1] = cellNoiseTexture;
                    Main.spriteBatch.Restart(SpriteSortMode.Immediate, effect: rasterizeShader.Shader);
                }
                orig(self, camera, drawPlayer, position, rotation, rotationOrigin, shadow, alpha, scale, headOnly);
            } finally {
                if (shaded) {
                    Main.spriteBatch.Restart();
                }
            }
        }

        private string Lang_GetDryadWorldStatusDialog(On.Terraria.Lang.orig_GetDryadWorldStatusDialog orig) {
            const int good = 1;
            const int evil = 2;
            const int blood = 4;
            const int defiled = 8;
            const int riven = 16;
	        string text = "";
	        int tGood = WorldGen.tGood;
	        int tEvil = WorldGen.tEvil;
	        int tBlood = WorldGen.tBlood;
            int tDefiled = OriginSystem.tDefiled;
            int tRiven = OriginSystem.tRiven;
            int tBad = tEvil + tBlood + tDefiled + tRiven;
            if(tDefiled==0&&tRiven==0) {
                return orig();
            }
            int tHas = (tGood>0?good:0)|(tEvil>0?evil:0)|(tBlood>0?blood:0)|(tDefiled>0?defiled:0)|(tRiven>0?riven:0);
            switch(tHas & (good | evil | blood)) {
                case good | evil | blood:
                text = Language.GetTextValue("DryadSpecialText.WorldStatusAll", Main.worldName, tGood, tEvil, tBlood);
                break;
                case good | evil:
                text = Language.GetTextValue("DryadSpecialText.WorldStatusHallowCorrupt", Main.worldName, tGood, tEvil, tBlood);
                break;
                case good | blood:
                text = Language.GetTextValue("DryadSpecialText.WorldStatusHallowCrimson", Main.worldName, tGood, tEvil, tBlood);
                break;
                case evil | blood:
                text = Language.GetTextValue("DryadSpecialText.WorldStatusCorruptCrimson", Main.worldName, tGood, tEvil, tBlood);
                break;
                case evil:
                text = Language.GetTextValue("DryadSpecialText.WorldStatusCorrupt", Main.worldName, tGood, tEvil, tBlood);
                break;
                case blood:
                text = Language.GetTextValue("DryadSpecialText.WorldStatusCrimson", Main.worldName, tGood, tEvil, tBlood);
                break;
                case good:
                text = Language.GetTextValue("DryadSpecialText.WorldStatusHallow", Main.worldName, tGood, tEvil, tBlood);
                break;
                case 0:
                text = Language.GetTextValue("DryadSpecialText.WorldStatusPure", Main.worldName, tGood, tEvil, tBlood);
                break;
            }
            //temp fix, unlocalized and never grammatically correct
            if(tDefiled > 0) text += $" and {tDefiled}% defiled wastelands";
            if(tRiven > 0) text += $" and {tRiven}% riven";
	        string str = (tGood * 1.2 >= tBad && tGood * 0.8 <= tBad) ?
                Language.GetTextValue("DryadSpecialText.WorldDescriptionBalanced") : ((tGood >= tBad) ?
                Language.GetTextValue("DryadSpecialText.WorldDescriptionFairyTale") : ((tBad > tGood + 20) ?
                Language.GetTextValue("DryadSpecialText.WorldDescriptionGrim") : ((tBad <= 10) ?
                Language.GetTextValue("DryadSpecialText.WorldDescriptionClose") :
                Language.GetTextValue("DryadSpecialText.WorldDescriptionWork"))));
	        return text + " " + str;
        }
        private delegate void orig_MinePower(int minePower, ref int damage);
        private delegate void hook_MinePower(orig_MinePower orig, int minePower, ref int damage);
        private void MineDamage(orig_MinePower orig, int minePower, ref int damage) {
	        ModTile modTile = MC.GetModTile(Main.tile[Player.tileTargetX, Player.tileTargetY].TileType);
            if (modTile is null) {
                damage += minePower;
            } else if(modTile is IComplexMineDamageTile damageTile){
                damageTile.MinePower(Player.tileTargetX, Player.tileTargetY, minePower, ref damage);
            } else {
                damage += ((int)(minePower / modTile.MineResist));
            }
        }
        private void WorldGen_CountTiles(On.Terraria.WorldGen.orig_CountTiles orig, int X) {
            if(X == 0)OriginSystem.UpdateTotalEvilTiles();
            orig(X);
        }

        private void WorldGen_AddUpAlignmentCounts(On.Terraria.WorldGen.orig_AddUpAlignmentCounts orig, bool clearCounts) {
            int[] tileCounts = WorldGen.tileCounts;
            if (clearCounts) {
                OriginSystem.totalDefiled2 = 0;
                OriginSystem.totalRiven2 = 0;
            }
            OriginSystem.totalDefiled2 += tileCounts[MC.TileType<Defiled_Stone>()] + tileCounts[MC.TileType<Defiled_Grass>()] + tileCounts[MC.TileType<Defiled_Sand>()]+tileCounts[MC.TileType<Defiled_Ice>()];
            OriginSystem.totalRiven2 += tileCounts[MC.TileType<Tiles.Riven.Riven_Flesh>()];
            orig(clearCounts);
        }
        public override void HandlePacket(BinaryReader reader, int whoAmI) {
            byte type = reader.ReadByte();
            if(Main.netMode == NetmodeID.MultiplayerClient) {
                switch(type) {
                    case MessageID.TileCounts:
                    OriginSystem.tDefiled = reader.ReadByte();
                    break;
                    default:
                    Logger.Warn($"Invalid packet type ({type}) received on client");
                    break;
                }
            }else if(Main.netMode == NetmodeID.Server) {
                switch(type) {
                    case MessageID.TileCounts:
                    OriginSystem.tDefiled = reader.ReadByte();
                    break;
                    default:
                    Logger.Warn($"Invalid packet type ({type}) received on server");
                    break;
                }
            }
        }

        private static void FixedDrawBreath(On.Terraria.Main.orig_DrawInterface_Resources_Breath orig) {
            Player localPlayer = Main.LocalPlayer;
            int breath = localPlayer.breath;
            int breathMax = localPlayer.breathMax;
            if(breathMax > 400) {
                localPlayer.breathMax = 400;
                localPlayer.breath = breath==breathMax?400:(int)(breath / (breathMax / 400f));
            }
            orig();
            localPlayer.breath = breath;
            localPlayer.breathMax = breathMax;
        }

        private void NPC_GetMeleeCollisionData(On.Terraria.NPC.orig_GetMeleeCollisionData orig, Rectangle victimHitbox, int enemyIndex, ref int specialHitSetter, ref float damageMultiplier, ref Rectangle npcRect) {
            NPC self = Main.npc[enemyIndex];
            MeleeCollisionNPCData.knockbackMult = 1f;
            if(self.ModNPC is IMeleeCollisionDataNPC meleeNPC) {
                meleeNPC.GetMeleeCollisionData(victimHitbox, enemyIndex, ref specialHitSetter, ref damageMultiplier, ref npcRect, ref MeleeCollisionNPCData.knockbackMult);
            } else {
                orig(victimHitbox, enemyIndex, ref specialHitSetter, ref damageMultiplier, ref npcRect);
            }
        }

        public override void Unload() {
            ExplosiveProjectiles = null;
            ExplosiveItems = null;
            ExplosiveAmmo = null;
            ExplosiveBaseDamage = null;
            DamageModOnHit = null;
            VanillaElements = null;
            celestineBoosters = null;
            perlinFade0 = null;
            blackHoleShade = null;
            solventShader = null;
            rasterizeShader = null;
            cellNoiseTexture = null;
            OriginExtensions.drawPlayerItemPos = null;
            Tolruk.glowmasks = null;
            HelmetGlowMasks = null;
            BreastplateGlowMasks = null;
            LeggingGlowMasks = null;
            instance = null;
			Defiled_Tree.Unload();
            OriginExtensions.unInitExt();
            OriginTile.IDs = null;
            OriginSystem worldInstance = ModContent.GetInstance<OriginSystem>();
            if(!(worldInstance is null)) {
                worldInstance.defiledResurgenceTiles = null;
                worldInstance.defiledAltResurgenceTiles = null;
            }
            eyndumCoreUITexture = null;
            eyndumCoreTexture = null;
        }
        public void SetEyndumCoreUI() {
            UserInterface setBonusUI = OriginSystem.instance.setBonusUI;
            if (setBonusUI.CurrentState is not Eyndum_Core_UI) {
                setBonusUI.SetState(new Eyndum_Core_UI());
            }
        }
        public void SetMimicSetUI() {
            UserInterface setBonusUI = OriginSystem.instance.setBonusUI;
            if (setBonusUI.CurrentState is not Mimic_Selection_UI) {
                setBonusUI.SetState(new Mimic_Selection_UI());
            }
        }

        public static void AddExplosive(Item item, bool noProj = false, bool noAmmo = false) {
            ExplosiveItems[item.type] = true;
            ExplosiveAmmo[item.type] = true;
            if(!noAmmo&&item.ammo!=AmmoID.None)ExplosiveAmmo[item.ammo] = true;
            if(!noAmmo&&item.useAmmo!=AmmoID.None)ExplosiveAmmo[item.useAmmo] = true;
            if(!noProj&&item.shoot!=ProjectileID.None)ExplosiveProjectiles[item.shoot] = true;
            instance.Logger.Info($"Registered {item.Name} as explosive: "+ExplosiveItems[item.type]);
        }
        internal static short AddGlowMask(string name){
            if (Main.netMode!=NetmodeID.Server){
                Asset<Texture2D>[] glowMasks = new Asset<Texture2D>[TextureAssets.GlowMask.Length + 1];
                for (int i = 0; i < TextureAssets.GlowMask.Length; i++){
                    glowMasks[i] = TextureAssets.GlowMask[i];
                }
                glowMasks[glowMasks.Length - 1] = instance.Assets.Request<Texture2D>("Items/" + name);
                TextureAssets.GlowMask = glowMasks;
                return (short)(glowMasks.Length - 1);
            }
            else return 0;
        }
        public static short AddGlowMask(ModItem item, string suffix = "_Glow") {
            if (Main.netMode != NetmodeID.Server) {
                string name = item.Texture + suffix;
                if (ModContent.RequestIfExists<Texture2D>(name, out Asset<Texture2D> asset)) {
                    Asset<Texture2D>[] glowMasks = new Asset<Texture2D>[TextureAssets.GlowMask.Length + 1];
                    for (int i = 0; i < TextureAssets.GlowMask.Length; i++) {
                        glowMasks[i] = TextureAssets.GlowMask[i];
                    }
                    glowMasks[glowMasks.Length - 1] = asset;
                    TextureAssets.GlowMask = glowMasks;
                    return (short)(glowMasks.Length - 1);
                }
            }
            return 0;
        }
        public override void AddRecipeGroups() {
            RecipeGroup group = new RecipeGroup(() => "Gem Staves", new int[] {
                ItemID.AmethystStaff,
                ItemID.TopazStaff,
                ItemID.SapphireStaff,
                ItemID.EmeraldStaff,
                ItemID.RubyStaff,
                ItemID.DiamondStaff
            });
            RecipeGroup.RegisterGroup("Origins:Gem Staves", group);
        }
        public override void PostAddRecipes() {
            int l = Main.recipe.Length;
            Recipe r;
            Recipe recipe;
            int roseID = ModContent.ItemType<Wilting_Rose_Item>();
            for(int i = 0; i < l; i++) {
                r = Main.recipe[i];
                if(!r.requiredItem.ToList().Exists((ing)=>ing.type==ItemID.Deathweed)) {
                    continue;
                }
                recipe = CloneRecipe(r);
                recipe.requiredItem = recipe.requiredItem.Select((it)=>it.type==ItemID.Deathweed?ItemFromType(roseID):it.CloneByID()).ToList();
                Logger.Info("adding procedural recipe: "+recipe.Stringify());
                recipe.Create();
            }
        }
        internal static void AddHelmetGlowmask(int armorID, string texture) {
            if (instance.RequestAssetIfExists(texture, out Asset<Texture2D> asset)) {
                HelmetGlowMasks.Add(armorID, asset);
            }
        }
        internal static void AddBreastplateGlowmask(int armorID, string texture) {
            if (instance.RequestAssetIfExists(texture, out Asset<Texture2D> asset)) {
                BreastplateGlowMasks.Add(armorID, asset);
            }
        }
        internal static void AddLeggingGlowMask(int armorID, string texture) {
            if (instance.RequestAssetIfExists(texture, out Asset<Texture2D> asset)) {
                LeggingGlowMasks.Add(armorID, asset);
            }
        }
        public static class Music {
            public static int Dusk = MusicID.Eerie;
            public static int Defiled = MusicID.Corruption;
            public static int UndergroundDefiled = MusicID.UndergroundCorruption;
            public static int Riven = MusicID.Corruption;
            public static int UndergroundRiven = MusicID.UndergroundCorruption;
        }
        public static class Sounds {
            public static SoundStyle Krunch = SoundID.Item36;
            public static SoundStyle HeavyCannon = SoundID.Item36;
            public static SoundStyle EnergyRipple = SoundID.Item8;
            public static SoundStyle DeepBoom = SoundID.Item14;
        }
    }
    public sealed class NonFishItem : ModItem {
        public override string Texture => "Terraria/Images/Item_2290";
        public static event Action ResizeItemArrays;
        public static event Action ResizeOtherArrays;
        public override bool IsQuestFish() {
            ResizeItemArrays();
            ResizeItemArrays = null;
            return false;
        }
        public override void SetStaticDefaults() {
            ResizeOtherArrays();
            ResizeOtherArrays = null;
            DisplayName.SetDefault("That Which Is Not a Fish");
        }
    }
}
