using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins {
	public class Origins : Mod {
        public static bool[] ExplosiveProjectiles;
        public static bool[] ExplosiveItems;
        public static bool[] ExplosiveAmmo;
        public static Dictionary<int,int> ExplosiveBaseDamage;
        public static List<int> ExplosiveModOnHit;
		public Origins() {
		}
        public override void AddRecipes() {
        #region explosive weapon registry
            ExplosiveBaseDamage = new Dictionary<int, int>();
            ExplosiveModOnHit = new List<int>() { };
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
            ExplosiveBaseDamage.Add(ItemID.Bomb, 80);
            ExplosiveBaseDamage.Add(ItemID.StickyBomb, 80);
            ExplosiveBaseDamage.Add(ItemID.BouncyBomb, 80);
            ExplosiveBaseDamage.Add(ItemID.BombFish, 80);
            ExplosiveBaseDamage.Add(ItemID.Dynamite, 200);
            ExplosiveBaseDamage.Add(ItemID.StickyDynamite, 200);
            ExplosiveBaseDamage.Add(ItemID.BouncyDynamite, 200);
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
        public override void Unload() {
            ExplosiveProjectiles = null;
            ExplosiveItems = null;
            ExplosiveAmmo = null;
            ExplosiveBaseDamage = null;
            ExplosiveModOnHit = null;
        }
    }
}
