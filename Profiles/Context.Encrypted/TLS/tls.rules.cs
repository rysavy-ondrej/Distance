using Distance.Diagnostics.Dns;
using Distance.Runtime;
using NRules.Fluent.Dsl;
using System;

namespace Distance.Diagnostics.Tls
{
    public class BuildTlsHandshakeRule : DistanceRule
    {
        public override void Define()
        {
            TlsCLientHello clientHello = null;
            TlsServerHello serverHello = null;
            When()
                .Match<TlsCLientHello>(() => clientHello)
                .Match<TlsServerHello>(() => serverHello,
                    s => s.IpSrc == clientHello.IpDst,
                    s => s.IpDst == clientHello.IpSrc,
                    s => s.TcpSrcport == clientHello.TcpDstport,
                    s => s.TcpDstport == clientHello.TcpSrcport);
            Then()
                .Do(ctx => ctx.TryInsert(new TlsHandshake {
                    Timestamp = clientHello.FrameTimeRelative,
                    IpSrc = clientHello.IpSrc,
                    IpDst = clientHello.IpDst,
                    TcpSrcport = clientHello.TcpSrcport,
                    TcpDstport = clientHello.TcpDstport,
                    ClientHello = clientHello, 
                    ServerHello = serverHello,
                }));
        }
    }

    public class BuildTlsContextRule : DistanceRule
    {
        public override void Define()
        {
            TlsHandshake handshake = null;
            DnsQueryResponse dnsQueryResponse = null;

            When()
                .Match<TlsHandshake>(() => handshake)
                .Match<DnsQueryResponse>(() => dnsQueryResponse,
                    d => d.Query.IpSrc == handshake.IpSrc,
                    d => d.Response.DnsA.Contains(handshake.IpDst)
                );
            Then()
                .Do(ctx => ctx.TryInsert(new ContextFlow<DnsQueryResponse, TlsHandshake> { 
                    Timestamp = handshake.Timestamp,
                    Flow = handshake,
                    Context = dnsQueryResponse,
                    Protocol = System.Net.Sockets.ProtocolType.Tcp,
                    IpSrc = handshake.IpSrc,
                    IpDst = handshake.IpDst,
                    TcpSrcport = handshake.TcpSrcport,
                    TcpDstport = handshake.TcpDstport
                }));
        }
    }


    /// <summary>
    /// Represents a single context flow record.
    /// </summary>
    /// <typeparam name="TContext">The context information.</typeparam>
    /// <typeparam name="TFlow">The target flow.</typeparam>
    public class ContextFlow<TContext,TFlow> : Distance.Runtime.DistanceEvent
    {
        public double Timestamp { get ; set ; }
        public System.Net.Sockets.ProtocolType Protocol { get; set; }
        public string IpSrc { get; set; }
        public string IpDst { get; set; }
        public int TcpSrcport { get; set; }
        public int TcpDstport { get; set; }
        public TContext Context { get; set; }
        public TFlow Flow { get; set; }

        public string ContextTypeName { get; set; }

        public override string Name => ContextTypeName;

        public override string Message => $"{Context} |- {Protocol} {IpSrc}:{TcpSrcport}->{IpDst}:{TcpDstport} [{Flow}] ";

        public override EventSeverity Severity => EventSeverity.Context;

        public override int GetHashCode()
        {
            return Distance.Utils.HashFunction.GetHashCode(this.Timestamp, this.Protocol, this.IpSrc, this.IpDst, this.TcpSrcport, this.TcpDstport, this.Context, this.Flow);
        }
        public override bool Equals(object obj)
        {
            var that = obj as ContextFlow<TContext, TFlow>;
            return (((((((((that != null)
                        && object.Equals(this.Timestamp, that.Timestamp))
                        && object.Equals(this.Protocol, that.Protocol))
                        && object.Equals(this.IpSrc, that.IpSrc))
                        && object.Equals(this.IpDst, that.IpDst))
                        && object.Equals(this.TcpSrcport, that.TcpSrcport))
                        && object.Equals(this.TcpDstport, that.TcpDstport))
                        && object.Equals(this.Context, that.Context))
                        && object.Equals(this.Flow, that.Flow));
        }
    }
}
