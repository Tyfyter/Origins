#define ANIMATED
using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;

namespace Origins.Items.Weapons.Demolitionist {
	public class Mine_Flayer : ModItem, ICustomWikiStat {
        public string[] Categories => new string[] {
            "Launcher",
			"CanistahUser"
        };
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TerraBlade);
			Item.shootsEveryUse = false;
			Item.damage = 48;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Melee];
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 4;
			Item.useAnimation = 36;
			Item.knockBack = 4f;
			Item.useAmmo = ModContent.ItemType<Resizable_Mine_One>();
			Item.shoot = ModContent.ProjectileType<Resizable_Mine_P_1>();
			Item.shootSpeed = 9;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 5);
			Item.reuseDelay = 60;
			Item.autoReuse = false;
			Item.UseSound = null;
            Item.ArmorPenetration += 2;
        }
#if ANIMATED
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.reuseDelay == 0) {
				Item.useStyle = ItemUseStyleID.RaiseLamp;
			} else {
				Item.useStyle = ItemUseStyleID.Swing;
			}
		}
		public override void UseItemFrame(Player player) {
			if (player.HeldItem.type != Type) return;
			if (player.itemAnimation == player.itemTime) {
				switch ((player.itemAnimation / 4) % 3) {
					case 0:
					player.bodyFrame.Y = player.bodyFrame.Height * 3;
					break;

					case 1:
					player.bodyFrame.Y = player.bodyFrame.Height * 2;
					break;

					case 2:
					player.bodyFrame.Y = player.bodyFrame.Height;
					break;
				}
				return;
			}
		}
#else
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.HeldItem.type != Type) return;
			player.chatOverhead.NewMessage("" + (player.HeldItem.useStyle == ItemUseStyleID.RaiseLamp), 2);
			if (player.itemAnimation == player.itemTime) {
				//player.itemLocation = Vector2.Zero;
				player.HeldItem.useStyle = ItemUseStyleID.RaiseLamp;
				switch (player.itemAnimation % 3) {
					case 0:
					player.bodyFrame.Y = player.bodyFrame.Height * 3;
					break;

					case 1:
					player.bodyFrame.Y = player.bodyFrame.Height * 2;
					break;

					case 2:
					player.bodyFrame.Y = player.bodyFrame.Height;
					break;
				}
				return;
			}
			player.HeldItem.useStyle = -1;
			switch ((player.itemAnimation * 3) / player.itemAnimationMax) {
				case 0: 
				player.itemLocation.X = player.position.X + player.width * 0.5f + (heldItemFrame.Width * 0.5f - 10) * player.direction;
				player.itemLocation.Y = player.position.Y + 24f;
				break;
				
				case 1: 
				player.itemLocation.X = player.position.X + player.width * 0.5f + (heldItemFrame.Width * 0.5f - 10) * player.direction;
				player.itemLocation.Y = player.position.Y + 10;
				break;
				
				case 2: 
				player.itemLocation.X = player.position.X + player.width * 0.5f + (heldItemFrame.Width * 0.5f - 10) * player.direction;
				player.itemLocation.Y = player.position.Y + 10;
				break;
			}
			player.itemRotation = (player.itemAnimation / (float)player.itemAnimationMax - 0.5f) * player.direction * -3.5f - player.direction * 0.3f;
		}
		public override void UseItemFrame(Player player) {
			if (player.HeldItem.type != Type) return;
			if (player.itemAnimation == player.itemTime) {
				switch (player.itemAnimation % 3) {
					case 0:
					player.bodyFrame.Y = player.bodyFrame.Height * 3;
					break;

					case 1:
					player.bodyFrame.Y = player.bodyFrame.Height * 2;
					break;

					case 2:
					player.bodyFrame.Y = player.bodyFrame.Height;
					break;
				}
				return;
			}
			switch ((player.itemAnimation * 3) / player.itemAnimationMax) {
				case 0: 
				player.bodyFrame.Y = player.bodyFrame.Height * 3;
				break;
				
				case 1: 
				player.bodyFrame.Y = player.bodyFrame.Height * 2;
				break;
				
				case 2: 
				player.bodyFrame.Y = player.bodyFrame.Height;
				break;
			}
		}
#endif
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return !Main.rand.NextBool(6);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Busted_Servo>(), 30);
			recipe.AddIngredient(ModContent.ItemType<Power_Core>());
			recipe.AddIngredient(ModContent.ItemType<Rotor>(), 8);
			recipe.AddTile(TileID.MythrilAnvil); //Fabricator
			recipe.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = OriginExtensions.Vec2FromPolar(player.direction == 1 ? player.itemRotation : player.itemRotation + MathHelper.Pi, velocity.Length());
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item61.WithPitch(0.25f), position);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}
		public override void HoldItem(Player player) {
		}
	}
}
