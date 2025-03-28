using Origins.Buffs;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
			if (Main.masterMode && Main.netMode != NetmodeID.MultiplayerClient && Projectile.penetrate != 0) {
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
						ModContent.NPCType<Mildew_Carrion_Mildew_Creeper>(),
						ai3: directionIndex
					);
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
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.CantTakeLunchMoney[Type] = true;
			NPCID.Sets.DontDoHardmodeScaling[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
		}
		public override void SetDefaults() {
			base.SetDefaults();
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) => 0;
	}
}
