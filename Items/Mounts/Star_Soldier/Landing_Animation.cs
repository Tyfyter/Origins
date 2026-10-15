#define EditAnimation
using Microsoft.Xna.Framework.Graphics;
using Origins.Core.Animation;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using static Origins.Core.Animation.KeyframeModifiers;
using static Origins.Core.Animation.KeyframeTypes;

namespace Origins.Items.Mounts.Star_Soldier;

public class Landing_Animation : KeyframeAnimation {
	public static Landing_Animation instance = new();
	public KeyframeSet<AnimationOffsets> wholeOffsetAnimation = new(default) {
		new(3, new(new(23.25f, 76.25f), 1.5895625f), [new SinEaseIn(), new Exponential(2f)]),
		new(11, new(new(23.118313f, 60.80384f), 1.5656489f), []),
		new(30, new(new(17.25f, 21.75f), 0.5f), [new Exponential(2f)]),
		new(45, new(new(4.5f, 6f), 0f), [new Exponential(0.5f)])
	};
	public KeyframeSet<AnimationOffsets> bothLegsAnimation = new(new(Vector2.Zero, 0.69666004f), new ChildOfOffset<AnimationOffsets>("wholeOffsetAnimation", new(new(-16f, 32f), 0f))) {
		new(3, new(new(0f, -8f), -0.029644685f), []),
		new(5, new(new(-13.5f, -4.9561043f), -1.6404033f), []),
		new(28, new(new(-4.8599997f, -1.7841973f), -0.7268256f), []),
		new(30, new(Vector2.Zero, -0.5f), [new Exponential(2f)]),
		new(45, default, [new Exponential(0.5f)])
	};
	public KeyframeSet<AnimationOffsets> backLegOffsetAnimation = new(default, new ChildOfOffset<AnimationOffsets>("bothLegsAnimation")) {
	};
	public KeyframeSet<AnimationOffsets> frontLegOffsetAnimation = new(default, new ChildOfOffset<AnimationOffsets>("bothLegsAnimation")) {
		new(5, new(new(0f, 5.25f), 0f), []),
		new(16, new(new(-12f, 3f), 0f), [])
	};
	public KeyframeSet<BackLegFrame> backLegFrameAnimation = new(0) {
		new(8, 4, new StepInterpolation<BackLegFrame>())
	};
	public KeyframeSet<FrontLegFrame> frontLegFrameAnimation = new(0) {
		new(5, 5, []),
		new(14, 7, []),
		new(17, 8, []),
		new(21, 4, []),
		new(26, 3, []),
		new(31, 2, []),
		new(35, 1, []),
		new(39, 0, []),
		new(42, 18, []),
		new(45, 9, [])
	};
	public KeyframeSet<Rotation> shoulderRotation = new(0f, new ChildOfOffset<AnimationOffsets>("wholeOffsetAnimation", new(new(-4f, -20f), -2.3127437f))) {
		new(1, -1.4165652f, []),
		new(2, -1.2525352f, []),
		new(3, -7.006748f, []),
		new(15, -7.3662834f, []),
		new(22, -7.042245f, []),
		new(33, -6.461565f, []),
		new(45, -6.3109245f, [])
	};
	public KeyframeSet<Rotation> forearmRotation = new(0f, new ChildOfOffset<Rotation>("shoulderRotation", new(new(0f, -32.6f), -1.6380026f))) {
		new(1, -0.5084028f, []),
		new(3, -1.289268f, []),
		new(15, -1.0435611f, []),
		new(22, -0.41986412f, []),
		new(33, 0.011934757f, []),
		new(45, 0.008013248f, [])
	};
	public KeyframeSet<Rotation> gunRotation = new(0f, new ChildOfOffset<Rotation>("forearmRotation", new(new(0f, -25.6f), -0.8091537f))) {
		new(1, 5.3982205f, []),
		new(3, 4.687042f, []),
		new(15, 4.950278f, []),
		new(22, 5.6677237f, []),
		new(33, 6.412785f, []),
		new(45, 6.296839f, [])
	};
	public EventKeyframeSet<EventParams> exampleEventTimeline = [
		new(1, LandingSound),
		new(3, HidePlayer),
		new(37, PleaseNameThisSound)
	];


	static void LandingSound(in EventParams parameters) {
		SoundEngine.PlaySound(Origins.Sounds.StarLand, parameters.Player.Bottom);
	}
	static void HidePlayer(in EventParams parameters) {
		parameters.WagonHandler.hidePlayer = true;
	}
	static void PleaseNameThisSound(in EventParams parameters) {
		SoundEngine.PlaySound(Origins.Sounds.TrenchmakerStep.WithPitchRange(0.5f, 0.7f).WithVolume(0.3f), parameters.Player.Bottom);
		SoundEngine.PlaySound(SoundID.Item88.WithPitchRange(1.6f, 1.9f).WithVolume(0.1f), parameters.Player.Bottom);
	}
	public record struct EventParams(Player Player, Star_Soldier_Wagon.MountHandler WagonHandler);
	public record struct AnimationOffsets(Vector2 Position, float Rotation) : KeyframeSet<AnimationOffsets>.ITypeHandler, IKeyframeOffsetModifier {
		public Vector2 Position = Position;
		public float Rotation = Rotation;
		public static KeyframeSet<AnimationOffsets>.IInterpolation Linear { get; } = new Interpolation();
		public readonly struct Interpolation : KeyframeSet<AnimationOffsets>.IInterpolation {
			readonly AnimationOffsets KeyframeSet<AnimationOffsets>.IInterpolation.Interpolate(AnimationOffsets prevValue, AnimationOffsets nextValue, float progress) =>
				new(
					Vector2.Lerp(prevValue.Position, nextValue.Position, progress),
					float.Lerp(prevValue.Rotation, nextValue.Rotation, progress)
				);
			public readonly override string ToString() => "Interpolated";
		}
		public static AnimationOffsets operator +(AnimationOffsets a, AnimationOffsets b) => new(a.Position + b.Position, a.Rotation + b.Rotation);
		public static void DrawGizmo(SpriteBatch spriteBatch, ref AnimationOffsets value, ref GizmoTracker gizmoTracker, GizmoOffset offset) {
			KeyframeTypes.Vec2Interpolation.DrawGizmo(spriteBatch, ref value.Position, ref gizmoTracker, offset);
			KeyframeTypes.DrawRotationGizmo(spriteBatch, ref value.Rotation, ref gizmoTracker, offset, 32);
		}
		public readonly void ModifyGizmoOffset(ref GizmoOffset gizmoOffset) => gizmoOffset += new GizmoOffset(Position, Rotation);
		public static string Export(AnimationOffsets value) => value == default ? "default" : $"new({KeyframeTypes.Vec2Interpolation.Export(value.Position)}, {value.Rotation}f)";
	}
	public record struct BackLegFrame(int Frame) : IAnimatableSpriteFrame<BackLegFrame> {
		public static Texture2D Texture => Star_Soldier.backLegTexture;
		public static int FrameCount => Star_Soldier.LegTextureFrames;
		public static implicit operator BackLegFrame(int value) => new(value);
		public static implicit operator int(BackLegFrame value) => value.Frame;
	}
	public record struct FrontLegFrame(int Frame) : IAnimatableSpriteFrame<FrontLegFrame> {
		public static Texture2D Texture => Star_Soldier.frontLegTexture;
		public static int FrameCount => Star_Soldier.LegTextureFrames;
		public static implicit operator FrontLegFrame(int value) => new(value);
		public static implicit operator int(FrontLegFrame value) => value.Frame;
	}
}