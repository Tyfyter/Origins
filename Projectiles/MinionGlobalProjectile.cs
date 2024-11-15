using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Items;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Tyfyter.Utils;

namespace Origins.Projectiles {
	//separate global for organization, might also make non-artifact projectiles less laggy than the alternative
	public class MinionGlobalProjectile : GlobalProjectile {
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => false;
		int timer = 0;
		float bonusUpdateCounter = 0;
		public float bonusUpdates = 0;
		public override void Load() {
			IL_Projectile.Update += IL_Projectile_Update;
		}

		private static void IL_Projectile_Update(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.Before,
				il => il.MatchStfld<Projectile>(nameof(Projectile.numUpdates))
			);
			c.EmitLdarg0();
			c.EmitDelegate((int updates, Projectile proj) => {
				if (proj.TryGetGlobalProjectile(out MinionGlobalProjectile global)) {
					if (global.bonusUpdates != 0) {
						global.bonusUpdateCounter += global.bonusUpdates * (updates + 1);
						if (global.bonusUpdates > 0) {
							while (global.bonusUpdateCounter > 1) {
								global.bonusUpdateCounter--;
								updates++;
							}
						} else {
							while (global.bonusUpdateCounter < -1) {
								global.bonusUpdateCounter++;
								updates--;
							}
						}
					}
				}
				return updates;
			});
		}
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			return entity.minion || entity.sentry || ProjectileID.Sets.MinionShot[entity.type] || ProjectileID.Sets.SentryShot[entity.type];
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			ModPrefix prefix = null;
			if (source is EntitySource_ItemUse itemUseSource) {
				prefix = PrefixLoader.GetPrefix(itemUseSource.Item.prefix);
			} else if (source is EntitySource_Parent source_Parent) {
				if (source_Parent.Entity is Projectile parentProjectile) {
					prefix = parentProjectile.TryGetGlobalProjectile(out OriginGlobalProj parent) ? parent.prefix : null;
				}
			}
			if (prefix is MinionPrefix minionPrefix) minionPrefix.OnSpawn(projectile, source);
		}
		public override void PostAI(Projectile projectile) {
			if (projectile.TryGetGlobalProjectile(out OriginGlobalProj self) && self.prefix is MinionPrefix artifactPrefix) {
				artifactPrefix.UpdateProjectile(projectile, timer);
				if (projectile.numUpdates == -1) timer++;
			}
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			binaryWriter.Write((float)bonusUpdates);
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			bonusUpdates = binaryReader.ReadSingle();
		}
	}
}
