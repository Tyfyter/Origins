using System;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace Origins.Core; 
public static class DrownOut {
	struct Flag : IMoveToPegFlag { }
	static bool isActive;
	static float drownOutMultiplier = 1;
	static ActiveSound fromSound;
	static float nextDrownOutMultiplier = 1;
	static ActiveSound nextFromSound;
	static void On_SoundPlayer_Update(On_Main.orig_UpdateAudio orig, Main self) {
		using Instance _ = new();
		nextDrownOutMultiplier = 1;
		orig(self);
		drownOutMultiplier = 1;
		if (nextDrownOutMultiplier != 1) Apply(nextFromSound, nextDrownOutMultiplier);
	}
	static void On_ActiveSound_Play(Action<ActiveSound> orig, ActiveSound self) {
		using Instance _ = new();
		orig(self);
	}
	public static void Apply(ActiveSound sound, float multiplier) {
		ArgumentNullException.ThrowIfNull(sound);
		if (multiplier < drownOutMultiplier) {
			if (fromSound is not null) fromSound.Volume *= drownOutMultiplier;
			drownOutMultiplier = multiplier;
			fromSound = sound;
			sound.Volume /= multiplier;
		}
	}
	public static void ApplyNext(ActiveSound sound, float multiplier) {
		ArgumentNullException.ThrowIfNull(sound);
		if (sound == fromSound) sound.Volume /= drownOutMultiplier;
		if (multiplier < nextDrownOutMultiplier) {
			nextDrownOutMultiplier = multiplier;
			nextFromSound = sound;
		}
	}
	readonly ref struct Instance : IAutoload<LoadImpl> {
		readonly ScopedOverride<float> s;
		readonly ScopedOverride<float> a;
		readonly ScopedOverride<float> m;
		readonly bool activated;
		public Instance() {
			if (!isActive.TrySet(true)) return;
			activated = true;
			s = Main.soundVolume.ScopedOverride(Main.soundVolume * drownOutMultiplier);
			a = Main.ambientVolume.ScopedOverride(Main.ambientVolume * drownOutMultiplier);
			m = Main.musicVolume.ScopedOverride(Main.musicVolume * drownOutMultiplier * drownOutMultiplier);
		}
		public void Dispose() {
			if (!activated) return;
			isActive = false;
			m.Dispose();
			a.Dispose();
			s.Dispose();
		}
	}
	struct LoadImpl : IAutoloader {
		public static void Autoload(Mod mod, Type type) {
			On_Main.UpdateAudio += On_SoundPlayer_Update;
			MonoModHooks.Add(typeof(ActiveSound).GetMethod("Play", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance), On_ActiveSound_Play);
		}
	}
}
