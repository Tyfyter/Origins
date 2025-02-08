using Origins.Dev;
using Origins.Items.Materials;
using Origins.Layers;
using System;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Wings)]
	public class Acrid_Beam_Thrusters : ModItem, ICustomWikiStat {
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMask<Wings_Glow_Layer>(Item.wingSlot, Texture + "_Wings_Glow");
			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new(130, 6.75f, 1);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 20);
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.SoulofFlight, 20)
			.AddIngredient<Eitrite_Bar>(10)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override bool WingUpdate(Player player, bool inUse) {
			if (player.controlJump) {
				player.wingFrameCounter++;
				const int timePerFrame = 2;
				if (player.wingFrameCounter >= timePerFrame * 3) player.wingFrameCounter = 0;
				player.wingFrame = 1 + player.wingFrameCounter / timePerFrame;

				bool noLight = player.wingsLogic != player.wings;
				ArmorShaderData shaderData = GameShaders.Armor.GetSecondaryShader(player.cWings, player);
				for (int i = -2; i < 4; i++) {
					if (Main.rand.NextBool(2)) {
						Vector2 offset = new((i * -12 - 1) * player.direction, 20f);
						Dust dust = Dust.NewDustDirect(player.Center, 0, 0, DustID.Terra, 0f, 0f, 100, Color.White, 0.8f);
						dust.noGravity = true;
						dust.noLightEmittence = noLight;
						dust.position = player.Center + offset;
						dust.velocity = player.DirectionTo(dust.position + new Vector2(player.direction * 6, 0)) * 2f;
						dust.velocity.X *= 0.5f;
						dust.velocity.Y *= 1.5f;
						if (!Main.rand.NextBool(10)) {
							dust.customData = this;
						} else {
							dust.fadeIn = 0.5f;
						}
						dust.shader = shaderData;
					}
				}
			} else {
				player.wingFrame = 0;
			}
			return true;
		}
	}
}
