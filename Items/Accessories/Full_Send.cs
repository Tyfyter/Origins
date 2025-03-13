using CalamityMod.NPCs.TownNPCs;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Balloon)]
	public class Full_Send : ModItem {
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 20);
			Item.shoot = ModContent.ProjectileType<Razorwire_P>();
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(gold: 1, silver: 85);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.BlueHorseshoeBalloon)
			.AddIngredient<Return_To_Sender>()
			.AddTile(TileID.TinkerersWorkbench)
			.AddOnCraftCallback((_, item, consumedItems, destinationStack) => item.Prefix(PrefixID.Arcane))
			.Register();
		}
		public override void UpdateEquip(Player player) {
			player.jumpBoost = true;
			ref ExtraJumpState jumpState = ref player.GetJumpState<Full_Send_Jump>();
			jumpState.Enable();
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.fullSend = true;
			originPlayer.fullSendItem = Item;
			if (jumpState.Active) {
				originPlayer.fullSendPos += Collision.TileCollision(
					originPlayer.fullSendPos,
					player.position - originPlayer.fullSendStartPos,
					player.width,
					player.height,
					true,
					true,
					(int)player.gravDir
				);
				player.position = originPlayer.fullSendStartPos;
				Main.instance.CameraModifiers.Add(new Full_Send_Camera_Modifier(originPlayer.fullSendPos - originPlayer.fullSendStartPos, nameof(Full_Send)));
			}
		}
	}
	public class Full_Send_Camera_Modifier(Vector2 offset, string uniqueIdentity = null) : ICameraModifier {
		public string UniqueIdentity { get; private set; } = uniqueIdentity;
		public bool Finished { get; private set; }
		public void Update(ref CameraInfo cameraInfo) {
			Finished = true;
			if (!Main.LocalPlayer.GetJumpState<Full_Send_Jump>().Active) return;
			cameraInfo.CameraPosition += offset;
		}
	}
	public class Full_Send_Jump : ExtraJump {
		public override Position GetDefaultPosition() => BeforeBottleJumps;
		public override float GetDurationMultiplier(Player player) => 0.75f;
		public override void OnStarted(Player player, ref bool playSound) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.fullSendStartPos = originPlayer.fullSendPos = player.position;
			int feet = player.height;
			if (player.gravDir == -1f)
				feet = 0;

			for (int num23 = 0; num23 < 10; num23++) {
				Dust dust = Dust.NewDustDirect(new Vector2(player.position.X - 34f, player.position.Y + feet - 16f), 102, 32, DustID.Cloud, (0f - player.velocity.X) * 0.5f, player.velocity.Y * 0.5f, 100, default, 1.5f);
				dust.velocity.X = dust.velocity.X * 0.5f - player.velocity.X * 0.1f;
				dust.velocity.Y = dust.velocity.Y * 0.5f - player.velocity.Y * 0.3f;
			}
			Vector2 startPos = new(player.position.X + ((player.width / 2) - 16f), player.position.Y + feet - 16f);
			Vector2 cloudOffset = new(20, 0);
			for (int i = -1; i <= 1; i++) {
				Gore gore = Gore.NewGoreDirect(null,
					startPos + cloudOffset * i,
					new Vector2(0f - player.velocity.X, 0f - player.velocity.Y),
					Main.rand.Next(11, 14)
				);
				gore.velocity.X = gore.velocity.X * 0.1f - player.velocity.X * 0.1f;
				gore.velocity.Y = gore.velocity.Y * 0.1f - player.velocity.Y * 0.05f;
				/*for (int j = 0; j < 2; j++) {
					gore = Gore.NewGoreDirect(null,
						startPos + cloudOffset * i,
						new Vector2(0f - player.velocity.X, 0f - player.velocity.Y),
						GoreID.TreeLeaf_HallowJim
					);
					gore.velocity.X = gore.velocity.X * 0.1f - player.velocity.X * 0.1f;
					gore.velocity.Y = gore.velocity.Y * 0.1f - player.velocity.Y * 0.05f + 8;
				}*/
			}
		}
		public override void OnEnded(Player player) {
			player.position = player.OriginPlayer().fullSendPos;
		}
		public override void ShowVisuals(Player player) {
			int feet = player.height;
			if (player.gravDir == -1f)
				feet = -6;

			Dust dust = Dust.NewDustDirect(new Vector2(player.position.X - 4f, player.position.Y + feet), player.width + 8, 4, DustID.Cloud, player.velocity.X * -0.5f, player.velocity.Y * 0.5f, 100, default, 1.5f);
			dust.velocity.X = dust.velocity.X * 0.5f - player.velocity.X * 0.1f;
			dust.velocity.Y = dust.velocity.Y * 0.5f - player.velocity.Y * 0.3f;
		}
	}
}
