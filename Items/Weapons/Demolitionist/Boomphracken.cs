using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Ranged;
using Origins.Tiles.Dusk;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Boomphracken : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Handcannon"
        ];
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			ID = Type;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Musket);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
            Item.noMelee = true;
            Item.damage = 80;
			Item.width = 56;
			Item.height = 26;
			Item.useTime = 57;
			Item.useAnimation = 57;
			Item.shoot = ModContent.ProjectileType<Boomphracken_P>();
			Item.useAmmo = ModContent.ItemType<Metal_Slug>();
			Item.knockBack = 8f;
			Item.shootSpeed = 12f;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = Origins.Sounds.Krunch.WithPitch(-0.25f);
			Item.autoReuse = true;
            Item.ArmorPenetration += 8;
        }
		public override bool AltFunctionUse(Player player) {
			if (player.selectedItem == 58 || player.mouseInterface) return false;
			for (int i = Main.InventoryItemSlotsStart; i < Main.InventoryItemSlotsCount; i++) {
				Item item = player.inventory[i];
				if (!item.IsAir && item.useStyle != ItemUseStyleID.None && item.CountsAsClass<Thrown_Explosive>()) {
					//PlayerInput.TryEnteringFastUseModeForInventorySlot(i);
					if (player.nonTorch == -1) {
						player.nonTorch = player.selectedItem;
					}
					player.selectedItem = i;
					player.controlUseItem = true;
					player.releaseUseItem = true;
					player.controlUseTile = false;
					break;
				}
			}
			return false;
		}
		public override Vector2? HoldoutOffset() {
			return Vector2.Zero;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.IllegalGunParts, 2)
			.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>(), 8)
			.AddIngredient(ModContent.ItemType<Hallowed_Cleaver>())
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == Metal_Slug_P.ID) type = Item.shoot;
            Vector2 offset = (velocity.RotatedBy(MathHelper.PiOver2 * -player.direction) * 5) / velocity.Length();
            position += offset;
        }
	}
	public class Boomphracken_P : Metal_Slug_P {
		public override string Texture => "Origins/Projectiles/Ammo/Boomphracken_P";
	}
}
