using Microsoft.Xna.Framework.Graphics;
using Origins.Events;
using System.Numerics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using ThoriumMod.Projectiles;
using static Origins.Core.AdvancedMiscShaderData.Parameter;
using static Terraria.ModLoader.ModContent;

namespace Origins.NPCs.Ashen {
	public class Wind_Pail : Glowing_Mod_NPC, IAshenEnemy {
		public bool IsOpaqueEnoughToAttack => NPC.Opacity > 0.5f;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 1;
			GetInstance<Smog_Storm.SpawnRates>().AddSpawn(Type, BiomeSpawnChance);
		}
		public override void SetDefaults() {
			NPC.lifeMax = 80;
			NPC.defense = 22;
			NPC.damage = 34;
			NPC.width = 40;
			NPC.height = 184;
			NPC.value = Item.buyPrice(0, 0, 2);
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			NPC.knockBackResist = 0f;
			SpawnModBiomes = [
				GetInstance<Smog_Storm>().Type,
			];
			NPC.alpha = 255;
			NPC.setFrameSize = true;
		}
		public override void AI() {
			const int attack_range = 16 * 15;
			if (!NPC.HasValidTarget || (Main.player[NPC.target].Center.X - NPC.Center.X) * NPC.direction < 1) {
				NPC.target = -1;
				if (LinearSmoothing(ref NPC.alpha, 255, 255 / 60)) AI_TryTeleport();
				return;
			} else {
				LinearSmoothing(ref NPC.alpha, 0, 255 / 60);
				if (!IsOpaqueEnoughToAttack) return;
			}
			Rectangle attackHitbox = NPC.Hitbox;
			attackHitbox.Width += attack_range;
			if (NPC.direction < 0) attackHitbox.X -= attack_range;
			switch (NPC.aiAction) {
				case 1:
				if (LinearSmoothing(ref NPC.ai[0], 1, 1f / 15)) {
					Player target = Main.player[NPC.target];
					target.AddBuff(ModContent.BuffType<Wind_Pail_Debuff>(), 15);
					target.OriginPlayer().windPailPosition = NPC.Center.X;
					for (int i = 0; i < 2; i++) {
						//todo: custom dust with alpha fade-in
						Dust dust = Dust.NewDustDirect(
							attackHitbox.TopLeft(),
							attackHitbox.Width,
							attackHitbox.Height,
							DustID.Silt
						);
						dust.velocity.Y = 2f + Main.rand.NextFloat() * 0.2f;
						dust.velocity.Y *= dust.scale;
						dust.velocity.Y *= 0.35f;
						dust.velocity.X = NPC.direction * -5f + Main.rand.NextFloat();
						dust.velocity.X += NPC.direction * 0.7f * -10f;
						dust.fadeIn += 0.7f * 0.2f;
						dust.velocity *= 1f + 0.35f * 0.5f;
						dust.velocity *= 1f + 0.35f;
					}
					if (!attackHitbox.Intersects(Main.player[NPC.target].Hitbox)) NPC.aiAction = 2;
				}
				break;
				case 2:
				if (NPC.ai[0].Cooldown(rate: 1f / 15)) {
					NPC.aiAction = 0;
					NPC.ai[1] = 60;
				}
				break;
				default: {
					if (!LinearSmoothing(ref NPC.ai[1], 0, 1)) break;
					if (attackHitbox.Intersects(Main.player[NPC.target].Hitbox)) {
						SoundEngine.PlaySound(SoundID.Item15.WithPitch(-1).WithPitchVarience(0) with { MaxInstances = 0 }, NPC.Center);
						SoundEngine.PlaySound(SoundID.Item15.WithPitch(0).WithPitchVarience(0) with { MaxInstances = 0 }, NPC.Center);
						NPC.aiAction = 1;
					}
					break;
				}
			}
		}
		public void AI_TryTeleport() {
			const float min_dist = 16 * 15;
			const float max_dist = 16 * 40;
			static float GetDist() => Main.rand.NextBool().ToDirectionInt() * Main.rand.NextFloat(min_dist, max_dist);
			Rectangle hitbox = NPC.Hitbox;
			float distSQ = float.PositiveInfinity;
			Vector2? targetPos = null;
			int targetIndex = 0;
			foreach (Player player in Main.ActivePlayers) {
				float currentDistSQ = player.DistanceSQ(NPC.Bottom);
				if (/*player.InModBiome<Smog_Storm>() &&*/ distSQ > currentDistSQ && TryGetPosition(player) is Vector2 pos) {
					distSQ = currentDistSQ;
					targetPos = pos;
					targetIndex = player.whoAmI;
				}
			}
			if (!targetPos.HasValue) return;
			NPC.target = targetIndex;
			NPC.targetRect = Main.player[NPC.target].Hitbox;
			NPC.Bottom = targetPos.Value;
			NPC.FaceTarget();
			bool posIsValidTiles(Vector2 pos) {
				hitbox.X = (int)(pos.X - (float)hitbox.Width * 0.5f);
				hitbox.Y = (int)pos.Y - hitbox.Height;
				return !hitbox.OverlapsAnyTiles() && hitbox.Add(Vector2.UnitY * 16).OverlapsAnyTiles();
			}
			bool posIsValidPlayers(Vector2 pos) {
				foreach (Player player in Main.ActivePlayers) {
					if (pos.WithinRange(player.Center, min_dist)) return false;
				}
				return true;
			}
			Vector2? TryGetPosition(Player player) {
				int tries = 0;
				while (++tries < 100) {
					Vector2 testPos = player.Center + new Vector2(GetDist(), GetDist() * 0.5f);
					if (posIsValidTiles(testPos) && posIsValidPlayers(testPos)) return testPos;
				}
				return null;
			}
		}
		public override bool CanHitPlayer(Player target, ref int cooldownSlot) => IsOpaqueEnoughToAttack;
		public override bool CanHitNPC(NPC target) => IsOpaqueEnoughToAttack;
		//Temp
		static bool LinearSmoothing<T>(ref T smoothed, T target, T rate) where T : INumberBase<T>, IComparisonOperators<T, T, bool> {
			if (target != smoothed) {
				if (T.Abs(target - smoothed) < rate) {
					smoothed = target;
				} else {
					if (target > smoothed) {
						smoothed += rate;
					} else if (target < smoothed) {
						smoothed -= rate;
					}
				}
			}
			return smoothed == target;
		}
		public static float BiomeSpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerInTown) return 0;
			return Smog_Storm.SpawnRates.WindPail;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void DrawBehind(int index) {
			if (NPC.setFrameSize.TrySet(false)) {
				NPC.frame.X = 94 * Main.rand.Next(3);
				NPC.frame.Width = 92;
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
			SpriteEffects spriteEffects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			spriteBatch.DrawGlowingNPCPart(
				TextureAssets.Npc[Type].Value,
				GlowTexture,
				NPC.Bottom - screenPos,
				NPC.frame,
				drawColor * NPC.Opacity,
				NPC.GetTintColor(Color.White) * NPC.Opacity * (NPC.ai[0] * 0.5f + 0.5f),
				0,
				NPC.frame.Size() * new Vector2(0.5f, 1),
				NPC.scale,
				spriteEffects
			);
			return false;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }
	}
	public class Wind_Pail_Debuff : ModBuff {
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
			On_Player.HorizontalMovement += On_Player_HorizontalMovement;
		}
		static void On_Player_HorizontalMovement(On_Player.orig_HorizontalMovement orig, Player player) {
			bool windPailPushed = player.OriginPlayer().windPailPushed;
			if (windPailPushed) player.windPushed = false;
			orig(player);
			if (windPailPushed) {
				float windStrength = float.CopySign(0.24f, player.OriginPlayer().windPailPosition - player.Center.X);
				if (player.velocity.Y != 0f) windStrength *= 1.35f;
				player.velocity.X += windStrength;
			}
		}
		public override void Update(Player player, ref int buffIndex) {
			player.windPushed = true;
			player.OriginPlayer().windPailPushed = true;
		}
	}
}
