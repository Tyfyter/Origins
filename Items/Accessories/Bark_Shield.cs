using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using PegasusLib;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Shield)]
	public class Bark_Shield : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public virtual int MaxDurability => 65;
		public int durability;
		public override void SetDefaults() {
			Item.DefaultToAccessory(36, 38);
			Item.damage = 10;
			Item.knockBack = 0.5f;
			Item.defense = 3;
			durability = MaxDurability;
			Item.shoot = ModContent.ProjectileType<Bark_Shield_Splinters>();
			Item.value = Item.sellPrice(copper: 3);
			Item.rare = ItemRarityID.White;
		}
		public override void UpdateEquip(Player player) {
			player.noKnockback = true;
			player.GetModPlayer<OriginPlayer>().barkShieldItem = Item;
		}
		public virtual int CalculateDamage(Player.HurtInfo info) => info.Damage;
		public virtual void Break(Player player, Player.HurtInfo info) {
			// todo: add sound
			int damage = player.GetWeaponDamage(Item);
			float knockback = player.GetWeaponKnockback(Item);
			for (int i = Main.rand.Next(3, 6); i > 0; i--) {
				player.SpawnProjectile(
					player.GetSource_Accessory_OnHurt(Item, info.DamageSource),
					player.MountedCenter + Vector2.UnitX * player.direction * player.width * 0.45f,
					Vector2.UnitX * player.direction * 4 + Main.rand.NextVector2Circular(2, 2),
					Item.shoot,
					damage,
					knockback
				);
			}
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = 0; i < tooltips.Count; i++) {
				if (tooltips[i].Text.Contains("{0}")) {
					tooltips[i].Text = string.Format(tooltips[i].Text, durability, MaxDurability);
					break;
				}
			}
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			float portion = durability / (float)MaxDurability;
			const int width = 32;
			position -= new Vector2(width * 0.5f, (origin.Y - frame.Height) - 2) * scale;
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				new Rectangle((int)position.X, (int)position.Y, (int)(width * scale), 2),
				Color.Black
			);
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				new Rectangle((int)position.X, (int)position.Y, (int)(width * portion * scale), 2),
				Main.hslToRgb(portion * 0.25f, 1, 0.5f)
			);
		}
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<Bark>(3)
			.Register();
		public override void NetSend(BinaryWriter writer) => writer.Write(durability);
		public override void NetReceive(BinaryReader reader) => durability = reader.ReadInt32();
		public override void SaveData(TagCompound tag) => tag[nameof(durability)] = durability;
		public override void LoadData(TagCompound tag) => durability = tag.TryGet(nameof(durability), out durability) ? durability : MaxDurability;
	}
	public class Bark_Shield_Splinters : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
		}
		public override void SetDefaults() {
			Projectile.aiStyle = ProjAIStyleID.Arrow;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.trap = false;
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.extraUpdates = 0;
			Projectile.penetrate = 1;
			Projectile.frame = Main.rand.Next(Main.projFrames[Type]);
		}
	}
}
