using System.Runtime.InteropServices;
using HEAL.HeuristicLibContracts.Enums;
using JetBrains.Annotations;

namespace HEAL.HeuristicLibContracts.Dtos;

[StructLayout(LayoutKind.Auto)]
public readonly record struct TrainingStatusDto([UsedImplicitly] TrainingStatus Status);
