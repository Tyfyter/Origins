using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Armor.Felnum;
using Origins.Items.Weapons.Explosives;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
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
        public static int FelnumHeadArmorID;
        public static int FelnumBodyArmorID;
        public static int FelnumLegsArmorID;
        public static int[] celestineBoosters;
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
            OriginExtensions.drawPlayerItemPos = (Func<float,int,Vector2>)typeof(Main).GetMethod("DrawPlayerItemPos",BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate(typeof(Func<float,int,Vector2>), Main.instance);
        }
        public override void Unload() {
            ExplosiveProjectiles = null;
            ExplosiveItems = null;
            ExplosiveAmmo = null;
            ExplosiveBaseDamage = null;
            ExplosiveModOnHit = null;
            celestineBoosters = null;
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
