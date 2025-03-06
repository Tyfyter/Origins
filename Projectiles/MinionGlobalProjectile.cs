using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Items;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.World;
using System;
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
using ThoriumMod.Projectiles.Minions;
using Tyfyter.Utils;

namespace Origins.Projectiles {
	//separate global for organization, might also make non-artifact projectiles less laggy than the alternative
	public class MinionGlobalProjectile : GlobalProjectile {
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => false;
		int timer = 0;
		float bonusUpdateCounter = 0;
		public float bonusUpdates = 0;
		public float tempBonusUpdates = 0;
		public float TotalBonusUpdates => bonusUpdates + tempBonusUpdates;
		public int relayRodTime = 0;
		public int relayRodStrength = 0;
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
				OriginsGlobalBiome.isConvertingProjectilePlayerOwned = !proj.hostile && !proj.npcProj && proj.owner != Main.maxPlayers;
				if (proj.TryGetGlobalProjectile(out MinionGlobalProjectile global)) {
					if (proj.friendly && proj.TryGetOwner(out Player player)) {
						if (proj.TryGetGlobalProjectile(out ArtifactMinionGlobalProjectile artifact)) {
							artifact.maxHealthModifier = StatModifier.Default;
						}
						player.OriginPlayer()?.broth?.PreUpdateMinion(proj);
					}
					if (global.relayRodStrength != 0) global.tempBonusUpdates += global.relayRodStrength * 0.01f * 0.1f;
					if (global.TotalBonusUpdates != 0) {
						global.bonusUpdateCounter += global.TotalBonusUpdates * (updates + 1);
						if (global.TotalBonusUpdates > 0) {
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
						global.tempBonusUpdates = 0;
					}
				}
				return updates;
			});
		}
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			return entity.IsMinionOrSentryRelated;
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
		public override bool PreAI(Projectile projectile) {
			if (relayRodStrength != 0) {
				float strength = relayRodStrength * 0.01f;
				if (projectile.minion || projectile.sentry || projectile.ContinuouslyUpdateDamageStats) projectile.damage += (int)(projectile.damage * strength * 0.1f);
				const int rate = 40;
				if (projectile.ModProjectile is IArtifactMinion artifactMinion && artifactMinion.Life < artifactMinion.MaxLife && timer % rate == 0) {
					float oldHealth = artifactMinion.Life;
					// heal 10/sec + damage bonuses
					artifactMinion.Life += strength * 10f * (rate / 60f);
					if (artifactMinion.Life > artifactMinion.MaxLife) artifactMinion.Life = artifactMinion.MaxLife;
					int diff = (int)artifactMinion.Life - (int)oldHealth;
					if (diff != 0) CombatText.NewText(projectile.Hitbox, CombatText.HealLife, diff, false, dot: true);
				}
			}
			return true;
		}
		public override void PostAI(Projectile projectile) {
			if (projectile.TryGetGlobalProjectile(out OriginGlobalProj self) && self.prefix is MinionPrefix artifactPrefix) {
				artifactPrefix.UpdateProjectile(projectile, timer);
			}
			if (projectile.numUpdates == -1) {
				timer++;
				if (relayRodTime > 0) relayRodTime--;
				if (relayRodTime <= 0) relayRodStrength = 0;
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
