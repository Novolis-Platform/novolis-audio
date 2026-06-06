using Novolis.Audio.Analysis;
using Novolis.Audio.Live;

namespace Novolis.Audio.Live.Visuals;

public sealed record LiveVisualFrame(
    LiveTransportSnapshot Transport,
    AudioAnalysisSnapshot Analysis,
    LiveGraphNode? ProgramGraph);
