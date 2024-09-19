using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.NPCs;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Summoner {
	public class Ziptie : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Whip"
		];
		public override void SetDefaults() {
			Item.DefaultToWhip(ModContent.ProjectileType<Ziptie_P>(), 47, 5, 4, 35);
			Item.DamageType = DamageClass.SummonMeleeSpeed;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override bool MeleePrefix() => true;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.ownedProjectileCounts[type] > 0) return false;
			Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI).scale *= player.GetAdjustedItemScale(Item);
			return false;
		}
	}
	public class Ziptie_P : ModProjectile, IWhipProjectile {
		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsAWhip[Type] = true;
		}
		List<int> hitEnemies = [];
		public override void SetDefaults() {
			Projectile.DefaultToWhip();
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = true; // This prevents the projectile from hitting through solid tiles.
			Projectile.extraUpdates = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		private float Timer {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Without PiOver2, the rotation would be off by 90 degrees counterclockwise.

			Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;

			Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

			float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates - 1;

			if (Timer >= swingTime || owner.itemAnimation <= 0) {
				Projectile.Kill();
				return;
			}

			owner.heldProj = Projectile.whoAmI;

			// These two lines ensure that the timing of the owner's use animation is correct.
			owner.itemAnimation = owner.itemAnimationMax - (int)(Timer / Projectile.MaxUpdates);
			owner.itemTime = owner.itemAnimation;

			if (Timer == swingTime / 2) {
				// Plays a whipcrack sound at the tip of the whip.
				List<Vector2> points = Projectile.WhipPointsForCollision;
				Projectile.FillWhipControlPoints(Projectile, points);
				SoundEngine.PlaySound(SoundID.Item153, points[^1]);
			}
			for (int i = 0; i < hitEnemies.Count; i++) {
				if (hitEnemies[i] >= 0 && !Main.npc[hitEnemies[i]].CanBeChasedBy(Projectile, true)) hitEnemies[i] = -1;
			}
		}

		public void GetWhipSettings(out float timeToFlyOut, out int segments, out float rangeMultiplier) {
			timeToFlyOut = Main.player[Projectile.owner].itemAnimationMax * Projectile.MaxUpdates;
			segments = 20;
			rangeMultiplier = 0.9f * Projectile.scale;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.CanBeChasedBy(Projectile, true)) hitEnemies.Add(target.whoAmI);
			target.AddBuff(Ziptie_Buff.ID, 240);
		}
		public override void OnKill(int timeLeft) {
			if (hitEnemies.Count >= 3) {
				for (int i = 0; i < hitEnemies.Count; i++) {
					if (hitEnemies[i] >= 0) {
						NPC target = Main.npc[hitEnemies[i]];
						if (target.CanBeChasedBy(Projectile, true)) {
							target.AddBuff(Rasterized_Debuff.ID, 60);
							target.AddBuff(Ziptie_Buff.ID, 240);
						}
					}
				}
				Main.player[Projectile.owner].AddBuff(Ziptie_Buff.ID, 300);
			}
		}
		private static void DrawLine(List<Vector2> list) {
			Texture2D texture = TextureAssets.FishingLine.Value;
			Rectangle frame = texture.Frame();
			Vector2 origin = new(frame.Width / 2, 2);

			Vector2 pos = list[0];
			Color color = new(44, 39, 58);
			for (int i = 0; i < list.Count - 1; i++) {
				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;

				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Vector2 scale = new(1, (diff.Length() + 2) / frame.Height);

				Main.EntitySpriteDraw(
					texture,
					pos - Main.screenPosition,
					frame,
					Lighting.GetColor(element.ToTileCoordinates(), color),
					rotation,
					origin,
					scale,
					SpriteEffects.None,
				0);

				pos += diff;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			List<Vector2> list = [];
			Projectile.FillWhipControlPoints(Projectile, list);

			DrawLine(list);

			SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Main.instance.LoadProjectile(Type);
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Vector2 pos = list[0];

			for (int i = 0; i < list.Count - 1; i++) {
				// These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
				// You can change them if they don't!
				Rectangle frame = new(0, 0, 48, 28);
				Vector2 origin = new(24, 14);
				Vector2 scale = new Vector2(0.85f) * Projectile.scale;

				if (i == list.Count - 2) {
					frame.Y = 112;
				} else if (i > 10) {
					frame.Y = 84;
				} else if (i > 5) {
					frame.Y = 56;
				} else if (i > 0) {
					frame.Y = 28;
				}

				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;

				float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
				Color color = Lighting.GetColor(element.ToTileCoordinates());

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

				pos += diff;
			}
			return false;
		}
	}
	public class Ziptie_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_311";
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().ziptieDebuff = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.35f;
		}
	}
}
