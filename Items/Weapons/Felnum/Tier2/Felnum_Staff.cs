using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Felnum.Tier2 {
    public class Felnum_Staff : ModItem {
        public const int baseDamage = 78;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Hævateinn");
            Tooltip.SetDefault("Receives 50% higher damage bonuses\nsprite needs recoloring.");
			Item.staff[item.type] = true;
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.CrystalVileShard);
            item.shoot = ModContent.ProjectileType<Felnum_Lightning>();
            item.damage = baseDamage;
            item.useAnimation = 30;
            item.useTime = 30;
            item.shootSpeed/=2;
			item.rare = ItemRarityID.Lime;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Valkyrum_Bar>(), 15);
            recipe.AddIngredient(ItemID.SkyFracture, 1);
            recipe.SetResult(this);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.AddRecipe();
        }
        public override void GetWeaponDamage(Player player, ref int damage) {
            damage+=(damage-baseDamage)/2;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Vector2 speed = new Vector2(speedX, speedY);
            //damage+=(damage-35)/2;
			Main.PlaySound(2, (int)player.Center.X, (int)player.Center.Y, 122, 2f, 1f);
            Projectile.NewProjectile(position, speed.RotatedByRandom(0.5f)*Main.rand.NextFloat(0.9f,1.1f), type, damage, knockBack, item.owner, speed.ToRotation(), Main.rand.NextFloat());
            return false;
        }
    }
    public class Felnum_Lightning : ModProjectile {
        public override string Texture => "Terraria/Projectile_466";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Hævateinn");
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.CultistBossLightningOrbArc);
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 0;
            projectile.hostile = false;
            projectile.friendly = true;
            projectile.timeLeft/=3;
            projectile.penetrate = -1;
        }
        public override void AI() {
            projectile.type = ProjectileID.CultistBossLightningOrbArc;
			Vector2 targetPos = projectile.Center;
			bool foundTarget = false;
            Vector2 testPos;
            for (int i = 0; i < Main.maxNPCs; i++) {
				NPC target = Main.npc[i];
				if (target.CanBeChasedBy() && !target.HasBuff(ModContent.BuffType<LightningImmuneFixBuff>())) {
                    testPos = projectile.Center.Clamp(target.Hitbox);
					Vector2 difference = testPos-projectile.Center;
                    float distance = difference.Length();
					bool closest = Vector2.Distance(projectile.Center, targetPos) > distance;
                    bool inRange = distance < 96 && (difference.SafeNormalize(Vector2.Zero)*projectile.velocity.SafeNormalize(Vector2.Zero)).Length()>0.1f;//magRange;
					if ((!foundTarget || closest) && inRange) {
						targetPos = testPos;
						foundTarget = true;
					}
				}
			}
            if(foundTarget) {
				Vector2 direction = targetPos - projectile.Center;
				direction.Normalize();
				direction *= projectile.velocity.Length();
				projectile.velocity = direction;
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
        public override bool Autoload(ref string name, ref string texture) {
            texture = "Terraria/Buff_204";
            return base.Autoload(ref name, ref texture);
        }
    }
}
