using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics.Primitives;
using Origins.Items.Other.LootBags;
using Origins.LootConditions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Terraria;
using Terraria.GameContent;
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
		public static LegAnimation defaultLegAnimation;
		public override void Load() {
			this.AddBossControllerItem();
		}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 8;
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
			NPC.npcSlots = 200;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-2f);
			NPC.knockBackResist = 0;
			Array.Fill(PreviousStates, NPC.aiAction);
		}
		public Leg[] legs = [new(), new()];
		public override void AI() {
			this.GetState().DoAIState(this);
		}
		public override void PostAI() {
			NPC.velocity.Y += 0.4f;
			DoCollision(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, true);
			for (int i = 0; i < legs.Length; i++) UpdateLeg(i);
		}
		public void UpdateLeg(int index) {
			GetLegPositions(legs[index], out _, out _, out Vector2 oldFootPos);
			oldFootPos -= new Vector2(27, 7).Apply(SpriteEffects, new Vector2(54, 30));
			legs[index].CurrentAnimation.Update(this, ref legs[index], legs[index ^ 1]);
			GetLegPositions(legs[index], out _, out _, out Vector2 footPos);

			footPos -= new Vector2(27, 7).Apply(SpriteEffects, new Vector2(54, 30));
			Vector2 newFootPos = oldFootPos;
			Vector2 footOffset = newFootPos - NPC.position;
			Vector2 footVelocity = (footPos - oldFootPos) + NPC.velocity;
			Vector2 oldFootVelocity = footVelocity;
			DoCollision(ref newFootPos, ref footVelocity, 54, 22);
			bool standing = footVelocity.Y != oldFootVelocity.Y;
			if (standing) footVelocity.X *= 1f / float.Pi;
			NPC.position += (newFootPos - NPC.position) - footOffset;
			NPC.velocity += footVelocity - oldFootVelocity;
			legs[index].WasStanding = standing;
		}
		public static void DoCollision(ref Vector2 position, ref Vector2 velocity, int width, int height, bool fallThrough = false) {
			Vector4 slopeCollision = Collision.SlopeCollision(position, velocity, width, height);
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
			SpriteEffects effects = SpriteEffects;
			GetLegPositions(leg, out Vector2 thighPos, out Vector2 calfPos, out Vector2 footPos);
			Vector2 thighPistonPos = thighPos + new Vector2(1, 25).Apply(effects, default).RotatedBy(leg.ThighRot);
			Vector2 calfPistonPos = calfPos + new Vector2(26, 6).Apply(effects, default).RotatedBy(leg.CalfRot);
			Vector2 thighUnit = Vector2.UnitX.RotatedBy(leg.ThighRot) * 4;
			Vector2 calfUnit = Vector2.UnitX.RotatedBy(leg.CalfRot) * 4;
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
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			SpriteEffects effects = SpriteEffects;
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
			for (int i = legs.Length - 1; i >= 0; i--) {
				int brightness = (int)(255 * float.Pow(0.75f, i));
				DrawLeg(spriteBatch, screenPos, new Color(brightness, brightness, brightness), legs[i]);
			}
			spriteBatch.Draw(
				armTexture,
				NPC.Center + new Vector2(19, -4).Apply(effects, default) - screenPos,
				armTexture.Frame(verticalFrames: 2),
				NPC.GetTintColor(drawColor),
				0,
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

			npcLoot.Add(new DropBasedOnExpertMode(
				normalDropRule,
				new DropLocalPerClientAndResetsNPCMoneyTo0(ModContent.ItemType<Trenchmaker_Bag>(), 1, 1, 1, null)
			));
		}
		public class AutomaticIdleState : AutomaticIdleState<Trenchmaker> { }
		public abstract class AIState : AIState<Trenchmaker> { }
		public record struct Leg(float ThighRot, float CalfRot, LegAnimation CurrentAnimation, bool WasStanding = false) {
			LegAnimation currentAnimation = CurrentAnimation;
			public LegAnimation CurrentAnimation {
				get => currentAnimation ??= defaultLegAnimation;
				set => currentAnimation = value;
			}

		}
		public abstract class LegAnimation : ILoadable {
			private static readonly List<LegAnimation> animations = [];
			public int Type { get; private set; }
			public abstract void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg);
			public abstract LegAnimation Continue(Trenchmaker npc, Leg leg, Vector2 movement);
			public static LegAnimation Get(int type) => animations[type];
			public void Load(Mod mod) {
				Type = animations.Count;
				animations.Add(this);
				Load();
			}
			public virtual void Load() { }
			public void Unload() { }
		}
	}
}
