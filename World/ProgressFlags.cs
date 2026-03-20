using Terraria;

namespace Origins.World; 
public class ProgressFlags : ProgressFlagSystem {
	public static ProgressFlag SolarEclipseOccurred { get; private set; }

	public static ProgressFlag DownedFiberglassWeaver { get; private set; }
	public static ProgressFlag DownedLostDiver { get; private set; }
	public static ProgressFlag DownedShimmerConstruct { get; private set; }

	public static ProgressFlag DownedChambersiteSentinel { get; private set; }

	public static ProgressFlag DownedDefiledMimic { get; private set; }
	public static ProgressFlag DownedRivenMimic { get; private set; }
	public static ProgressFlag DownedTrashCompactorMimic { get; private set; }
	public override void Load() {
		base.Load();
		On_Main.UpdateTime_StartNight += (On_Main.orig_UpdateTime_StartNight orig, ref bool stopEvents) => {
			if (Main.eclipse && !Main.IsFastForwardingTime()) SolarEclipseOccurred.Set(true);
			orig(ref stopEvents);
		};
	}
}
