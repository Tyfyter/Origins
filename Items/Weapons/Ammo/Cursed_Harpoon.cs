﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
    public class Cursed_Harpoon : ModItem {
        public override string Texture => "Origins/Items/Weapons/Ammo/Cursed_Harpoon";
        public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Cursed Harpoon");
            SacrificeTotal = 99;
            ID = Type;
        }
        public override void SetDefaults() {
            Item.damage = 20;
            Item.DamageType = DamageClass.Ranged;
            Item.consumable = true;
            Item.maxStack = 99;
            Item.shoot = Cursed_Harpoon_P.ID;
            Item.ammo = Harpoon.ID;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.IronBar);
            recipe.AddIngredient(ItemID.CursedFlame);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
            recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.LeadBar);
            recipe.AddIngredient(ItemID.CursedFlame);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    public class Cursed_Harpoon_P : Harpoon_P {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Harpoon;
		public static new int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Cursed Harpoon");
            ID = Type;
        }
		public override void AI() {//still needs its own AI override since it has unique AI functionality
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
            if (Projectile.penetrate == 1) {
                Projectile.penetrate--;
            }
        }
    }
    public class Cursed_Harpoon_Flame : ModProjectile {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CursedDartFlame;
        public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Cursed Harpoon");
            ID = Type;
        }
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.CursedDartFlame);
            Projectile.friendly = false;
            Projectile.alpha = 255;
            Projectile.aiStyle = 0;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }
        public override void OnSpawn(IEntitySource source) {
            Projectile.timeLeft = 10 + (int)Projectile.ai[0];
        }
		public override bool PreAI() {
			return Projectile.timeLeft <= 10;
		}
		public override void AI() {
            Projectile.friendly = true;
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 75, 0f, 0f, 100);
            dust.position.X -= 2f;
            dust.position.Y += 2f;
            dust.scale += Main.rand.Next(50) * 0.01f;
            dust.noGravity = true;
            dust.velocity.Y -= 2f;
            if (Main.rand.NextBool(2)) {
                Dust dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 75, 0f, 0f, 100);
                dust2.position.X -= 2f;
                dust2.position.Y += 2f;
                dust2.scale += 0.3f + Main.rand.Next(50) * 0.01f;
                dust2.noGravity = true;
                dust2.velocity *= 0.1f;
            }
        }
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			target.AddBuff(BuffID.CursedInferno, Main.rand.Next(270, 360));
		}
	}
}