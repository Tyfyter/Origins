using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;

using Origins.Dev;
namespace Origins.Items.Weapons.Ranged {
	public class Broken_Fiberglass_Bow : AnimatedModItem, IElementalItem, ICustomWikiStat {
        public string[] Categories => [
            "Bow"
        ];
        public ushort Element => Elements.Fiberglass;
		protected override bool CloneNewInstances => true;
		int strung = 0;
		const int strungMax = 50;
		static DrawAnimationManual animation;
		public override DrawAnimation Animation {
			get {
				animation.Frame = strung > 0 ? 1 : 0;
				return animation;
			}
		}
		public override void SetStaticDefaults() {
			animation = new DrawAnimationManual(2);
			Main.RegisterItemAnimation(Item.type, animation);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 17;
			Item.DamageType = DamageClass.Ranged;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.width = 18;
			Item.height = 36;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 1;
			Item.shootSpeed = 9;
			Item.autoReuse = false;
			Item.useAmmo = AmmoID.Arrow;
			Item.shoot = ProjectileID.WoodenArrowFriendly;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item5;
		}
		public override void LoadData(TagCompound tag) {
			strung = tag.GetInt("strung");
		}
		public override void SaveData(TagCompound tag)/* Edit tag parameter rather than returning new TagCompound */ {
			tag.Add("strung", strung);
		}
		public override void HoldItem(Player player) {
			if (player.itemAnimation != 0) player.GetModPlayer<OriginPlayer>().itemLayerWrench = true;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-0.5f, 0);
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override bool CanUseItem(Player player) {
			if (player.altFunctionUse == 2) {
				if (strung < strungMax) if (player.ConsumeItem(ItemID.VineRope)) {
						strung += strung > 0 ? 2 : 1;
					} else if (player.ConsumeItem(ItemID.Vine) && strung < strungMax - 10) {
						strung += 25;
					}
				Item.noUseGraphic = true;
				Item.useStyle = ItemUseStyleID.Guitar;
				Item.shoot = ProjectileID.None;
				Item.UseSound = null;
				if (strung > strungMax) strung = strungMax;
				return true;
			}
			if (strung <= 0) return false;
			SetDefaults();
			strung--;
			if (strung <= 0) {
				SoundEngine.PlaySound(SoundID.Item102.WithPitch(1).WithVolume(0.75f), player.Center);
				Vector2 pos = player.Center + (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero) * (10 - player.direction * 2);
				Gore.NewGoreDirect(player.GetSource_ItemUse(Item), pos, player.velocity, ModContent.GoreType<Gores.NPCs.FG2_Gore>()).position = pos;
			}
			return base.CanUseItem(player);
		}
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return player.altFunctionUse != 2;
		}
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			Texture2D texture = TextureAssets.Item[Item.type].Value;
			spriteBatch.Draw(texture, position, Animation.GetFrame(texture), drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
			return false;
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (Main.playerInventory) return;
			float inventoryScale = Main.inventoryScale;
			string str = strung.ToString();
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, str, position + new Vector2(16f + str.Length, -4f) * scale, Colors.RarityNormal, 0f, Vector2.Zero, new Vector2(scale * 0.8f), -1f, scale);
		}
	}
}
