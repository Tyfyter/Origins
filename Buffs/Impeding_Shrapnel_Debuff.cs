using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Ranged;
using Origins.NPCs;
using Origins.World;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Impeding_Shrapnel_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			ID = Type;
		}
		public static void SpawnShrapnel(NPC npc, int buffTime) {
			int count = Main.rand.Next(5, 8);
			float rot = MathHelper.TwoPi / count;
            SoundEngine.PlaySound(Origins.Sounds.ShrapnelFest, npc.Center);
            Vector2 velocity = new Vector2(4f, 0).RotatedByRandom(MathHelper.Pi);
			int damage = (int)Math.Pow(Math.Log(buffTime, 1.5f), 1.5f);
			for (int i = count; i-->0;) {
				Projectile.NewProjectile(
					npc.GetSource_Death(),
					npc.Center,
					velocity.RotatedBy(rot * i + Main.rand.NextFloat(-0.2f, 0.2f)),
					Impeding_Shrapnel_Shard.ID,
					damage,
					3,
					Main.myPlayer
				);
			}
		}
	}
	public class Impeding_Shrapnel_Shard : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BoneGloveProj;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 3;
			Projectile.extraUpdates = 3;
			Projectile.width = Projectile.height = 8;
			Projectile.timeLeft = 420;
			Projectile.ignoreWater = true;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.localAI[1] = ModContent.ProjectileType<Shardcannon_P1>() + Main.rand.Next(3);
			Projectile.localAI[2] = Main.rand.NextFloat(0.35f, 0.5f) * Main.rand.NextBool().ToDirectionInt();
		}
		public override void AI() {
			if (Projectile.timeLeft == 420) {
				Projectile.rotation = Projectile.velocity.ToRotation();
				Projectile.ai[0] = Main.rand.Next(256) * 30;
				Projectile.ai[1] = Main.rand.NextFloat(0.75f, 2.5f);
			}
			Projectile.localAI[0] += (GenRunners.GetWallDistOffset(Projectile.ai[0]) + 0.295f) * Projectile.localAI[2];
			Vector2 perp = new(Projectile.velocity.Y, -Projectile.velocity.X);
			Vector2 offset = Projectile.localAI[0] * perp;
			Projectile.localAI[0] = MathHelper.Clamp(Projectile.localAI[0], -2, 2);
			if (Projectile.ai[2] != 0) {
				float factor =
					(GenRunners.GetWallDistOffset(Projectile.ai[0]) + 0.75f)
					- (GenRunners.GetWallDistOffset(Projectile.ai[0] - Projectile.ai[1]) + 0.17f);
				offset -= perp * factor * Projectile.ai[2];
			}
			if (Collision.TileCollision(
				Projectile.position,
				offset,
				Projectile.width,
				Projectile.height,
				true,
				true
			) == offset) {
				Projectile.position += offset;
			} else {
				Projectile.velocity = offset;
				Projectile.Kill();
			}
			Projectile.ai[0] += Projectile.ai[1];
			Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Vector2.Zero).noGravity = true;
			Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.localAI[0] * 0.02f);
			//Dust.NewDustPerfect(Projectile.Center, 29, diff * 32);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictImpedingShrapnel(target, 300);
		}
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[(int)Projectile.localAI[1]].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation,
				texture.Size() * 0.5f,
				1,
				SpriteEffects.None
			);
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(Projectile.localAI[1]);
			writer.Write(Projectile.localAI[2]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.localAI[1] = reader.ReadSingle();
			Projectile.localAI[2] = reader.ReadSingle();
		}
	}
}
