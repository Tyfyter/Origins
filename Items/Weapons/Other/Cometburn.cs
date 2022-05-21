using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.DataStructures;
using Origins.Projectiles.Misc;
using Terraria.Graphics.Shaders;
using Tyfyter.Utils;

namespace Origins.Items.Weapons.Other {
    public class Cometburn : ModItem {
        public override bool CloneNewInstances => true;
        float shootSpeed;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Cometburn");
            Tooltip.SetDefault("");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.MeteorStaff);
            item.damage = 99;
            item.magic = true;
            item.noUseGraphic = true;
            item.noMelee = true;
            item.width = 58;
            item.height = 58;
            item.useStyle = 5;
            item.useTime = 22;
            item.useAnimation = 22;
            item.knockBack = 9.5f;
            item.value = 500000;
            item.rare = ItemRarityID.Purple;
            item.shoot = Cometburn_P.ID;
            item.shootSpeed = 10f;
            item.autoReuse = true;
            item.mana = 15;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips[0].overrideColor = new Color(0, Main.mouseTextColor, 0, Main.mouseTextColor);
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			for (int i = 0; i < 3; i++) {
                Vector2 speed = new Vector2(0, item.shootSpeed).RotatedByRandom(1);
                Projectile.NewProjectile(Main.MouseWorld - new Vector2(0, 72) - (speed * 80), speed, type, damage, knockBack, player.whoAmI, ai1:Main.MouseWorld.Y);
            }
            return false;
        }
    }
    public class Cometburn_P : ModProjectile {
        public static int ID { get; private set; }
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Cometburn");
            ID = projectile.type;
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Meteor1);
            projectile.width = 42;
            projectile.height = 44;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10;
            projectile.penetrate = -1;
            projectile.extraUpdates = 1;
        }
		public override void AI() {
            Lighting.AddLight(projectile.Center, 0, 0.5f, 0);
            if (Main.rand.NextBool(9)) {
                Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Electric, 0, 0, 100, new Color(0, 255, 0), 0.5f);
                dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
                dust.fadeIn = Main.rand.NextFloat(0.1f);
                dust.noGravity = false;
                dust.noLight = true;
            }
			if (projectile.Center.Y > projectile.ai[1]) {
                projectile.tileCollide = true;
			}
        }
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if(Main.rand.NextBool(3)) target.AddBuff(BuffID.VortexDebuff, 720);//placeholder
        }
        public override void OnHitPlayer(Player target, int damage, bool crit) {
            if (Main.rand.NextBool(3)) target.AddBuff(BuffID.VortexDebuff, 720);//placeholder
        }
    }
}
