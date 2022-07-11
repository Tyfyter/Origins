using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.NPCs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using Terraria.GameContent.Creative;

namespace Origins.Items.Weapons.Defiled {
	public class Infusion : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Infusion");
			Tooltip.SetDefault("");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
			Item.damage = 4;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Item.mana = 5;
            Item.noMelee = true;
            Item.noUseGraphic = false;
			Item.width = 30;
			Item.height = 36;
			Item.useTime = 9;
			Item.useAnimation = 9;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 5;
            Item.shoot = ModContent.ProjectileType<Infusion_P>();
            Item.shootSpeed = 16f;
			Item.value = 5000;
            Item.useTurn = false;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = Origins.Sounds.DefiledIdle.WithPitchRange(0.9f, 1f);
			Item.autoReuse = true;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(8, 0);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = velocity.RotatedByRandom(0.075f);
		}
	}
    public class Infusion_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Defiled/Infusion_P";
		PolarVec2 embedPos;
		float embedRotation;
		const int embed_duration = 600;
		int EmbedTime { get => (int)Projectile.localAI[0]; set => Projectile.localAI[0] = value; }
		int EmbedTarget { get => (int)Projectile.localAI[1]; set => Projectile.localAI[1] = value; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Infusion");
		}
		public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 15;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 1;
			Projectile.hide = true;
		}
		public override void AI() {
			//projectile.aiStyle = projectile.wet?0:1;
			if (EmbedTime > 0) {//embedded in enemy
				EmbedTime++;
				NPC target = Main.npc[EmbedTarget];
				Projectile.Center = target.Center + (Vector2)embedPos.RotatedBy(target.rotation);
				Projectile.rotation = embedRotation + target.rotation;
				if(Projectile.numUpdates == 0 && EmbedTime > 10) OriginGlobalNPC.AddInfusionSpike(target, Projectile.whoAmI);
				if (!target.active) {
					EmbedTime = embed_duration + 1;
				}
				if (EmbedTime > embed_duration) {
					Projectile.Kill();
				}
			} else if (Projectile.aiStyle == 1) {//not embedded
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi * 0.25f;
				Vector2 boxSize = (Vector2)new PolarVec2(3, Projectile.rotation - MathHelper.PiOver2);
				Rectangle tipHitbox = OriginExtensions.BoxOf(Projectile.Center + boxSize, Projectile.Center - boxSize, 2);
				for (int i = 0; i < Projectile.localNPCImmunity.Length; i++) {
					if (Projectile.localNPCImmunity[i] > 0) {
						NPC target = Main.npc[i];
						Rectangle targetHitbox = target.Hitbox;
						if (target.active && targetHitbox.Intersects(tipHitbox)) {
							EmbedTime++;
							EmbedTarget = i;
							Projectile.aiStyle = 0;
							Projectile.velocity = Vector2.Zero;
							embedPos = ((PolarVec2)(Projectile.Center - target.Center)).RotatedBy(-target.rotation);
							embedRotation = Projectile.rotation - target.rotation;
							break;
						}
					}
				}
			} else {//embedded/embedding in ground
				if (embedPos.R > 0) {
					Vector2 movement = (Vector2)embedPos;
					int size = 4;
					Vector2 startOffset = new Vector2(size / 2);
					Vector2 checkPosition = Projectile.Center + movement - startOffset;
					if (!Collision.SolidCollision(checkPosition, size, size)) {
						Projectile.timeLeft = embed_duration;
						Projectile.position += movement;
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
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, null, new Color(Lighting.GetSubLight(Projectile.Center)), Projectile.rotation, new Vector2(13, 3), Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(Projectile.localAI[0]);
			writer.Write(Projectile.localAI[1]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.localAI[0] = reader.ReadSingle();
			Projectile.localAI[1] = reader.ReadSingle();
		}
	}
}
