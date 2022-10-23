using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using System;
using System.Linq;
using static Tyfyter.Utils.UITools;
using Origins.Items.Accessories.Eyndum_Cores;
using Origins.World;
using Terraria.GameContent;
using Terraria.UI.Gamepad;
using Terraria.DataStructures;

namespace Origins.UI {
	public class Journal_UI_Open : UIState {
		public static AutoCastingAsset<Texture2D> BackTexture;
		public static AutoCastingAsset<Texture2D> PageTexture;
		UIElement baseElement;
		public override void OnInitialize() {
			this.RemoveAllChildren();
			baseElement = new UIElement();
			baseElement.Width.Set(0f, 0.875f);
			baseElement.MaxWidth.Set(900f, 0f);
			baseElement.MinWidth.Set(700f, 0f);
			baseElement.Top.Set(190f, 0f);
			baseElement.Height.Set(-310f, 1f);
			baseElement.HAlign = 0.5f;
			Append(baseElement);
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			SpriteBatchState spriteBatchState = spriteBatch.GetState();
			spriteBatch.Restart(spriteBatchState, samplerState: SamplerState.PointClamp);
			Rectangle bounds = baseElement.GetDimensions().ToRectangle();
			spriteBatch.Draw(BackTexture, bounds, Color.White);
			spriteBatch.Draw(PageTexture, bounds, Color.White);
			spriteBatch.Restart(spriteBatchState);
		}
	}
}