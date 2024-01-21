using Microsoft.Xna.Framework;
using Origins.World;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Impeding_Shrapnel_Debuff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			ID = Type;
		}
		public static void SpawnShrapnel(NPC npc, int buffTime) {
			int count = Main.rand.Next(5, 8);
			float rot = MathHelper.TwoPi / count;
			Vector2 velocity = new Vector2(0.5f, 0).RotatedByRandom(MathHelper.Pi);
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
		public static int ID { get; private set; } = -1;
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
			Projectile.timeLeft = 240;
			Projectile.ignoreWater = true;
		}
		public override void AI() {
			if (Projectile.timeLeft == 240) {
				Projectile.rotation = Projectile.velocity.ToRotation();
				Projectile.ai[0] = Main.rand.Next(256) * 30;
				Projectile.ai[1] = Main.rand.NextFloat(0.5f, 1.5f);
			}
			Vector2 diff = new Vector2(0, (GenRunners.GetWallDistOffset(Projectile.ai[0]) + 0.31f)).RotatedBy(Projectile.rotation);
			Projectile.velocity += diff;
			Projectile.ai[0] += Projectile.ai[1];
			Dust.NewDustPerfect(Projectile.Center, 6, Vector2.Zero).noGravity = true;
			//Dust.NewDustPerfect(Projectile.Center, 29, diff * 32);
		}
	}
}
