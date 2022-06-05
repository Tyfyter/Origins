using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;

namespace Origins.Items.Weapons.Explosives {
    public class Fallout : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fallout");
			Tooltip.SetDefault("");
            glowmask = Origins.AddGlowMask(this);
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.ProximityMineLauncher);
			Item.damage = 250;
			Item.useTime = 90;
			Item.useAnimation = 90;
			Item.value = 5000;
            Item.shootSpeed*=0.75f;
            Item.shoot = ModContent.ProjectileType<Fallout_P1>();
			Item.rare = ItemRarityID.Lime;
            Item.autoReuse = true;
            Item.glowMask = glowmask;
        }
        public override void AddRecipes() {
            Origins.AddExplosive(Item);
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			//type = item.shoot+(type-item.shoot)/3;
            Projectile.NewProjectile(source, position, velocity, Item.shoot, damage, knockback, player.whoAmI, 0, type-Item.shoot+ProjectileID.RocketI);
            return false;
        }
    }
    public class Fallout_P1 : ModProjectile {
        public override string Texture => "Terraria/Images/Projectile_"+ProjectileID.RocketI;
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.RocketI);
            Projectile.aiStyle = 0;
            Projectile.penetrate = 1;
            Projectile.width+=4;
            Projectile.height+=4;
            Projectile.scale = 1.25f;
        }
        public override void AI() {
            Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;
            int num248 = Dust.NewDust(Projectile.Center - Projectile.velocity * 0.5f-new Vector2(0,4), 0, 0, DustID.Torch, 0f, 0f, 100);
			Dust dust3 = Main.dust[num248];
			dust3.scale *= 1f + Main.rand.Next(10) * 0.1f;
			dust3.velocity *= 0.2f;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            Projectile.Kill();
            return false;
        }
        public override bool PreKill(int timeLeft) {
            Projectile.type = ProjectileID.RocketI;
            return true;
        }
        public override void Kill(int timeLeft) {
            Projectile.ai[0] = 1;
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 640;
			Projectile.height = 640;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Fallout_Field>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, Projectile.ai[1]);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            return Projectile.ai[0]>0?(bool?)((Projectile.Center.Clamp(targetHitbox)-Projectile.Center).Length()<=320):null;
        }
    }
    public class Fallout_Field : ModProjectile {
        public override string Texture => "Origins/Projectiles/Pixel";
        public List<Vector2> targets = new List<Vector2>(){};
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.RocketI);
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 900;
            Projectile.tileCollide = false;
			Projectile.width = 640;
			Projectile.height = 640;
        }
        public override void AI() {
            if(Projectile.timeLeft%15==0) {
                Vector2 offset = Main.rand.NextVector2Circular(160, 160)+Main.rand.NextVector2Circular(160, 160);
                if(targets.Count>0&&!Main.rand.NextBool(3))offset = Main.rand.Next(targets);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center+offset, Vector2.Zero, ModContent.ProjectileType<Fallout_Cloud>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.ai[1]);
                targets.Clear();
            }
            for(int i = 0; i < 6; i++) {
                Dust dust = Dust.NewDustDirect(Projectile.Center+Main.rand.NextVector2Circular(140,140)+Main.rand.NextVector2Circular(140,140), 0, 0, DustID.Electric, 0, 0, 100, new Color(0, 255, 0), 0.75f*Projectile.scale);
                dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
                dust.noGravity = true;
                dust.noLight = true;
            }
        }
        public override void Kill(int timeLeft) {
            Projectile.ai[0] = 1;
			Projectile.Damage();
        }
        public override bool? CanHitNPC(NPC target) {
            if((target.Center-Projectile.Center).Length()<320)targets.Add(target.Center-Projectile.Center);
            return Projectile.ai[0]>0?null:(bool?)false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            return (Projectile.Center.Clamp(targetHitbox)-Projectile.Center).Length()<=320;
        }
    }
    public class Fallout_Cloud : ModProjectile {
        public override string Texture => "Origins/Projectiles/Pixel";
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.RocketI);
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 0;
        }
        public override bool PreKill(int timeLeft) {
            Projectile.type = (int)Projectile.ai[0];
            return true;
        }
        public override void Kill(int timeLeft) {
            for(int i = 0; i < 7; i++) {
                Dust dust = Dust.NewDustDirect(Projectile.position, 10, 10, DustID.Electric, 0, 0, 100, new Color(0, 255, 0), 1.25f*Projectile.scale);
                dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
                dust.noGravity = true;
                dust.noLight = true;
            }
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = Projectile.ai[0]>ProjectileID.RocketII? 128: 96;
			Projectile.height = Projectile.width;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            return (Projectile.Center.Clamp(targetHitbox)-Projectile.Center).Length()<=(Projectile.ai[0]>ProjectileID.RocketII? 72:48);
        }
    }
}
