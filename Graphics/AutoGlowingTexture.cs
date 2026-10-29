using Microsoft.Xna.Framework.Graphics;

namespace Origins.Graphics; 
public struct AutoGlowingTexture(string asset) : IUnloadable, IBatchLoadable {
	AutoLoadingTexture texture = asset;
	AutoLoadingTexture glowTexture = asset + "_Glow";
	public Texture2D Texture => texture.Value;
	public Texture2D GlowTexture => glowTexture.Value;
	public static implicit operator AutoGlowingTexture(string asset) => new(asset);
	void IBatchLoadable.Load() {
		texture.LoadAsset();
		glowTexture.LoadAsset();
	}
	void IUnloadable.Unload() {
		texture.Unload();
		glowTexture.Unload();
	}
	void IBatchLoadable.Wait() {
		texture.Wait();
		glowTexture.Wait();
	}
}
