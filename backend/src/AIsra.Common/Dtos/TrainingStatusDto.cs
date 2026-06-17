using System.Runtime.InteropServices;
using AIsra.Common.Enums;
using JetBrains.Annotations;

namespace AIsra.Common.Dtos;

[StructLayout(LayoutKind.Auto)]
public readonly record struct TrainingStatusDto([UsedImplicitly] TrainingStatus Status);
