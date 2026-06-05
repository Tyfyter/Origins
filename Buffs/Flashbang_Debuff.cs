using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Buffs; 
public class Flashbang_Debuff : ModBuff {
	public static LocalizedText descriptionOverride;
	public static int ID { get; private set; }
	public override void SetStaticDefaults() {
		Main.debuff[Type] = true;
		BuffID.Sets.GrantImmunityWith[Type] = [
			BuffID.Darkness,
			BuffID.Obstructed
		];
		ID = Type;
	}
	public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare) {
		if (descriptionOverride is not null) tip = descriptionOverride.Value;
	}
}
internal class Flashbang_Overlay() : Overlay(EffectPriority.VeryHigh, RenderLayers.All), ILoadable {
	public override void Draw(SpriteBatch spriteBatch) {
		int index = Main.LocalPlayer.FindBuffIndex(Flashbang_Debuff.ID);
		if (index < 0) {
			Opacity = 0;
			Flashbang_Debuff.descriptionOverride = null;
			return;
		}
		float str = float.Min(Main.LocalPlayer.buffTime[index] / 45f, 1);
		MathUtils.LinearSmoothing(ref Opacity, float.Pow(str, str + 0.5f), 0.25f);
		spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), OriginClientConfig.Instance.FlashbangColor with { A = 255 } * Opacity);
	}
	public override void Update(GameTime gameTime) { }
	public override void Activate(Vector2 position, params object[] args) {
		Mode = OverlayMode.Active;
	}
	public override void Deactivate(params object[] args) { }
	public override bool IsVisible() => true;
	public static void ForceActive() {
		if (Overlays.Scene[typeof(Flashbang_Overlay).FullName].Mode != OverlayMode.Active) {
			Overlays.Scene.Activate(typeof(Flashbang_Overlay).FullName, default);
		}
	}
	public void Load(Mod mod) {
		Overlays.Scene[GetType().FullName] = this;
	}
	public void Unload() { }
}
