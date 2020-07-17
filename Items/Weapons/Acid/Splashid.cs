using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Acid {
    public class Splashid : ModItem, IElementalItem {
        public short element => Elements.acid;

        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acid Splash");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.RubyStaff);
			item.damage = 100;
			item.magic = true;
			item.noMelee = true;
			item.noUseGraphic = true;
            item.useStyle = 1;
			item.width = 28;
			item.height = 30;
			item.useTime = 24;
			item.useAnimation = 24;
            item.reuseDelay = 8;
			item.mana = 18;
			item.value = 5000;
            item.shoot = ModContent.ProjectileType<Acid_Splash_P>();
			item.rare = ItemRarityID.Lime;
		}
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            int a = Main.rand.Next(5,7);
            for(int i = 0; ++i < a; a = Main.rand.Next(5,7)) {
                Projectile.NewProjectile(position, new Vector2(speedX, speedY).RotatedBy(((i-a/2f)/a)*0.75), type, damage/6, knockBack, player.whoAmI);
            }
            return false;
        }
    }
    public class Acid_Splash_P : ModProjectile {
        public override string Texture => "Origins/Projectiles/Pixel";
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Bullet);
            projectile.penetrate = 1;//when projectile.penetrate reaches 0 the projectile is destroyed
            projectile.extraUpdates = 1;
            projectile.width = projectile.height = 10;
            projectile.light = 0;
            projectile.timeLeft = 180;
        }
        public override void AI() {
            if(projectile.timeLeft<168) {
                Lighting.AddLight(projectile.Center, 0, 0.75f, 0.3f);
                Dust dust = Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity*-0.25f, 100, new Color(0, 255, 0), 1f);
                dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
                dust.noGravity = false;
                dust.noLight = true;
            } else {
			    projectile.Center = Main.player[projectile.owner].itemLocation+projectile.velocity;
            }
        }
        public override void Kill(int timeLeft) {
            for(int i = 0; i < 7; i++) {
                Dust dust = Dust.NewDustDirect(projectile.position, 10, 10, 226, 0, 0, 100, new Color(0, 255, 0), 1.25f);
                dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
                dust.noGravity = true;
                dust.noLight = true;
            }
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = 128;
			projectile.height = 128;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
			projectile.Damage();
            Main.PlaySound(SoundID.Item10, projectile.position);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(ModContent.BuffType<SolventBuff>(), 480);
            Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, 226, 0, 0, 100, new Color(0, 255, 0), 1.25f);
            dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
            dust.noGravity = false;
            dust.noLight = true;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            return false;
        }
    }
    public class SolventBuff : ModBuff {
        public override bool Autoload(ref string name, ref string texture) {
            texture = "Terraria/Buff_160";
            return true;
        }
        public override void SetDefaults() {
            DisplayName.SetDefault("Solvent");
        }
        public override void Update(NPC npc, ref int buffIndex) {
            npc.lifeRegen -= 12;
        }
    }
}
