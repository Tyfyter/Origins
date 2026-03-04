using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Ranged;
using Origins.Projectiles;
using Origins.World.BiomeData;
using ReLogic.Utilities;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
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
		Vector2 WeldingTorchPos => NPC.Center + new Vector2(NPC.direction * 26, 4);
		AutoLoadingAsset<Texture2D> glowTexture = typeof(Repairboy).GetDefaultTMLName("_Glow");
		AutoLoadingAsset<Texture2D> armTexture = typeof(Repairboy).GetDefaultTMLName("_Arm");
		AutoLoadingAsset<Texture2D> armGlowTexture = typeof(Repairboy).GetDefaultTMLName("_Arm_Glow");
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.UsesNewTargetting[Type] = true;
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 45;
			NPC.defense = 8;
			NPC.damage = 18;
			NPC.width = 40;
			NPC.height = 26;
			NPC.noGravity = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Ashen_Biome>().Type,
			];
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
		}
		static float? NPCSearcher(NPC target, Rectangle area, NPC searcher, ref Rectangle hitbox) {
			if (target.ModNPC is IReparable reparable) {
				float weight = searcher.Distance(target.Center);
				if (reparable.NeedsRepair(searcher, ref weight, ref hitbox) is bool canBeRepaired) return canBeRepaired ? weight : null;
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
			if (!target.HasTarget || NPC.life < NPC.lifeMax || target.TargetType == TargetSearchTypes.Players) TargetClosest();
			if (target.HasTarget) {
				NPC.targetRect = target.TargetRect;
				MoveTowards(NPC.Center.Clamp(NPC.targetRect), out Vector2 targetDir, out float dist);
				if (target.TargetType != TargetSearchTypes.Tiles) NPC.ai[1] = 0;
				if (dist <= AttackDist) {
					if (NPC.ai[0].CycleUp(30)) {
						NPC.velocity += new Vector2(targetDir.Y, -targetDir.X) * Main.rand.NextBool().ToDirectionInt();
						NPC.netUpdate = true;
					}
					if (target.TargetTile is Tile tile) {
						if (NPC.ai[1].CycleUp(60)) {
							bool canKeepTarget = false;
							if (tile.HasTile && TileLoader.GetTile(tile.TileType) is IReparableTile reparableTile) {
								(int i, int j) = tile.GetTilePosition();
								reparableTile.Repair(i, j);
								Rectangle targetHitbox = target.LastTargetRect;
								float _ = 0;
								canKeepTarget = reparableTile.NeedsRepair(i, j, ref _, ref targetHitbox);
								target.LastTargetRect = targetHitbox;
							}
							if (!canKeepTarget) TargetClosest();
						}
						PlayWeldingSound(2);
					}
					if (MathUtils.LinearSmoothing(ref NPC.localAI[0], 3, 1 / 5f)) {
						if (NPC.ai[2].CycleUp(20)) {
							SoundEngine.PlaySound(SoundID.Item34.WithPitch(0.5f).WithVolume(0.75f), WeldingTorchPos);
							if (NPC.life >= NPC.lifeMax) TargetClosest();
						}
						if (NPC.ai[2] % 5 == 0) {
							Vector2 targetPos = WeldingTorchPos.Clamp(NPC.targetRect);
							if (targetPos != WeldingTorchPos) targetDir = WeldingTorchPos.DirectionTo(targetPos);
							NPC.SpawnProjectile(
								null,
								WeldingTorchPos,
								targetDir * 4,
								ModContent.ProjectileType<Repairboy_P>(),
								Main.rand.RandomRound(11 * ContentExtensions.DifficultyDamageMultiplier),
								0.425f
							);
						}
					}
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
				return NPC.active;
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
			public bool? NeedsRepair(NPC repairboy, ref float weight, ref Rectangle hitbox);
			/// <summary>
			/// Return true to skip healing
			/// </summary>
			public bool Repair(int repairAmount);
		}
		public interface IReparableTile {
			public bool NeedsRepair(int i, int j, ref float weight, ref Rectangle hitbox);
			public void Repair(int i, int j);
		}
		public SlotId weldingTorchSound;
		public int weldingTorchSoundTime = 0;
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
