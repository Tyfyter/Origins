using Microsoft.Xna.Framework.Graphics;
using Origins.Dusts;
using Origins.Items.Armor.Ashen;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Weapons.Ranged;
using Origins.LootConditions;
using Origins.Projectiles;
using Origins.World.BiomeData;
using ReLogic.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Origins.NPCExtensions;
using static Origins.NPCs.Ashen.Repairboy;

namespace Origins.NPCs.Ashen {
	public class Repairboy : ModNPC, IAshenEnemy {
		static float TargetDistMin => 24;
		static float TargetDistMax => 48;
		static float AttackDist => TargetDistMax + 16;
		static float TileRepairTime => 20 * 60 * 5;
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			return OriginSystem.repairboyTiles > 0 ? 0.2f : 0.0001f;
		}
		Vector2 WeldingTorchPos => NPC.Center + new Vector2(NPC.direction * 26, 4);
		AutoLoadingAsset<Texture2D> glowTexture = typeof(Repairboy).GetDefaultTMLName("_Glow");
		AutoLoadingAsset<Texture2D> armTexture = typeof(Repairboy).GetDefaultTMLName("_Arm");
		AutoLoadingAsset<Texture2D> armGlowTexture = typeof(Repairboy).GetDefaultTMLName("_Arm_Glow");
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.UsesNewTargetting[Type] = true;
			ModContent.GetInstance<Ashen_Biome.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 45;
			NPC.defense = 8;
			NPC.damage = 18;
			NPC.width = 32;
			NPC.height = 26;
			NPC.noGravity = true;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1f);
			NPC.DeathSound = SoundID.NPCDeath44;
			SpawnModBiomes = [
				ModContent.GetInstance<Ashen_Biome>().Type,
			];
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(new CommonDrop(ModContent.ItemType<Biocomponent10>(), 1, 1, 3));
			npcLoot.Add(ScavengerBonus.Scrap(1, 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BBQ_Skewer>(), 19));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ashen2_Helmet>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ashen2_Breastplate>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ashen2_Greaves>(), 525));
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		AdvancedTargetSearchResults target;
		void MoveTowards(Vector2 targetPos, out Vector2 targetDir, out float dist) {
			const float acceleration = 0.35f;
			Vector2 movementDir = NPC.velocity.Normalized(out float speed);
			targetDir = (targetPos - NPC.Center).Normalized(out dist);
			if (dist < TargetDistMin) {
				NPC.velocity -= targetDir * acceleration * 0.5f;
				if (targetDir == default) {
					if (NPC.velocity.X * NPC.direction > -0.1f) NPC.velocity.X -= NPC.direction * acceleration * 0.5f;
					targetDir.X = NPC.direction;
				}
			} else if (dist > TargetDistMax) {
				float speedInRightDir = Vector2.Dot(movementDir, targetDir) * speed;
				float predictedTime = (float.Sqrt(2 * (dist - TargetDistMax) * -acceleration + speedInRightDir * speedInRightDir) + speedInRightDir) / acceleration;
				if (!float.IsNaN(predictedTime)) {
					NPC.velocity -= targetDir * acceleration;
				} else {
					NPC.velocity += targetDir * acceleration;
				}
			}
			NPC.direction = targetDir.X < 0 ? -1 : 1;
		}
		public void TargetClosest(bool resetTarget = true, bool faceTarget = true) {
			Tile? oldTargetPos = target.TargetTile;
			ReduceTargetingCount(oldTargetPos);
			ReduceTargetingCount(target.TargetNPC);
			AdvancedTargetSearchResults searchResults = NPC.SearchForTarget(
				new Rectangle(0, 0, 1920 * 2, 1080 * 2).Recentered(NPC.Center),
				TargetSearchTypes.All,
				playerSearcher: static (Player player, Rectangle area, NPC searcher, ref Rectangle hitbox) => searcher.Distance(player.Center) + (!searcher.playerInteraction[player.whoAmI]).Mul(800) - player.aggro,
				npcSearcher: NPCSearcher,
				tileSearcher: TileSearcher
			);
			if (searchResults.HasTarget || resetTarget) target = searchResults;
			NPC.targetRect = target.TargetRect;
			if (faceTarget) NPC.FaceTarget();
			IncreaseTargetingCount(target.TargetTile);
			IncreaseTargetingCount(target.TargetNPC);
		}
		static float? NPCSearcher(NPC target, Rectangle area, NPC searcher, ref Rectangle hitbox) {
			if (target.ModNPC is IReparable reparable) {
				if (repairboysTargetingNPCs.Count(target, npc => !npc.active) >= reparable.RepairboyLimit) return null;
				float weight = searcher.Distance(target.Center);
				if (reparable.NeedsRepair(searcher, ref weight, ref hitbox) is bool canBeRepaired) return canBeRepaired ? weight : null;
			} else if (repairboysTargetingNPCs.Count(target, npc => !npc.active) >= 3) {
				return null;
			}
			if (target.ModNPC is not IAshenEnemy { CanBeRepaired: true }) {
				if (target.friendly || target.CountsAsACritter) return searcher.Distance(target.Center);
				return null;
			}
			if (target.type == searcher.type || target.life >= target.lifeMax) return null;
			return searcher.Distance(target.Center) + (target.life - target.lifeMax);
		}
		static (float cost, int id, Rectangle hitbox)? TileSearcher(Rectangle area, NPC searcher) {
			Point low = area.TopLeft().ToTileCoordinates();
			Point high = area.BottomRight().ToTileCoordinates();
			Max(ref low.X, 0);
			Max(ref low.Y, 0);
			Min(ref high.X, Main.maxTilesX);
			Min(ref high.Y, Main.maxTilesY);

			float bestCost = float.PositiveInfinity;
			Rectangle bestHitbox = default;
			Tile bestTile = default;

			Rectangle hitbox = default;
			Vector2 center = searcher.Center;
			for (int j = low.Y; j < high.Y; j++) {
				for (int i = low.X; i < high.X; i++) {
					Tile tile = Main.tile[i, j];
					if (!tile.HasTile || TileLoader.GetTile(tile.TileType) is not IReparableTile reparableTile || TileObjectData.GetTileData(tile) is not TileObjectData data) continue;
					TileUtils.GetMultiTileTopLeft(i, j, data, out hitbox.X, out hitbox.Y);
					if (i != hitbox.X || j != hitbox.Y) continue;
					if (repairboysTargeting.Count(new(i, j), npc => !npc.active) >= reparableTile.RepairboyLimit) continue;
					hitbox.X *= 16;
					hitbox.Y *= 16;
					hitbox.Width = data.Width * 16;
					hitbox.Height = data.Height * 16;
					float weight = center.Distance(hitbox.Center());
					if (!reparableTile.NeedsRepair(i, j, ref weight, ref hitbox)) continue;
					if (Minimize(ref bestCost, weight)) {
						bestHitbox = hitbox;
						bestTile = tile;
					}
				}
			}
			if (bestHitbox == default) return null;
			return (bestCost, Unsafe.BitCast<Tile, int>(bestTile), bestHitbox);
		}
		public override void AI() {
			const float strafe_accel = 0.05f;
			if (!target.HasTarget || NPC.life < NPC.lifeMax || target.TargetType == TargetSearchTypes.Players) TargetClosest();
			if (target.HasTarget) {
				NPC.targetRect = target.TargetRect;
				Vector2 targetPos = NPC.Center.Clamp(NPC.targetRect);
				MoveTowards(targetPos, out Vector2 targetDir, out float dist);
				if (target.TargetType != TargetSearchTypes.Tiles) NPC.ai[1] = 0;
				if (dist <= AttackDist) {
					targetPos = WeldingTorchPos.Clamp(NPC.targetRect);
					Vector2 torchDir = targetDir;
					if (targetPos != WeldingTorchPos) torchDir = WeldingTorchPos.DirectionTo(targetPos);

					if (!MathUtils.LinearSmoothing(ref NPC.ai[1], 0, 1)) {
						NPC.velocity += targetDir.Perpendicular(Math.Sign(NPC.ai[1])) * strafe_accel;
					} else if (NPC.ai[0].CycleUp(target.TargetType == TargetSearchTypes.Tiles ? 180 : 30)) {
						NPC.ai[1] = Main.rand.NextBool().ToDirectionInt() * (Main.rand.Next(20, 40) + 1);
						if (Math.Abs(torchDir.Y) > Math.Abs(torchDir.X)) {
							NPC.ai[1] = -40 * Math.Sign(torchDir.Y) * NPC.direction;
						}
						NPC.netUpdate = true;
					}
					if (NPC.ai[1] == 0 && MathUtils.LinearSmoothing(ref NPC.localAI[0], 3, 1 / 5f)) {
						if (NPC.ai[2].CycleUp(20)) {
							SoundEngine.PlaySound(SoundID.Item34.WithPitch(0.5f).WithVolume(0.75f), WeldingTorchPos);
							if (NPC.life >= NPC.lifeMax) TargetClosest();
						}
						switch (NPC.ai[2] % 5) {
							case 0: {
								NPC.SpawnProjectile(
									null,
									WeldingTorchPos,
									torchDir * 4,
									ModContent.ProjectileType<Repairboy_P>(),
									Main.rand.RandomRound(11 * ContentExtensions.DifficultyDamageMultiplier),
									0.425f
								);
								if (target.TargetType == TargetSearchTypes.Tiles) {
									bool canKeepTarget = false;
									if (target.TargetTile is Tile tile) {
										Rectangle targetHitbox = target.LastTargetRect;
										canKeepTarget = ProgressRepair(tile, ref targetHitbox);
										target.LastTargetRect = targetHitbox;
										PlayWeldingSound(7);
									}
									if (!canKeepTarget) TargetClosest();
								}
								break;
							}
							case 4: {
								Vector2 pos = targetPos;
								float collisionPoint = 0f;
								if (Collision.CheckAABBvLineCollision(NPC.targetRect.TopLeft(), NPC.targetRect.Size(), WeldingTorchPos, WeldingTorchPos + torchDir * 64, 0.0001f, ref collisionPoint)) {
									pos = WeldingTorchPos + torchDir * collisionPoint;
								}
								Dust dust = Dust.NewDustDirect(pos, 0, 0, ModContent.DustType<Spark_Dust>(), Scale: 1.43f);
								dust.velocity *= 2;
								dust.noGravity = true;
								break;
							}
						}
					}
					if (NPC.ai[1] != 0) MathUtils.LinearSmoothing(ref NPC.localAI[0], 0, 1 / 5f);
				} else {
					MathUtils.LinearSmoothing(ref NPC.localAI[0], 0, 1 / 5f);
					NPC.ai[0] = 0;
				}
			}
			NPC.velocity *= 0.97f;
			NPC.DoFrames(4);
			if (weldingTorchSoundTime.Cooldown()) {
				weldingTorchSound.GetSound()?.Stop();
				SoundEngine.PlaySound(Origins.Sounds.WeldingTorchCancel, NPC.Center);
			}
		}
		public void PlayWeldingSound(int duration) {
			weldingTorchSound.PlaySoundIfInactive(Origins.Sounds.WeldingTorch, NPC.Center, sound => {
				sound.Position = NPC.Center;
				return NPC.active && Main.npc[NPC.whoAmI] == NPC;
			});
			weldingTorchSoundTime = duration;
		}
		public override bool? CanFallThroughPlatforms() => true;
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			SpriteEffects effects = NPC.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			drawColor = NPC.GetTintColor(drawColor);
			DrawData data = new(TextureAssets.Npc[Type].Value,
				NPC.Center - screenPos,
				NPC.frame,
				drawColor,
				0,
				new Vector2(32, 21).Apply(effects, NPC.frame.Size()),
				NPC.scale,
				effects
			);
			data.Draw(spriteBatch);
			data.texture = glowTexture;
			data.color = Color.White;
			data.Draw(spriteBatch);

			data.sourceRect = NPC.frame with { Y = NPC.frame.Height * (int)NPC.localAI[0] };
			data.texture = armTexture;
			data.color = drawColor;
			data.Draw(spriteBatch);
			data.texture = armGlowTexture;
			data.color = Color.White;
			data.Draw(spriteBatch);
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			target.Write(writer);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			target = AdvancedTargetSearchResults.Read(reader);
		}
		public interface IReparable {
			public int RepairboyLimit => 3;
			public bool? NeedsRepair(NPC repairboy, ref float cost, ref Rectangle hitbox);
			/// <summary>
			/// Return true to skip healing
			/// </summary>
			public bool Repair(int repairAmount);
		}
		public interface IReparableTile {
			public int RepairboyLimit => 3;
			public bool NeedsRepair(int i, int j, ref float cost, ref Rectangle hitbox);
			public void Repair(int i, int j);
		}
		public SlotId weldingTorchSound;
		public int weldingTorchSoundTime = 0;
		/// <returns>Whether or not repairs should continue</returns>
		public static bool ProgressRepair(Tile tile, ref Rectangle hitbox, int amount = 5) {
			repairProgress ??= [];
			if (!tile.HasTile || TileLoader.GetTile(tile.TileType) is not IReparableTile reparableTile) return false;
			Point16 pos = new(tile.GetTilePosition());
			if ((repairProgress[pos] += amount) >= TileRepairTime) {
				reparableTile.Repair(pos.X, pos.Y);
				repairProgress[pos] = 0;
			}
			float _ = 0;
			if (reparableTile.NeedsRepair(pos.X, pos.Y, ref _, ref hitbox)) return true;
			repairProgress[pos] = 0;
			return false;
		}
		public static void ResetRepair(Tile tile) => ResetRepair(tile.GetTilePosition());
		public static void ResetRepair(Point pos) => ResetRepair(new Point16(pos));
		public static void ResetRepair(int i, int j) => ResetRepair(new Point16(i, j));
		public static void ResetRepair(Point16 pos) {
			repairProgress ??= [];
			repairProgress[pos] = 0;
		}
		internal static FungibleSet<Point16> repairProgress;
		internal static MultiSet<Point, NPC> repairboysTargeting = new();
		internal static MultiSet<NPC, NPC> repairboysTargetingNPCs = new();
		protected void ReduceTargetingCount(Tile? tile) {
			if (tile.HasValue) repairboysTargeting.Remove(tile.Value.GetTilePosition(), NPC);
		}
		protected void IncreaseTargetingCount(Tile? tile) {
			if (tile.HasValue) repairboysTargeting.Add(tile.Value.GetTilePosition(), NPC);
		}
		protected void ReduceTargetingCount(NPC npc) {
			if (npc is not null) repairboysTargetingNPCs.Remove(npc, NPC);
		}
		protected void IncreaseTargetingCount(NPC npc) {
			if (npc?.active ?? false) repairboysTargetingNPCs.Add(npc, NPC);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore1");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore2");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore3");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore4");
				for (int i = 0; i < 5; i++) {
					Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
				}
			}
		}
		public class MultiSet<TKey, TValue>(IEqualityComparer<TKey> comparer = null, IEqualityComparer<TValue> valueComparer = null) {
			readonly Dictionary<TKey, HashSet<TValue>> inner = new(comparer);
			public bool Contains(TKey key, TValue value) {
				if (!inner.TryGetValue(key, out HashSet<TValue> set)) return false;
				return set.Contains(value);
			}
			public bool Add(TKey key, TValue value) {
				if (!inner.TryGetValue(key, out HashSet<TValue> set)) inner[key] = set = new(valueComparer);
				return set.Add(value);
			}
			public bool Remove(TKey key, TValue value) {
				if (!inner.TryGetValue(key, out HashSet<TValue> set)) return false;
				bool retVal = set.Remove(value);
				if (set.Count <= 0) inner.Remove(key);
				return retVal;
			}
			public int Count(TKey key) {
				if (!inner.TryGetValue(key, out HashSet<TValue> set)) return 0;
				return set.Count;
			}
			public int Count(TKey key, Predicate<TValue> removeWhere) {
				if (!inner.TryGetValue(key, out HashSet<TValue> set)) return 0;
				set.RemoveWhere(removeWhere);
				return set.Count;
			}
			public void Prune(Predicate<TKey> removeWhere) {
				TKey[] keys = new TKey[inner.Count];
				inner.Keys.CopyTo(keys, 0);
				for (int i = 0; i < keys.Length; i++) {
					if (removeWhere(keys[i])) inner.Remove(keys[i]);
				}
			}
		}
	}
	public class Repairboy_P : Welding_Torch_P {
		public override string Texture => typeof(Welding_Torch_P).GetDefaultTMLName();
		protected Repairboy_P() : base() {
			healCooldown ??= new int[Main.maxNPCs];
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.friendly = false;
			Projectile.hostile = true;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[1] = -1;
			if (source is EntitySource_Parent { Entity: NPC parentNPC } && parentNPC.ModNPC is Repairboy) Projectile.ai[1] = parentNPC.whoAmI;
		}
		public override void DoHealing(Rectangle hitbox) {
			bool doSound = false;
			foreach (NPC other in Main.ActiveNPCs) {
				if (healCooldown[other.whoAmI] > 0) continue;
				if (other.ModNPC is Repairboy) continue;
				if (other.life < other.lifeMax && Projectile.Colliding(hitbox, other.Hitbox)) {
					doSound = true;
					if (other.ModNPC is IReparable reparable && reparable.Repair(Projectile.damage)) continue;
					if (other.ModNPC is IAshenEnemy { CanBeRepaired: true }) {
						float oldHealth = other.life;
						other.life += Main.rand.RandomRound(Projectile.damage * 0.15f);
						if (other.life > other.lifeMax) other.life = other.lifeMax;
						CombatText.NewText(other.Hitbox, CombatText.HealLife, (int)Math.Round(other.life - oldHealth), true, dot: true);
						healCooldown[other.whoAmI] = 20;
					}
				}
			}
			if (doSound && Main.npc.GetIfInRange((int)Projectile.ai[1])?.ModNPC is Repairboy repairboy) {
				repairboy.PlayWeldingSound(10);
			}
		}
	}
}
