using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Tyfyter.Utils;

namespace Origins.Projectiles {
	//separate global for organization
	public class YoyoGlobalProjectile : GlobalProjectile {
		bool isReturning = false;
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => false;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			return entity.aiStyle == ProjAIStyleID.Yoyo;
		}
		public override void SetDefaults(Projectile projectile) {
			isReturning = false;
		}
		public override void AI(Projectile projectile) {
			Player player = Main.player[projectile.owner];
			if (projectile.ai[0] == -1) {
				if (!isReturning && Vector2.Dot(projectile.velocity.SafeNormalize(default), projectile.DirectionTo(player.MountedCenter)) > 0) {
					OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
					if (originPlayer.turboReel2) {
						projectile.extraUpdates += 2;
					} else if (originPlayer.turboReel) {
						projectile.extraUpdates++;
					}
					isReturning = true;
				}
			}
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			bitWriter.WriteBit(isReturning);
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			isReturning = bitReader.ReadBit();
		}
	}
}
