using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Origins.Tiles.Other;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Autohandcannon : ModItem {
		public static int ID { get; private set; }
		public static LocalizedText JammedText { get; private set; }
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			ID = Type;
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
			JammedText = this.GetLocalization("JammedText");
		}
		public override void Load() {
			On_Player.ItemCheck_HackHoldStyles += On_Player_ItemCheck_HackHoldStyles;
		}
		static void On_Player_ItemCheck_HackHoldStyles(On_Player.orig_ItemCheck_HackHoldStyles orig, Player self, Item sItem) {
			orig(self, sItem);
			if (self.whoAmI == Main.myPlayer && sItem.type == ID && self.ItemAnimationActive && self.itemAnimation < self.itemAnimationMax * 0.8f && !self.OriginPlayer().autohandcannonJammed) {
				if (self.controlUseItem && self.releaseUseItem) {
					self.itemAnimation = 0;
					self.itemTime = 0;
					if (Main.rand.NextBool(7, 37)) {
						SoundEngine.PlaySound(SoundID.Item178.WithPitch(1.6f));
						SoundEngine.PlaySound(SoundID.Unlock.WithPitch(-1.2f));
						self.OriginPlayer().autohandcannonJammed = true;
						CombatText.NewText(self.Hitbox, Color.DarkGray, JammedText.Value);
					}
				}
			}
		}
		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.noMelee = true;
			Item.damage = 68;
			Item.crit = 8;
			Item.width = 56;
			Item.height = 26;
			Item.useTime = 46;
			Item.useAnimation = 46;
			Item.shoot = ModContent.ProjectileType<Metal_Slug_P>();
			Item.useAmmo = ModContent.ItemType<Metal_Slug>();
			Item.knockBack = 8f;
			Item.shootSpeed = 12f;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = Origins.Sounds.HeavyCannon.WithPitch(1.5f);
			Item.autoReuse = true;
			Item.ArmorPenetration += 8;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Busted_Servo>(), 15)
			.AddIngredient(ModContent.ItemType<Power_Core>())
			.AddIngredient(ModContent.ItemType<Rotor>(), 4)
			.AddIngredient(ModContent.ItemType<Rubber>(), 8)
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
		public override bool AltFunctionUse(Player player) => true;
		public override bool CanUseItem(Player player) => (player.altFunctionUse == 2) == player.OriginPlayer().autohandcannonJammed;
		public override void UseItemFrame(Player player) {
			player.bodyFrame.Y = player.bodyFrame.Height * 3;
		}
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.altFunctionUse != 2) return;
			SoundEngine.PlaySound(SoundID.Item1.WithPitch(-0.5f));
			player.itemRotation = (player.itemAnimation / (float)player.itemAnimationMax) * player.direction * MathHelper.TwoPi * 2;
			player.itemLocation = player.GetHandPosition();
			player.itemLocation.Y -= 12;
			if (player.ItemAnimationEndingOrEnded) {
				player.OriginPlayer().autohandcannonJammed = false;
				SoundEngine.PlaySound(SoundID.Item149.WithPitch(-0.5f));
			}
		}
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse != 2) SoundEngine.PlaySound(Item.UseSound, player.itemLocation);
			return base.UseItem(player);
		}
		public override float UseSpeedMultiplier(Player player) => player.altFunctionUse == 2 ? 1.5f : 1;
		public override Vector2? HoldoutOffset() => Main.CurrentPlayer.altFunctionUse == 2 ? new(-22, -7) : Vector2.Zero;
		public override bool? CanAutoReuseItem(Player player) => player.itemAnimation <= 0;
		public override bool CanConsumeAmmo(Item ammo, Player player) => player.altFunctionUse != 2;
		public override bool CanShoot(Player player) => player.altFunctionUse != 2;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			Vector2 offset = velocity.RotatedBy(MathHelper.PiOver2 * -player.direction) * 5 / velocity.Length();
			position += offset;
		}
	}
}
