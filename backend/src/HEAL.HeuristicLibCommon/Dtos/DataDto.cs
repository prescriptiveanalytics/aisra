using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace HEAL.HeuristicLibContracts.Dtos;

[StructLayout(LayoutKind.Auto)]
public readonly record struct DataDto([UsedImplicitly] DateTimeOffset Timestamp, double[] Data);

