using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Ravel : ModItem {
        public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Ravel");
            Tooltip.SetDefault("Double tap down to transform into a small, rolling ball");
            SacrificeTotal = 1;
            ID = Type;
        }
        public override void SetDefaults() {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 8);
            Item.shoot = ModContent.MountType<Ravel_Mount>();//can't use mountType because that'd make it fit in the mount slot
        }
        public override void UpdateEquip(Player player) {
            const int down = 0;
            bool toggle = player.controlDown && player.releaseDown && player.doubleTapCardinalTimer[down] < 15;
            if (player.mount.Type == Item.shoot) {
                UpdateRaveled(player);
                if (toggle) player.mount.Dismount(player);
            } else {
                if (toggle) player.mount.SetMount(Item.shoot, player);
            }
        }
        protected virtual void UpdateRaveled(Player player) {}
		public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player) {
            return equippedItem.ModItem is not Ravel;
		}
	}
    public class Ravel_Mount : ModMount {
		public override string Texture => "Origins/Items/Accessories/Ravel";
        public static int ID { get; private set; } = -1;
        protected virtual void SetID() {
            MountData.buff = ModContent.BuffType<Ravel_Mount_Buff>();
            ID = Type;
        }
        public override void SetStaticDefaults() {
            // Movement
            MountData.jumpHeight = 8; // How high the mount can jump.
            MountData.acceleration = 0.19f; // The rate at which the mount speeds up.
            MountData.jumpSpeed = 8f; // The rate at which the player and mount ascend towards (negative y velocity) the jump height when the jump button is pressed.
            MountData.blockExtraJumps = true; // Determines whether or not you can use a double jump (like cloud in a bottle) while in the mount.
            MountData.constantJump = false; // Allows you to hold the jump button down.
            MountData.fallDamage = 0.5f; // Fall damage multiplier.
            MountData.runSpeed = 8f; // The speed of the mount
            MountData.dashSpeed = 8f; // The speed the mount moves when in the state of dashing.
            MountData.flightTimeMax = 0; // The amount of time in frames a mount can be in the state of flying.
            MountData.heightBoost = -22;

            // Misc
            MountData.fatigueMax = 0;
            //MountData.buff = Ravel_Mount_Buff.ID; // The ID number of the buff assigned to the mount.

            MountData.totalFrames = 1; // Amount of animation frames for the mount
            MountData.playerYOffsets = new int[] { -22 };
            SetID();
        }
		public override void UpdateEffects(Player player) {
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            originPlayer.mountOnly = true;
            originPlayer.ravel = true;
        }
		public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity) {
            const float factor = 10f / 12f;
            mountedPlayer.mount._frameCounter += velocity.X * factor;
            return false;
		}
		public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
            //playerDrawData.Clear();
            rotation = drawPlayer.mount._frameCounter * 0.1f;
            texture = Terraria.GameContent.TextureAssets.Item[Ravel.ID].Value;
            drawOrigin = new Vector2(12, 12);
            DrawData item = new DrawData(texture, drawPosition, null, drawColor, rotation, drawOrigin, drawScale, spriteEffects, 0);
            item.shader = Mount.currentShader;
            playerDrawData.Add(item);
            return false;
		}
    }
    public class Ravel_Mount_Buff : ModBuff {
        public override string Texture => "Terraria/Images/Buff_160";
        protected virtual int MountID => ModContent.MountType<Ravel_Mount>();
        public override void SetStaticDefaults() {
            BuffID.Sets.BasicMountData[Type] = new BuffID.Sets.BuffMountData() {
                mountID = MountID
            };
            Main.buffNoTimeDisplay[Type] = true;
        }
		public override void Update(Player player, ref int buffIndex) {
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            originPlayer.changeSize = true;
            originPlayer.targetWidth = 20;
            originPlayer.targetHeight = 20;
        }
	}
}
