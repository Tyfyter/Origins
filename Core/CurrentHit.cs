using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Core; 
internal class CurrentHit : ILoadable {
	public static int Damage { get => canGet ? field : throw new InvalidOperationException("Can only access current hit during a hit"); private set; }
	public static int hitDirection { get => canGet ? field : throw new InvalidOperationException("Can only access current hit during a hit"); private set; }
	public static bool pvp { get => canGet ? field : throw new InvalidOperationException("Can only access current hit during a hit"); private set; }
	public static bool quiet { get => canGet ? field : throw new InvalidOperationException("Can only access current hit during a hit"); private set; }
	public static int cooldownCounter { get => canGet ? field : throw new InvalidOperationException("Can only access current hit during a hit"); private set; }
	public static bool dodgeable { get => canGet ? field : throw new InvalidOperationException("Can only access current hit during a hit"); private set; }
	public static float armorPenetration { get => canGet ? field : throw new InvalidOperationException("Can only access current hit during a hit"); private set; }
	public static float scalingArmorPenetration { get => canGet ? field : throw new InvalidOperationException("Can only access current hit during a hit"); private set; }
	public static float knockback { get => canGet ? field : throw new InvalidOperationException("Can only access current hit during a hit"); private set; }
	static bool canGet;
	void ILoadable.Load(Mod mod) {
		On_Player.Hurt_PlayerDeathReason_int_int_refHurtInfo_bool_bool_int_bool_float_float_float += On_Player_Hurt_PlayerDeathReason_int_int_refHurtInfo_bool_bool_int_bool_float_float_float;
	}
	void ILoadable.Unload() { }
	static double On_Player_Hurt_PlayerDeathReason_int_int_refHurtInfo_bool_bool_int_bool_float_float_float(On_Player.orig_Hurt_PlayerDeathReason_int_int_refHurtInfo_bool_bool_int_bool_float_float_float orig, Player self, Terraria.DataStructures.PlayerDeathReason damageSource, int Damage, int hitDirection, out Player.HurtInfo info, bool pvp, bool quiet, int cooldownCounter, bool dodgeable, float armorPenetration, float scalingArmorPenetration, float knockback) {
		using ScopedOverride<bool> _ = canGet.ScopedOverride(true);
		CurrentHit.Damage = Damage;
		CurrentHit.hitDirection = hitDirection;
		CurrentHit.pvp = pvp;
		CurrentHit.quiet = quiet;
		CurrentHit.cooldownCounter = cooldownCounter;
		CurrentHit.dodgeable = dodgeable;
		CurrentHit.armorPenetration = armorPenetration;
		CurrentHit.scalingArmorPenetration = scalingArmorPenetration;
		CurrentHit.knockback = knockback;
		return orig(self, damageSource, Damage, hitDirection, out info, pvp, quiet, cooldownCounter, dodgeable, armorPenetration, scalingArmorPenetration, knockback);
	}
}
