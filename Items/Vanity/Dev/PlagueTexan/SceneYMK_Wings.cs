using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using PegasusLib;
using System.Diagnostics.CodeAnalysis;

namespace Origins.Items.Vanity.Dev.PlagueTexan; 
[AutoloadEquip(EquipType.Wings)]
public class SceneYMK_Wings : ModItem {
	public static int WingsID { get; private set; }

	public override void SetStaticDefaults() {
		ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new(150, 7f, hasHoldDownHoverFeatures: true);
	}
	public override void SetDefaults() {
		Item.DefaultToAccessory();
		Item.rare = AltCyanRarity.ID;
		WingsID = Item.wingSlot;
	}

	public override bool WingUpdate(Player player, bool inUse) {
		if (inUse) {
			int framesMax = 5;
			if (player.TryingToHoverDown && !player.controlLeft && !player.controlRight) framesMax = 6;
			if (++player.wingFrameCounter > framesMax) {
				player.wingFrameCounter = 0;
				if (++player.wingFrame >= 4) player.wingFrame = 0;
				if (player.wingFrame == 3) SoundEngine.PlaySound(SoundID.Item32.WithPitchOffset(-0.05f), player.Center);
			}
			return true;
		}
		if (player.wingFrame == 3) 			if (!player.flapSound) {
				SoundEngine.PlaySound(SoundID.Item32, player.Center);
				player.flapSound = true;
			}
else 			player.flapSound = false;
		return false;
	}
	public override void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration) {
		float braking_factor = 0.98f;
		if (player.controlJump && player.TryingToHoverDown) {
			speed *= 1.05f;
			acceleration *= 1.05f;
			braking_factor = 0.94f;
		} else if (player.controlDown) {
			player.velocity.Y += player.gravity * player.gravDir * 0.15f;
			player.maxFallSpeed *= 1.05f;
			braking_factor = 0.90f;
		}
		if (!player.controlRight && player.velocity.X > 0) player.velocity.X *= braking_factor;
		if (!player.controlLeft && player.velocity.X < 0) player.velocity.X *= braking_factor;
	}
	public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
		const float braking_factor = 0.95f;
		if (player.TryingToHoverDown) {
			player.velocity.Y *= braking_factor;
			player.wingTime += player.controlLeft || player.controlRight ? 0.5f : 0.75f;
			ascentWhenFalling = player.gravity + player.velocity.Y * 0.05f * player.gravDir;
			ascentWhenRising = -(player.gravity + player.velocity.Y * 0.05f * player.gravDir);
			constantAscend = -player.gravity;
		}
	}
}
public abstract class SceneYMK_Dye_Slot(SceneYMK_Dye_Slot.GetDye dyeGetter) : ExtraDyeSlot {
	public delegate ref int? GetDye(OriginsDyeSlots player);
	static int GetWingSlot(Item equipped, Item vanity) {
		int slot = vanity?.wingSlot ?? -1;
		if (slot != -1) return slot;
		return equipped.wingSlot;
	}
	public override bool UseForSlot(Item equipped, Item vanity, bool equipHidden) => GetWingSlot(equipped, vanity) == SceneYMK_Wings.WingsID;
	public override void ApplyDye(Player player, [NotNull] Item dye) {
		dyeGetter(player.GetModPlayer<OriginsDyeSlots>()) = dye.dye;
	}
}
public class SceneYMK_Dye_Slot_0() : SceneYMK_Dye_Slot(player => ref player.cSceneMYKDye0) { }
public class SceneYMK_Dye_Slot_1() : SceneYMK_Dye_Slot(player => ref player.cSceneMYKDye1) { }
public class SceneYMK_Dye_Slot_2() : SceneYMK_Dye_Slot(player => ref player.cSceneMYKDye2) { }
public class SceneYMK_Wings_Layer : PlayerDrawLayer {
	AutoLoadingAsset<Texture2D> feathers0 = typeof(SceneYMK_Wings).GetDefaultTMLName() + "_Feathers_0";
	AutoLoadingAsset<Texture2D> feathers1 = typeof(SceneYMK_Wings).GetDefaultTMLName() + "_Feathers_1";
	AutoLoadingAsset<Texture2D> feathers2 = typeof(SceneYMK_Wings).GetDefaultTMLName() + "_Feathers_2";
	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.wings == SceneYMK_Wings.WingsID;
	public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Wings);
	protected override void Draw(ref PlayerDrawSet drawInfo) {
		for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--) {
			DrawData data = drawInfo.DrawDataCache[i];
			if (data.texture == TextureAssets.Wings[SceneYMK_Wings.WingsID]?.Value) {
				OriginsDyeSlots dyes = drawInfo.drawPlayer.GetModPlayer<OriginsDyeSlots>();

				data.texture = feathers0;
				data.shader = dyes.cSceneMYKDye0 ?? drawInfo.cWings;
				drawInfo.DrawDataCache.Insert(++i, data);

				data.texture = feathers1;
				data.shader = dyes.cSceneMYKDye1 ?? drawInfo.cWings;
				drawInfo.DrawDataCache.Insert(++i, data);

				data.texture = feathers2;
				data.shader = dyes.cSceneMYKDye2 ?? drawInfo.cWings;
				drawInfo.DrawDataCache.Insert(++i, data);
				break;
			}
		}
	}
}
