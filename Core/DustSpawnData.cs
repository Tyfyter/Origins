using System;
using Terraria;

namespace Origins.Core; 
public record struct DustSpawnData(int DustType, int Alpha = 0, Color Color = default, float Scale = 1f, Action<Dust> AfterSpawn = null) {
	public readonly Dust SpawnDust(Vector2 position, int width, int height) {
		Dust dust = Dust.NewDustDirect(position, width, height, DustType, 0f, 0f, Alpha, Color, Scale);
		AfterSpawn?.Invoke(dust);
		return dust;
	}
	public static implicit operator DustSpawnData(int type) => new(type);
}