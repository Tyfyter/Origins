using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Reflection;
using ReLogic.Content;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Vanity.Dev.PlagueTexan;
[AutoloadEquip(EquipType.Wings)]
public class SceneYMK_Wings : ModItem, IRightClickableAccessory {
	public static int WingsID { get; private set; }
	public override void SetStaticDefaults() {
		ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new(150, 7f, hasHoldDownHoverFeatures: true);
		TextureSwitcher.Add(() => ref TextureAssets.Item[Type], (AutoLoadingTexture)(typeof(SceneYMK_Wings).GetDefaultTMLName() + "_Undyed"));
	}
	public override void SetDefaults() {
		Item.DefaultToAccessory();
		Item.rare = AltCyanRarity.ID;
		WingsID = Item.wingSlot;
		Item.value = Item.sellPrice(gold: 8);
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
		if (player.wingFrame == 3) {
			if (!player.flapSound) {
				SoundEngine.PlaySound(SoundID.Item32, player.Center);
				player.flapSound = true;
			} else {
				player.flapSound = false;
			}
		}
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
	public bool RightClickAccessory(Item[] inv, int context, int slot) {
		OriginPlayer originPlayer = Main.LocalPlayer.OriginPlayer();
		originPlayer.sceneYMKWingsNaturalColor[Main.LocalPlayer.CurrentLoadoutIndex] ^= true;
		originPlayer.SyncPlayer(-1, Main.myPlayer, false, 0, PlayerVisualSyncDatas.NaturalMoonlightWings);
		return false;
	}
	public override void UpdateInventory(Player player) => UpdateName();
	public override void UpdateEquip(Player player) => UpdateName();
	public override void UpdateVanity(Player player) => UpdateName();
	public override void Update(ref float gravity, ref float maxFallSpeed) => UpdateName();
	public void UpdateName() {
		bool natural = Main.LocalPlayer.OriginPlayer().sceneYMKWingsNaturalColor[Main.LocalPlayer.CurrentLoadoutIndex];
		if (natural) Item.SetNameOverride(this.GetLocalization("AltDisplayName").Value);
		else Item.ClearNameOverride();
	}

	public class TextureSwitcher : IPreDrawAnything {
		static readonly List<(RefGet<Asset<Texture2D>> slot, Asset<Texture2D> natural, Asset<Texture2D> dyed)> options = [];
		static bool lastNatural = false;
		public static void Add(RefGet<Asset<Texture2D>> slot, Asset<Texture2D> natural) {
			if (Main.dedServ) return;
			options.Add((slot, natural, slot()));
		}
		static TextureSwitcher() => OriginSystem.assetSwitchers.Add(new TextureSwitcher());
		void IPreDrawAnything.PreDrawAnything() {
			if (!lastNatural.TrySet(Main.LocalPlayer.OriginPlayer().sceneYMKWingsNaturalColor[Main.LocalPlayer.CurrentLoadoutIndex])) return;
			for (int i = 0; i < options.Count; i++) {
				(RefGet<Asset<Texture2D>> slot, Asset<Texture2D> natural, Asset<Texture2D> dyed) = options[i];
				slot() = lastNatural ? natural : dyed;
			}
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
	AutoLoadingAsset<Texture2D> undyed = typeof(SceneYMK_Wings).GetDefaultTMLName() + "_Wings_Undyed";
	AutoLoadingAsset<Texture2D> feathers0 = typeof(SceneYMK_Wings).GetDefaultTMLName() + "_Feathers_0";
	AutoLoadingAsset<Texture2D> feathers1 = typeof(SceneYMK_Wings).GetDefaultTMLName() + "_Feathers_1";
	AutoLoadingAsset<Texture2D> feathers2 = typeof(SceneYMK_Wings).GetDefaultTMLName() + "_Feathers_2";
	static int letMeBeClear = -1;
	public override void SetStaticDefaults() {
		letMeBeClear = ShaderDataMethods.RegisterArmorShader(new ArmorShaderData(Mod.Assets.Request<Effect>("Effects/Misc"), "Transparency"));
	}
	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.wings == SceneYMK_Wings.WingsID;
	public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Wings);
	protected override void Draw(ref PlayerDrawSet drawInfo) {
		for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--) {
			DrawData data = drawInfo.DrawDataCache[i];
			if (data.texture == TextureAssets.Wings[SceneYMK_Wings.WingsID]?.Value) {
				OriginsDyeSlots dyes = drawInfo.drawPlayer.GetModPlayer<OriginsDyeSlots>();
				if (drawInfo.drawPlayer.OriginPlayer().sceneYMKWingsNaturalColor[drawInfo.drawPlayer.CurrentLoadoutIndex]) {
					data.texture = undyed;
					drawInfo.DrawDataCache[i] = data;
					dyes.cSceneMYKDye0 ??= letMeBeClear;
					dyes.cSceneMYKDye1 ??= letMeBeClear;
					dyes.cSceneMYKDye2 ??= letMeBeClear;
				}
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
