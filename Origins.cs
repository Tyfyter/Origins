using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using Origins.Items;
using Origins.Items.Armor.Felnum;
using Origins.Items.Armor.Vanity.Terlet.PlagueTexan;
using Origins.Items.Materials;
using Origins.Items.Weapons.Explosives;
using Origins.Items.Weapons.Felnum.Tier2;
using Origins.Tiles;
using Origins.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins {
    public class Origins : Mod {
        public static Origins instance;

        public static bool[] ExplosiveProjectiles;
        public static bool[] ExplosiveItems;
        public static bool[] ExplosiveAmmo;

        public static Dictionary<int, int> ExplosiveBaseDamage;
        public static List<int> ExplosiveModOnHit;
        public static ushort[] VanillaElements;

        public static int FelnumHeadArmorID;
        public static int FelnumBodyArmorID;
        public static int FelnumLegsArmorID;

        public static int PlagueTexanJacketID;

        public static int[] celestineBoosters;

        public static MiscShaderData perlinFade0;
        public static MiscShaderData blackHoleShade;
        public static MiscShaderData solventShader;
        public static Texture2D cellNoiseTexture;
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
            ExplosiveModOnHit.Add(ProjectileID.Bomb);
            ExplosiveModOnHit.Add(ProjectileID.StickyBomb);
            ExplosiveModOnHit.Add(ProjectileID.BouncyBomb);
            ExplosiveModOnHit.Add(ProjectileID.BombFish);
            ExplosiveModOnHit.Add(ProjectileID.Dynamite);
            ExplosiveModOnHit.Add(ProjectileID.StickyDynamite);
            ExplosiveModOnHit.Add(ProjectileID.BouncyDynamite);
        #endregion vanilla explosive base damage registry
        #region armor slot ids
            FelnumHeadArmorID = ModContent.GetInstance<Felnum_Helmet>().item.headSlot;
            FelnumBodyArmorID = ModContent.GetInstance<Felnum_Breastplate>().item.bodySlot;
            FelnumLegsArmorID = ModContent.GetInstance<Felnum_Greaves>().item.legSlot;
            PlagueTexanJacketID = ModContent.GetInstance<Plague_Texan_Jacket>().item.bodySlot;
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
            ExplosiveModOnHit = new List<int>() {};
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
            if(Main.netMode != NetmodeID.Server) {
                OriginExtensions.drawPlayerItemPos = (Func<float, int, Vector2>)typeof(Main).GetMethod("DrawPlayerItemPos", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate(typeof(Func<float, int, Vector2>), Main.instance);
                perlinFade0 = new MiscShaderData(new Ref<Effect>(GetEffect("Effects/PerlinFade")), "RedFade");
                //perlinFade0.UseImage("Images/Misc/Perlin");
                perlinFade0.Shader.Parameters["uThreshold0"].SetValue(0.6f);
                perlinFade0.Shader.Parameters["uThreshold1"].SetValue(0.6f);
                blackHoleShade = new MiscShaderData(new Ref<Effect>(GetEffect("Effects/BlackHole")), "BlackHole");
				Filters.Scene["Origins:ZoneDusk"] = new Filter(new ScreenShaderData(new Ref<Effect>(GetEffect("Effects/BiomeShade")), "VoidShade"), EffectPriority.High);
				Filters.Scene["Origins:ZoneDefiled"] = new Filter(new ScreenShaderData(new Ref<Effect>(GetEffect("Effects/BiomeShade")), "DefiledShade"), EffectPriority.High);
                solventShader = new MiscShaderData(new Ref<Effect>(GetEffect("Effects/Solvent")), "Dissolve");
                GameShaders.Misc["Origins:Solvent"] = solventShader;
                cellNoiseTexture = GetTexture("Textures/Cell_Noise_Pixel");
                //Filters.Scene["Origins:ZoneDusk"].GetShader().UseOpacity(0.35f);
                //Ref<Effect> screenRef = new Ref<Effect>(GetEffect("Effects/ScreenDistort")); // The path to the compiled shader file.
                //Filters.Scene["BlackHole"] = new Filter(new ScreenShaderData(screenRef, "BlackHole"), EffectPriority.VeryHigh);
                //Filters.Scene["BlackHole"].Load();
            }
            Sounds.Krunch = AddSound("Sounds/Custom/BurstCannon", SoundType.Item);
            //OriginExtensions.initClone();
            Music.Dusk = MusicID.Eerie;
            Music.Defiled = MusicID.Corruption;
            Music.UndergroundDefiled = MusicID.UndergroundCorruption;
            On.Terraria.WorldGen.GERunner+=OriginWorld.GERunnerHook;
            On.Terraria.WorldGen.Convert+=OriginWorld.ConvertHook;
            On.Terraria.Item.NewItem_int_int_int_int_int_int_bool_int_bool_bool+=OriginGlobalItem.NewItemHook;
            Mod blockSwap = ModLoader.GetMod("BlockSwap");
            if(!(blockSwap is null || blockSwap.Version>new Version(1,0,1)))On.Terraria.TileObject.CanPlace+=(On.Terraria.TileObject.orig_CanPlace orig, int x, int y, int type, int style, int dir, out TileObject objectData, bool onlyCheck, bool checkStay) => {
				if (type == 20){
					Tile soil = Main.tile[x, y + 1];
					if (soil.active()){
                        TileLoader.SaplingGrowthType(soil.type, ref type, ref style);
					}
				}
                return orig(x, y, type, style, dir, out objectData, onlyCheck, checkStay);
            };
            Tiles.Defiled.Defiled_Tree.Load();
            OriginWorld worldInstance = ModContent.GetInstance<OriginWorld>();
            if(!(worldInstance is null)) {
                worldInstance.defiledResurgenceTiles = new List<(int, int)> { };
                worldInstance.defiledAltResurgenceTiles = new List<(int, int, ushort)> { };
            }
            //IL.Terraria.WorldGen.GERunner+=OriginWorld.GERunnerHook;
        }
        public override void Unload() {
            ExplosiveProjectiles = null;
            ExplosiveItems = null;
            ExplosiveAmmo = null;
            ExplosiveBaseDamage = null;
            ExplosiveModOnHit = null;
            VanillaElements = null;
            celestineBoosters = null;
            perlinFade0 = null;
            blackHoleShade = null;
            solventShader = null;
            cellNoiseTexture = null;
            OriginExtensions.drawPlayerItemPos = null;
            Tolruk.glowmasks = null;
            instance = null;
            Tiles.Defiled.Defiled_Tree.Unload();
            OriginExtensions.unInitExt();
            OriginTile.IDs = null;
            OriginWorld worldInstance = ModContent.GetInstance<OriginWorld>();
            if(!(worldInstance is null)) {
                worldInstance.defiledResurgenceTiles = null;
                worldInstance.defiledAltResurgenceTiles = null;
            }
        }
        public override void UpdateMusic(ref int music, ref MusicPriority priority) {
            if (Main.myPlayer == -1 || Main.gameMenu || !Main.LocalPlayer.active) {
				return;
			}
            Vector2 position = Main.LocalPlayer.Bottom/16;
            OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
			if (originPlayer.ZoneVoid&&priority<MusicPriority.Event) {
				music = Music.Dusk;
				priority = MusicPriority.Event;
			}else if (originPlayer.ZoneDefiled&&priority<MusicPriority.Event) {
				music = (position.Y>=(Main.worldSurface+30))?Music.UndergroundDefiled:Music.Defiled;
				priority = MusicPriority.Event;
			}
        }

        public static void AddExplosive(Item item, bool noProj = false) {
            ExplosiveItems[item.type] = true;
            ExplosiveAmmo[item.type] = true;
            if(item.ammo!=AmmoID.None)ExplosiveAmmo[item.ammo] = true;
            if(item.useAmmo!=AmmoID.None)ExplosiveAmmo[item.useAmmo] = true;
            if(!noProj&&item.shoot!=ProjectileID.None)ExplosiveProjectiles[item.shoot] = true;
            instance.Logger.Info($"Registered {item.Name} as explosive: "+ExplosiveItems[item.type]);
        }
        public static short AddGlowMask(string name){
            if (!Main.dedServ){
                Texture2D[] glowMasks = new Texture2D[Main.glowMaskTexture.Length + 1];
                for (int i = 0; i < Main.glowMaskTexture.Length; i++){
                    glowMasks[i] = Main.glowMaskTexture[i];
                }
                glowMasks[glowMasks.Length - 1] = instance.GetTexture("Items/" + name);
                Main.glowMaskTexture = glowMasks;
                return (short)(glowMasks.Length - 1);
            }
            else return 0;
        }
        public static int AddSound(string path, SoundType type = SoundType.Custom, ModSound modSound = null) {
            instance.AddSound(type, "Origins/"+path, modSound);
            return SoundLoader.GetSoundSlot(type, "Origins/"+path);
        }
        public override void PostAddRecipes() {
            int l = Main.recipe.Length;
            Recipe r;
            ModRecipe recipe;
            int roseID = ModContent.ItemType<Wilting_Rose_Item>();
            for(int i = 0; i < l; i++) {
                r = Main.recipe[i];
                if(!r.requiredItem.ToList().Exists((ing)=>ing.type==ItemID.Deathweed)) {
                    continue;
                }
                recipe = r.Clone(this);
                recipe.requiredItem = recipe.requiredItem.Select((it)=>it.type==ItemID.Deathweed?ItemFromType(roseID):it.CloneByID()).ToArray();
                Logger.Info("adding procedural recipe: "+recipe.Stringify());
                recipe.AddRecipe();
            }
        }
        public static class Music {
            public static int Dusk = MusicID.Eerie;
            public static int Defiled = MusicID.Corruption;
            public static int UndergroundDefiled = MusicID.UndergroundCorruption;
        }
        public static class Sounds {
            public static int Krunch = 36;
        }
    }
    public sealed class NonFishItem : ModItem {
        public override string Texture => "Terraria/Item_2290";
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
