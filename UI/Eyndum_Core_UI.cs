using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using static Tyfyter.Utils.UITools;
using Origins.Items.Accessories.Eyndum_Cores;

namespace Origins.UI {
	public class Eyndum_Core_UI : SingleItemSlotUI {

		public override float SlotX => Main.screenWidth - 64 - 28 - 142 - (Main.netMode == NetmodeID.MultiplayerClient ? 38 : 0);
		public override float SlotY => (174 + (!Main.mapFullscreen && Main.mapStyle == 1 ? 204 : 0)) + (1 * 56) * 0.85f;
		public bool hasSetBonus = true;
		public override void OnInitialize() {
			// Ensures that the player's core slot item is not null, then adds the slot
			ref Ref<Item> item = ref Main.LocalPlayer.GetModPlayer<OriginPlayer>().eyndumCore;
			if (item is null) {
				item = new Ref<Item>(new Item());
				item.Value.SetDefaults(ItemID.None);
			}
			SetItemSlot(
				item,
				new Vector2(SlotX, SlotY),
				_ValidItemFunc: (i) => (i?.IsAir ?? true) || i.ModItem is Eyndum_Core,
				slotColor: new Color(50, 106, 46, 220),
				shiftClickToInventory: true,
				extraTextures: (Origins.eyndumCoreUITexture, new Color(50, 106, 46, 160))
			);
		}
		public override void Update(GameTime gameTime) {
			itemSlot.slotSourceMissing = !hasSetBonus;
			base.Update(gameTime);
			hasSetBonus = true;
		}
	}
}