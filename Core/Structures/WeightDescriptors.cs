using System.Collections.Generic;
using System.Linq;
using Terraria;
using static Origins.Core.Structures.ARoom;

namespace Origins.Core.Structures;
public class WithoutRoom : WeightDescriptor.Kind {
	protected override Accumulator<WeightParameters, float> Create(string[] parameters) => Create(parameters.ToHashSet());
	public static Accumulator<WeightParameters, float> Create(HashSet<string> requiredRooms) => (WeightParameters instance, ref float output) => 
	output *= (!instance.Structure.rooms.Any(r => requiredRooms.Contains(r.Room.Identifier))).ToInt();
}