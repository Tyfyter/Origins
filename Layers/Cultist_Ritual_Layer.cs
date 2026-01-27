using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Accessories;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Cultist_Ritual_Layer : PlayerDrawLayer {
		public static int Offset => 204;
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.shadow == 0 && drawInfo.drawPlayer.OriginPlayer().lunaticsRuneCharge > 0;
		public override Position GetDefaultPosition() => PlayerDrawLayers.BeforeFirstVanillaLayer;
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			OriginPlayer originPlayer = drawInfo.drawPlayer.OriginPlayer();
			float charge = originPlayer.lunaticsRuneCharge / (float)Lunatics_Rune.ChargeThreshold;
			float lunaticsRuneRotation = originPlayer.lunaticsRuneRotation;
			Vector2 position = drawInfo.Position + drawInfo.drawPlayer.Size * 0.5f - Main.screenPosition;
			position.Y += Offset;
			Main.instance.LoadProjectile(ProjectileID.CultistRitual);
			Texture2D ritualTexture = TextureAssets.Projectile[ProjectileID.CultistRitual].Value;
			Color color = Color.White * charge;

			Texture2D ritualExtra = TextureAssets.Extra[ExtrasID.CultistRitual].Value;
			DrawData data = new(ritualTexture, position, null, color, lunaticsRuneRotation, ritualTexture.Size() * 0.5f, float.Pow(charge, 0.5f) * 0.85f, SpriteEffects.None) {
				shader = originPlayer.lunaticsRuneDye
			};
			drawInfo.DrawDataCache.Add(data);

			data.texture = ritualExtra;
			data.origin = ritualExtra.Size() * 0.5f;
			data.rotation = -lunaticsRuneRotation;
			drawInfo.DrawDataCache.Add(data);

			data.texture = ritualTexture;
			data.origin = ritualTexture.Size() * 0.5f;
			data.rotation = lunaticsRuneRotation;
			data.scale *= 0.42f;
			drawInfo.DrawDataCache.Add(data);

			data.texture = ritualExtra;
			data.origin = ritualExtra.Size() * 0.5f;
			data.rotation = -lunaticsRuneRotation;
			drawInfo.DrawDataCache.Add(data);
		}
	}
}
