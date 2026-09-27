using System;
using System.Collections.Generic;
using System.Linq;

namespace Modules.Module03_Diagnostics.Domain
{
    public enum ProbeStatus { Success, InvalidSource, InvalidDestination, SourceDisabled, NoRoute, AddressUnresolved, AddressConflict, ReplyUnavailable }
    public enum TracePhase { ArpRequest, ArpReply, EchoRequest, EchoReply }
    public enum TraceLinkKind { Cable, PassiveContinuity, SwitchForwarding }

    /// <summary>Un tramo lógico observado; la presentación resuelve IDs a splines/anclajes.</summary>
    public sealed class ProbeTraceStep
    {
        public TracePhase Phase { get; }
        public TraceLinkKind Kind { get; }
        public string LinkId { get; }
        public string FromPort { get; }
        public string ToPort { get; }
        public int Attempt { get; }
        internal ProbeTraceStep(TracePhase phase, TraceLinkKind kind, string link, string from, string to, int attempt = 0)
        { Phase = phase; Kind = kind; LinkId = link; FromPort = from; ToPort = to; Attempt = attempt; }
    }

    public sealed class ProbeResult
    {
        internal Guid SessionId { get; }
        public long Revision { get; }
        public string SourcePort { get; }
        public string DestinationIp { get; }
        public string ResponderPort { get; }
        public ProbeStatus Status { get; }
        public string Message { get; }
        // Intentos solicitados por el comando, no número de tramas Ethernet emitidas.
        public int Sent { get; }
        public int Received => Status == ProbeStatus.Success ? Sent : 0;
        public IReadOnlyList<ProbeTraceStep> Trace { get; }
        internal ProbeResult(Guid sessionId, long revision, string source, string destination, string responder,
            ProbeStatus status, string message, int count, IEnumerable<ProbeTraceStep> trace)
        {
            SessionId = sessionId; Revision = revision; SourcePort = source; DestinationIp = destination;
            ResponderPort = responder; Status = status; Message = message; Sent = count;
            Trace = Array.AsReadOnly(trace.ToArray());
        }
    }

    public sealed class NeighbourObservation
    {
        public string Ip { get; }
        public string Mac { get; }
        public string PortId { get; }
        public long Revision { get; }
        internal NeighbourObservation(PortDefinition p, long revision)
        { Ip = p.ipv4; Mac = p.mac; PortId = p.id; Revision = revision; }
    }
}
