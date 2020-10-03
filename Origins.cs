using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Armor.Felnum;
using Origins.Items.Weapons.Explosives;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins {
	public class Origins : Mod {
        public static Origins instance;
        /// <summary>
        /// Due to the placement of ResizeArrays this can only be set after Mod.AddRecipes,
        /// for cross-mod content use ExplosiveProjectilePreRegistry or AddExplosive instead
        /// </summary>
        public static bool[] ExplosiveProjectiles;
        /// <summary>
        /// Due to the placement of ResizeArrays this can only be set after Mod.AddRecipes,
        /// for cross-mod content use ExplosiveItemPreRegistry or AddExplosive instead
        /// </summary>
        public static bool[] ExplosiveItems;
        /// <summary>
        /// Due to the placement of ResizeArrays this can only be set after Mod.AddRecipes,
        /// for cross-mod content use ExplosiveAmmoPreRegistry or AddExplosive instead
        /// </summary>
        public static bool[] ExplosiveAmmo;
        public static Stack<int> ExplosiveProjectilePreRegistry;
        public static Stack<int> ExplosiveItemPreRegistry;
        public static Stack<int> ExplosiveAmmoPreRegistry;
        public static Dictionary<int,int> ExplosiveBaseDamage;
        public static List<int> ExplosiveModOnHit;
        public static ushort[] VanillaElements;
        public static int FelnumHeadArmorID;
        public static int FelnumBodyArmorID;
        public static int FelnumLegsArmorID;
        public static int[] celestineBoosters;
        public static MiscShaderData perlinFade0;
		public Origins() {
            instance = this;
            celestineBoosters = new int[3];
        }
        public override void AddRecipes() {
        #region explosive weapon registry
            ExplosiveProjectiles = new bool[ProjectileID.Sets.CanDistortWater.Length];
            ExplosiveItems = (bool[])ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn.Clone();
#region items
            //ExplosiveItems[ProjectileID.Grenade] = true;
            //ExplosiveItems[ProjectileID.StickyGrenade] = true;
            //ExplosiveItems[ProjectileID.BouncyGrenade] = true;
            //ExplosiveItems[ProjectileID.Bomb] = true;
            //ExplosiveItems[ProjectileID.StickyBomb] = true;
            ExplosiveItems[ItemID.BouncyBomb] = true;
            ExplosiveItems[ItemID.HellfireArrow] = true;
            //ExplosiveItems[ProjectileID.Dynamite] = true;
            //ExplosiveItems[ProjectileID.StickyDynamite] = true;
            //ExplosiveItems[ProjectileID.BouncyDynamite] = true;
            ExplosiveItems[ItemID.BombFish] = true;
            ExplosiveItems[ItemID.PartyGirlGrenade] = true;
            ExplosiveItems[ItemID.Beenade] = true;
            ExplosiveItems[ItemID.MolotovCocktail] = true;
#endregion items
#region projectiles
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
            ExplosiveProjectiles[ModContent.ProjectileType<Awe_Grenade_P>()] = true;
            ExplosiveProjectiles[ModContent.ProjectileType<Awe_Grenade_Blast>()] = true;
#endregion projectiles
#region ammo
            ExplosiveAmmo = (bool[])ExplosiveItems.Clone();
            ExplosiveAmmo[AmmoID.Rocket] = true;
            ExplosiveAmmo[AmmoID.StyngerBolt] = true;
#endregion ammo
#region base damage
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
            #endregion base damage
#region preregistry
            Stack<int> stack = ExplosiveItemPreRegistry;
            int i;
            while(stack.Count > 0) {
                i = stack.Pop();
                ExplosiveItems[i] = true;
            }
            stack = ExplosiveProjectilePreRegistry;
            while(stack.Count > 0) {
                i = stack.Pop();
                ExplosiveProjectiles[i] = true;
            }
            stack = ExplosiveAmmoPreRegistry;
            while(stack.Count > 0) {
                i = stack.Pop();
                ExplosiveAmmo[i] = true;
            }
            ExplosiveItemPreRegistry = null;
            ExplosiveProjectilePreRegistry = null;
            ExplosiveAmmoPreRegistry = null;
#endregion preregistry
        #endregion explosive weapon registry
            FelnumHeadArmorID = ModContent.GetInstance<Felnum_Helmet>().item.headSlot;
            FelnumBodyArmorID = ModContent.GetInstance<Felnum_Breastplate>().item.bodySlot;
            FelnumLegsArmorID = ModContent.GetInstance<Felnum_Greaves>().item.legSlot;
        }
        public override void Load() {
            ExplosiveBaseDamage = new Dictionary<int, int>();
            ExplosiveModOnHit = new List<int>() {};
            ExplosiveProjectilePreRegistry = new Stack<int>();
            ExplosiveItemPreRegistry = new Stack<int>();
            ExplosiveAmmoPreRegistry = new Stack<int>();
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
            OriginExtensions.drawPlayerItemPos = (Func<float,int,Vector2>)typeof(Main).GetMethod("DrawPlayerItemPos",BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate(typeof(Func<float,int,Vector2>), Main.instance);
            perlinFade0 = new MiscShaderData(new Ref<Effect>(GetEffect("Effects/PerlinFade")), "RedFade");
            perlinFade0.UseImage("Images/Misc/Perlin");
            perlinFade0.Shader.Parameters["uThreshold0"].SetValue(0.6f);
            perlinFade0.Shader.Parameters["uThreshold1"].SetValue(0.6f);
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
            OriginExtensions.drawPlayerItemPos = null;
            instance = null;
        }
        public static void AddExplosive(Item item, bool noProj = false) {
            if(ExplosiveItems == null) {
                ExplosiveItemPreRegistry.Push(item.type);
                ExplosiveAmmoPreRegistry.Push(item.type);
                if(item.ammo!=AmmoID.None)ExplosiveAmmoPreRegistry.Push(item.type);
                if(item.useAmmo!=AmmoID.None)ExplosiveAmmoPreRegistry.Push(item.type);
                if(!noProj&&item.shoot!=ProjectileID.None)ExplosiveProjectilePreRegistry.Push(item.type);
                return;
            }
            ExplosiveItems[item.type] = true;
            ExplosiveAmmo[item.type] = true;
            if(item.ammo!=AmmoID.None)ExplosiveAmmo[item.ammo] = true;
            if(item.useAmmo!=AmmoID.None)ExplosiveAmmo[item.useAmmo] = true;
            if(!noProj&&item.shoot!=ProjectileID.None)ExplosiveProjectiles[item.shoot] = true;
            instance.Logger.Info($"Registered {item.Name} as explosive :"+ExplosiveItems[item.type]);
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
    }
}
