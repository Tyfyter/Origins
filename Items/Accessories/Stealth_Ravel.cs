using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Stealth_Ravel : Ravel {
        public static new int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Stealth Ravel");
            Tooltip.SetDefault("Double tap down to transform into a small, rolling ball\nEnemies are less likely to target you when raveled");
            SacrificeTotal = 1;
            ID = Type;
        }
        public override void SetDefaults() {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 8);
            Item.shoot = ModContent.MountType<Stealth_Ravel_Mount>();//can't use mountType because that'd make it fit in the mount slot
        }
		protected override void UpdateRaveled(Player player) {
            player.aggro -= 400;
            player.blackBelt = true;
        }
		public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.PutridScent);
            recipe.AddIngredient(Ravel.ID);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
		}
	}
    public class Stealth_Ravel_Mount : Ravel_Mount {
        public override string Texture => "Origins/Items/Accessories/Stealth_Ravel";
        public static new int ID { get; private set; } = -1;
        protected override void SetID() {
            MountData.buff = ModContent.BuffType<Stealth_Ravel_Mount_Buff>();
            ID = Type;
        }
        public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity) {
            OriginPlayer originPlayer = mountedPlayer.GetModPlayer<OriginPlayer>();
            const float factor = 10f / 12f;
            if (originPlayer.ceilingRavel) {
                mountedPlayer.mount._frameCounter -= velocity.X * factor;
            } else {
                mountedPlayer.mount._frameCounter += velocity.X * factor;
                if (originPlayer.collidingX) {
                    mountedPlayer.mount._frameCounter -= velocity.Y * originPlayer.oldXSign * factor;
                }
            }
            return false;
        }
        public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
            //playerDrawData.Clear();
            rotation = drawPlayer.mount._frameCounter * 0.1f;
            texture = Terraria.GameContent.TextureAssets.Item[Stealth_Ravel.ID].Value;
            drawOrigin = new Vector2(12, 12);
            DrawData item = new DrawData(texture, drawPosition, null, drawColor, rotation, drawOrigin, drawScale, spriteEffects, 0);
            item.shader = Mount.currentShader;
            playerDrawData.Add(item);
            return false;
        }
    }
    public class Stealth_Ravel_Mount_Buff : Ravel_Mount_Buff {
        public override string Texture => "Origins/Buffs/Ravel_Generic_Buff";
        protected override int MountID => ModContent.MountType<Stealth_Ravel_Mount>();
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Stealth Ravel");
            Description.SetDefault("10% chance to dodge. Less likely to be targeted");
			base.SetStaticDefaults();
        }
    }
}
