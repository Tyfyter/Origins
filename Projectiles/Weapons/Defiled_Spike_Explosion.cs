using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Projectiles.Weapons {
	public class Defiled_Spike_Explosion : ModProjectile {
		public override string Texture => "Origins/Projectiles/Weapons/Dismay_End";
		public override void SetDefaults() {
			Projectile.timeLeft = 600;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.hide = true;
            Projectile.rotation = Main.rand.NextFloatDirection();
        }
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parentProj) {
                Projectile.npcProj = parentProj.npcProj;
                Projectile.hostile = false;
                Projectile.friendly = parentProj.friendly;
                Projectile.DamageType = parentProj.DamageType;
            }
		}
		public override bool? CanHitNPC(NPC target) => false;
        public override bool CanHitPlayer(Player target) => false;
        public override bool CanHitPvp(Player target) => false;
        public override void AI() {
			if (Projectile.ai[0] > 0) {
                Projectile.ai[0]--;
                int[] immune = Projectile.localNPCImmunity.ToArray();
                Projectile proj = Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    (Vector2)new PolarVec2(Main.rand.NextFloat(8, 16), Projectile.ai[1]++),
                    Defiled_Spike_Explosion_Spike.ID,
                    Projectile.damage,
                    0,
                    Projectile.owner,
                    ai1:Projectile.whoAmI
                );
                for (int i = 0; i < 200; i++) { // for some reason spawning the spikes 
                    if (immune[i] != Projectile.localNPCImmunity[i]) {
                        Projectile.localNPCImmunity[i] = immune[i];
                    }
                }
                proj.localNPCImmunity = Projectile.localNPCImmunity;
                //localNPCImmunity is never overwritten in vanilla, and since it's an array I can just do this to permanently link the cooldowns of two projectiles
            }
        }
    }
	public class Defiled_Spike_Explosion_Spike : ModProjectile {
		public override string Texture => "Origins/Projectiles/Weapons/Dismay_End";
        public static int ID { get; private set; } = -1;
        Vector2 realPosition;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Spike Eruption");
            ID = Projectile.type;
        }
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.timeLeft = Main.rand.Next(22, 25);
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.aiStyle = 0;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = false;
            Projectile.hide = true;
        }
        public override void OnSpawn(IEntitySource source) {
            if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parentProj) {
                Projectile.npcProj = parentProj.npcProj;
                Projectile.hostile = parentProj.npcProj && !parentProj.friendly;
                Projectile.friendly = parentProj.friendly;
                Projectile.DamageType = parentProj.DamageType;
            }
            realPosition = Projectile.Center;
        }
        public float movementFactor {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override void AI() {
            Projectile.Center = realPosition - Projectile.velocity;
            if (movementFactor == 0f) {
                movementFactor = 1f;
                //if(projectile.timeLeft == 25)projectile.timeLeft = projOwner.itemAnimationMax-1;
                Projectile.netUpdate = true;
            }
            if (Projectile.timeLeft > 18) {
                movementFactor += 1f;
            }
            Projectile.position += Projectile.velocity * movementFactor;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.rotation += MathHelper.PiOver2;
            Main.projectile[(int)Projectile.ai[1]].timeLeft = 7;
        }
		public override bool? CanHitNPC(NPC target) {
			if (target.Hitbox.Intersects(Projectile.Hitbox)) {

			}
			if (Projectile.localNPCImmunity[target.whoAmI] == 0) {
                return null;
			}
			return false;
        }
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
            behindNPCsAndTiles.Add(index);
		}
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            Projectile.localNPCImmunity[target.whoAmI] = 35*7;
            target.immune[Projectile.owner] = 0;
        }
		public override bool PreDraw(ref Color lightColor) {
            float totalLength = Projectile.velocity.Length() * movementFactor;
            int avg = (lightColor.R + lightColor.G + lightColor.B) / 3;
            lightColor = Color.Lerp(lightColor, new Color(avg, avg, avg), 0.5f);
            Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 18, System.Math.Min(58, (int)totalLength)), lightColor, Projectile.rotation, new Vector2(9, 0), Projectile.scale, SpriteEffects.None, 0);
            totalLength -= 58;
            Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.Zero) * 58;
            Texture2D texture = Mod.Assets.Request<Texture2D>("Projectiles/Weapons/Dismay_Mid").Value;
            int c = 0;
            Vector2 pos;
            for (int i = (int)totalLength; i > 0; i -= 58) {
                c++;
                pos = (Projectile.Center - Main.screenPosition) - (offset * c);
                //lightColor = Projectile.GetAlpha(new Color(Lighting.GetColor((pos + Projectile.velocity * 2).ToTileCoordinates()).ToVector4()));
                Main.EntitySpriteDraw(texture, pos, new Rectangle(0, 0, 18, Math.Min(58, i)), lightColor, Projectile.rotation, new Vector2(9, 0), Projectile.scale, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
