using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Tyfyter.Utils;

namespace Origins.Projectiles {
	//separate global for organization, might also make non-artifact projectiles less laggy than the alternative
	public class Artifact_Minions_Global : GlobalProjectile {
		bool isRespawned = false;
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => false;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			if (entity.ModProjectile is IArtifactMinion) Origins.ArtifactMinion[entity.type] = true;
			return Origins.ArtifactMinion[entity.type];
		}
		public override void SetDefaults(Projectile projectile) {
			isRespawned = false;
			if (projectile.ModProjectile is IArtifactMinion artifact) {
				artifact.Life = artifact.MaxLife;
			}
		}
		public bool CanRespawn(Projectile projectile) {
			return !isRespawned && Main.player[projectile.owner].GetModPlayer<OriginPlayer>().spiritShard;
		}
		public override void OnKill(Projectile projectile, int timeLeft) {
			if (projectile.ModProjectile is ICustomRespawnArtifact customRespawnArtifact) {
				customRespawnArtifact.Respawn();
			} else {
				if (CanRespawn(projectile)) {
					//basically just stops the old one from counting for one frame
					//done this way since maxMinions is the only thing that's always read for minion slot code
					Main.player[projectile.owner].maxMinions += (int)projectile.minionSlots + 1;
					Projectile proj = Projectile.NewProjectileDirect(
						projectile.GetSource_Death(),
						projectile.Center,
						projectile.velocity,
						projectile.type,
						projectile.originalDamage,
						projectile.knockBack,
						projectile.owner
					);
					proj.originalDamage = projectile.originalDamage;
					proj.GetGlobalProjectile<Artifact_Minions_Global>().isRespawned = true;
				}
			}
		}
		public override Color? GetAlpha(Projectile projectile, Color lightColor) {
			if (isRespawned) return new Color(175, 225, 255, 128);
			return null;
		}
		public override void PostDraw(Projectile projectile, Color lightColor) {
			if (projectile.owner == Main.myPlayer && projectile.ModProjectile is IArtifactMinion) {
				ArtifactMinionSystem.artifactMinions.Add(projectile.whoAmI);
			}
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			if (projectile.ModProjectile is IArtifactMinion artifact) {
				binaryWriter.Write(artifact.Life);
			}
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			if (projectile.ModProjectile is IArtifactMinion artifact) {
				artifact.Life = binaryReader.ReadInt32();
			}
		}
	}
	public class ArtifactMinionSystem : ModSystem {
		public static List<int> artifactMinions = [];
		public override void Unload() => artifactMinions = null;
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Entity Health Bars"));
			if (inventoryIndex != -1) {//error prevention & null check
				layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
					"Origins: Artifact Minion Health Bars",
					delegate {
						for (int i = 0; i < artifactMinions.Count; i++) {
							Projectile projectile = Main.projectile[artifactMinions[i]];
							if (projectile.ModProjectile is IArtifactMinion artifact) {
								Vector2 position = default;
								if (Main.HealthBarDrawSettings != 0) {
									if (Main.HealthBarDrawSettings == 1) {
										position = projectile.Bottom + new Vector2(projectile.gfxOffY + 10);
									} else {
										position = projectile.Top + new Vector2(projectile.gfxOffY - 24);
									}
									Main.instance.DrawHealthBar(
										position.X, position.Y,
										artifact.Life,
										artifact.MaxLife,
										Lighting.Brightness((int)position.X / 16, (int)(projectile.Center.Y + projectile.gfxOffY) / 16) * 0.5f + 0.5f,
										0.85f
									);
								}
							}
						}
						artifactMinions.Clear();
						return true;
					},
					InterfaceScaleType.Game) { Active = artifactMinions.Count > 0 }
				);
			}
		}
	}
	public interface IArtifactMinion {
		int MaxLife { get; set; }
		int Life { get; set; }
		void OnHurt(int damage) { }
	}
	public static class ArtifactMinionExtensions {
		public static void DamageArtifactMinion(this IArtifactMinion minion, int damage) {
			minion.Life -= damage;
			minion.OnHurt(damage);
			if (minion.Life <= 0 && minion is ModProjectile proj) proj.Projectile.Kill();
		}
		public static void DamageArtifactMinion(this Projectile minion, int damage) {
			if (minion.ModProjectile is IArtifactMinion artifact) artifact.DamageArtifactMinion(damage);
		}
	}
}
