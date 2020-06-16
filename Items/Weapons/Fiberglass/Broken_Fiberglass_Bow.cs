using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;

namespace Origins.Items.Weapons.Fiberglass {
	public class Broken_Fiberglass_Bow : ModItem, IAnimatedItem {
        int strung = 0;
        const int strungMax = 50;
        static DrawAnimationManual animation;
        public DrawAnimation Animation => animation;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Unstrung Fiberglass Bow");
			Tooltip.SetDefault("Not very useful without a bowstring\nMaybe you could find something to replace it");
            animation = new DrawAnimationManual(2);
			Main.RegisterItemAnimation(item.type, animation);
		}
		public override void SetDefaults() {
			item.damage = 17;
			item.ranged = true;
			item.noMelee = true;
			item.noUseGraphic = false;
			item.width = 18;
			item.height = 36;
			item.useTime = 20;
			item.useAnimation = 20;
			item.useStyle = 5;
			item.knockBack = 1;
			item.value = 5000;
			item.shootSpeed = 9;
			item.autoReuse = false;
            item.useAmmo = AmmoID.Arrow;
            item.shoot = ProjectileID.WoodenArrowFriendly;
			item.rare = ItemRarityID.Green;
			item.UseSound = SoundID.Item5;
		}
        public override void Load(TagCompound tag) {
            strung = tag.GetInt("strung");
        }
        public override TagCompound Save() {
            return new TagCompound() {{"strung", strung}};
        }
        public override void HoldItem(Player player) {
            if(player.itemAnimation!=0)player.GetModPlayer<OriginPlayer>().ItemLayerWrench = true;
        }
        public override Vector2? HoldoutOffset() {
            return new Vector2(-0.5f,0);
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override bool CanUseItem(Player player) {
            if(player.altFunctionUse == 2) {
                if(strung<strungMax)if(player.ConsumeItem(ItemID.VineRope)) {
                    strung+=strung>0 ? 2 : 1;
                }else if(player.ConsumeItem(ItemID.Vine)&&strung<strungMax-10) {
                    strung+=25;
                }
			    item.noUseGraphic = true;
			    item.useTime = 20;
			    item.useAnimation = 20;
			    item.useStyle = 1;
                item.shoot = ProjectileID.None;
			    item.UseSound = null;
                if(strung>strungMax)strung = strungMax;
                return true;
            }
            animation.Frame = strung>0 ? 1 : 0;
            if(strung<=0)return false;
            SetDefaults();
            strung--;
            return base.CanUseItem(player);
        }
        public override bool ConsumeAmmo(Player player) {
            return player.altFunctionUse != 2;
        }
        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            if(Main.playerInventory)return;
            float inventoryScale = Main.inventoryScale;
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, strung.ToString(), position + new Vector2(8f, -4f) * inventoryScale, Colors.RarityNormal, 0f, Vector2.Zero, new Vector2(inventoryScale * 0.8f), -1f, inventoryScale);
        }
    }
}
