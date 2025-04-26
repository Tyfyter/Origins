using Origins.Buffs;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Misc.Physics;

namespace Origins.NPCs.Brine.Boss {
	public class Mildew_Carrion_Spore : ModProjectile {
		public static List<int> types = [];
		public override void SetStaticDefaults() {
			types.Add(Type);
		}
		public override void SetDefaults() {
			Projectile.hostile = true;
			Projectile.ignoreWater = true;
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.penetrate = 1;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			Projectile.penetrate--;
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				int npcType = ModContent.NPCType<Mildew_Carrion_Mildew_Creeper>();
				if (Main.rand.NextFloat(3 + NPC.CountNPCS(npcType)) < ContentExtensions.DifficultyDamageMultiplier && Projectile.penetrate != 0) {
					Vector2[] directions = [
						Vector2.UnitX,
					-Vector2.UnitX,
					Vector2.UnitY,
					-Vector2.UnitY
					];
					const float offsetLen = 0;
					Vector2 basePos = Projectile.Center;
					float dist = 16;
					int directionIndex = 2;
					Vector2 bestPosition = basePos + directions[directionIndex] * (dist - offsetLen);
					bool canSpawn = false;
					for (int i = 0; i < directions.Length; i++) {
						float newDist = CollisionExt.Raymarch(basePos, directions[i], dist);
						if (newDist < dist) {
							dist = newDist;
							bestPosition = basePos + directions[i] * (dist - offsetLen);
							directionIndex = i;
							canSpawn = true;
						}
					}
					if (canSpawn) {
						NPC.NewNPC(
							Projectile.GetSource_Death(),
							(int)bestPosition.X,
							(int)bestPosition.Y,
							npcType,
							ai3: directionIndex
						);
					}
				} else {
					for (int i = 0; i < 7; i++) {
						Dust dust = Dust.NewDustDirect(
							Projectile.position - Vector2.One * 2,
							Projectile.width + 4,
							Projectile.height + 4,
							DustID.Bone,
							Projectile.oldVelocity.X * 0.4f,
							Projectile.oldVelocity.Y * 0.4f,
							100,
							Scale: 1.2f
						);
						dust.noGravity = true;
						dust.velocity *= 1.2f;
						dust.velocity.Y -= 0.5f;
					}
					for (int i = Main.rand.Next(5, 8); i > 0; i--) {
						Origins.instance.SpawnGoreByName(
							Projectile.GetSource_Death(),
							Projectile.position,
							Projectile.velocity,
							$"Gores/NPC/{nameof(Mildew_Creeper)}_Gore_2"
						);
					}
				}
			}
		}
	}
	public class Mildew_Carrion_Spore2 : Mildew_Carrion_Spore {
		public override string Texture => "Origins/NPCs/Brine/Boss/Mildew_Carrion_Cell1";
	}
	public class Mildew_Carrion_Spore3 : Mildew_Carrion_Spore {
		public override string Texture => "Origins/NPCs/Brine/Boss/Mildew_Carrion_Cell2";
	}
	public class Mildew_Carrion_Mildew_Creeper : Mildew_Creeper {
		public override string Texture => typeof(Mildew_Creeper).GetDefaultTMLName();
		public override int ChainLength => 10;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.CantTakeLunchMoney[Type] = true;
			NPCID.Sets.DontDoHardmodeScaling[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.lifeMax = 260;
			NPC.damage = 26;
			NPC.value = 0;
			// stat changes here
		}
		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
			NPC.lifeMax = (int)(NPC.lifeMax / MathF.Sqrt(ContentExtensions.DifficultyDamageMultiplier));
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) { }
		public override float SpawnChance(NPCSpawnInfo spawnInfo) => 0;
	}
}
