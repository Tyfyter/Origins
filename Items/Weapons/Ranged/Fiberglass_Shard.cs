using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ranged {
	public class Fiberglass_Shard : ModItem, IElementalItem {
		public ushort Element => Elements.Fiberglass;
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ThrowingKnife);
			Item.width = 12;
			Item.height = 14;
			Item.useTime -= 6;
			Item.useAnimation -= 6;
			Item.shootSpeed += 4;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<Fiberglass_Shard_P>();
			Item.value = Item.sellPrice(copper: 20);
		}
	}
	public class Fiberglass_Shard_P : ModProjectile {
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThrowingKnife);
			Projectile.aiStyle = 1;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.trap = false;
			Projectile.width = 5;
			Projectile.height = 5;
			Projectile.extraUpdates = 1;
			Projectile.penetrate = 7;
			Projectile.hide = true;
			Projectile.light = 0.025f;
			CooldownSlot = ImmunityCooldownID.WrongBugNet;
		}
		public override void AI() {
			if (Main.rand.NextBool(200)) {
				Main.dust[Dust.NewDust(Projectile.Center + new Vector2(6, 6).RotatedBy(Projectile.rotation), 0, 0, DustID.TreasureSparkle, 0f, 0f, 0, new Color(255, 255, 255), 1f)].velocity *= 0.2f;
			}
			if (Projectile.damage == 1) {
				Vector2 center = Projectile.Center;
				double rot = Math.Round(Projectile.rotation / MathHelper.PiOver4);
				Vector2 offset = new Vector2(0, 8).RotatedBy(rot * MathHelper.PiOver4);
				if (Collision.CanHitLine(center - offset, 0, 0, center + offset, 0, 0)) {
					Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center, ModContent.ItemType<Fiberglass_Shard>());
					Projectile.Kill();
				}
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Main.rand.NextBool(Projectile.penetrate)) return true;
			Projectile.aiStyle = 0;
			Projectile.hostile = true;
			Projectile.friendly = true;
			Projectile.damage = 1;
			Projectile.position += new Vector2(2.5f, 2.5f);
			Projectile.width = 15;
			Projectile.height = 15;
			Projectile.position -= new Vector2(7.5f, 7.5f);
			Projectile.knockBack = 0;
			oldVelocity = oldVelocity.SafeNormalize(oldVelocity);
			Projectile.position += oldVelocity * 12 * (Projectile.velocity.Length() / 21f);
			bool exposed = true;
			int tries = 0;
			while (exposed) {
				Projectile.position += oldVelocity;
				Vector2 center = Projectile.Center;
				double rot = Math.Round(Projectile.rotation / MathHelper.PiOver4);
				Vector2 offset = new Vector2(0, 8).RotatedBy(rot * MathHelper.PiOver4);
				exposed = ++tries < 50 && Collision.CanHitLine(center - offset, 1, 1, center + offset, 1, 1);
			}
			Projectile.velocity *= 0;
			return false;
		}
		public override bool? CanHitNPC(NPC target) {
			return Projectile.damage == 1 ? false : base.CanHitNPC(target);
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			if (Projectile.damage == 1) {
				modifiers.Knockback *= 0f;
				Projectile.Kill();
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			if (Projectile.damage == 1) {
				int item = Item.NewItem(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					ModContent.ItemType<Fiberglass_Shard>()
				);
				if (Main.netMode != NetmodeID.SinglePlayer) {
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
				}
			}
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			if (Projectile.hide) behindNPCsAndTiles.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 10, 14), lightColor, Projectile.rotation, new Vector2(5, 14), 1f, SpriteEffects.None, 0);
			return false;
		}
	}
}
