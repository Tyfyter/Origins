using Microsoft.Xna.Framework.Graphics;
using Origins.Core.Animation;
using Origins.Graphics;
using Origins.World.BiomeData;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Core.Animation.KeyframeData;
using static Origins.NPCs.Ashen.D2L2;
using static Terraria.ModLoader.ModContent;

namespace Origins.NPCs.Ashen;
public class D2L2 : ModNPC, IAshenEnemy {
	static AutoLoadingTexture gunTex = typeof(D2L2).GetDefaultTMLName("_Gun");
	static AutoGlowingTexture thighTex = typeof(D2L2).GetDefaultTMLName("_Thigh");
	static AutoLoadingTexture calfTex = typeof(D2L2).GetDefaultTMLName("_Calf");
	static AutoLoadingTexture footTex = typeof(D2L2).GetDefaultTMLName("_Foot");
	public D2L2Animation Animation {
		get => D2L2Animation.animations[NPC.aiAction];
		set {
			if (NPC.aiAction.TrySet(value.Type)) AnimationTime = 0;
		}
	}
	public ref float AnimationTime => ref NPC.ai[0];
	public override void Load() => this.AddBanner();
	public override void SetStaticDefaults() {
		NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft with {
			Position = new Vector2(35, 0),
			PortraitPositionXOverride = 0,
			PortraitPositionYOverride = 0
		};
		GetInstance<Ashen_Biome.SpawnRates>().AddSpawn(Type, Ashen_Biome.SpawnRates.ScrapyardStryder);
	}
	public override void SetDefaults() {
		NPC.aiStyle = NPCAIStyleID.ActuallyNone;
		NPC.lifeMax = 475;
		NPC.defense = 24;
		NPC.damage = 48;
		NPC.width = 70;
		NPC.height = 76;
		NPC.value = 230;
		NPC.knockBackResist = 0.5f;
		NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
		NPC.DeathSound = SoundID.NPCDeath44;
		NPC.noTileCollide = false;
		NPC.noGravity = false;
		//NPC.noGravity = true;
		SpawnModBiomes = [
			GetInstance<Ashen_Biome>().Type,
		];
	}
	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
		bestiaryEntry.CustomBestiaryName(Type, this.GetLocalizationKey("FullName"));
		bestiaryEntry.AddTags(
			this.GetBestiaryFlavorText()
		);
	}
	Vector2 oldAnimotion;
	Vector2 frontLegPos;
	Vector2 backLegPos;
	D2L2Animation.AnchorPoint oldAnchor;
	int collideY;
	public bool CollideY {
		get => collideY < 4;
		set => collideY = (!value).Mul(4);
	}
	public override void AI() {
		//NPC.velocity = default;
		Vector2 animotion = default;
		GetLegTransforms(false, out _, out _, out PosRotScale frontFoot);
		GetLegTransforms(true, out _, out _, out PosRotScale backFoot);
		Vector2 frontLegPos = frontFoot.Position;
		Vector2 backLegPos = backFoot.Position;
		D2L2Animation.AnchorPoint newAnchor = Animation.anchorPoint.GetCurrentValue(AnimationTime);
		switch (oldAnchor) {
			case D2L2Animation.AnchorPoint.FrontFoot:
			animotion = this.frontLegPos - frontLegPos;
			break;
			case D2L2Animation.AnchorPoint.BackFoot:
			animotion = this.backLegPos - backLegPos;
			break;
		}
		animotion.Y = 0;
		if (oldAnchor != D2L2Animation.AnchorPoint.None) {
			NPC.velocity = (NPC.collideY || CollideY) ?
			(NPC.velocity - oldAnimotion) * 0.9f + animotion :
			NPC.velocity * 0.97f;
			//NPC.velocity += animotion - oldAnimotion;
		}
		oldAnimotion = animotion;
		if (oldAnchor.TrySet(newAnchor)) {
			Vector2 position = default;
			switch (oldAnchor) {
				case D2L2Animation.AnchorPoint.FrontFoot:
				position = frontLegPos;
				break;
				case D2L2Animation.AnchorPoint.BackFoot:
				position = backLegPos;
				break;
			}
			while (position.Y > 0 && new Rectangle((int)position.X, (int)position.Y, 70, 18).OverlapsAnyTiles(false)) {
				NPC.position.Y -= 2;
				position.Y -= 2;
			}
		}
		this.frontLegPos = frontLegPos;
		this.backLegPos = backLegPos;
	}
	public override bool? CanFallThroughPlatforms() {
		collideY++;
		NPC.velocity.Y += NPC.gravity;
		ApplyCollision(frontLegPos);
		ApplyCollision(backLegPos);
		NPC.velocity.Y -= NPC.gravity;
		return base.CanFallThroughPlatforms();
	}
	void ApplyCollision(Vector2 legPos) {
		Vector2 footPos = NPC.Center + legPos - new Vector2(35, 8);
		Vector2 oldFootPos = footPos;
		Vector2 oldVel = NPC.velocity;
		DoCollision(ref footPos, ref NPC.velocity, 70, 18);
		//footPos.X = oldFootPos.X;
		NPC.velocity.X = oldVel.X;
		NPC.position += footPos - oldFootPos;
		if (footPos.Y < oldFootPos.Y) {
			CollideY = true;
			Max(ref NPC.velocity.Y, 0);
		} else if (NPC.velocity.Y < oldVel.Y) CollideY = true;
	}
	public static void DoCollision(ref Vector2 position, ref Vector2 velocity, int width, int height, bool fallThrough = false) {
		int tries = 0;
		while (new Rectangle((int)position.X, (int)position.Y + 10, width, height - 10).OverlapsAnyTiles(fallThrough) && ++tries > 4) position.Y -= 1;
		if (tries > 0) {
			Min(ref velocity.Y, 0);
			position.Y = (int)position.Y;
		}
		Vector4 slopeCollision = Collision.SlopeCollision(position, velocity, width, height, fall: fallThrough);
		position = slopeCollision.XY();
		velocity = slopeCollision.ZW();
		velocity = Collision.TileCollision(position, velocity, width, height, fallThrough, fallThrough);
	}
	public override void FindFrame(int frameHeight) {
		NPC.spriteDirection = NPC.direction;
		D2L2Animation animation = Animation;
		float oldTime = AnimationTime;
		animationControls.Play(ref AnimationTime, animation, loop: Animation.Loop);
		animation.events.Update(this, oldTime, AnimationTime);
	}
	public override void ModifyNPCLoot(NPCLoot npcLoot) {
	}
	public override void HitEffect(NPC.HitInfo hit) {
		base.HitEffect(hit);
	}
	public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) {
		npcHitbox.Height -= 22;
		npcHitbox.Y += 22;
		return base.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);
	}
	public override void SendExtraAI(BinaryWriter writer) {
		writer.Write(NPC.aiAction);
	}
	public override void ReceiveExtraAI(BinaryReader reader) {
		NPC.aiAction = reader.ReadInt32();
	}
	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		drawColor = NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(drawColor));
		using ScopedOverride<bool> o = NPC.oiled.ScopedOverride(false);
		Color glowColor = NPC.GetTintColor(Color.White);
		D2L2Animation animation = Animation;
		Vector2 pos = NPC.Center - screenPos + (NPC.oldPosition - NPC.position) * 0.5f;
		DrawLeg(spriteBatch, pos, true, drawColor.MultiplyRGBA(Color.Gray), glowColor.MultiplyRGBA(Color.Gray));
		spriteBatch.DrawRiggedPart(
			gunTex,
			pos,
			D2L2Animation.gunOffset + animation.gun.GetCurrentValue(AnimationTime),
			null,
			drawColor,
			0,
			new(62, 9),
			1,
			SpriteEffects.None
		);
		spriteBatch.DrawRiggedPart(
			TextureAssets.Npc[Type].Value,
			pos,
			new(),
			null,
			drawColor,
			0,
			new(35, 38),
			1,
			SpriteEffects.None
		);
		DrawLeg(spriteBatch, pos, false, drawColor, glowColor);
		NPC.DrawConfused();
		Editor.D2L2 = this;
		Animation_Editor_UI.DrawEditorUI = Editor.DrawUI;
		return false;
	}
	public void GetLegTransforms(bool back, out PosRotScale thigh, out PosRotScale calf, out PosRotScale foot) {
		D2L2Animation animation = Animation;
		KeyframeSet<PosRotScale> thighOffset;
		KeyframeSet<PosRotScale> calfOffset;
		KeyframeSet<PosRotScale> footOffset;
		if (back) {
			thighOffset = animation.thighBack;
			calfOffset = animation.calfBack;
			footOffset = animation.footBack;
		} else {
			thighOffset = animation.thighFront;
			calfOffset = animation.calfFront;
			footOffset = animation.footFront;
		}
		PosRotScale legTransform = D2L2Animation.thighOffset + thighOffset.GetCurrentValue(AnimationTime);
		thigh = legTransform;

		legTransform += D2L2Animation.calfOffset;
		legTransform += calfOffset.GetCurrentValue(AnimationTime);
		calf = legTransform;

		legTransform += D2L2Animation.footOffset;
		legTransform += footOffset.GetCurrentValue(AnimationTime);
		foot = legTransform;
	}
	public void DrawLeg(SpriteBatch spriteBatch, Vector2 pos, bool back, Color drawColor, Color glowColor) {
		KeyframeSet<FootFrame> footFrame = back ? Animation.footBackFrame : Animation.footFrontFrame;
		GetLegTransforms(back, out PosRotScale thigh, out PosRotScale calf, out PosRotScale foot);
		if (!back) {
			spriteBatch.DrawGlowingRiggedPart(
				thighTex,
				pos,
				thigh,
				null,
				drawColor,
				glowColor,
				0,
				new(16, 16),
				1,
				SpriteEffects.None
			);
		}
		spriteBatch.DrawRiggedPart(
			calfTex,
			pos,
			calf,
			null,
			drawColor,
			0,
			new(68, 20),
			1,
			SpriteEffects.None
		);
		if (back) {
			spriteBatch.DrawGlowingRiggedPart(
				thighTex.Texture,
				thighTex.GlowTexture,
				pos,
				thigh,
				null,
				drawColor,
				glowColor,
				0,
				new(16, 16),
				1,
				SpriteEffects.None
			);
		}
		spriteBatch.DrawRiggedPart(
			footTex,
			pos,
			foot,
			footTex.Frame(verticalFrames: 2, frameY: footFrame.GetCurrentValue(AnimationTime)),
			drawColor,
			0,
			new(32, 10),
			1,
			SpriteEffects.None
		);
		/*
		Vector2 footPos = NPC.Center + foot.Position - new Vector2(35, 8);
		OriginExtensions.DrawDebugLineSprite(footPos + new Vector2(0, 18), footPos + new Vector2(70, 18), Color.Red, useScreenPos: true);
		*/
	}
	KeyframeAnimation.PlayingState animationControls;
	static class Editor {
		public static D2L2 D2L2;
		public static void DrawUI(SpriteBatch spriteBatch) {
			D2L2Animation animation = D2L2.Animation;
			animation.gizmoBasePosition = D2L2.NPC.Center - Main.screenPosition;
			animation.DrawEditorUI(spriteBatch, ref D2L2.AnimationTime, ref D2L2.animationControls);
			if (KeyframeAnimation.DrawSwapper(spriteBatch, D2L2Animation.animations, out D2L2Animation selected, D2L2.Animation)) D2L2.Animation = selected;
		}
	}
	public record struct FootFrame(int Frame) : IAnimatableSpriteFrame<FootFrame> {
		public static Texture2D Texture => footTex;
		public static int FrameCount => 2;
		public static implicit operator FootFrame(int value) => new(value);
		public static implicit operator int(FootFrame value) => value.Frame;
	}
}
#region animations
public abstract class D2L2Animation : KeyframeAnimation, ILoadable {
	public static List<D2L2Animation> animations = [];
	public readonly static Vector2 gunOffset = new(-11, 29);
	public readonly static Vector2 thighOffset = new(15, -19);
	public readonly static Vector2 calfOffset = new(74, 0);
	public readonly static Vector2 footOffset = new(-54, 100);
	public KeyframeSet<PosRotScale> gun = new(new(), thighOffset);
	public KeyframeSet<PosRotScale> thighFront = new(new());
	public KeyframeSet<PosRotScale> calfFront = new(new(), new ChildOfOffset<PosRotScale>("thighFront", calfOffset));
	public KeyframeSet<PosRotScale> footFront = new(new(), new ChildOfOffset<PosRotScale>("calfFront", footOffset));
	public KeyframeSet<FootFrame> footFrontFrame = new();
	public KeyframeSet<PosRotScale> thighBack = new(new(), thighOffset);
	public KeyframeSet<PosRotScale> calfBack = new(new(), new ChildOfOffset<PosRotScale>("thighBack", calfOffset));
	public KeyframeSet<PosRotScale> footBack = new(new(), new ChildOfOffset<PosRotScale>("calfBack", footOffset));
	public KeyframeSet<FootFrame> footBackFrame = new();
	public EnumKeyframeSet<AnchorPoint> anchorPoint = new();
	public EventKeyframeSet<D2L2> events = [];

	public int Type { get; private set; }
	public void Load(Mod mod) {
		Type = animations.Count;
		animations.Add(this);
	}
	public void Unload() { }
	public virtual bool Loop => false;
	public virtual void DrawExtras(SpriteBatch spriteBatch, D2L2 D2L2, Vector2 position, Color drawColor) { }
	public enum AnchorPoint {
		[Description("The D2L2 will not attempt to move")]
		Stop,
		[Description("The D2L2's movement will be based on its front foot")]
		FrontFoot,
		[Description("The D2L2's movement will be based on its back foot")]
		BackFoot,
		[Description("The D2L2's movement will only be based on its momentum")]
		None
	}
}
public class D2L2_Walk_Start : D2L2Animation {
	public D2L2_Walk_Start() {
		gun = new(new(), new Vector2(-11f, 29f)) {
			new(21, new(new(-0.75f, -2.25f), 0f, 1f), [])
		};
		thighFront = new(new(), new Vector2(15f, -19f)) {
			new(13, new(Vector2.Zero, -0.7978264f, 1f), [new Exponential(2f)]),
			new(27, new(Vector2.Zero, 0.7435203f, 1f), [new Exponential(2f)])
		};
		calfFront = new(new(), new ChildOfOffset<PosRotScale>("thighFront", new(new(74f, 0f), 0f))) {
			new(13, new(Vector2.Zero, 0.39644134f, 1f), []),
			new(20, new(Vector2.Zero, 0.42721963f, 1f), []),
			new(27, new(Vector2.Zero, -0.058840573f, 1f), [new Exponential(2f)])
		};
		footFront = new(new(), new ChildOfOffset<PosRotScale>("calfFront", new(new(-54f, 100f), 0f))) {
			new(13, new(Vector2.Zero, -0.30294824f, 1f), []),
			new(20, new(Vector2.Zero, 0.41238415f, 1f), []),
			new(27, new(Vector2.Zero, -0.6523212f, 1f), [])
		};
		footFrontFrame = new(0) {
			new(13, 1, []),
			new(27, 0, [])
		};
		thighBack = new(new(), new Vector2(15f, -19f)) {
			new(13, new(Vector2.Zero, -0.08883035f, 1f), []),
			new(27, new(Vector2.Zero, -0.27503765f, 1f), [new Exponential(2f)])
		};
		calfBack = new(new(), new ChildOfOffset<PosRotScale>("thighBack", new(new(74f, 0f), 0f))) {
			new(13, new(Vector2.Zero, -0.10938823f, 1f), []),
			new(27, new(Vector2.Zero, -0.3041209f, 1f), [])
		};
		footBack = new(new(), new ChildOfOffset<PosRotScale>("calfBack", new(new(-54f, 100f), 0f))) {
			new(13, new(Vector2.Zero, 0.19647622f, 1f), [])
		};
		footBackFrame = new(0) {
			new(27, 1, [])
		};
		anchorPoint = new(AnchorPoint.BackFoot) {
			new(27, AnchorPoint.FrontFoot, Stepped)
		};
		events = [
					new(27, EnterLoop)
				];
	}
	public static void EnterLoop(in D2L2 d2L2) => d2L2.Animation = Main.LocalPlayer.controlSmart ? GetInstance<D2L2_Walk_End>() : GetInstance<D2L2_Walk_Loop>();
}
public class D2L2_Walk_Loop : D2L2Animation {
	public override bool Loop => true;
	public D2L2_Walk_Loop() {
		gun = new(new(new(-0.75f, -2.25f), 0f, 1f), (true, []), new Vector2(-11f, 29f)) {
			new(1, new(new(0f, 3.75f), 0f, 1f), [new Exponential(2f)]),
			new(5, new(new(0.75f, 0.75f), 0f, 1f), []),
			new(27, new(new(0.75f, -1.5f), 0f, 1f), [])
		};
		thighFront = new(new(Vector2.Zero, 0.7435203f, 1f), (true, []), new Vector2(15f, -19f)) {
			new(17, new(Vector2.Zero, 0.26160896f, 1f), []),
			new(34, new(Vector2.Zero, -0.23204267f, 1f), []),
			new(51, new(Vector2.Zero, -0.5969832f, 1f), [new Exponential(2f)])
		};
		calfFront = new(new(Vector2.Zero, -0.058840573f, 1f), (true, []), new ChildOfOffset<PosRotScale>("thighFront", new(new(74f, 0f), 0f))) {
			new(17, new(Vector2.Zero, -0.021928906f, 1f), []),
			new(34, new(Vector2.Zero, -0.18748999f, 1f), []),
			new(51, new(Vector2.Zero, -0.091175914f, 1f), [])
		};
		footFront = new(new(Vector2.Zero, -0.6523212f, 1f), (true, []), new ChildOfOffset<PosRotScale>("calfFront", new(new(-54f, 100f), 0f))) {
			new(17, new(Vector2.Zero, -0.24834383f, 1f), []),
			new(34, new(Vector2.Zero, 0.4205594f, 1f), []),
			new(51, new(Vector2.Zero, -0.19758749f, 1f), [])
		};
		footFrontFrame = new(0) {
			new(17, 0, Stepped),
			new(35, 1, [])
		};
		thighBack = new(new(Vector2.Zero, -0.27503765f, 1f), (true, []), new Vector2(15f, -19f)) {
			new(17, new(Vector2.Zero, -0.5969832f, 1f), [new Exponential(2f)]),
			new(34, new(Vector2.Zero, 0.47995877f, 1f), []),
			new(51, new(Vector2.Zero, 0.26160896f, 1f), [])
		};
		calfBack = new(new(Vector2.Zero, -0.3041209f, 1f), (true, []), new ChildOfOffset<PosRotScale>("thighBack", new(new(74f, 0f), 0f))) {
			new(17, new(Vector2.Zero, -0.091175914f, 1f), []),
			new(26, new(Vector2.Zero, 0.3534186f, 1f), [new SinEaseBoth()]),
			new(34, new(Vector2.Zero, 0.04447925f, 1f), [new Exponential(2f)]),
			new(51, new(Vector2.Zero, -0.021928906f, 1f), [])
		};
		footBack = new(new(Vector2.Zero, 0.19647622f, 1f), (true, []), new ChildOfOffset<PosRotScale>("calfBack", new(new(-54f, 100f), 0f))) {
			new(2, new(Vector2.Zero, 0.53961456f, 1f), []),
			new(10, new(Vector2.Zero, 0.031573534f, 1f), []),
			new(17, new(Vector2.Zero, -0.19758749f, 1f), []),
			new(26, new(Vector2.Zero, 0.28837478f, 1f), []),
			new(34, new(Vector2.Zero, -0.49688005f, 1f), []),
			new(51, new(Vector2.Zero, -0.24834383f, 1f), [])
		};
		footBackFrame = new(1) {
			new(34, 0, Stepped)
		};
		anchorPoint = new(AnchorPoint.FrontFoot) {
			new(34, AnchorPoint.BackFoot, Stepped)
		};
		events = [
			new(34, ExitLoopFront)
		];
		MinimumLength = 68f;
	}
	public static void ExitLoopFront(in D2L2 d2L2) {
		if (Main.LocalPlayer.controlSmart) d2L2.Animation = GetInstance<D2L2_Walk_End>();
	}
	//public static void ExitLoopBack(in D2L2 d2L2) => d2L2.Animation = GetInstance<D2L2_Walk_End>();
}
public class D2L2_Walk_End : D2L2Animation {
	public D2L2_Walk_End() {
		gun = new(new(), new Vector2(-11f, 29f)) {
		};
		thighFront = new(new(Vector2.Zero, -0.23204267f, 1f), new Vector2(15f, -19f)) {
			new(8, new(Vector2.Zero, -0.36172557f, 1f), []),
			new(16, new(Vector2.Zero, 0.008316994f, 1f), [])
		};
		calfFront = new(new(Vector2.Zero, -0.18748999f, 1f), new ChildOfOffset<PosRotScale>("thighFront", new(new(74f, 0f), 0f))) {
			new(8, new(Vector2.Zero, 0.23668003f, 1f), []),
			new(16, new(Vector2.Zero, 0.0060328245f, 1f), [])
		};
		footFront = new(new(Vector2.Zero, 0.4205594f, 1f), new ChildOfOffset<PosRotScale>("calfFront", new(new(-54f, 100f), 0f))) {
			new(8, new(Vector2.Zero, 0.3908286f, 1f), []),
			new(16, new(Vector2.Zero, -0.006381631f, 1f), [])
		};
		footFrontFrame = new(0) {
			new(4, 1, []),
			new(14, 0, [])
		};
		thighBack = new(new(Vector2.Zero, 0.47995877f, 1f), new Vector2(15f, -19f)) {
			new(16, new(Vector2.Zero, 0.010640144f, 1f), [])
		};
		calfBack = new(new(Vector2.Zero, 0.04447925f, 1f), new ChildOfOffset<PosRotScale>("thighBack", new(new(74f, 0f), 0f))) {
			new(16, new(Vector2.Zero, 0.020093799f, 1f), [])
		};
		footBack = new(new(Vector2.Zero, -0.49688005f, 1f), new ChildOfOffset<PosRotScale>("calfBack", new(new(-54f, 100f), 0f))) {
			new(16, new(Vector2.Zero, -0.031288266f, 1f), [])
		};
		footBackFrame = new(0) {
		};
		anchorPoint = new(AnchorPoint.BackFoot) {
		};
		events = [];
	}
}
public class D2L2_Crouch : D2L2Animation {
	public D2L2_Crouch() {
		gun = new(new(), gunOffset) {
			new(27, new(Vector2.Zero, 0.06814659f, 1f), []),
			new(42, new(Vector2.Zero, 0.1473881f, 1f), [])
		};
		thighFront = new(new(), thighOffset) {
			new(27, new(Vector2.Zero, -0.23580456f, 1f), []),
			new(42, new(Vector2.Zero, -0.5379113f, 1f), [])
		};
		calfFront = new(new(), new ChildOfOffset<PosRotScale>("thighFront", new(new(74f, 0f), 0f))) {
			new(27, new(Vector2.Zero, 0.23044586f, 1f), []),
			new(42, new(Vector2.Zero, 0.3668859f, 1f), [])
		};
		footFront = new(new(), new ChildOfOffset<PosRotScale>("calfFront", new(new(-54f, 100f), 0f))) {
			new(27, new(Vector2.Zero, -0.018389583f, 1f), []),
			new(42, new(Vector2.Zero, 0.16861808f, 1f), [])
		};
		footFrontFrame = new(0) {
		};
		thighBack = new(new(), thighOffset) {
			new(27, new(Vector2.Zero, -0.3155309f, 1f), []),
			new(42, new(Vector2.Zero, -0.5722594f, 1f), [])
		};
		calfBack = new(new(), new ChildOfOffset<PosRotScale>("thighBack", new(new(74f, 0f), 0f))) {
			new(27, new(Vector2.Zero, 0.16658342f, 1f), []),
			new(42, new(Vector2.Zero, 0.31270564f, 1f), [])
		};
		footBack = new(new(), new ChildOfOffset<PosRotScale>("calfBack", new(new(-54f, 100f), 0f))) {
			new(27, new(Vector2.Zero, 0.15873635f, 1f), []),
			new(42, new(Vector2.Zero, 0.23482752f, 1f), [])
		};
		footBackFrame = new(0) {
		};
		events = [];
	}
}
public class D2L2_Brace : D2L2Animation {
	public D2L2_Brace() {
		gun = new(new(), gunOffset) {
			new(81, new(Vector2.Zero, -0.28897548f, 1f), [])
		};
		thighFront = new(new(), thighOffset) {
			new(1, new(Vector2.Zero, -0.6934594f, 1f), []),
			new(81, new(Vector2.Zero, 0.11655903f, 1f), [])
		};
		calfFront = new(new(), new ChildOfOffset<PosRotScale>("thighFront", new(new(74f, 0f), 0f))) {
			new(1, new(Vector2.Zero, 0.38217604f, 1f), []),
			new(81, new(Vector2.Zero, 0.17847073f, 1f), [])
		};
		footFront = new(new(), new ChildOfOffset<PosRotScale>("calfFront", new(new(-54f, 100f), 0f))) {
			new(1, new(Vector2.Zero, -0.4154806f, 1f), []),
			new(81, new(Vector2.Zero, 0.23621702f, 1f), [])
		};
		footFrontFrame = new(0) {
			new(1, 1, [])
		};
		thighBack = new(new(), thighOffset) {
			new(1, new(Vector2.Zero, -0.53021574f, 1f), []),
			new(81, new(Vector2.Zero, -0.077278495f, 1f), [])
		};
		calfBack = new(new(), new ChildOfOffset<PosRotScale>("thighBack", new(new(74f, 0f), 0f))) {
			new(1, new(Vector2.Zero, 0.38659465f, 1f), []),
			new(81, new(Vector2.Zero, 0.21317196f, 1f), [])
		};
		footBack = new(new(), new ChildOfOffset<PosRotScale>("calfBack", new(new(-54f, 100f), 0f))) {
			new(1, new(Vector2.Zero, -0.31511056f, 1f), []),
			new(81, new(Vector2.Zero, -0.018684626f, 1f), [])
		};
		footBackFrame = new(0) {
			new(1, 1, [])
		};
		events = [];
	}
}
public class D2L2_Land : D2L2Animation {
	public D2L2_Land() {
		gun = new(new(), gunOffset) {
			new(1, new(Vector2.Zero, -0.26547194f, 1f), []),
			new(5, new(new(0f, 6f), -0.23197216f, 1f), []),
			new(54, new(Vector2.Zero, 0.01776874f, 1f), [new SinEaseOut()])
		};
		thighFront = new(new(), thighOffset) {
			new(5, new(Vector2.Zero, 0.035529256f, 1f), []),
			new(54, new(Vector2.Zero, 0.01335752f, 1f), [new SinEaseOut()])
		};
		calfFront = new(new(), new ChildOfOffset<PosRotScale>("thighFront", new(new(74f, 0f), 0f))) {
			new(5, new(Vector2.Zero, 0.38280404f, 1f), []),
			new(7, new(Vector2.Zero, 0.39694715f, 1f), []),
			new(54, new(Vector2.Zero, -0.009625077f, 1f), [new SinEaseOut()])
		};
		footFront = new(new(), new ChildOfOffset<PosRotScale>("calfFront", new(new(-54f, 100f), 0f))) {
			new(1, new(Vector2.Zero, -0.009972572f, 1f), []),
			new(5, new(Vector2.Zero, -0.46617174f, 1f), []),
			new(6, new(Vector2.Zero, -0.47338784f, 1f), []),
			new(7, new(Vector2.Zero, -0.34367657f, 1f), []),
			new(54, new(Vector2.Zero, -0.002026081f, 1f), [new SinEaseOut()])
		};
		footFrontFrame = new(0) {
			new(1, 1, []),
			new(5, 0, []),
			new(40, 0, [])
		};
		thighBack = new(new(), thighOffset) {
			new(5, new(Vector2.Zero, -0.46497536f, 1f), []),
			new(54, new(Vector2.Zero, 0.0837723f, 1f), [new SinEaseBoth()])
		};
		calfBack = new(new(), new ChildOfOffset<PosRotScale>("thighBack", new(new(74f, 0f), 0f))) {
			new(5, new(Vector2.Zero, 0.32001746f, 1f), []),
			new(6, new(Vector2.Zero, 0.41635263f, 1f), []),
			new(54, new(Vector2.Zero, -0.012745738f, 1f), [new Exponential(2f)])
		};
		footBack = new(new(), new ChildOfOffset<PosRotScale>("calfBack", new(new(-54f, 100f), 0f))) {
			new(1, new(Vector2.Zero, 0.44591558f, 1f), []),
			new(5, new(Vector2.Zero, 0.17388475f, 1f), []),
			new(6, new(Vector2.Zero, -0.011266351f, 1f), []),
			new(54, new(Vector2.Zero, -0.067816734f, 1f), [new Exponential(2f)])
		};
		footBackFrame = new(0) {
			new(1, 1, []),
			new(5, 0, []),
			new(7, 1, []),
			new(48, 0, [])
		};
		events = [];
	}
}
public class D2L2_Survey : D2L2Animation {
	public D2L2_Survey() {
		gun = new(new(), gunOffset) {
			new(79, new(Vector2.Zero, -0.6585114f, 1f), [new Exponential(2f)]),
			new(121, new(Vector2.Zero, 0.5128758f, 1f), []),
			new(154, new(Vector2.Zero, -0.13262355f, 1f), []),
			new(191, new(Vector2.Zero, 0.039567232f, 1f), [])
		};
		thighFront = new(new(), thighOffset) {
			new(48, new(Vector2.Zero, 1.2767458f, 1f), []),
			new(51, new(Vector2.Zero, 1.2749019f, 1f), [])
		};
		calfFront = new(new(), new ChildOfOffset<PosRotScale>("thighFront", new(new(74f, 0f), 0f))) {
			new(48, new(Vector2.Zero, -1.6558015f, 1f), []),
			new(51, new(Vector2.Zero, -1.6341233f, 1f), [])
		};
		footFront = new(new(), new ChildOfOffset<PosRotScale>("calfFront", new(new(-54f, 100f), 0f))) {
			new(48, new(Vector2.Zero, 0.3861146f, 1f), []),
			new(51, new(Vector2.Zero, 0.34202564f, 1f), [])
		};
		footFrontFrame = new(0) {
		};
		thighBack = new(new(), thighOffset) {
			new(48, new(Vector2.Zero, 1.23141f, 1f), [])
		};
		calfBack = new(new(), new ChildOfOffset<PosRotScale>("thighBack", new(new(74f, 0f), 0f))) {
			new(48, new(Vector2.Zero, -1.5620223f, 1f), [])
		};
		footBack = new(new(), new ChildOfOffset<PosRotScale>("calfBack", new(new(-54f, 100f), 0f))) {
			new(48, new(Vector2.Zero, 0.29120684f, 1f), [])
		};
		footBackFrame = new(0) {
		};
		events = [];
	}
}
public class D2L2_Stomp_Back : D2L2Animation {
	public D2L2_Stomp_Back() {
		gun = new(new(), gunOffset) {
		};
		thighFront = new(new(), thighOffset) {
			new(22, new(Vector2.Zero, -0.9056177f, 1f), []),
			new(25, new(Vector2.Zero, -0.119954824f, 1f), [new Exponential(2f)]),
			new(31, new(Vector2.Zero, -0.1333272f, 1f), []),
			new(37, new(Vector2.Zero, -0.28042328f, 1f), []),
			new(77, new(Vector2.Zero, 0.009786606f, 1f), [new SinEaseBoth()])
		};
		calfFront = new(new(), new ChildOfOffset<PosRotScale>("thighFront", new(new(74f, 0f), 0f))) {
			new(22, new(Vector2.Zero, 0.5188476f, 1f), []),
			new(25, new(Vector2.Zero, -0.22598588f, 1f), [new Exponential(2f)]),
			new(31, new(Vector2.Zero, -0.18914051f, 1f), []),
			new(37, new(Vector2.Zero, -0.005116105f, 1f), []),
			new(77, new(Vector2.Zero, -0.00023555756f, 1f), [new SinEaseBoth()])
		};
		footFront = new(new(), new ChildOfOffset<PosRotScale>("calfFront", new(new(-54f, 100f), 0f))) {
			new(22, new(Vector2.Zero, 0.37851155f, 1f), []),
			new(25, new(Vector2.Zero, 0.34649706f, 1f), [new Exponential(2f)]),
			new(31, new(Vector2.Zero, 0.3396661f, 1f), []),
			new(37, new(Vector2.Zero, 0.2645254f, 1f), []),
			new(77, new(Vector2.Zero, -0.020270586f, 1f), [new Exponential(2f)])
		};
		footFrontFrame = new(0) {
			new(1, 1, []),
			new(25, 0, []),
			new(31, 0, []),
			new(34, 1, []),
			new(66, 0, [])
		};
		thighBack = new(new(), thighOffset) {
			new(22, new(Vector2.Zero, 0.37503123f, 1f), []),
			new(25, new(Vector2.Zero, 0.47233462f, 1f), []),
			new(31, new(Vector2.Zero, 0.45254993f, 1f), []),
			new(77, new(Vector2.Zero, -0.0024981499f, 1f), [new Exponential(2f)])
		};
		calfBack = new(new(), new ChildOfOffset<PosRotScale>("thighBack", new(new(74f, 0f), 0f))) {
			new(22, new(Vector2.Zero, -0.14165735f, 1f), []),
			new(25, new(Vector2.Zero, -0.0672096f, 1f), []),
			new(31, new(Vector2.Zero, -0.063370526f, 1f), []),
			new(77, new(Vector2.Zero, 0.024928212f, 1f), [new Exponential(2f)])
		};
		footBack = new(new(), new ChildOfOffset<PosRotScale>("calfBack", new(new(-54f, 100f), 0f))) {
			new(22, new(Vector2.Zero, -0.24013591f, 1f), []),
			new(25, new(Vector2.Zero, -0.39336872f, 1f), []),
			new(31, new(Vector2.Zero, -0.37773246f, 1f), []),
			new(77, new(Vector2.Zero, -0.018097997f, 1f), [new Exponential(2f)])
		};
		footBackFrame = new(0) {
		};
		events = [];
	}
}
public class D2L2_Stomp_Front : D2L2Animation {
	public D2L2_Stomp_Front() {
		gun = new(new(), gunOffset) {
		};
		thighFront = new(new(), thighOffset) {
			new(12, new(Vector2.Zero, -1.1782508f, 1f), []),
			new(16, new(Vector2.Zero, 1.405914f, 1f), [new Exponential(2f)]),
			new(22, new(Vector2.Zero, 1.405914f, 1f), []),
			new(28, new(Vector2.Zero, 1.3011019f, 1f), []),
			new(49, new(Vector2.Zero, -0.0003746748f, 1f), [new Exponential(2f)])
		};
		calfFront = new(new(), new ChildOfOffset<PosRotScale>("thighFront", new(new(74f, 0f), 0f))) {
			new(1, new(Vector2.Zero, 0.03597937f, 1f), []),
			new(12, new(Vector2.Zero, 0.43175244f, 1f), []),
			new(14, new(Vector2.Zero, 0.48510098f, 1f), []),
			new(16, new(Vector2.Zero, -0.70028305f, 1f), [new Exponential(2f), new SinEaseBoth()]),
			new(22, new(Vector2.Zero, -0.70028305f, 1f), []),
			new(35, new(Vector2.Zero, 0.0074474216f, 1f), []),
			new(49, new(Vector2.Zero, 0.007778883f, 1f), [])
		};
		footFront = new(new(), new ChildOfOffset<PosRotScale>("calfFront", new(new(-54f, 100f), 0f))) {
			new(12, new(Vector2.Zero, 0.12097025f, 1f), []),
			new(14, new(Vector2.Zero, 0.5720742f, 1f), []),
			new(16, new(Vector2.Zero, -0.72844285f, 1f), [new Exponential(2f)]),
			new(22, new(Vector2.Zero, -0.72844285f, 1f), []),
			new(28, new(Vector2.Zero, -0.20869774f, 1f), []),
			new(35, new(Vector2.Zero, 0.30097374f, 1f), []),
			new(42, new(Vector2.Zero, -0.4453956f, 1f), []),
			new(49, new(Vector2.Zero, -0.06760502f, 1f), [])
		};
		footFrontFrame = new(0) {
			new(2, 1, []),
			new(16, 0, [new Exponential(2f)]),
			new(22, 0, []),
			new(26, 1, []),
			new(42, 0, [])
		};
		thighBack = new(new(), thighOffset) {
			new(12, new(Vector2.Zero, 0.23679745f, 1f), []),
			new(16, new(Vector2.Zero, 0.004606843f, 1f), [new Exponential(2f)]),
			new(22, new(Vector2.Zero, 0.004606843f, 1f), [])
		};
		calfBack = new(new(), new ChildOfOffset<PosRotScale>("thighBack", new(new(74f, 0f), 0f))) {
			new(12, new(Vector2.Zero, -0.14076781f, 1f), []),
			new(16, new(Vector2.Zero, -0.3651837f, 1f), [new SinEaseBoth()]),
			new(22, new(Vector2.Zero, -0.3651837f, 1f), []),
			new(49, new(Vector2.Zero, -0.017724276f, 1f), [])
		};
		footBack = new(new(), new ChildOfOffset<PosRotScale>("calfBack", new(new(-54f, 100f), 0f))) {
			new(12, new(Vector2.Zero, -0.0926435f, 1f), []),
			new(16, new(Vector2.Zero, 0.36702704f, 1f), [new Exponential(2f)]),
			new(22, new(Vector2.Zero, 0.36702704f, 1f), []),
			new(49, new(Vector2.Zero, 0.03156054f, 1f), [])
		};
		footBackFrame = new(0) {
		};
		events = [];
	}
}
#endregion animations