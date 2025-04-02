using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Broths {
	public class Greasy_Broth : BrothBase {
		public static int cd = 30;
		public static int cdMax = 90;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				new Color(124, 67, 128),
				new Color(89, 52, 96),
				new Color(38, 73, 66)
			];
		}
		public override void ModifyMinionHit(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			target.AddBuff(BuffID.Oiled, 360);
			if (MinionGlobalProjectile.IsArtifact(proj)) {
				//target.AddBuff(BuffID.OnFire, 360);
				if (cd >= cdMax) {
					Projectile.NewProjectile(Terraria.Entity.InheritSource(proj), proj.Center, new Vector2(), Greasy_Gas.ID, 0, 0);
					cd = 0;
				}
			}
		}
		public override void UpdateBuff(Player player, ref int buffIndex) {
			cdMax = 90;
			cd++;
		}
	}
	public class Greasy_Gas : ModProjectile {
		public static int ID { get; private set; }
		public override string Texture => "Origins/Projectiles/Misc/Smonk";
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.width = 74;
			Projectile.height = 68;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.ignoreWater = false;
			Projectile.netImportant = true;
			Projectile.timeLeft = 60;
		}
		public override bool? CanCutTiles() {
			return false;
		}
		public override void AI() {
			#region General behavior
			foreach (NPC npc in Main.ActiveNPCs) {
				if (Projectile.getRect().Intersects(npc.getRect()) && npc.chaseable && !npc.immortal && !npc.friendly) {
					npc.AddBuff(BuffID.OnFire, 360);
				}
			}
			if (Projectile.ai[0] > 0) Projectile.timeLeft = 60;
			#endregion

			#region Animation and visuals
			// This is a simple "loop through all frames from top to bottom" animation
			int frameSpeed = 5;
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= frameSpeed) {
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= Main.projFrames[Projectile.type]) {
					Projectile.frame = 0;
				}
			}

			// Some visuals here
			#endregion
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
			Main.EntitySpriteDraw(
				texture,
				Projectile.Bottom - new Vector2(0, 4) - Main.screenPosition,
				frame,
				Projectile.GetAlpha(lightColor.MultiplyRGB(new Color(89, 52, 96))),
				Projectile.rotation,
				new Vector2(frame.Width / 2, frame.Height - 4),
				Projectile.scale,
				SpriteEffects.None,
				0);
			return false;
		}
	}
}
