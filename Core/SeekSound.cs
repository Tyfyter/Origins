using Microsoft.Xna.Framework.Audio;
using PegasusLib.Reflection;
using ReLogic.Utilities;
using System;
using System.Reflection;
using System.Reflection.Emit;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace Origins.Core; 
public static class SeekSound {
	/// <summary>
	/// Attempts to play a sound with the provided sound style, and returns a valid <see cref="T:ReLogic.Utilities.SlotId" /> handle to it on success.
	/// </summary>
	/// <param name="style"> The sound style that describes everything about the played sound. </param>
	/// <param name="position"> An optional 2D position to play the sound at. When null, this sound will be heard everywhere. </param>
	/// <param name="updateCallback"> A callback for customizing the behavior of the created sound instance, like tying its existence to a projectile using <see cref="T:Terraria.Audio.ProjectileAudioTracker" />. </param>
	public static SlotId PlaySound(in SoundStyle style, TimeSpan seek, Vector2? position = null, SoundUpdateCallback updateCallback = null) {
		if (Main.dedServ || !SoundEngine.IsAudioSupported) {
			return SlotId.Invalid;
		}
		SoundEffect soundEffect = style.GetSoundEffect();
		TimeSpan duration = soundEffect.Duration;
		if (seek >= duration) return SlotId.Invalid;
		return PlaySound(style, (uint)(seek.Divide(duration) * soundEffect.GetHandle().PlayLength), position, updateCallback);
	}
	/// <summary>
	/// Attempts to play a sound with the provided sound style, and returns a valid <see cref="T:ReLogic.Utilities.SlotId" /> handle to it on success.
	/// </summary>
	/// <param name="style"> The sound style that describes everything about the played sound. </param>
	/// <param name="position"> An optional 2D position to play the sound at. When null, this sound will be heard everywhere. </param>
	/// <param name="updateCallback"> A callback for customizing the behavior of the created sound instance, like tying its existence to a projectile using <see cref="T:Terraria.Audio.ProjectileAudioTracker" />. </param>
	public static SlotId PlaySound(in SoundStyle style, uint seekSamples, Vector2? position = null, SoundUpdateCallback updateCallback = null) {
		if (Main.dedServ || !SoundEngine.IsAudioSupported) {
			return SlotId.Invalid;
		}
		if (seekSamples >= style.GetSoundEffect().GetHandle().PlayLength) return SlotId.Invalid;
		uint? oldVal = Handler.overridePlayBegin;
		try {
			Handler.overridePlayBegin = seekSamples;
			return SoundPlayer_Play(in style, position, updateCallback);
		} finally {
			Handler.overridePlayBegin = oldVal;
		}
	}
	static SlotId SoundPlayer_Play(in SoundStyle style, Vector2? position = null, SoundUpdateCallback updateCallback = null) {
		if (Main.dedServ) {
			return SlotId.Invalid;
		}
		if (position.HasValue && Vector2.DistanceSquared(Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2), position.Value) > 100000000f) {
			return SlotId.Invalid;
		}
		if (style.PlayOnlyIfFocused && !Main.hasFocus) {
			return SlotId.Invalid;
		}
		if (!Program.IsMainThread) {
			SoundStyle styleCopy = style;
			return Main.RunOnMainThread(() => Play_Inner(in styleCopy, position, updateCallback)).GetAwaiter().GetResult();
		}
		return Play_Inner(in style, position, updateCallback);
	}
	static SlotId Play_Inner(in SoundStyle style, Vector2? position, SoundUpdateCallback updateCallback) {
		SoundStyle chosenStyle = style.WithSelectedVariant();
		int maxInstances = chosenStyle.MaxInstances;
		if (maxInstances > 0) {
			int attempts = ((!chosenStyle.LimitsArePerVariant) ? 1 : (chosenStyle.RerollAttempts + 1));
			for (int attempt = 0; attempt < attempts; attempt++) {
				bool tryAgain = false;
				int instanceCount = 0;
				foreach (SlotVector<ActiveSound>.ItemPair item in Reflection._trackedSounds.GetValue(SoundEngine.SoundPlayer)) {
					ActiveSound activeSound = item.Value;
					if (activeSound.IsPlaying && chosenStyle.IsTheSameAs(activeSound.Style) && ++instanceCount >= maxInstances) {
						if (attempt + 1 < attempts) {
							tryAgain = true;
							chosenStyle = style.WithSelectedVariant();
							break;
						}
						if (chosenStyle.SoundLimitBehavior != SoundLimitBehavior.ReplaceOldest) {
							return SlotId.Invalid;
						}
						activeSound.Sound?.Stop(immediate: true);
					}
				}
				if (!tryAgain) {
					break;
				}
			}
		}
		if (chosenStyle.UsesMusicPitch()) {
			chosenStyle.Pitch += Main.musicPitch;
		}

		ActiveSound value = new(chosenStyle, position, updateCallback);
		return Reflection._trackedSounds.GetValue(SoundEngine.SoundPlayer).Add(value);
	}
	class Handler : ILoadable {
		public static uint? overridePlayBegin;
		public void Load(Mod mod) {
			On_ActiveSound.Play += On_ActiveSound_Play;
		}
		static void On_ActiveSound_Play(On_ActiveSound.orig_Play orig, ActiveSound self) {
			if (Handler.overridePlayBegin is not uint overridePlayBegin || self.Sound is DynamicSoundEffectInstance) {
				orig(self);
				return;
			}
			ref FAudio.FAudioBuffer audioBuffer = ref self.Style.GetSoundEffect().GetHandle();
			using (audioBuffer.PlayLength.ScopedOverride(audioBuffer.PlayLength - (overridePlayBegin - audioBuffer.PlayBegin))) {
				using (audioBuffer.PlayBegin.ScopedOverride(overridePlayBegin)) {
					orig(self);
				}
			}
		}
		public void Unload() { }
	}
	class Reflection : ReflectionLoader {
		[ReflectionParentType(typeof(SoundStyle))]
		public static _WithSelectedVariant WithSelectedVariant;
		public delegate SoundStyle _WithSelectedVariant(in SoundStyle style, int? random = null);
		[ReflectionParentType(typeof(SoundStyle))]
		public static _UsesMusicPitch get_UsesMusicPitch;
		public delegate bool _UsesMusicPitch(in SoundStyle style);
		public static FastFieldInfo<SoundPlayer, SlotVector<ActiveSound>> _trackedSounds;
		public override void OnLoad() {
			WithSelectedVariant = typeof(SoundStyle).GetMethod(nameof(WithSelectedVariant), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).CreateDelegate<_WithSelectedVariant>();
			get_UsesMusicPitch = typeof(SoundStyle).GetProperty("UsesMusicPitch", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetMethod.CreateDelegate<_UsesMusicPitch>();
		}
	}
	static readonly HandleGetter getHandle = PegasusLib.PegasusLib.Compile<HandleGetter>(nameof(getHandle),
		(OpCodes.Ldarg_0, null),
		(OpCodes.Ldflda, typeof(SoundEffect).GetField("handle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)),
		(OpCodes.Ret, null)
	);
	public delegate ref FAudio.FAudioBuffer HandleGetter(SoundEffect effect);
	static bool UsesMusicPitch(in this SoundStyle style) => Reflection.get_UsesMusicPitch(in style);
	static SoundStyle WithSelectedVariant(in this SoundStyle style, int? random = null) => Reflection.WithSelectedVariant(in style, random);
	static ref FAudio.FAudioBuffer GetHandle(this SoundEffect effect) => ref getHandle(effect);
}
