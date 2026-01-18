using Origins.Layers;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories;
[AutoloadEquip(EquipType.Wings)]
public class Gills : ModItem {
	public override void SetStaticDefaults() {
		ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new(60, 3.25f, hasHoldDownHoverFeatures: true);
		Origins.AddGlowMask(this);
		Accessory_Glow_Layer.AddGlowMasks(Item, EquipType.Wings);
	}
	public override void SetDefaults() {
		Item.DefaultToAccessory();
		Item.rare = ItemRarityID.Blue;
		Item.value = Item.sellPrice(silver: 6);
	}
	public static bool ForceHover(Player player) => player.wingTime <= (player.wingTimeMax + (player.rocketBoots != 0 ? 6 * 6 : 0)) * 0.3f + 25;
	public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
		const float braking_factor = 0.97f;
		if (player.TryingToHoverDown || ForceHover(player)) {
			player.velocity.Y *= braking_factor;
			player.wingTime += 0.5f;
			ascentWhenFalling = player.gravity + player.velocity.Y * 0.05f * player.gravDir;
			ascentWhenRising = -(player.gravity + player.velocity.Y * 0.05f * player.gravDir);
			constantAscend = -player.gravity;
		}
	}
	public override bool WingUpdate(Player player, bool inUse) {
		ref bool gillsDidVisual = ref player.OriginPlayer().gillsDidVisual;
		if (player.wingsLogic != player.wings) {
			gillsDidVisual = false;
			return base.WingUpdate(player, inUse);
		}
		if (ForceHover(player)) {
			if (gillsDidVisual.TrySet(true)) {
				SoundEngine.PlaySound(Origins.Sounds.SmallSawStart.WithPitch(-0.6f).WithVolumeScale(0.75f));
				SoundEngine.PlaySound(SoundID.Item132.WithPitch(-0.5f));
				ArmorShaderData shaderData = GameShaders.Armor.GetSecondaryShader(player.cWings, player);
				for (int i = -3; i < 3; i++) {
					for (int j = 0; j < 2; j++) {
						Vector2 offset = new((j == 1).ToDirectionInt() * 16, 4 * i);
						Dust dust = Dust.NewDustDirect(player.Center, 0, 0, DustID.OrangeTorch, 0f, 0f, 100, Color.White);
						dust.noGravity = true;
						dust.position = player.Center + offset;
						dust.velocity = player.DirectionTo(dust.position + new Vector2(player.direction * 6, 0)) * 2f;
						dust.position.Y -= 16;
						dust.velocity.X *= 0.5f;
						//dust.velocity.Y *= 1.5f;
						if (!Main.rand.NextBool(10)) {
							dust.customData = this;
						} else {
							dust.fadeIn = 0.5f;
						}
						dust.shader = shaderData;
					}
				}
			}
		} else {
			gillsDidVisual = false;
		}
		if (player.controlJump) {
			player.wingFrameCounter++;
			const int timePerFrame = 3;
			if (player.wingFrameCounter >= timePerFrame * 3) player.wingFrameCounter = 0;
			player.wingFrame = 1 + player.wingFrameCounter / timePerFrame;
		} else {
			player.wingFrame = 0;
		}
		return true;
	}
}
