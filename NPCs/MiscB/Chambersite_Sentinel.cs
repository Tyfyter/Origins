using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Magic;
using Origins.Tiles.Other;
using PegasusLib;
using ReLogic.Utilities;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscB {
	public class Chambersite_Sentinel : ModNPC {
		public override void Load() => this.AddBanner(25);
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Drippler);
			NPC.lifeMax = 1600;
			NPC.defense = 12;
			NPC.damage = 54;
			NPC.width = 24;
			NPC.height = 47;
			NPC.friendly = false;
			NPC.HitSound = SoundID.DD2_CrystalCartImpact;
			NPC.DeathSound = SoundID.Item119.WithPitch(-1f);
			NPC.knockBackResist = 0.75f;
			NPC.value = 15000;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) => (OriginSystem.chambersiteTiles + OriginSystem.chambersiteWalls) * 0.0001f;
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns
			);
		}
		public override void OnKill() {
			Boss_Tracker.Instance.downedChambersiteSentinel = true;
			NetMessage.SendData(MessageID.WorldData);
		}
		Aim[] aims;
		Aim[] decayingAims;
		public override void AI() {
			aims ??= new Aim[Main.maxPlayers];
			decayingAims ??= new Aim[20];
			float maxLengthSQ = 16 * 16 * 12 * 12;
			if (--NPC.ai[3] <= 0) {
				DoShoot(maxLengthSQ);
			}
			Vector2 center = NPC.Center;
			int activeAims = 0;
			float highestProgress = 0;
			for (int i = 0; i < aims.Length; i++) {
				if (aims[i].active) {
					activeAims++;
					if (aims[i].Update(i, center, maxLengthSQ)) AddDecayingAim(aims[i]);
					highestProgress = float.Max(highestProgress, aims[i].Progress);
				}
			}
			for (int i = 0; i < decayingAims.Length; i++) {
				decayingAims[i].UpdateDecaying();
				highestProgress = float.Max(highestProgress, decayingAims[i].Progress);
			}
			if (SoundEngine.TryGetActiveSound(soundSlot, out ActiveSound sound)) {
				sound.Volume = highestProgress;
				sound.Position = NPC.Center;
			} else if (highestProgress > 0) {
				int projType = Type;
				soundSlot = SoundEngine.PlaySound(SoundID.Pixie.WithPitch(-1), NPC.Center, soundInstance => soundInstance.Volume > 0 && NPC.active && NPC.type == projType);
			}
			if (SoundEngine.TryGetActiveSound(soundSlot2, out sound)) {
				sound.Volume = highestProgress;
				sound.Position = NPC.Center;
			} else if (highestProgress > 0) {
				int projType = Type;
				soundSlot2 = SoundEngine.PlaySound(Origins.Sounds.LightningCharging.WithPitch(-1), NPC.Center, soundInstance => soundInstance.Volume > 0 && NPC.active && NPC.type == projType);
			}
			if (activeAims <= 0) {
				if (--NPC.localAI[0] <= 0) NPC.localAI[0] = Teleport() ? 300 : 20;
			} else {
				NPC.localAI[0] = 600;
			}
			Triangle hitTri;
			Vector2 perp;
			int dp2s = Main.rand.RandomRound(40 * ContentExtensions.DifficultyDamageMultiplier);
			foreach (Player player in Main.ActivePlayers) {
				Rectangle npcHitbox = player.Hitbox;
				for (int i = 0; i < aims.Length; i++) {
					if (!aims[i].active) continue;
					Vector2 motion = aims[i].Motion;
					if (motion == Vector2.Zero) continue;
					Vector2 norm = motion.SafeNormalize(Vector2.Zero);
					perp.X = norm.Y;
					perp.Y = -norm.X;
					hitTri = new(center + perp * 16, center - perp * 16, center + motion * NPC.scale + norm * 16);
					if (hitTri.Intersects(npcHitbox)) {
						player.lifeRegenCount -= (int)(dp2s * 1.6f);
						break;
					}
				}
			}
			NPC.spriteDirection = NPC.direction;
		}
		SlotId soundSlot;
		SlotId soundSlot2;
		bool Teleport() {
			List<Point> positions = [];
			static bool CheckTeleport(int x, int y) {
				for (int i = x - 1; i <= x + 1; i++) {
					for (int j = y - 3; j < y; j++) {
						if (Framing.GetTileSafely(i, j).HasFullSolidTile()) return false;
					}
				}
				return true;
			}
			Point pos = NPC.targetRect.Center().ToTileCoordinates();
			const int range = 15;
			const int invalid_range = 10;
			for (int i = -range; i <= range; i++) {
				if (i > -invalid_range && i < invalid_range) continue;
				for (int j = -range; j <= range; j++) {
					if (j > -invalid_range && j < invalid_range) continue;
					if (CheckTeleport(pos.X + i, pos.Y + j)) {
						positions.Add(new(pos.X + i, pos.Y + j - 3));
					}
				}
			}
			if (positions.Count <= 0) return false;
			NPC.Teleport(Main.rand.Next(positions).ToWorldCoordinates(0, -8), 13); // could have custom effect
			return true;
		}
		void AddDecayingAim(Aim aim) {
			aim.active = true;
			float bestLength = float.PositiveInfinity;
			int bestDecaying = 0;
			for (int i = 0; i < decayingAims.Length; i++) {
				if (decayingAims[i].active) {
					float length = decayingAims[i].Motion.LengthSquared();
					if (bestLength > length) {
						bestLength = length;
						bestDecaying = i;
					}
				} else {
					bestDecaying = i;
					break;
				}
			}
			decayingAims[bestDecaying] = aim;
		}
		bool DoShoot(float maxLengthSQ) {
			Vector2 aimOrigin = NPC.Center;
			foreach (Player player in Main.ActivePlayers) {
				if (aims[player.whoAmI].active) continue;
				Vector2 diff = player.Center - aimOrigin;
				float lengthSQ = diff.LengthSquared();
				if (lengthSQ > maxLengthSQ) continue;
				aims[player.whoAmI].Set();
			}

			NPC.ai[3] = 20;
			NPC.netUpdate = true;
			return true;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (aims is null) return true;
			Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Shinedown_Staff_P>()].Value;
			Vector2 position = NPC.Center + Vector2.UnitY * NPC.gfxOffY - Main.screenPosition;
			Vector2 origin = texture.Frame().Bottom();
			float spriteLengthFactor = 1.1f / texture.Height;
			Color color = Color.Black * 0.4f;
			DrawData data;
			for (int i = 0; i < aims.Length; i++) {
				if (!aims[i].active) continue;
				Vector2 motion = aims[i].Motion;
				data = new DrawData(
					texture,
					position,
					null,
					color,
					motion.ToRotation() + MathHelper.PiOver2,
					origin,
					new Vector2(1.2f, motion.Length() * spriteLengthFactor),
					SpriteEffects.None
				);
				for (int j = 0; j < 3; j++) Main.EntitySpriteDraw(data);
			}
			for (int i = 0; i < decayingAims.Length; i++) {
				if (!decayingAims[i].active) continue;
				Vector2 motion = decayingAims[i].Motion;
				data = new DrawData(
					texture,
					position,
					null,
					color,
					motion.ToRotation() + MathHelper.PiOver2,
					origin,
					new Vector2(1.2f, motion.Length() * spriteLengthFactor),
					SpriteEffects.None
				);
				for (int j = 0; j < 3; j++) Main.EntitySpriteDraw(data);
			}
			return true;
		}
		struct Aim {
			Vector2 motion;
			float progress;
			public bool active;
			public readonly float Progress => progress;
			public readonly Vector2 Motion => motion;
			public void Set() {
				motion = default;
				active = true;
				progress = 0;
			}
			public bool Update(int index, Vector2 position, float maxLengthSQ) {
				Player target = Main.player[index];
				if (!target.active || target.dead) target = null;
				if (target is null) {
					active = false;
					return true;
				}
				Vector2 diff = target.Center - position;
				if (diff.LengthSquared() > maxLengthSQ) {
					active = false;
					return true;
				}
				MathUtils.LinearSmoothing(ref progress, 1, 1 / 60f);
				float speed = progress + 1;
				MathUtils.LinearSmoothing(ref motion, diff, 4 * speed);
				motion = Utils.rotateTowards(Vector2.Zero, motion, diff, 0.3f * speed);
				return false;
			}
			public void UpdateDecaying() {
				MathUtils.LinearSmoothing(ref progress, 0, 1 / 60f);
				if (active) {
					float length = Motion.Length();
					motion *= 1 - (1 - 0.99f * ((length - 2) / length)) * (2 - progress);
					active = length > 4;
				}
			}
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Carburite_Item>(), 1, 7, 27));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Chambersite_Item>(), 1, 3, 6));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Shinedown>(), 19));
		}
	}
}
