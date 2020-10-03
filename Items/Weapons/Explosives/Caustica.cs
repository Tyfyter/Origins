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

namespace Origins.Items.Weapons.Explosives {
    public class Caustica : ModItem, IElementalItem {
        public ushort element => Elements.Acid;

        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Caustica");
			Tooltip.SetDefault("Needs Resprite, currently vampire knives with palette from Caustacyst");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.RubyStaff);
			item.damage = 270;
			item.magic = true;
			item.noMelee = true;
			item.noUseGraphic = true;
            item.useStyle = 1;
			item.width = 28;
			item.height = 30;
			item.useTime = 24;
			item.useAnimation = 24;
            item.reuseDelay = 8;
			item.mana = 16;
			item.value = 5000;
            item.shoot = ModContent.ProjectileType<Caustica_P>();
			item.rare = ItemRarityID.Lime;
		}
        public override void AddRecipes() {
            Origins.AddExplosive(item);
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            int a = Main.rand.Next(5,7);
            for(int i = 0; ++i < a; a = Main.rand.Next(5,7)) {
                Projectile.NewProjectile(position, new Vector2(speedX, speedY).RotatedBy(((i-a/2f)/a)*0.35), type, damage/2, knockBack, player.whoAmI, 0, 12f);
            }
            return false;
        }
    }
    public class Caustica_P : ModProjectile {
        public override string Texture => "Origins/Projectiles/Pixel";
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Bullet);
            projectile.aiStyle = 0;
            projectile.penetrate = -1;
            projectile.extraUpdates = 0;
            projectile.width = projectile.height = 10;
            projectile.light = 0;
            projectile.timeLeft = 120;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 20;
            projectile.ignoreWater = true;
        }
        public override void AI() {
            if(projectile.ai[0]>0) {
                projectile.ai[0]++;
                projectile.timeLeft++;
                if(Main.npc[(int)projectile.ai[1]].active && !projectile.Hitbox.Intersects(Main.npc[(int)projectile.ai[1]].Hitbox)) {
                    projectile.ai[0] = 15;
                    //projectile.Center-=(projectile.Center-Main.npc[(int)projectile.ai[1]].Center)*0.1f;
                }
                if(projectile.ai[0]>=15) {
                    projectile.ai[0] = -1;
                    projectile.Kill();
                }
            }
            //for(int i = 2; 0<--i;) {
                Dust dust = Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity.RotatedByRandom(1.4)/4f, 100, projectile.timeLeft%2==0 ? Color.LightSeaGreen : Color.Coral, projectile.scale*(projectile.ai[0]>0?0.6f:1));
                dust.shader = GameShaders.Armor.GetSecondaryShader(projectile.timeLeft%2==0 ? 90 : 1, Main.LocalPlayer);
                dust.noGravity = false;
                dust.noLight = true;
            //}
        }
        public override bool? CanHitNPC(NPC target) {
            return ((int)projectile.ai[0]<=0)?null:((bool?)false);
        }
        public override void Kill(int timeLeft) {
            projectile.ai[0] = 0;
            for(int i = 0; i < 9; i++) {
                Dust dust = Dust.NewDustDirect(projectile.position, 10, 10, 226, 0, 0, 100, i%2==0 ? Color.LightSeaGreen : Color.Coral, projectile.scale);
                dust.shader = GameShaders.Armor.GetSecondaryShader(i%2==0 ? 90 : 1, Main.LocalPlayer);
                dust.velocity*=4;
                dust.noGravity = true;
                dust.noLight = true;
            }
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = (int)(96*projectile.scale);
			projectile.height = (int)(96*projectile.scale);
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
            projectile.damage = (int)(projectile.damage*0.75f);
			projectile.Damage();
            //Main.PlaySound(SoundID.Item10, projectile.position);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if(projectile.ai[0]==0) {
                projectile.ai[0]++;
                projectile.ai[1] = target.whoAmI;
            }
            target.AddBuff(ModContent.BuffType<Acid.SolventBuff>(), 480);
            //target.AddBuff(ModContent.BuffType<Toxic>(), 480);
            target.AddBuff(BuffID.CursedInferno, 480);
            target.AddBuff(BuffID.Venom, 480);
            for(int i = 5; 0<--i;) {
                Dust dust = Dust.NewDustDirect(target.position,target.width,target.height, 226, 0, 0, 100, i%2==0 ? Color.LightSeaGreen : Color.Coral, projectile.scale);
                dust.shader = GameShaders.Armor.GetSecondaryShader(i%2==0 ? 90 : 1, Main.LocalPlayer);
                dust.noGravity = false;
                dust.noLight = true;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            return false;
        }
    }
}
