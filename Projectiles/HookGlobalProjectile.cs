using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Tyfyter.Utils;

namespace Origins.Projectiles {
	//separate global for organization
	public class HookGlobalProjectile : GlobalProjectile {
		bool isRetracting = false;
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => false;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			return entity.aiStyle == ProjAIStyleID.Hook;
		}
		public override void SetDefaults(Projectile projectile) {
			isRetracting = false;
		}
		public override void AI(Projectile projectile) {
			Player player = Main.player[projectile.owner];
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (projectile.ai[0] == 1) {
				if (!isRetracting) {
					if (originPlayer.automatedReturnsHandler) {
						projectile.extraUpdates += 2;
					}
					isRetracting = true;
				}
			}
		}
		public override void GrapplePullSpeed(Projectile projectile, Player player, ref float speed) {
			if (player.OriginPlayer().automatedReturnsHandler) speed *= 2f;
		}
		public override bool? GrappleCanLatchOnTo(Projectile projectile, Player player, int x, int y) {
			Tile tile = Main.tile[x, y];
			if (tile.HasTile && OriginsSets.Tiles.MultitileCollisionOffset.GetIfInRange(tile.TileType) is OriginsSets.MultitileCollisionOffsetter collision) {
				float posY = y * 16;
				int height = 16;
				collision(tile, ref posY, ref height);
				if (height < 0) return false;
			}
			return base.GrappleCanLatchOnTo(projectile, player, x, y);
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			//bitWriter.WriteBit(isRetracting);
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			//isRetracting = bitReader.ReadBit();
		}
	}
}
