using MagicStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Projectiles {
	public class HostileGlobalProjectile : GlobalProjectile {
		NPC ownerNPC;
		Vector2? ownerPosition;
		public override bool InstancePerEntity => true;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => OriginsSets.Projectiles.IsEnemyOwned[entity.type] |= entity.hostile && !entity.trap;
		public OwnerState TryGetOwnerPosition(out Vector2? ownerPosition) {
			if (ownerNPC is not null) {
				this.ownerPosition = ownerNPC.Center;
				if (!ownerNPC.active) ownerNPC = null;
			}
			ownerPosition = this.ownerPosition;
			if (ownerNPC?.active == true) return OwnerState.Alive;
			if (ownerPosition is null) return OwnerState.NeverOwned;
			return OwnerState.Dead;
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			if (source is EntitySource_Parent parentSource) {
				if (parentSource.Entity is NPC npc) {
					ownerNPC = npc;
				} else if (parentSource.Entity is Projectile parentProj && parentProj.TryGetGlobalProjectile(out HostileGlobalProjectile parentGlobal)) {
					ownerNPC = parentGlobal.ownerNPC;
					ownerPosition = parentGlobal.ownerPosition;
				}
			}
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			OwnerState state = TryGetOwnerPosition(out _);
			binaryWriter.Write((byte)state);
			switch (state) {
				case OwnerState.Alive:
				binaryWriter.Write((byte)ownerNPC.whoAmI);
				break;

				case OwnerState.Dead:
				binaryWriter.WritePackedVector2(ownerPosition.Value);
				break;
			}
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			switch ((OwnerState)binaryReader.ReadByte()) {
				case OwnerState.Alive:
				ownerNPC = Main.npc[binaryReader.ReadByte()];
				break;

				case OwnerState.Dead:
				ownerPosition = binaryReader.ReadPackedVector2();
				break;
			}
		}
		public enum OwnerState : byte {
			Alive,
			Dead,
			NeverOwned
		}
	}
}
