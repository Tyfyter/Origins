using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Weapons.Felnum.Tier2 {
    public class Felnum_Staff : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Hævateinn");
            Tooltip.SetDefault("Receives 50% higher damage bonuses\nsprite needs recoloring.");
			Item.staff[Item.type] = true;
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.CrystalVileShard);
            Item.shoot = ModContent.ProjectileType<Felnum_Lightning>();
            Item.damage = 78;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.shootSpeed/=2;
			Item.rare = ItemRarityID.Lime;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Valkyrum_Bar>(), 15);
            recipe.AddIngredient(ItemID.SkyFracture, 1);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
            damage = damage.MultiplyBonuses(1.5f);
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), position);
            Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.5f)*Main.rand.NextFloat(0.9f,1.1f), type, damage, knockback, Item.playerIndexTheItemIsReservedFor, velocity.ToRotation(), Main.rand.NextFloat());
            return false;
        }
    }
    public class Felnum_Lightning : ModProjectile {
        public override string Texture => "Terraria/Images/Projectile_466";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Hævateinn");
        }
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.CultistBossLightningOrbArc);
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 0;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.timeLeft/=3;
            Projectile.penetrate = -1;
        }
        public override void AI() {
            Projectile.type = ProjectileID.CultistBossLightningOrbArc;
			Vector2 targetPos = Projectile.Center;
			bool foundTarget = false;
            Vector2 testPos;
            for (int i = 0; i < Main.maxNPCs; i++) {
				NPC target = Main.npc[i];
				if (target.CanBeChasedBy() && !target.HasBuff(ModContent.BuffType<LightningImmuneFixBuff>())) {
                    testPos = Projectile.Center.Clamp(target.Hitbox);
					Vector2 difference = testPos-Projectile.Center;
                    float distance = difference.Length();
					bool closest = Vector2.Distance(Projectile.Center, targetPos) > distance;
                    bool inRange = distance < 96 && (difference.SafeNormalize(Vector2.Zero)*Projectile.velocity.SafeNormalize(Vector2.Zero)).Length()>0.1f;//magRange;
					if ((!foundTarget || closest) && inRange) {
						targetPos = testPos;
						foundTarget = true;
					}
				}
			}
            if(foundTarget) {
				Vector2 direction = targetPos - Projectile.Center;
				direction.Normalize();
				direction *= Projectile.velocity.Length();
				Projectile.velocity = direction;
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(ModContent.BuffType<LightningImmuneFixBuff>(), 4);
        }
        public override bool? CanHitNPC(NPC target) {
            return target.HasBuff(ModContent.BuffType<LightningImmuneFixBuff>())?false:base.CanHitNPC(target);
        }
    }
    public class LightningImmuneFixBuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_204";
	}
}
