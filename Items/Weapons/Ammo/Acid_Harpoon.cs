using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
    public class Acid_Harpoon : ModItem {
        public override string Texture => "Origins/Items/Weapons/Ammo/Acid_Harpoon";
        public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Acid Harpoon");
            SacrificeTotal = 99;
            ID = Type;
        }
        public override void SetDefaults() {
            Item.damage = 16;
            Item.DamageType = DamageClass.Ranged;
            Item.consumable = true;
            Item.maxStack = 99;
            Item.shoot = Acid_Harpoon_P.ID;
            Item.ammo = Harpoon.ID;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type, 8);
            recipe.AddIngredient(ItemID.IronBar, 8);
            recipe.AddIngredient(ModContent.ItemType<Acid_Bottle>());
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
            recipe = Recipe.Create(Type, 8);
            recipe.AddIngredient(ItemID.LeadBar);
            recipe.AddIngredient(ModContent.ItemType<Acid_Bottle>());
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    public class Acid_Harpoon_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Harpoon;
		public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Acid Harpoon");
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
                float len = diff.Length() * 0.25f;
                diff /= len;
                Vector2 pos = Projectile.Center;
                for (int i = 0; i < len; i++) {
                    Dust.NewDust(pos - new Vector2(2), 4, 4, DustID.Stone, Scale: 0.75f);
                    pos += diff;
                }
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(BuffID.Venom, Main.rand.Next(270, 360));
        }
    }
}
