using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Weapons.Melee;
using Origins.Journal;
using Origins.Misc;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Misc.Physics;

namespace Origins.NPCs.Brine {
	public class Sea_Dragon : Brine_Pool_NPC, IWikiNPC, IJournalEntrySource, ICustomWikiStat {
		public string EntryName => "Origins/" + typeof(Sea_Dragon_Entry).Name;
		public class Sea_Dragon_Entry : JournalEntry {
			public override string TextKey => "Sea_Dragon";
			public override JournalSortIndex SortIndex => new("Brine_Fiend", 3);
		}
		public Rectangle DrawRect => new(-28, 0, 94, 26);
		public int AnimationFrames => 64;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public Range FrameRange => new(28, 64);
		AutoLoadingAsset<Texture2D> strandTexture = typeof(Sea_Dragon).GetDefaultTMLName() + "_Strand";
		[field: CloneByReference]
		public HashSet<int> PredatorNPCTypes { get; private set; } = [];
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[NPC.type] = 7;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f
			};
			TargetNPCTypes.Add(ModContent.NPCType<Carpalfish>());
			TargetNPCTypes.Add(ModContent.NPCType<Brine_Latcher>());
			PredatorNPCTypes.Add(ModContent.NPCType<Carpalfish>());
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.aiStyle = -1;
			NPC.lifeMax = 210;
			NPC.defense = 21;
			NPC.damage = 60;
			NPC.width = 20;
			NPC.height = 20;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit19;
			NPC.DeathSound = SoundID.NPCDeath22;
			NPC.knockBackResist = 0.65f;
			NPC.value = 500;
			NPC.noGravity = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Brine_Pool>().Type
			];
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return Brine_Pool.SpawnRates.EnemyRate(spawnInfo, Brine_Pool.SpawnRates.Dragon);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags([
				this.GetBestiaryFlavorText()
			]);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(new LeadingConditionRule(DropConditions.PlayerInteraction).WithOnSuccess(
				ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<Alkaliphiliac_Tissue>(), 1, 1, 3)
			).WithOnSuccess(
				ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<Nematoclaw>(), 40)
			).WithOnSuccess(
				new LeadingConditionRule(new Conditions.IsHardmode()).WithOnSuccess(ItemDropRule.Food(ModContent.ItemType<Sour_Apple>(), 40))
			));
		}
		public override void AI() {
			DoTargeting();
			Vector2 direction;
			if (NPC.wet) {
				NPC.noGravity = true;
				if (TargetPos != default && !(NPC.HasValidTarget || TargetIsRipple)) TargetPos = default;
				if (TargetPos != default) {
					direction = NPC.DirectionTo(TargetPos);
					if (!TargetIsRipple && NPC.HasNPCTarget && PredatorNPCTypes.Contains(Main.npc[NPC.TranslatedTargetIndex].type)) {
						direction *= -1;
					}
				} else {
					if (NPC.collideX) NPC.velocity.X = -NPC.direction;
					NPC.direction = Math.Sign(NPC.velocity.X);
					if (NPC.direction == 0) NPC.direction = 1;
					direction = Vector2.UnitX * NPC.direction;
				}
				NPC.velocity *= 0.96f;
				const float dist = 16 * 4;
				float tileAvoidance = 0;
				for (int i = -3; i < 4; i++) {
					if (i == 0) continue;
					Vector2 dir = Vector2.UnitX.RotatedBy(NPC.rotation + i * 0.5f * NPC.direction);
					float newDist = CollisionExt.Raymarch(NPC.Center, dir, dist);
					//OriginExtensions.DrawDebugLine(NPC.Center, NPC.Center + dir * newDist);
					if (newDist < dist && Framing.GetTileSafely(NPC.Center + dir * (newDist + 2)).HasFullSolidTile()) {
						tileAvoidance += dist / (newDist * i + 1);
					}
				}
				if (tileAvoidance != 0) {
					direction = direction.RotatedBy(Math.Clamp(tileAvoidance * -0.5f * NPC.direction, -MathHelper.PiOver2, MathHelper.PiOver2));
				}
				GeometryUtils.AngularSmoothing(ref NPC.rotation, direction.ToRotation(), 0.1f);
				float oldRot = NPC.rotation;
				float diff = GeometryUtils.AngleDif(oldRot, NPC.rotation, out int bankDir) * 0.75f;
				NPC.velocity = NPC.velocity.RotatedBy(diff * bankDir) * (1 - diff * 0.1f);
				if (++NPC.ai[2] >= 40) {
					NPC.velocity += direction * 2;
					NPC.ai[2] = 0;
				} else if (NPC.ai[2] == 20) {
					NPC.velocity += direction * 2;
				} else {
					NPC.velocity += direction * 0.2f;
				}
				if (!Collision.WetCollision(NPC.position + NPC.velocity, 20, 20)) {
					NPC.velocity += direction * 2;
				}
			} else {
				NPC.noGravity = false;
				NPC.rotation = NPC.velocity.ToRotation();
				if (NPC.collideY) NPC.velocity.X *= 0.94f;
				//NPC.rotation += 0.01f;
			}
			NPC.spriteDirection = Math.Sign(Math.Cos(NPC.rotation));
		}
		public override bool? CanFallThroughPlatforms() => true;
		public override void FindFrame(int frameHeight) {
			float frame = (NPC.IsABestiaryIconDummy ? (float)++NPC.frameCounter : NPC.ai[2]) / 60;
			if (NPC.frameCounter >= 40) NPC.frameCounter = 0;
			NPC.frame = new Rectangle(0, (26 * (int)(frame * Main.npcFrameCount[Type])) % 182, 94, 26);
			chains ??= new Physics.Chain[4];
			for (int i = 0; i < chains.Length; i++) {
				Physics.Chain chain = chains[i];
				if (chain is null || chain.links[0].position.HasNaNs()) {
					SeaDragonAnchorPoint anchor = new(this, i % 2);
					const float spring = 0.5f;
					Gravity[] gravity = [
						new SeaDragonStrandGravity(this, i * 20)
					];
					switch (i) {
						case 2:
						gravity[0] = new SeaDragonStrandGravity(this, (i - 2) * 20 + 10);
						goto case 0;
						case 0:
						chains[i] = chain = new Physics.Chain() {
							anchor = anchor,
							links = [
								new(anchor.WorldPosition, default, 9.5f, gravity, drag: 0.93f, spring: spring),
								new(anchor.WorldPosition, default, 9.5f, gravity, drag: 0.93f, spring: spring),
								new(anchor.WorldPosition, default, 9.5f, gravity, drag: 0.93f, spring: spring),
								new(anchor.WorldPosition, default, 9.5f, gravity, drag: 0.93f, spring: spring),
								new(anchor.WorldPosition, default, 9.5f, gravity, drag: 0.93f, spring: spring)
							]
						};
						break;

						case 3:
						gravity[0] = new SeaDragonStrandGravity(this, (i - 2) * 20 + 10);
						goto case 1;
						case 1:
						chains[i] = chain = new Physics.Chain() {
							anchor = anchor,
							links = [
								new(anchor.WorldPosition, default, 9.5f, gravity, drag: 0.93f, spring: spring),
								new(anchor.WorldPosition, default, 9.5f, gravity, drag: 0.93f, spring: spring),
								new(anchor.WorldPosition, default, 9.5f, gravity, drag: 0.93f, spring: spring)
							]
						};
						break;
					}
				}
				int kMax = 2;
				for (int k = 0; k < kMax; k++) {
					chain.Update();
					if (kMax < 20 && (chain.links[0].position - chain.anchor.WorldPosition).LengthSquared() > 128 * 128) {
						kMax++;
					}
				}
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			target.AddBuff(BuffID.Venom, Main.rand.Next(120, 180));
			target.AddBuff(Toxic_Shock_Debuff.ID, Main.rand.Next(180, 240));
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVectorIn(spawnbox), projectile.velocity, "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			int halfWidth = NPC.width / 2;
			int baseX = player.direction > 0 ? 0 : halfWidth;
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(NPC.height)), hit.GetKnockbackFromHit(yMult: -0.5f), "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Origins.instance.SpawnGoreByName(
					NPC.GetSource_Death(),
					NPC.Center,
					Vector2.Zero,
					$"Gores/NPC/{nameof(Sea_Dragon)}_Gore_3"
				);
				Origins.instance.SpawnGoreByName(
					NPC.GetSource_Death(),
					NPC.Center + GeometryUtils.Vec2FromPolar(-16, NPC.rotation),
					Vector2.Zero,
					$"Gores/NPC/{nameof(Sea_Dragon)}_Gore_2"
				);
				Origins.instance.SpawnGoreByName(
					NPC.GetSource_Death(),
					NPC.Center + GeometryUtils.Vec2FromPolar(-32, NPC.rotation),
					Vector2.Zero,
					$"Gores/NPC/{nameof(Sea_Dragon)}_Gore_1"
				);
				int chainGore = Mod.GetGoreSlot($"Gores/NPC/{nameof(Sea_Dragon)}_Strand_Gore");
				for (int i = 1; i <= chains.Length; i++) {
					Physics.Chain chain = chains[^i];
					if (chain is null) continue;
					Vector2 startPoint = chain.anchor.WorldPosition;
					int frameOffset = i == 2 ? 0 : 2;
					for (int j = 0; j < chain.links.Length; j++) {
						Gore gore = Gore.NewGoreDirect(
							NPC.GetSource_Death(),
							chain.links[j].position,
							chain.links[j].velocity,
							chainGore
						);
						gore.Frame = new(5, 1, (byte)(j + frameOffset), 0);
					}
				}
			}
		}
		[CloneByReference] Physics.Chain[] chains;
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			SpriteEffects spriteEffects = SpriteEffects.FlipHorizontally;
			if (NPC.spriteDirection == 1) {
				spriteEffects |= SpriteEffects.FlipVertically;
			}
			if (NPC.IsABestiaryIconDummy) NPC.rotation = NPC.velocity.ToRotation();
			Texture2D texture = TextureAssets.Npc[Type].Value;
			Vector2 halfSize = new(texture.Width * 0.5f, texture.Height / Main.npcFrameCount[NPC.type] / 2);
			Vector2 position = new(NPC.position.X - screenPos.X + (NPC.width / 2) - texture.Width * NPC.scale / 2f + halfSize.X * NPC.scale, NPC.position.Y - screenPos.Y + NPC.height - texture.Height * NPC.scale / Main.npcFrameCount[NPC.type] + 4f + halfSize.Y * NPC.scale + NPC.gfxOffY);
			Vector2 origin = new(halfSize.X * 1.6f, halfSize.Y);
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
				position,
				NPC.frame,
				drawColor,
				NPC.rotation,
				origin,
				NPC.scale,
				spriteEffects,
			0);
			spriteEffects &= SpriteEffects.FlipVertically;
			if (chains is null) return false;
			for (int i = 1; i <= chains.Length; i++) {
				Physics.Chain chain = chains[^i];
				if (chain is null) continue;
				Vector2 startPoint = chain.anchor.WorldPosition;
				origin = strandTexture.Value.Size();
				origin.X *= 0.2f;
				origin.Y *= 0.5f;
				int frameOffset = i == 2 ? 0 : 2;
				for (int j = 0; j < chain.links.Length; j++) {
					spriteBatch.Draw(
						strandTexture,
						chain.links[j].position - screenPos,
						strandTexture.Value.Frame(5, frameX: j + frameOffset),
						NPC.IsABestiaryIconDummy ? Color.White : new Color(Lighting.GetSubLight(chain.links[j].position)),
						(chain.links[j].position - startPoint).ToRotation(),
						origin,
						1,
						spriteEffects,
					0);
					startPoint = chain.links[j].position;
				}
			}
			return false;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {

		}
		public class SeaDragonAnchorPoint(Sea_Dragon npc, int index) : AnchorPoint {
			public override Vector2 WorldPosition {
				get {
					if (npc.NPC.IsABestiaryIconDummy) npc.NPC.rotation = npc.NPC.velocity.ToRotation();
					Vector2 offset = Vector2.Zero;
					switch (index) {
						case 0:
						offset = new Vector2(-12, 0);
						break;

						case 1:
						offset = new Vector2(-32, -2);
						break;
					}
					return npc.NPC.Center + (offset * new Vector2(1, -npc.NPC.spriteDirection)).RotatedBy(npc.NPC.rotation);
				}
			}
		}
		public class SeaDragonStrandGravity(Sea_Dragon npc, int offset) : Gravity {
			public override Vector2 Acceleration {
				get {
					if (npc.NPC.IsABestiaryIconDummy) npc.NPC.rotation = npc.NPC.velocity.ToRotation();
					Vector2 dir = new Vector2(0, npc.NPC.spriteDirection).RotatedBy(npc.NPC.rotation) * 0.07f;
					Vector2 grav = new Vector2(npc.NPC.spriteDirection, 0).RotatedBy(npc.NPC.rotation) * 0.1f;
					int frame = npc.NPC.IsABestiaryIconDummy ? (int)npc.NPC.frameCounter : (int)npc.NPC.ai[2];
					frame = (frame + offset) % 40;
					const int time = 9;
					if (frame < time) {
						return grav - dir;
					}
					if (frame >= 20 && frame < 20 + time) {
						return grav + dir;
					}
					return grav;
				}
			}
		}
	}
}
