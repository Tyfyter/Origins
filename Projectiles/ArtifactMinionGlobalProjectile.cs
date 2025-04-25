using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Tyfyter.Utils;

namespace Origins.Projectiles {
	//separate global for organization, might also make non-artifact projectiles less laggy than the alternative
	public class ArtifactMinionGlobalProjectile : GlobalProjectile {
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => false;
		bool isRespawned = false;
		public StatModifier maxHealthModifier = StatModifier.Default;
		public int defense = 0;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			if (entity.ModProjectile is IArtifactMinion) Origins.ArtifactMinion[entity.type] = true;
			return Origins.ArtifactMinion[entity.type];
		}
		public override void SetDefaults(Projectile projectile) {
			isRespawned = false;
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
			if (projectile.ModProjectile is IArtifactMinion artifact) {
				if (prefix is ArtifactMinionPrefix artifactPrefix) artifact.MaxLife = (int)artifactPrefix.MaxLifeModifier.ApplyTo(artifact.MaxLife);
				artifact.Life = artifact.MaxLife;
			}
		}
		public static void ModifyHurt(Projectile projectile, ref int damage, bool fromDoT) {
			if (projectile.TryGetOwner(out Player player)) {
				player.OriginPlayer()?.broth?.ModifyHurt(projectile, ref damage, fromDoT);
			}
		}
		public static void OnHurt(Projectile projectile, int damage, bool fromDoT) {
			if (projectile.TryGetOwner(out Player player)) {
				player.OriginPlayer()?.broth?.OnHurt(projectile, damage, fromDoT);
			}
		}
		public override void PostAI(Projectile projectile) {
			if (projectile.ModProjectile is IArtifactMinion artifact) {
				if (artifact.Life <= 0) {
					artifact.Life = 0;
					if (artifact.CanDie) {
						projectile.Kill();
					}
				}
				if (projectile.owner == Main.myPlayer && projectile.numUpdates == -1) {
					ArtifactMinionSystem.nextArtifactMinions.Add(projectile.whoAmI);
				}
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
					proj.GetGlobalProjectile<ArtifactMinionGlobalProjectile>().isRespawned = true;
				}
			}
			if (projectile.TryGetGlobalProjectile(out OriginGlobalProj self) && self.prefix is ArtifactMinionPrefix artifactPrefix) {
				artifactPrefix.OnKill(projectile);
			}
		}
		public override Color? GetAlpha(Projectile projectile, Color lightColor) {
			if (isRespawned) return new Color(175, 225, 255, 128);
			return null;
		}
		public override void PostDraw(Projectile projectile, Color lightColor) {
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			if (projectile.ModProjectile is IArtifactMinion artifact) {
				binaryWriter.Write(artifact.Life);
			}
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			if (projectile.ModProjectile is IArtifactMinion artifact) {
				artifact.Life = binaryReader.ReadSingle();
			}
		}
	}
	public enum ArtifactMinionHealthbarStyles {
		Auto,
		UnderMinion,
		UnderBuff,
		Both
	}
	public class ArtifactMinionSystem : ModSystem {
		public static List<int> artifactMinions = [];
		public static List<int> nextArtifactMinions = [];
		public static int hoveredMinion = -1;
		public override void Unload() {
			artifactMinions = null;
			nextArtifactMinions = null;
		}
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			OriginExtensions.SwapClear(ref nextArtifactMinions, ref artifactMinions);
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Entity Health Bars"));
			if (inventoryIndex != -1) {//error prevention & null check
				if (OriginClientConfig.Instance.ArtifactMinionHealthbarStyle != ArtifactMinionHealthbarStyles.UnderBuff) {
					layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
						"Origins: Artifact Minion Health Bars",
						delegate {
							for (int i = 0; i < artifactMinions.Count; i++) {
								Projectile projectile = Main.projectile[artifactMinions[i]];
								if (projectile.ModProjectile is IArtifactMinion artifact) {
									Vector2 position = default;
									if (Main.HealthBarDrawSettings != 0 && artifact.Life < artifact.MaxLife) {
										if (Main.HealthBarDrawSettings == 1) {
											position = projectile.Bottom + new Vector2(0, projectile.gfxOffY + 10);
										} else {
											position = projectile.Top + new Vector2(0, projectile.gfxOffY - 24);
										}
										float light = Lighting.Brightness((int)position.X / 16, (int)(projectile.Center.Y + projectile.gfxOffY) / 16) * 0.5f + 0.5f;
										if (artifact.Life > 0) {
											Main.instance.DrawHealthBar(
												position.X, position.Y,
												(int)artifact.Life,
												artifact.MaxLife,
												light,
												0.85f
											);
										} else {
											artifact.DrawDeadHealthBar(position, light);
										}
									}
								}
							}
							return true;
						},
						InterfaceScaleType.Game) { Active = artifactMinions.Count > 0 }
					);
					inventoryIndex += 1;
				}
				if (OriginClientConfig.Instance.ArtifactMinionHealthbarStyle != ArtifactMinionHealthbarStyles.UnderMinion) {
					layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
						"Origins: Hovered Artifact Minion Indicator",
						delegate {
							if (hoveredMinion != -1) {
								Projectile projectile = Main.projectile[hoveredMinion];
								Texture2D value = TextureAssets.LockOnCursor.Value;
								Rectangle frame1 = new(0, 0, value.Width, 12);
								Rectangle frame2 = new(0, 16, value.Width, 12);
								Vector2 origin = frame1.Size() * new Vector2(0.5f, 1f);
								Color color1 = new(108, 255, 43, 220);
								float waveFactor = MathF.Sin(Main.GlobalTimeWrappedHourly * MathHelper.Pi);
								color1 *= 0.88f + waveFactor * 0.12f;
								Color color2 = color1.MultiplyRGBA(new Color(0.75f, 0.75f, 0.75f, 1f));
								Vector2 scale = new Vector2(0.58f, 1f) * (0.6f - waveFactor * 0.05f);
								Vector2 pos = projectile.Top - Vector2.UnitY * (0.6f + waveFactor * 0.4f) * 4 - Main.screenPosition;
								Main.spriteBatch.Draw(value, pos, frame1, color1, 0, origin, scale, SpriteEffects.None, 0f);
								Main.spriteBatch.Draw(value, pos, frame2, color2, 0, origin, scale, SpriteEffects.None, 0f);
							}
							hoveredMinion = -1;
							return true;
						},
						InterfaceScaleType.Game) { Active = artifactMinions.Count > 0 || hoveredMinion != -1 }
					);
					inventoryIndex += 1;
				}
			}
		}
		public static void DrawBuffHealthbars(int projectileType, ref BuffDrawParams drawParams, float startY) {
			Vector2 posOffset = Main.screenPosition;
			posOffset.X += drawParams.SourceRectangle.Width / 2;
			bool checkOnScreen = OriginClientConfig.Instance.ArtifactMinionHealthbarStyle == ArtifactMinionHealthbarStyles.Auto;
			bool tryKillHovered = Main.mouseRight && Main.mouseRightRelease;
			for (int i = 0; i < artifactMinions.Count; i++) {
				Projectile projectile = Main.projectile[artifactMinions[i]];
				if (projectile.type != projectileType) continue;
				if (checkOnScreen) {
					Vector2 pos = projectile.position.ToScreenPosition();
					if (pos.X <= Main.screenWidth && pos.Y <= Main.screenHeight && pos.X >= -projectile.width && pos.Y >= -projectile.height) {
						continue;
					}
				}
				if (projectile.ModProjectile is IArtifactMinion artifact) {
					if (drawParams.TextPosition.Y != startY) drawParams.TextPosition.Y += 2;
					if (artifact.Life > 0) {
						Main.instance.DrawHealthBar(
							drawParams.TextPosition.X + posOffset.X, drawParams.TextPosition.Y + posOffset.Y,
							(int)artifact.Life,
							artifact.MaxLife,
							drawParams.DrawColor.A / 255f,
							0.85f
						);
					} else {
						artifact.DrawDeadHealthBar(drawParams.TextPosition + posOffset, drawParams.DrawColor.A / 255f);
					}
					if (!Main.mouseText && new Rectangle((int)drawParams.TextPosition.X, (int)drawParams.TextPosition.Y, 36, 12).Contains(Main.MouseScreen)) {
						Main.LocalPlayer.mouseInterface = true;
						Main.instance.MouseText($"{(int)artifact.Life}/{artifact.MaxLife}");
						Main.mouseText = true;
						hoveredMinion = artifactMinions[i];
						if (tryKillHovered) {
							projectile.Kill();
							tryKillHovered = false;
						}
					}
					drawParams.TextPosition.Y += 10;
				}
			}
		}
	}
	public interface IArtifactMinion {
		int MaxLife { get; set; }
		float Life { get; set; }
		void ModifyHurt(ref int damage, bool fromDoT) { }
		void OnHurt(int damage, bool fromDoT) { }
		bool CanDie => true;
		void DrawDeadHealthBar(Vector2 position, float light) {
			Main.spriteBatch.Draw(
				TextureAssets.Hb2.Value,
				position - Main.screenPosition,
				null,
				Color.Gray * light,
				0f,
				new Vector2(18f, 0),
				0.85f,
				SpriteEffects.None,
			0);
		}
	}
	public static class ArtifactMinionExtensions {
		public static void DamageArtifactMinion(this IArtifactMinion minion, int damage, bool fromDoT = false, bool noCombatText = false) {
			minion.ModifyHurt(ref damage, fromDoT);
			ModProjectile proj = minion as ModProjectile;
			ArtifactMinionGlobalProjectile.ModifyHurt(proj.Projectile, ref damage, fromDoT);
			ArtifactMinionGlobalProjectile global = proj.Projectile.GetGlobalProjectile<ArtifactMinionGlobalProjectile>();
			if (!fromDoT) {
				damage -= global.defense;
				if (damage < 0) damage = 0;
			}
			float healthModifiedDamage = damage * (minion.MaxLife / global.maxHealthModifier.ApplyTo(minion.MaxLife));
			minion.Life -= healthModifiedDamage;
			minion.OnHurt(damage, fromDoT);
			ArtifactMinionGlobalProjectile.OnHurt(proj.Projectile, damage, fromDoT);
			if (minion.Life <= 0 && minion.CanDie) proj.Projectile.Kill();
			if (!noCombatText) CombatText.NewText(proj.Projectile.Hitbox, damage == 0 ? Color.Gray : CombatText.DamagedFriendly, damage, !fromDoT, dot: true);
		}
		public static void DamageArtifactMinion(this Projectile minion, int damage, bool fromDoT = false, bool noCombatText = false) {
			if (minion.ModProjectile is IArtifactMinion artifact) artifact.DamageArtifactMinion(damage, fromDoT, noCombatText);
		}
		public static bool GetHurtByHostiles(this IArtifactMinion minion, StatModifier? damageModifier = null) {
			if (minion is not ModProjectile proj) return false;
			Projectile projectile = proj.Projectile;
			StatModifier damageMod = damageModifier ?? StatModifier.Default;
			Rectangle projHitbox = projectile.Hitbox;
			int specialHitSetter = 1;
			float damageMultiplier = 1f;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.friendly && npc.damage > 0) {
					Rectangle npcRect = npc.Hitbox;
					NPC.GetMeleeCollisionData(projHitbox, npc.whoAmI, ref specialHitSetter, ref damageMultiplier, ref npcRect);
					if (npc.Hitbox.Intersects(projectile.Hitbox)) {
						NPC.HitInfo hit = new() {
							HitDirection = npc.Center.X > projectile.Center.X ? -1 : 1,
							Knockback = 2,
							Crit = false
						};
						projectile.velocity = OriginExtensions.GetKnockbackFromHit(hit);
						minion.DamageArtifactMinion((int)damageMod.ApplyTo(npc.damage * damageMultiplier));
						return true;
					}
				}
			}
			foreach (Projectile enemyProj in Main.ActiveProjectiles) {
				if (enemyProj.hostile && enemyProj.damage > 0 && enemyProj.Hitbox.Intersects(projectile.Hitbox)) {
					NPC.HitInfo hit = new() {
						HitDirection = enemyProj.direction,
						Knockback = 2,
						Crit = false
					};
					projectile.velocity = OriginExtensions.GetKnockbackFromHit(hit);
					minion.DamageArtifactMinion((int)damageMod.ApplyTo(enemyProj.damage));
					return true;
				}
			}
			return false;
		}
	}
}
