using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
    public class Flammable_Harpoon : ModItem {
        public override string Texture => "Origins/Items/Weapons/Ammo/Flammable_Harpoon";
        public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Flammable Harpoon");
            SacrificeTotal = 99;
            ID = Type;
        }
        public override void SetDefaults() {
            Item.damage = 10;
            Item.DamageType = DamageClass.Ranged;
            Item.consumable = true;
            Item.maxStack = 99;
            Item.shoot = Flammable_Harpoon_P.ID;
            Item.ammo = Harpoon.ID;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.IronBar);
            recipe.AddIngredient(ItemID.Gel, 3);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
            recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.LeadBar);
            recipe.AddIngredient(ItemID.Gel, 3);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    public class Flammable_Harpoon_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Harpoon;
		public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Flammable Harpoon");
            ID = Type;
        }
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Harpoon);
        }
		public override void OnSpawn(IEntitySource source) {
            if (Projectile.ai[1] == 1) {
                Projectile.penetrate = 2;
            }
		}
		public override void AI() {
            if (Projectile.ai[0] == 1 && Projectile.penetrate >= 0) {
                Projectile.aiStyle = 1;
                Projectile.velocity = Projectile.oldVelocity;
                Projectile.tileCollide = true;
                Vector2 diff = Main.player[Projectile.owner].itemLocation - Projectile.Center;
                SoundEngine.PlaySound(SoundID.Item10, Projectile.Center + diff / 2);
                float len = diff.Length() * 0.125f;
                diff /= len;
                Vector2 pos = Projectile.Center;
                for (int i = 0; i < len; i++) {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, default, Cursed_Harpoon_Flame.ID, Projectile.damage, 0, Projectile.owner, i * 0.15f);
                    pos += diff;
				}
            }
		}
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(BuffID.OnFire, Main.rand.Next(270, 360));
        }
    }
}
