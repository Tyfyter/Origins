using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.NPCs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Items.Weapons.Defiled {
	public class Infusion : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Infusion");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
			item.damage = 19;
			item.magic = true;
			item.mana = 7;
            item.noMelee = true;
            item.noUseGraphic = false;
			item.width = 30;
			item.height = 36;
			item.useTime = 20;
			item.useAnimation = 20;
			item.useStyle = 5;
			item.knockBack = 5;
            item.shoot = ModContent.ProjectileType<Infusion_P>();
            item.shootSpeed = 16f;
			item.value = 5000;
            item.useTurn = false;
			item.rare = ItemRarityID.Blue;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(8, 0);
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			Vector2 vel = new Vector2(speedX, speedY).RotatedByRandom(0.075f);
			speedX = vel.X;
			speedY = vel.Y;
			return true;
		}
	}
    public class Infusion_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Defiled/Infusion_P";
		PolarVec2 embedPos;
		float embedRotation;
		const int embed_duration = 600;
		int EmbedTime { get => (int)projectile.localAI[0]; set => projectile.localAI[0] = value; }
		int EmbedTarget { get => (int)projectile.localAI[1]; set => projectile.localAI[1] = value; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Infusion");
		}
		public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			projectile.ranged = false;
			projectile.magic = true;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 15;
			projectile.width = 8;
			projectile.height = 8;
			projectile.penetrate = -1;
			projectile.extraUpdates = 1;
			projectile.hide = true;
		}
		public override void AI() {
			//projectile.aiStyle = projectile.wet?0:1;
			if (EmbedTime > 0) {//embedded in enemy
				EmbedTime++;
				NPC target = Main.npc[EmbedTarget];
				projectile.Center = target.Center + (Vector2)embedPos.RotatedBy(target.rotation);
				projectile.rotation = embedRotation + target.rotation;
				if(projectile.numUpdates == 0 && EmbedTime > 10) OriginGlobalNPC.AddInfusionSpike(target, projectile.whoAmI);
				if (!target.active) {
					EmbedTime = embed_duration + 1;
				}
				if (EmbedTime > embed_duration) {
					projectile.Kill();
				}
			} else if (projectile.aiStyle == 1) {//not embedded
				projectile.rotation = projectile.velocity.ToRotation() + MathHelper.Pi * 0.25f;
				Vector2 boxSize = (Vector2)new PolarVec2(3, projectile.rotation - MathHelper.PiOver2);
				Rectangle tipHitbox = OriginExtensions.BoxOf(projectile.Center + boxSize, projectile.Center - boxSize, 2);
				for (int i = 0; i < projectile.localNPCImmunity.Length; i++) {
					if (projectile.localNPCImmunity[i] > 0) {
						NPC target = Main.npc[i];
						Rectangle targetHitbox = target.Hitbox;
						if (target.active && targetHitbox.Intersects(tipHitbox)) {
							EmbedTime++;
							EmbedTarget = i;
							projectile.aiStyle = 0;
							projectile.velocity = Vector2.Zero;
							embedPos = ((PolarVec2)(projectile.Center - target.Center)).RotatedBy(-target.rotation);
							embedRotation = projectile.rotation - target.rotation;
							break;
						}
					}
				}
			} else {//embedded/embedding in ground
				if (embedPos.R > 0) {
					Vector2 movement = (Vector2)embedPos;
					int size = 4;
					Vector2 startOffset = new Vector2(size / 2);
					Vector2 checkPosition = projectile.Center + movement - startOffset;
					if (!Collision.SolidCollision(checkPosition, size, size)) {
						projectile.timeLeft = embed_duration;
						projectile.position += movement;
					} else {
						embedPos = default;
					}
				}
			}
			//Dust.NewDustPerfect(lightPos, 226, Vector2.Zero, 100, new Color(0, 255, 191), 0.5f).noGravity = true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			return true;
		}
		public override bool? CanHitNPC(NPC target) {
			if (EmbedTime > 0) {
				return false;
			}
			return null;
		}
		public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI) {
			drawCacheProjsBehindNPCsAndTiles.Add(index);
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, null, new Color(Lighting.GetSubLight(projectile.Center)), projectile.rotation, new Vector2(27, 7), projectile.scale, SpriteEffects.None, 0);
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(projectile.localAI[0]);
			writer.Write(projectile.localAI[1]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			projectile.localAI[0] = reader.ReadSingle();
			projectile.localAI[1] = reader.ReadSingle();
		}
	}
}
