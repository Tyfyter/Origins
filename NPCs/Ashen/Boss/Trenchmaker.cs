using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics.Primitives;
using Origins.Items.Materials;
using Origins.Items.Other.LootBags;
using Origins.Items.Vanity.BossMasks;
using Origins.LootConditions;
using Origins.Tiles.Ashen;
using Origins.Tiles.BossDrops;
using Origins.World.BiomeData;
using PegasusLib;
using PegasusLib.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.NPCs.StateBossMethods<Origins.NPCs.Ashen.Boss.Trenchmaker>;

namespace Origins.NPCs.Ashen.Boss {
	[AutoloadBossHead]
	public class Trenchmaker : ModNPC, IStateBoss<Trenchmaker> {
		public static AIList<Trenchmaker> AIStates { get; } = [];
		public int[] PreviousStates { get; } = new int[6];
		internal static IItemDropRule normalDropRule;
		protected static AutoLoadingAsset<Texture2D> glowTexture = typeof(Trenchmaker).GetDefaultTMLName() + "_Glow";
		protected static AutoLoadingAsset<Texture2D> armTexture = typeof(Trenchmaker).GetDefaultTMLName() + "_Arm";
		protected static AutoLoadingAsset<Texture2D> pistonTexture = typeof(Trenchmaker).GetDefaultTMLName() + "_Leg_Piston";
		protected static AutoLoadingAsset<Texture2D> hipTexture = typeof(Trenchmaker).GetDefaultTMLName() + "_Hip";
		protected static AutoLoadingAsset<Texture2D> hipGlowTexture = typeof(Trenchmaker).GetDefaultTMLName() + "_Hip_Glow";
		protected static AutoLoadingAsset<Texture2D> thighTexture = typeof(Trenchmaker).GetDefaultTMLName() + "_Thigh";
		protected static AutoLoadingAsset<Texture2D> calfTexture = typeof(Trenchmaker).GetDefaultTMLName() + "_Calf";
		protected static AutoLoadingAsset<Texture2D> footTexture = typeof(Trenchmaker).GetDefaultTMLName() + "_Foot";
		protected SpriteEffects SpriteEffects => NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		public Vector2 GunPos => NPC.Center + new Vector2(19, -4).Apply(SpriteEffects, default);
		public static LegAnimation defaultLegAnimation;
		public override void Load() {
			this.AddBossControllerItem();
		}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 8;
			NPCID.Sets.BossBestiaryPriority.Add(Type);
			NPCID.Sets.CantTakeLunchMoney[Type] = true;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			Origins.NPCOnlyTargetInBiome.Add(Type, ModContent.GetInstance<Ashen_Biome>());
			ContentSamples.NpcBestiaryRarityStars[Type] = 3;
			this.SetupStates();
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.width = 104;
			NPC.height = 74;
			NPC.lifeMax = 6600;
			NPC.damage = 27;
			NPC.defense = 6;
			NPC.boss = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.waterMovementSpeed = 1;
			NPC.npcSlots = 200;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-2f);
			NPC.knockBackResist = 0;
			Array.Fill(PreviousStates, NPC.aiAction);
			SpawnModBiomes = [
				ModContent.GetInstance<Ashen_Biome>().Type
			];
			this.SetAIState(StateIndex<PhaseOneIdleState>());
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			NPC.KillsCountTowardsNPC<Fearmaker>(bestiaryEntry);

			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(false, true)
			);
		}
		public Leg[] legs = [new(), new()];
		public override void AI() {
			Vector2 diff = NPC.GetTargetData().Center - GunPos;
			Vector2 direction = diff.SafeNormalize(Vector2.UnitY);
			GeometryUtils.AngularSmoothing(ref NPC.rotation, direction.ToRotation(), 0.05f);
			this.GetState().DoAIState(this);
		}
		Vector2 hoikOffset = default;
		void SetHoikOffset(Vector2 value) {
			if (Math.Abs(hoikOffset.X) < Math.Abs(value.X)) hoikOffset.X = value.X;
			if (Math.Abs(hoikOffset.Y) < Math.Abs(value.Y)) hoikOffset.Y = value.Y;
		}
		public override void PostAI() {
			if (!NPC.noGravity) return;
			NPC.velocity.Y += 0.4f;
			DoCollision(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, true);
			for (int i = 0; i < legs.Length; i++) UpdateLeg(i);
			NPC.position += hoikOffset;
			hoikOffset = default;
		}
		public override void OnKill() {
			if (!NPC.downedBoss2 || Main.rand.NextBool(2)) WorldGen.spawnMeteor = true;
			NPC.SetEventFlagCleared(ref NPC.downedBoss2, GameEventClearedID.DefeatedEaterOfWorldsOrBrainOfChtulu);
		}
		public void UpdateLeg(int index) {
			GetLegPositions(legs[index], out _, out _, out Vector2 oldFootPos);
			oldFootPos -= new Vector2(27, 7).Apply(SpriteEffects, new Vector2(54, 30));
			legs[index].CurrentAnimation.Update(this, ref legs[index], legs[(index + 1) % legs.Length]);
			float diff = GeometryUtils.AngleDif(legs[index].ThighRot - MathHelper.PiOver4, legs[index].CalfRot + MathHelper.PiOver4, out int dir);
			if (diff > 2) {
				legs[index].CalfRot -= (diff - 2) * dir;
			}
			GetLegPositions(legs[index], out _, out _, out Vector2 footPos);

			footPos -= new Vector2(27, 7).Apply(SpriteEffects, new Vector2(54, 30));
			Vector2 newFootPos = oldFootPos;
			Vector2 footOffset = newFootPos - NPC.position;
			Vector2 footVelocity = (footPos - oldFootPos) + NPC.velocity;
			Vector2 oldFootVelocity = footVelocity;
			DoCollision(ref newFootPos, ref footVelocity, 54, 22);
			bool standing = Math.Abs(footVelocity.Y - oldFootVelocity.Y) > 0.25f;
			if (standing) footVelocity.X *= 1f / float.Pi;
			SetHoikOffset((newFootPos - NPC.position) - footOffset);
			NPC.velocity += footVelocity - oldFootVelocity;
			DoCollision(ref newFootPos, ref NPC.velocity, 54, 22);

			if (legs[index].WasStanding == standing) legs[index].TimeStanding++;
			else legs[index].TimeStanding = 0;
			legs[index].WasStanding = standing;

			LegAnimation oldAnimation = legs[index].CurrentAnimation;
			legs[index].CurrentAnimation = legs[index].CurrentAnimation.Continue(this, legs[index], legs[(index + 1) % legs.Length], footVelocity - oldFootVelocity);
			if (legs[index].CurrentAnimation != oldAnimation) legs[index].TimeInAnimation = 0;
			else legs[index].TimeInAnimation++;
		}
		public static void DoCollision(ref Vector2 position, ref Vector2 velocity, int width, int height, bool fallThrough = false) {
			Vector4 slopeCollision = Collision.SlopeCollision(position, velocity, width, height, fall: fallThrough);
			position = slopeCollision.XY();
			velocity = slopeCollision.ZW();
			velocity = Collision.TileCollision(position, velocity, width, height, fallThrough, fallThrough);
		}
		private static VertexRectangle VertexRectangle = new VertexRectangle();
		public void GetLegPositions(Leg leg, out Vector2 thighPos, out Vector2 calfPos, out Vector2 footPos) {
			SpriteEffects effects = SpriteEffects;
			thighPos = NPC.Center + (new Vector2(-1, 29) + new Vector2(6, 19)).Apply(effects, default);
			calfPos = thighPos + new Vector2(-27, 31).Apply(effects, default).RotatedBy(leg.ThighRot * NPC.direction);
			footPos = calfPos + new Vector2(27, 31).Apply(effects, default).RotatedBy(leg.CalfRot * NPC.direction);
		}
		public void DrawLeg(SpriteBatch spriteBatch, Vector2 screenPos, Color tintColor, Leg leg) {
			SpriteBatchState state = spriteBatch.GetState();
			try {
				spriteBatch.Restart(state, SpriteSortMode.Immediate);
				SpriteEffects effects = SpriteEffects;
				GetLegPositions(leg, out Vector2 thighPos, out Vector2 calfPos, out Vector2 footPos);
				Vector2 thighPistonPos = thighPos + new Vector2(1, 25).Apply(effects, default).RotatedBy(leg.ThighRot * NPC.direction);
				Vector2 calfPistonPos = calfPos + new Vector2(26, 6).Apply(effects, default).RotatedBy(leg.CalfRot * NPC.direction);
				Vector2 thighUnit = Vector2.UnitX.RotatedBy(leg.ThighRot * NPC.direction) * 4;
				Vector2 calfUnit = Vector2.UnitX.RotatedBy(leg.CalfRot * NPC.direction) * 4;
				MiscShaderData shader = GameShaders.Misc["Origins:Identity"];
				shader.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(0, 0, 0, 1));
				shader.Shader.Parameters["uSourceRect0"].SetValue(new Vector4(0, 0, 1, 1));
				shader.UseImage0(pistonTexture);
				//shader.UseImage2(ModContent.Request<Texture2D>("Origins/Textures/SC_Mask"));
				shader.Apply();
				VertexRectangle.DrawLit(screenPos, thighPistonPos - thighUnit, thighPistonPos + thighUnit, calfPistonPos - calfUnit, calfPistonPos + calfUnit);
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				spriteBatch.Draw(
					thighTexture,
					thighPos - screenPos,
					null,
					NPC.GetTintColor(Lighting.GetColor(thighPos.ToTileCoordinates(), tintColor)),
					leg.ThighRot * NPC.direction,
					new Vector2(49, 15).Apply(effects, thighTexture.Value.Size()),
					1,
					effects,
				0);
				spriteBatch.Draw(
					calfTexture,
					calfPos - screenPos,
					null,
					NPC.GetTintColor(Lighting.GetColor(calfPos.ToTileCoordinates(), tintColor)),
					leg.CalfRot * NPC.direction,
					new Vector2(14, 14).Apply(effects, calfTexture.Value.Size()),
					1,
					effects,
				0);
				spriteBatch.Draw(
					footTexture,
					footPos - screenPos,
					footTexture.Frame(verticalFrames: 2),
					NPC.GetTintColor(Lighting.GetColor(footPos.ToTileCoordinates(), tintColor)),
					0,
					new Vector2(27, 7).Apply(effects, footTexture.Value.Size()),
					1,
					effects,
				0);
			} catch {
				spriteBatch.Restart(state);
				throw;
			}
			spriteBatch.Restart(state);
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			SpriteEffects effects = SpriteEffects;
			int i = legs.Length - 1;
			int halfLegs = legs.Length / 2;
			for (; i >= halfLegs; i--) {
				int brightness = (int)(255 * float.Pow(0.75f, i));
				DrawLeg(spriteBatch, screenPos, new Color(brightness, brightness, brightness), legs[i]);
			}
			spriteBatch.DrawGlowingNPCPart(
				hipTexture,
				hipGlowTexture,
				NPC.Center - screenPos,
				null,
				NPC.GetTintColor(drawColor),
				NPC.GetTintColor(Color.White),
				0,
				new Vector2(33, -29).Apply(effects, hipTexture.Value.Size()),
				1,
				effects
			);
			spriteBatch.DrawGlowingNPCPart(
				TextureAssets.Npc[Type].Value,
				glowTexture,
				NPC.Center - screenPos,
				NPC.frame,
				NPC.GetTintColor(drawColor),
				NPC.GetTintColor(Color.White),
				0,
				new Vector2(52, 37).Apply(effects, NPC.frame.Size()),
				1,
				effects
			);
			for (; i >= 0; i--) {
				int brightness = (int)(255 * float.Pow(0.75f, i));
				DrawLeg(spriteBatch, screenPos, new Color(brightness, brightness, brightness), legs[i]);
			}
			spriteBatch.Draw(
				armTexture,
				GunPos - screenPos,
				armTexture.Frame(verticalFrames: 2),
				NPC.GetTintColor(drawColor),
				NPC.rotation + (effects.HasFlag(SpriteEffects.FlipHorizontally) ? 0 : MathHelper.Pi),
				new Vector2(47, 15).Apply(effects, armTexture.Value.Size()),
				1,
				effects,
			0);
			return false;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			normalDropRule = new LeadingSuccessRule();

			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Sanguinite_Ore_Item>(), 1, 140, 330));
			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<NE8>(), 1, 40, 100));
			//normalDropRule.OnSuccess(ItemDropRule.OneFromOptions(1, ModContent.ItemType<Low_Signal>(), ModContent.ItemType<Return_To_Sender>()));

			normalDropRule.OnSuccess(ItemDropRule.Common(TrophyTileBase.ItemType<Trenchmaker_Trophy>(), 10));
			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Trenchmaker_Mask>(), 10));

			npcLoot.Add(new DropBasedOnExpertMode(
				normalDropRule,
				new DropLocalPerClientAndResetsNPCMoneyTo0(ModContent.ItemType<Trenchmaker_Bag>(), 1, 1, 1, null)
			));
			//npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Mysterious_Spray>(), 4));
			npcLoot.Add(ItemDropRule.MasterModeCommonDrop(RelicTileBase.ItemType<Trenchmaker_Relic>()));
			//npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Blockus_Tube>(), 4));
		}
		public Rectangle GetFootHitbox(Leg leg) {
			GetLegPositions(leg, out _, out _, out Vector2 footPos);
			footPos -= new Vector2(27, 7).Apply(SpriteEffects, new Vector2(54, 30));
			return new((int)footPos.X, (int)footPos.Y, 54, 30);
		}
		public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) {
			for (int i = 0; i < legs.Length; i++) {
				if (!legs[i].CurrentAnimation.HasHitbox(this, legs[i])) continue;
				Rectangle footHitbox = GetFootHitbox(legs[i]);
				if (footHitbox.Intersects(victimHitbox)) {
					npcHitbox = footHitbox;
					return false;
				}
			}
			return base.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);
		}
		public class AutomaticIdleState : AutomaticIdleState<Trenchmaker> { }
		public abstract class AIState : AIState<Trenchmaker> {
			public virtual float WalkDist => 10 * 16;
		}
		public record struct Leg(float ThighRot, float CalfRot, LegAnimation CurrentAnimation, bool WasStanding = false, int TimeStanding = 0, int TimeInAnimation = 0) {
			LegAnimation currentAnimation = CurrentAnimation;
			public LegAnimation CurrentAnimation {
				get => currentAnimation ??= defaultLegAnimation;
				set => currentAnimation = value;
			}
			public bool RotateThigh(float target, float rate) {
				float thighRot = ThighRot;
				bool ret = GeometryUtils.AngularSmoothing(ref thighRot, target, rate);
				ThighRot = thighRot;
				return ret;
			}
			public bool RotateCalf(float target, float rate) {
				float calfRot = CalfRot;
				bool ret = GeometryUtils.AngularSmoothing(ref calfRot, target, rate);
				CalfRot = calfRot;
				return ret;
			}
		}
		public abstract class LegAnimation : ILoadable {
			private static readonly List<LegAnimation> animations = [];
			public int Type { get; private set; }
			public abstract void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg);
			public abstract LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement);
			public virtual bool HasHitbox(Trenchmaker npc, Leg leg) => false;
			public static LegAnimation Get(int type) => animations[type];
			public void Load(Mod mod) {
				Type = animations.Count;
				animations.Add(this);
				Load();
			}
			public virtual void Load() { }
			public void Unload() { }
			public static float PistonLength(Trenchmaker npc, Leg leg) {
				npc.GetLegPositions(leg, out Vector2 thighPos, out Vector2 calfPos, out _);
				SpriteEffects effects = npc.SpriteEffects;
				Vector2 thighConnectionPos = thighPos + new Vector2(1, 25).Apply(effects, default).RotatedBy(leg.ThighRot * npc.NPC.direction);
				Vector2 calfConnectionPos = calfPos + new Vector2(26, 6).Apply(effects, default).RotatedBy(leg.CalfRot * npc.NPC.direction);
				return thighConnectionPos.Distance(calfConnectionPos);
			}
			public static void PistonTo(Trenchmaker npc, ref Leg leg, float targetLength, float speedMult = 1) {
				if (targetLength < 2) targetLength = 2;
				if (targetLength > 44) targetLength = 44;
				GeometryUtils.AngleDif(leg.CalfRot + MathHelper.PiOver2, leg.ThighRot, out int dir);
				float dif = PistonLength(npc, leg) - targetLength;
				//Min(ref dif, 1);
				leg.CalfRot -= dir * dif * 0.01f * speedMult;
			}
		}
	}
}
