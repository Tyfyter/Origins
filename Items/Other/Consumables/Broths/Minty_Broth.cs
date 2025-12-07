using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using PegasusLib;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Broths {
	public class Minty_Broth : BrothBase {
		public static float Size => 64;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				new(32, 228, 149),
				new(45, 172, 121),
				new(34, 113, 81)
			];
			ItemID.Sets.FoodParticleColors[Type] = [
				new(8, 44, 30)
			];
		}
		public override int Duration => 4;
		public override void PostDrawMinion(Projectile minion, Color lightColor) {
			Texture2D texture = TextureAssets.Extra[174].Value;
			int oldShader = Main.CurrentDrawnEntityShader;
			try {
				Main.CurrentDrawnEntityShader = 0;
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				Main.spriteBatch.Draw(texture, minion.Center - Main.screenPosition, null, new Color(20, 100, 120, 60), 0, texture.Size() * 0.5f, new Vector2(Size / 48), SpriteEffects.None, 0);
			} finally {
				Main.CurrentDrawnEntityShader = oldShader;
			}
		}
		public override void OnHurt(Projectile minion, int damage, bool fromDoT) {
			if (Main.myPlayer != minion.owner || fromDoT) return;
			Vector2 center = minion.Center;
			NPC.HitInfo hit = new() {
				Damage = damage,
				DamageType = DamageClass.Summon,
				Knockback = 4
			};
			foreach (NPC npc in Main.ActiveNPCs) {
				if (!NPCID.Sets.CountsAsCritter[npc.type] && !npc.friendly && center.WithinRange(center.Clamp(npc.Hitbox), Size)) {
					hit.HitDirection = Math.Sign(npc.Center.X - center.X);
					Main.LocalPlayer.addDPS(npc.StrikeNPC(hit));
					if (Main.netMode != NetmodeID.SinglePlayer) NetMessage.SendStrikeNPC(npc, hit);
				}
			}
			Projectile.NewProjectile(minion.GetSource_FromThis(), center, Vector2.Zero, ModContent.ProjectileType<Minty_Retaliation_Visual>(), 0, 0);
		}
		public override void UpdateMinion(Projectile minion, int time) {
			Vector2 center = minion.Center;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (!NPCID.Sets.CountsAsCritter[npc.type] && !npc.friendly && center.WithinRange(center.Clamp(npc.Hitbox), Size)) {
					npc.AddBuff(BuffID.Frostburn2, 30);
				}
			}
		}
	}
	public class Minty_Retaliation_Visual : ModProjectile {
		public override string Texture => "Terraria/Images/Extra_174";
		public override void SetDefaults() {
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			if (Projectile.ai[1] == 0) {
				Projectile.ai[1] = 1;
				return;
			}
			Projectile.ai[0] += 1f / 8;
			if (Projectile.ai[0] > 1) {
				Projectile.ai[0] = 1;
				Projectile.ai[2] += 1f / 8;
				if (Projectile.ai[2] >= 1) Projectile.Kill();
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Extra[174].Value;
			Main.spriteBatch.Draw(
				texture,
				Projectile.position - Main.screenPosition,
				null,
				new Color(20, 150, 180, 90) * (1 - Projectile.ai[2]),
				0,
				texture.Size() * 0.5f,
				new Vector2(Minty_Broth.Size / 48) * Projectile.ai[0],//(MathF.Pow(Projectile.ai[0], 0.9f)),
				SpriteEffects.None,
			0);
			return false;
		}
	}
}
