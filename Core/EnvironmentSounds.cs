using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Core {
	public static class EnvironmentSounds {
		public static int SoundCount => Sounds.Count;
		static readonly List<AEnvironmentSound> Sounds = [];
		public static TSound Register<TSound>() where TSound : AEnvironmentSound, new() {
			TSound sound = new();
			sound.Register();
			return sound;
		}
		public static void Register(this AEnvironmentSound environmentSound) {
			environmentSound.Register(Sounds.Count);
			Sounds.Add(environmentSound);
		}
		public static void UpdateSounds() => SoundPositions.UpdateSounds();
		public static void ResetSounds(bool forTiles) => SoundPositions.ResetSounds(forTiles);
		[ReinitializeDuringResizeArrays]
		public static class SoundPositions {
			static readonly Vector2?[] positions = new Vector2?[SoundCount];
			static readonly Vector2?[] oldPositions = new Vector2?[SoundCount];
			public static void TrySet(AEnvironmentSound environmentSound, Vector2 position) {
				if (positions[environmentSound.Type] is Vector2 oldPos && position.DistanceSQ(Main.Camera.Center) >= oldPos.DistanceSQ(Main.Camera.Center)) return;
				positions[environmentSound.Type] = position;
			}
			public static Vector2? GetNearest(AEnvironmentSound environmentSound) => positions[environmentSound.Type];
			public static Vector2? GetOldNearest(AEnvironmentSound environmentSound) => oldPositions[environmentSound.Type];
			public static void UpdateSounds() {
				for (int i = 0; i < positions.Length; i++) {
					if (positions[i] is Vector2 pos) Sounds[i].UpdateSound(pos);
				}
				ResetSounds(false);
			}
			public static void ResetSounds(bool forTiles) {
				for (int i = 0; i < positions.Length; i++) {
					if (forTiles != Sounds[i].IsTileSound) continue;
					oldPositions[i] = positions[i];
					positions[i] = null;
				}
			}
		}
	}
	public abstract class AEnvironmentSound {
		public int Type { get; private set; } = -1;
		internal void Register(int index) {
			if (Type != -1) throw new InvalidOperationException($"Cannot register one environment sound multiple times");
			Type = index;
		}
		public virtual bool IsTileSound => true;
		public void TrySetNearest(Vector2 position) => EnvironmentSounds.SoundPositions.TrySet(this, position);
		public Vector2? GetPosition() => EnvironmentSounds.SoundPositions.GetNearest(this) ?? EnvironmentSounds.SoundPositions.GetOldNearest(this);
		public bool IsPlaying() => GetPosition().HasValue;
		public abstract void UpdateSound(Vector2 position);
	}
}
