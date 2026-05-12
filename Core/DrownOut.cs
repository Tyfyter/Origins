using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace Origins.Core; 
public static class DrownOut {
	struct Flag : IMoveToPegFlag { }
	static bool isActive;
	static VolumeData<float> drownOutMultiplier = new(1);
	static VolumeData<ActiveSound> fromSound;
	static VolumeData<float> nextDrownOutMultiplier = new(1);
	static VolumeData<ActiveSound> nextFromSound;
	static void On_SoundPlayer_Update(On_Main.orig_UpdateAudio orig, Main self) {
		using Instance _ = new();
		nextDrownOutMultiplier.Fill(1);
		orig(self);
		drownOutMultiplier.Fill(1);
		for (int i = 0; i < volume_data_length; i++) {
			if (nextDrownOutMultiplier[i] != 1) Apply(nextFromSound[i], nextDrownOutMultiplier[i], i);
		}
	}
	static void On_ActiveSound_Play(Action<ActiveSound> orig, ActiveSound self) {
		using Instance _ = new();
		orig(self);
	}
	public static void Apply(ActiveSound sound, float multiplier) {
		ArgumentNullException.ThrowIfNull(sound);
		for (int i = 0; i < volume_data_length; i++) Apply(sound, multiplier, i);
	}
	public static void Apply(ActiveSound sound, float multiplier, params Span<SoundType> soundTypes) {
		ArgumentNullException.ThrowIfNull(sound);
		for (int i = 0; i < soundTypes.Length; i++) Apply(sound, multiplier, (int)soundTypes[i]);
	}
	static void Apply(ActiveSound sound, float multiplier, int index) {
		if (multiplier < drownOutMultiplier[index]) {
			if (fromSound[index] is not null && fromSound[index].Style.Type == (SoundType)index) fromSound[index].Volume *= drownOutMultiplier[index];
			drownOutMultiplier[index] = multiplier;
			fromSound[index] = sound;
			if (sound.Style.Type == (SoundType)index) sound.Volume /= multiplier;
		}
	}
	public static void ApplyNext(ActiveSound sound, float multiplier) {
		ArgumentNullException.ThrowIfNull(sound);
		for (int i = 0; i < volume_data_length; i++) ApplyNext(sound, multiplier, i);
	}
	public static void ApplyNext(ActiveSound sound, float multiplier, params Span<SoundType> soundTypes) {
		ArgumentNullException.ThrowIfNull(sound);
		for (int i = 0; i < soundTypes.Length; i++) ApplyNext(sound, multiplier, (int)soundTypes[i]);
	}
	static void ApplyNext(ActiveSound sound, float multiplier, int index) {
		ArgumentNullException.ThrowIfNull(sound);
		if (sound == fromSound[index] && sound.Style.Type == (SoundType)index) sound.Volume /= drownOutMultiplier[index];
		if (multiplier < nextDrownOutMultiplier[index]) {
			nextDrownOutMultiplier[index] = multiplier;
			nextFromSound[index] = sound;
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
			s = Main.soundVolume.ScopedOverride(Main.soundVolume * drownOutMultiplier[0]);
			a = Main.ambientVolume.ScopedOverride(Main.ambientVolume * drownOutMultiplier[1]);
			m = Main.musicVolume.ScopedOverride(Main.musicVolume * drownOutMultiplier[2] * drownOutMultiplier[2]);
		}
		public void Dispose() {
			if (!activated) return;
			isActive = false;
			m.Dispose();
			a.Dispose();
			s.Dispose();
		}
	}
	const int volume_data_length = 3;
	[InlineArray(volume_data_length)]
	struct VolumeData<T> {
		T value;
		public VolumeData(T value) {
			Fill(value);
		}
		public void Fill(T value) {
			for (int i = 0; i < volume_data_length; i++) this[i] = value;
		}
	}
	static string ToString<T>(ref this VolumeData<T> data) => $"[{data[0]}, {data[1]}, {data[2]}]";
	struct LoadImpl : IAutoloader {
		public static void Autoload(Mod mod, Type type) {
			On_Main.UpdateAudio += On_SoundPlayer_Update;
			MonoModHooks.Add(typeof(ActiveSound).GetMethod("Play", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance), On_ActiveSound_Play);
		}
	}
}
