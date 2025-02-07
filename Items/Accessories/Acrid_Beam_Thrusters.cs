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
			Item.rare = ItemRarityID.Yellow;
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

				/*bool noLight = player.wingsLogic != player.wings;
				ArmorShaderData shaderData = GameShaders.Armor.GetSecondaryShader(player.cWings, player);
				for (int i = 0; i < 4; i++) {
					if (Main.rand.NextBool(4)) {
						Vector2 offset = (-0.74539816f + MathHelper.Pi / 8f * i + 0.03f * i).ToRotationVector2() * new Vector2(player.direction * -20, 20f);
						Dust dust = Dust.NewDustDirect(player.Center, 0, 0, DustID.Vortex, 0f, 0f, 100, Color.White, 0.8f);
						dust.noGravity = true;
						dust.noLightEmittence = noLight;
						dust.position = player.Center + offset;
						dust.velocity = player.DirectionTo(dust.position) * 2f;
						if (!Main.rand.NextBool(10)) {
							dust.customData = this;
						} else {
							dust.fadeIn = 0.5f;
						}
						dust.shader = shaderData;
					}
				}
				for (int i = 0; i < 4; i++) {
					if (Main.rand.NextBool(8)) {
						Vector2 offset = (-0.7053982f + MathHelper.Pi / 8f * i + 0.03f * i).ToRotationVector2() * new Vector2(player.direction * 20, 24f) + new Vector2(player.direction * -16f, 0f);
						Dust dust = Dust.NewDustDirect(player.Center, 0, 0, DustID.Vortex, 0f, 0f, 100, Color.White, 0.5f);
						dust.noGravity = true;
						dust.noLightEmittence = noLight;
						dust.position = player.Center + offset;
						dust.velocity = Vector2.Normalize(dust.position - player.Center - new Vector2(player.direction * -16f, 0f)) * 2f;
						dust.position += dust.velocity * 5f;
						if (!Main.rand.NextBool(10)) {
							dust.customData = this;
						} else {
							dust.fadeIn = 0.5f;
						}
						dust.shader = shaderData;
					}
				}*/
			} else {
				player.wingFrame = 0;
			}
			return true;
		}
	}
}
