using Microsoft.Xna.Framework;
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
        /// Due to the placement of ResizeArrays this can only be set after Mod.AddRecipes
        /// </summary>
        public static bool[] ExplosiveProjectiles;
        /// <summary>
        /// Due to the placement of ResizeArrays this can only be set after Mod.AddRecipes
        /// </summary>
        public static bool[] ExplosiveItems;
        /// <summary>
        /// Due to the placement of ResizeArrays this can only be set after Mod.AddRecipes
        /// </summary>
        public static bool[] ExplosiveAmmo;
        public static Dictionary<int,int> ExplosiveBaseDamage;
        public static List<int> ExplosiveModOnHit;
		public Origins() {
            instance = this;
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
            ExplosiveProjectiles[ProjectileID.BombFish] = true;
            ExplosiveProjectiles[ProjectileID.PartyGirlGrenade] = true;
            ExplosiveProjectiles[ProjectileID.Beenade] = true;
            ExplosiveProjectiles[ProjectileID.MolotovCocktail] = true;
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
        #endregion explosive weapon registry
        }
        public override void Load() {
            ExplosiveBaseDamage = new Dictionary<int, int>();
            ExplosiveModOnHit = new List<int>() {};
            OriginExtensions.drawPlayerItemPos = (Func<float,int,Vector2>)typeof(Main).GetMethod("DrawPlayerItemPos",BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate(typeof(Func<float,int,Vector2>), Main.instance);
        }
        public override void Unload() {
            ExplosiveProjectiles = null;
            ExplosiveItems = null;
            ExplosiveAmmo = null;
            ExplosiveBaseDamage = null;
            ExplosiveModOnHit = null;
            instance = null;
            OriginExtensions.drawPlayerItemPos = null;
        }
        public static void AddExplosive(Item item, bool noProj = false) {
            ExplosiveItems[item.type] = true;
            ExplosiveAmmo[item.type] = true;
            if(item.ammo!=AmmoID.None)ExplosiveAmmo[item.ammo] = true;
            if(item.useAmmo!=AmmoID.None)ExplosiveAmmo[item.useAmmo] = true;
            if(!noProj&&item.shoot!=ProjectileID.None)ExplosiveProjectiles[item.shoot] = true;
            instance.Logger.Info($"Registered {item.Name} as explosive :"+ExplosiveItems[item.type]);
        }
    }
    public static class OriginExtensions {
        public static Func<float, int, Vector2> drawPlayerItemPos;
        public static void PlaySound(string Name, Vector2 Position, float Volume = 1f, float PitchVariance = 1f){
            if (Main.dedServ || string.IsNullOrEmpty(Name)) return;
            var sound = Origins.instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/" + Name);
            Main.PlaySound(sound.WithVolume(Volume).WithPitchVariance(PitchVariance), Position);
        }
        public static Vector2 DrawPlayerItemPos(float gravdir, int itemtype) {
            return drawPlayerItemPos(gravdir, itemtype);
        }
    }
}
