using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using PegasusLib;
using PegasusLib.Graphics;

namespace Origins.Items.Other.Dyes {
	public abstract class Dye_Item : ModItem {
		internal static List<Dye_Item> dyeItems = [];
		public virtual bool UseShaderOnSelf => false;
		public override void SetDefaults() {
			int dye = Item.dye;
			Item.CloneDefaults(ItemID.RedandBlackDye);
			Item.dye = dye;
		}
		public override void Load() {
			dyeItems.Add(this);
		}
		public override void Unload() {
			dyeItems = null;
		}
		public virtual bool AddToDyeTrader(Player player) => true;
		SpriteBatchState lastState;
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (!UseShaderOnSelf) {
				return true;
			}
			lastState = spriteBatch.GetState();
			Main.spriteBatch.Restart(lastState, SpriteSortMode.Immediate);

			DrawData data = new() {
				texture = TextureAssets.Item[Item.type].Value,
				sourceRect = frame,
				position = position,
				color = drawColor,
				rotation = 0f,
				scale = new Vector2(scale),
				shader = Item.dye
			};
			GameShaders.Armor.ApplySecondary(Item.dye, null, data);
			return true;
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (!UseShaderOnSelf) {
				return;
			}
			Main.spriteBatch.Restart(lastState);
		}
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			if (!UseShaderOnSelf) {
				return true;
			}
			lastState = spriteBatch.GetState();
			Main.spriteBatch.Restart(lastState, SpriteSortMode.Immediate, samplerState: SamplerState.AnisotropicWrap);

			DrawData data = new() {
				texture = TextureAssets.Item[Item.type].Value,
				position = Item.position - Main.screenPosition,
				color = lightColor,
				rotation = rotation,
				scale = new Vector2(scale),
				shader = Item.dye
			};
			GameShaders.Armor.ApplySecondary(Item.dye, Main.player[Item.playerIndexTheItemIsReservedFor], data);
			return true;
		}
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
			if (!UseShaderOnSelf) {
				return;
			}
			Main.spriteBatch.Restart(lastState);
		}
	}
	public class DelegatedArmorShaderData : ArmorShaderData {
		public readonly Action<DelegatedArmorShaderData, Entity, DrawData?> setParameters;
		[Obsolete]
		public DelegatedArmorShaderData(Ref<Effect> shader, string passName) : base(shader, passName) {
		}
		public DelegatedArmorShaderData(Asset<Effect> shader, string passName, Action<DelegatedArmorShaderData, Entity, DrawData?> setParameters) : base(shader, passName) {
			this.setParameters = setParameters;
		}
		public override void Apply(Entity entity, DrawData? drawData = null) {
			setParameters(this, entity, drawData);
			base.Apply(entity, drawData);
		}
	}
}
