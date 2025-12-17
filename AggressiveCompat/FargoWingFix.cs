using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.AggressiveCompat {
	internal class FargoWingFix : AggressiveCompatLoader {
		const string mod_name = "Fargowiltas";
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod(mod_name);
		public override void Load() {
			ILHook(mod_name, "Fargowiltas.Items.FargoGlobalItem", "VerticalWingSpeeds", static il => {
				ILCursor c = new(il);
				if (!c.TryGotoNext(i => i.MatchLdfld<Item>(nameof(Item.wingSlot)))) return;
				c.GotoNext(MoveType.After, i => i.MatchBrtrue(out _));
				ILLabel skip = c.MarkLabel();
				c.GotoPrev(MoveType.AfterLabel, i => i.MatchLdsfld<ArmorIDs.Wing.Sets>(nameof(ArmorIDs.Wing.Sets.Stats)));
				c.EmitBr(skip);
			});
		}
	}
}
