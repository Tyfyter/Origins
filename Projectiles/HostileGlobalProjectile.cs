using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Origins.Projectiles {
	public class HostileGlobalProjectile : GlobalProjectile {
		NPC ownerNPC;
		Vector2? ownerPosition;
		public override bool InstancePerEntity => true;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.hostile;
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
		public enum OwnerState {
			Alive,
			Dead,
			NeverOwned
		}
	}
}
