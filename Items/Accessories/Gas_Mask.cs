using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using PegasusLib;
using PegasusLib.Graphics;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Gas_Mask : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public override void Load() {
			On_ItemSlot.isEquipLocked += On_ItemSlot_isEquipLocked;
			On_ItemSlot.SwapVanityEquip += On_ItemSlot_SwapVanityEquip;
			On_ItemSlot.AccCheck += (orig, itemCollection, item, slot) => {
				try {
					accChecking = true;
					return orig(itemCollection, item, slot);
				} finally {
					accChecking = false;
				}
			};
			On_ItemSlot.AccCheck_ForPlayer += (orig, player, itemCollection, item, slot) => {
				try {
					accChecking = true;
					return orig(player, itemCollection, item, slot);
				} finally {
					accChecking = false;
				}
			};
			On_ChatCommandProcessor.CreateOutgoingMessage += On_ChatCommandProcessor_CreateOutgoingMessage;
		}

		static ChatMessage On_ChatCommandProcessor_CreateOutgoingMessage(On_ChatCommandProcessor.orig_CreateOutgoingMessage orig, ChatCommandProcessor self, string text) {
			ChatMessage message = orig(self, text);
			if (message.Text == text && OriginsModIntegrations.CheckAprilFools() && (OriginPlayer.LocalOriginPlayer?.gasMask ?? false)) {
				message.Text = "Are you my mummy?";
			}
			return message;
		}

		static bool accChecking = false;
		void On_ItemSlot_SwapVanityEquip(On_ItemSlot.orig_SwapVanityEquip orig, Item[] inv, int context, int slot, Player player) {
			if (OriginsModIntegrations.CheckAprilFools()) {
				int tSlot = slot - inv.Length / 2;
				if (!(inv[tSlot]?.IsAir ?? false) && inv[tSlot].type == Type) return;
			}
			orig(inv, context, slot, player);
		}

		bool On_ItemSlot_isEquipLocked(On_ItemSlot.orig_isEquipLocked orig, int type) {
			if (!accChecking && OriginsModIntegrations.CheckAprilFools() && type == Type) return true;
			return orig(type);
		}

		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 20);
			Item.defense = 2;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateEquip(Player player) {
			player.OriginPlayer().gasMask = true;
		}
		public override void UpdateItemDye(Player player, int dye, bool hideVisual) {
			player.OriginPlayer().gasMaskDye = dye;
		}
	}
	internal class Gas_Mask_Overlay() : Overlay(EffectPriority.High, RenderLayers.All), ILoadable {
		AutoLoadingAsset<Texture2D> vignette = "Origins/Textures/Vignette";
		public override void Draw(SpriteBatch spriteBatch) {
			MathUtils.LinearSmoothing(ref Opacity, (OriginPlayer.LocalOriginPlayer?.gasMask ?? false).ToInt(), 0.1f);
			if (Opacity == 0) return;
			Color baseColor = Color.Black;
			if (OriginPlayer.LocalOriginPlayer.gasMaskDye != 0) baseColor = Color.Gray;
			Origins.shaderOroboros.Capture(spriteBatch);
			SpriteBatchState state = spriteBatch.GetState();
			spriteBatch.Restart(state, samplerState: SamplerState.LinearClamp);
			spriteBatch.Draw(vignette, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), baseColor * Opacity);
			spriteBatch.Restart(state);
			if (GameShaders.Armor.GetSecondaryShader(OriginPlayer.LocalOriginPlayer.gasMaskDye, Main.LocalPlayer) is ArmorShaderData shader) {
				int dir = Main.LocalPlayer.direction;
				Main.LocalPlayer.direction = 1;
				Origins.shaderOroboros.Stack(shader, Main.LocalPlayer);
				Main.LocalPlayer.direction = dir;
			}
			Origins.shaderOroboros.Release();
		}
		public override void Update(GameTime gameTime) { }
		public override void Activate(Vector2 position, params object[] args) {
			Mode = OverlayMode.Active;
		}
		public override void Deactivate(params object[] args) { }
		public override bool IsVisible() => true;
		public static void ForceActive() {
			if (Overlays.Scene[typeof(Gas_Mask_Overlay).FullName].Mode != OverlayMode.Active) {
				Overlays.Scene.Activate(typeof(Gas_Mask_Overlay).FullName, default);
			}
		}
		public void Load(Mod mod) {
			Overlays.Scene[GetType().FullName] = this;
		}
		public void Unload() { }
	}
}
