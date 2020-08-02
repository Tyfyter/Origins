using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Felnum.Tier2 {
    public class Felnum_Staff : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Hævateinn");
            Tooltip.SetDefault("Recieves 50% higher damage bonuses\nplaceholder sprite... maybe.");
			Item.staff[item.type] = true;
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.CrystalVileShard);
            item.shoot = ModContent.ProjectileType<Felnum_Lightning>();
            item.damage = 47;
            item.shootSpeed/=2;
			item.rare = ItemRarityID.Lime;
        }
        public override void GetWeaponDamage(Player player, ref int damage) {
            //if(!OriginPlayer.ItemChecking)
            damage+=(damage-35)/2;
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
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(ModContent.BuffType<LightningImmuneFixBuff>(), 10);
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
