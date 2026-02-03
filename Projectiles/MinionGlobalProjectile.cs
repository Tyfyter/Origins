using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Items;
using Origins.Items.Other.Consumables.Broths;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.World;
using PegasusLib;
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
		bool fromArtifact = false;
		BrothBase activeBroth;
		public float brothEffectAngle = 0;
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
					if (proj.TryGetOwner(out Player player)) {
						if (proj.TryGetGlobalProjectile(out ArtifactMinionGlobalProjectile artifact)) {
							artifact.maxHealthModifier = StatModifier.Default;
							if (proj.ModProjectile is IArtifactMinion) ArtifactMinionSystem.nextArtifactMinions.Add(proj.whoAmI);
						}
						BrothBase currentBroth = player.OriginPlayer()?.broth;
						if (global.activeBroth != currentBroth) {
							global.activeBroth?.SwitchActive(proj, -1);
							global.activeBroth = currentBroth;
							global.activeBroth?.SwitchActive(proj, 1);
						}
						currentBroth?.PreUpdateMinion(proj);
					}
					if (global.relayRodStrength != 0) global.tempBonusUpdates += global.relayRodStrength * 0.01f * 0.1f;
					if (OriginsSets.Projectiles.SupportsRealSpeedBuffs[proj.type] is Action<Projectile, float> setter) {
						setter(proj, global.TotalBonusUpdates);
					} else if (global.TotalBonusUpdates != 0) {
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
					}
					global.tempBonusUpdates = 0;
				}
				return updates;
			});
			c.GotoNext(MoveType.After,
				il => il.MatchCallOrCallvirt<Player>(nameof(Player.GetTotalDamage))
			);
			c.EmitLdarg0();
			c.EmitDelegate((StatModifier damage, Projectile proj) => {
				if (Origins.ArtifactMinion[proj.type]) damage = damage.CombineWith(Main.player[proj.owner].OriginPlayer().artifactDamage);
				return damage;
			});
			c.GotoNext(MoveType.Before,
				il => il.MatchLdarg0(),
				il => il.MatchCall<Projectile>(nameof(Projectile.AI))
			);
			c.EmitLdarg0();
			c.EmitDelegate((Projectile projectile) => {
				if (projectile.numUpdates == -1 && OriginsSets.Projectiles.UsesTypeSpecificMinionPos[projectile.type]) {
					Player owner = Main.player[projectile.owner];
					projectile.minionPos = owner.OriginPlayer().GetNewMinionIndexByType(projectile.type);
				}
			});

			c = new(il);
			c.GotoNext(MoveType.After,
				il => il.MatchLdfld<Player>(nameof(Player.maxMinions))
			);
			c.GotoNext(MoveType.AfterLabel,
				il => il.MatchLdarg0(),
				il => il.MatchCall<Projectile>(nameof(Projectile.Kill))
			);
			ILLabel dontKillMe = c.DefineLabel();

			c.EmitLdarg0();
			c.EmitDelegate<Func<Projectile, bool>>(proj => {
				if (proj.ModProjectile is ISpecialOverCapacityMinion special) {
					special.KillOverCapacity();
					return true;
				}
				return false;
			});
			c.EmitBrtrue(dontKillMe);

			c.GotoNext(MoveType.After,
				il => il.MatchLdarg0(),
				il => il.MatchCall<Projectile>(nameof(Projectile.Kill))
			);
			c.MarkLabel(dontKillMe);
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
					fromArtifact = IsArtifact(parentProjectile);
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
			if (projectile.TryGetGlobalProjectile(out OriginGlobalProj self) && self.prefix is MinionPrefix minionPrefixPrefix) {
				minionPrefixPrefix.UpdateProjectile(projectile, timer);
			}
			if (projectile.TryGetOwner(out Player player)) {
				player.OriginPlayer()?.broth?.UpdateMinion(projectile, timer);
			}
			if (projectile.numUpdates == -1) {
				timer++;
				if (relayRodTime > 0) relayRodTime--;
				if (relayRodTime <= 0) relayRodStrength = 0;
			}
			if (!init.TrySet(true) && projectile.minion && projectile.tileCollide) {
				float highest = 0;
				foreach (Point pos in Collision.GetTilesIn(projectile.position, projectile.BottomRight)) {
					Tile tile = Framing.GetTileSafely(pos);
					if (tile.HasTile) highest = Math.Max(OriginsSets.Tiles.MinionSlowdown[tile.TileType], highest);
				}
				Vector2.Lerp(ref projectile.position, ref projectile.oldPosition, highest, out projectile.position);
			}
		}
		bool init = false;
		static AutoLoadingAsset<Texture2D> mildewGrowthTexture = "Origins/Items/Armor/Mildew/Mildew_Spore";
		float? mildewGrowthAngle = null;
		float mildewGrowthFrame = 0;
		public override void PostDraw(Projectile projectile, Color lightColor) {
			if (projectile.TryGetOwner(out Player player)) {
				OriginPlayer originPlayer = player.OriginPlayer();
				originPlayer.broth?.PostDrawMinion(projectile, lightColor);
				if (originPlayer.mildewSet && (projectile.minion || projectile.sentry)) {
					mildewGrowthAngle ??= Main.rand.NextFloat(MathHelper.TwoPi);
					Rectangle frame = mildewGrowthTexture.Frame(verticalFrames: 3, frameY: (int)(mildewGrowthFrame % 3));
					float scale = projectile.scale;
					if (ContentSamples.ProjectilesByType.TryGetValue(projectile.type, out Projectile baseProj) && baseProj.scale != 0) scale /= baseProj.scale;
					Main.EntitySpriteDraw(
						mildewGrowthTexture,
						projectile.Center + (mildewGrowthAngle.Value + projectile.rotation).ToRotationVector2() * 4 * scale - Main.screenPosition,
						frame,
						lightColor,
						mildewGrowthAngle.Value + projectile.rotation,
						frame.Size() * new Vector2(0.5f, 1),
						scale,
						SpriteEffects.None
					);
					mildewGrowthFrame += 1f / 6;
				}
			}
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			binaryWriter.Write((float)bonusUpdates);
			bitWriter.WriteBit(fromArtifact);
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			bonusUpdates = binaryReader.ReadSingle();
			fromArtifact = bitReader.ReadBit();
		}
		public override void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox) {
			if (projectile.TryGetOwner(out Player player) && player.OriginPlayer()?.broth is Plain_Broth) {
				hitbox.Inflate(hitbox.Width / 8, hitbox.Height / 8);
			}
		}
		/// <summary>
		/// Checks if a projectile is an artifact minion or came from one
		/// </summary>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public static bool IsArtifact(Projectile projectile) {
			if (Origins.ArtifactMinion[projectile.type]) return true;
			if (!projectile.TryGetGlobalProjectile(out MinionGlobalProjectile minionGlobal)) return false;
			return minionGlobal.fromArtifact;
		}
		/// <summary>
		/// Gets the minion or sentry that spawned this projectile, or the projectile itself if it's a minion or sentry
		/// </summary>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public static Projectile GetOwnerMinion(Projectile projectile) {
			if (projectile.minion || projectile.sentry) {
				return projectile;
			} else if (projectile.TryGetGlobalProjectile(out OriginGlobalProj global) && global.ownerMinion is OwnerMinionKey minionKey) {
				foreach (Projectile proj in Main.ActiveProjectiles) {
					if (proj.type == minionKey.Type && proj.owner == minionKey.Owner && proj.identity == minionKey.Identity) {
						return proj;
					}
				}
			}
			return null;
		}
	}
	public abstract class MinionSpeedModifierProjectile : ModProjectile {
		public float SpeedModifier { get; protected set; } = 1;
		public override void SetStaticDefaults() {
			OriginsSets.Projectiles.SupportsRealSpeedBuffs[Type] = static (projectile, bonus) => {
				if (projectile.ModProjectile is MinionSpeedModifierProjectile tab) tab.SpeedModifier = bonus + 1;
			};
		}
	}
	public interface ISpecialOverCapacityMinion {
		public void KillOverCapacity();
	}
}
