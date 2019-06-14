//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Distance.Diagnostics.Lan {
    using Distance.Runtime;
    using Distance.Utils;
    using System;
    
    
    public class IpPacket : Distance.Runtime.DistanceFact {
        
        private Int32 _FrameNumber;
        
        private String _EthSrc;
        
        private String _EthDst;
        
        private String _IpSrc;
        
        private String _IpDst;
        
        public static string Filter = "ip";
        
        public static string[] Fields = new string[] {
                "frame.number",
                "eth.src",
                "eth.dst",
                "ip.src",
                "ip.dst"};
        
        [FieldName("frame.number")]
        public virtual Int32 FrameNumber {
            get {
                return this._FrameNumber;
            }
            set {
                this._FrameNumber = value;
            }
        }
        
        [FieldName("eth.src")]
        public virtual String EthSrc {
            get {
                return this._EthSrc;
            }
            set {
                this._EthSrc = value;
            }
        }
        
        [FieldName("eth.dst")]
        public virtual String EthDst {
            get {
                return this._EthDst;
            }
            set {
                this._EthDst = value;
            }
        }
        
        [FieldName("ip.src")]
        public virtual String IpSrc {
            get {
                return this._IpSrc;
            }
            set {
                this._IpSrc = value;
            }
        }
        
        [FieldName("ip.dst")]
        public virtual String IpDst {
            get {
                return this._IpDst;
            }
            set {
                this._IpDst = value;
            }
        }
        
        public override string ToString() {
            return string.Format("IpPacket: frame.number={0} eth.src={1} eth.dst={2} ip.src={3} ip.dst={4}", this.FrameNumber, this.EthSrc, this.EthDst, this.IpSrc, this.IpDst);
        }
        
        public override int GetHashCode() {
            return Distance.Utils.HashFunction.GetHashCode(this.FrameNumber, this.EthSrc, this.EthDst, this.IpSrc, this.IpDst);
        }
        
        public override bool Equals(object obj) {
            IpPacket that = obj as IpPacket;
            return ((((((that != null) 
                        && object.Equals(this.FrameNumber, that.FrameNumber)) 
                        && object.Equals(this.EthSrc, that.EthSrc)) 
                        && object.Equals(this.EthDst, that.EthDst)) 
                        && object.Equals(this.IpSrc, that.IpSrc)) 
                        && object.Equals(this.IpDst, that.IpDst));
        }
        
        public static IpPacket Create(System.Func<string, string, string> mapper, string[] values) {
            IpPacket newObj = new IpPacket();
            newObj._FrameNumber = mapper.Invoke("frame.number", values[0]).ToInt32();
            newObj._EthSrc = mapper.Invoke("eth.src", values[1]).ToString();
            newObj._EthDst = mapper.Invoke("eth.dst", values[2]).ToString();
            newObj._IpSrc = mapper.Invoke("ip.src", values[3]).ToString();
            newObj._IpDst = mapper.Invoke("ip.dst", values[4]).ToString();
            return newObj;
        }
    }
    
    public class IpEndpoint : Distance.Runtime.DistanceDerived {
        
        private String _IpAddr;
        
        [FieldName("ip.addr")]
        public virtual String IpAddr {
            get {
                return this._IpAddr;
            }
            set {
                this._IpAddr = value;
            }
        }
        
        public override string ToString() {
            return string.Format("IpEndpoint: ip.addr={0}", this.IpAddr);
        }
        
        public override int GetHashCode() {
            return Distance.Utils.HashFunction.GetHashCode(this.IpAddr);
        }
        
        public override bool Equals(object obj) {
            IpEndpoint that = obj as IpEndpoint;
            return ((that != null) 
                        && object.Equals(this.IpAddr, that.IpAddr));
        }
    }
    
    public class IpSourceEndpoint : Distance.Runtime.DistanceDerived {
        
        private String _IpAddr;
        
        [FieldName("ip.addr")]
        public virtual String IpAddr {
            get {
                return this._IpAddr;
            }
            set {
                this._IpAddr = value;
            }
        }
        
        public override string ToString() {
            return string.Format("IpSourceEndpoint: ip.addr={0}", this.IpAddr);
        }
        
        public override int GetHashCode() {
            return Distance.Utils.HashFunction.GetHashCode(this.IpAddr);
        }
        
        public override bool Equals(object obj) {
            IpSourceEndpoint that = obj as IpSourceEndpoint;
            return ((that != null) 
                        && object.Equals(this.IpAddr, that.IpAddr));
        }
    }
    
    public class IpDestinationEndpoint : Distance.Runtime.DistanceDerived {
        
        private String _IpAddr;
        
        [FieldName("ip.addr")]
        public virtual String IpAddr {
            get {
                return this._IpAddr;
            }
            set {
                this._IpAddr = value;
            }
        }
        
        public override string ToString() {
            return string.Format("IpDestinationEndpoint: ip.addr={0}", this.IpAddr);
        }
        
        public override int GetHashCode() {
            return Distance.Utils.HashFunction.GetHashCode(this.IpAddr);
        }
        
        public override bool Equals(object obj) {
            IpDestinationEndpoint that = obj as IpDestinationEndpoint;
            return ((that != null) 
                        && object.Equals(this.IpAddr, that.IpAddr));
        }
    }
    
    public class EthEndpoint : Distance.Runtime.DistanceDerived {
        
        private String _EthAddr;
        
        [FieldName("eth.addr")]
        public virtual String EthAddr {
            get {
                return this._EthAddr;
            }
            set {
                this._EthAddr = value;
            }
        }
        
        public override string ToString() {
            return string.Format("EthEndpoint: eth.addr={0}", this.EthAddr);
        }
        
        public override int GetHashCode() {
            return Distance.Utils.HashFunction.GetHashCode(this.EthAddr);
        }
        
        public override bool Equals(object obj) {
            EthEndpoint that = obj as EthEndpoint;
            return ((that != null) 
                        && object.Equals(this.EthAddr, that.EthAddr));
        }
    }
    
    public class GatewayCandidate : Distance.Runtime.DistanceDerived {
        
        private String _IpAddr;
        
        private String _EthAddr;
        
        [FieldName("ip.addr")]
        public virtual String IpAddr {
            get {
                return this._IpAddr;
            }
            set {
                this._IpAddr = value;
            }
        }
        
        [FieldName("eth.addr")]
        public virtual String EthAddr {
            get {
                return this._EthAddr;
            }
            set {
                this._EthAddr = value;
            }
        }
        
        public override string ToString() {
            return string.Format("GatewayCandidate: ip.addr={0} eth.addr={1}", this.IpAddr, this.EthAddr);
        }
        
        public override int GetHashCode() {
            return Distance.Utils.HashFunction.GetHashCode(this.IpAddr, this.EthAddr);
        }
        
        public override bool Equals(object obj) {
            GatewayCandidate that = obj as GatewayCandidate;
            return (((that != null) 
                        && object.Equals(this.IpAddr, that.IpAddr)) 
                        && object.Equals(this.EthAddr, that.EthAddr));
        }
    }
    
    public class AddressMapping : Distance.Runtime.DistanceDerived {
        
        private String _IpAddr;
        
        private String _EthAddr;
        
        [FieldName("ip.addr")]
        public virtual String IpAddr {
            get {
                return this._IpAddr;
            }
            set {
                this._IpAddr = value;
            }
        }
        
        [FieldName("eth.addr")]
        public virtual String EthAddr {
            get {
                return this._EthAddr;
            }
            set {
                this._EthAddr = value;
            }
        }
        
        public override string ToString() {
            return string.Format("AddressMapping: ip.addr={0} eth.addr={1}", this.IpAddr, this.EthAddr);
        }
        
        public override int GetHashCode() {
            return Distance.Utils.HashFunction.GetHashCode(this.IpAddr, this.EthAddr);
        }
        
        public override bool Equals(object obj) {
            AddressMapping that = obj as AddressMapping;
            return (((that != null) 
                        && object.Equals(this.IpAddr, that.IpAddr)) 
                        && object.Equals(this.EthAddr, that.EthAddr));
        }
    }
    
    public class LocalNetworkPrefix : Distance.Runtime.DistanceDerived {
        
        private String _IpNetwork;
        
        private Int32 _IpPrefix;
        
        [FieldName("ip.network")]
        public virtual String IpNetwork {
            get {
                return this._IpNetwork;
            }
            set {
                this._IpNetwork = value;
            }
        }
        
        [FieldName("ip.prefix")]
        public virtual Int32 IpPrefix {
            get {
                return this._IpPrefix;
            }
            set {
                this._IpPrefix = value;
            }
        }
        
        public override string ToString() {
            return string.Format("LocalNetworkPrefix: ip.network={0} ip.prefix={1}", this.IpNetwork, this.IpPrefix);
        }
        
        public override int GetHashCode() {
            return Distance.Utils.HashFunction.GetHashCode(this.IpNetwork, this.IpPrefix);
        }
        
        public override bool Equals(object obj) {
            LocalNetworkPrefix that = obj as LocalNetworkPrefix;
            return (((that != null) 
                        && object.Equals(this.IpNetwork, that.IpNetwork)) 
                        && object.Equals(this.IpPrefix, that.IpPrefix));
        }
    }
    
    public class IpAddressConflict : Distance.Runtime.DistanceEvent {
        
        private String _IpAddress;
        
        private String[] _EthAddresses;
        
        [FieldName("ip.address")]
        public virtual String IpAddress {
            get {
                return this._IpAddress;
            }
            set {
                this._IpAddress = value;
            }
        }
        
        [FieldName("eth.addresses")]
        public virtual String[] EthAddresses {
            get {
                return this._EthAddresses;
            }
            set {
                this._EthAddresses = value;
            }
        }
        
        public override string Name {
            get {
                return "IpAddressConflict";
            }
        }
        
        public override string Message {
            get {
                return string.Format("Two or more network hosts has assigned the same network address {0}: {1}.", Distance.Utils.StringUtils.ToString(this.IpAddress), Distance.Utils.StringUtils.ToString(this.EthAddresses));
            }
        }
        
        public override Distance.Runtime.EventSeverity Severity {
            get {
                return Distance.Runtime.EventSeverity.Error;
            }
        }
        
        public override string ToString() {
            return string.Format("IpAddressConflict: ip.address={0} eth.addresses={1}", this.IpAddress, this.EthAddresses);
        }
        
        public override int GetHashCode() {
            return Distance.Utils.HashFunction.GetHashCode(this.IpAddress, this.EthAddresses);
        }
        
        public override bool Equals(object obj) {
            IpAddressConflict that = obj as IpAddressConflict;
            return (((that != null) 
                        && object.Equals(this.IpAddress, that.IpAddress)) 
                        && object.Equals(this.EthAddresses, that.EthAddresses));
        }
    }
    
    public class IpAddressMismatch : Distance.Runtime.DistanceEvent {
        
        private String _IpAddress;
        
        private String _EthAddress;
        
        [FieldName("ip.address")]
        public virtual String IpAddress {
            get {
                return this._IpAddress;
            }
            set {
                this._IpAddress = value;
            }
        }
        
        [FieldName("eth.address")]
        public virtual String EthAddress {
            get {
                return this._EthAddress;
            }
            set {
                this._EthAddress = value;
            }
        }
        
        public override string Name {
            get {
                return "IpAddressMismatch";
            }
        }
        
        public override string Message {
            get {
                return string.Format("The IP address {0} of a local host {1} is not within the scope of the local netwo" +
                        "rk.", Distance.Utils.StringUtils.ToString(this.IpAddress), Distance.Utils.StringUtils.ToString(this.EthAddress));
            }
        }
        
        public override Distance.Runtime.EventSeverity Severity {
            get {
                return Distance.Runtime.EventSeverity.Error;
            }
        }
        
        public override string ToString() {
            return string.Format("IpAddressMismatch: ip.address={0} eth.address={1}", this.IpAddress, this.EthAddress);
        }
        
        public override int GetHashCode() {
            return Distance.Utils.HashFunction.GetHashCode(this.IpAddress, this.EthAddress);
        }
        
        public override bool Equals(object obj) {
            IpAddressMismatch that = obj as IpAddressMismatch;
            return (((that != null) 
                        && object.Equals(this.IpAddress, that.IpAddress)) 
                        && object.Equals(this.EthAddress, that.EthAddress));
        }
    }
    
    public class LinkLocalIpAddressUse : Distance.Runtime.DistanceEvent {
        
        private String _IpAddress;
        
        private String _EthAddress;
        
        [FieldName("ip.address")]
        public virtual String IpAddress {
            get {
                return this._IpAddress;
            }
            set {
                this._IpAddress = value;
            }
        }
        
        [FieldName("eth.address")]
        public virtual String EthAddress {
            get {
                return this._EthAddress;
            }
            set {
                this._EthAddress = value;
            }
        }
        
        public override string Name {
            get {
                return "LinkLocalIpAddressUse";
            }
        }
        
        public override string Message {
            get {
                return string.Format("Host {0} uses link local IP address {1}.", Distance.Utils.StringUtils.ToString(this.EthAddress), Distance.Utils.StringUtils.ToString(this.IpAddress));
            }
        }
        
        public override Distance.Runtime.EventSeverity Severity {
            get {
                return Distance.Runtime.EventSeverity.Error;
            }
        }
        
        public override string ToString() {
            return string.Format("LinkLocalIpAddressUse: ip.address={0} eth.address={1}", this.IpAddress, this.EthAddress);
        }
        
        public override int GetHashCode() {
            return Distance.Utils.HashFunction.GetHashCode(this.IpAddress, this.EthAddress);
        }
        
        public override bool Equals(object obj) {
            LinkLocalIpAddressUse that = obj as LinkLocalIpAddressUse;
            return (((that != null) 
                        && object.Equals(this.IpAddress, that.IpAddress)) 
                        && object.Equals(this.EthAddress, that.EthAddress));
        }
    }
    
    public class MultipleDefaultGateways : Distance.Runtime.DistanceEvent {
        
        private String[] _Gateways;
        
        [FieldName("gateways")]
        public virtual String[] Gateways {
            get {
                return this._Gateways;
            }
            set {
                this._Gateways = value;
            }
        }
        
        public override string Name {
            get {
                return "MultipleDefaultGateways";
            }
        }
        
        public override string Message {
            get {
                return string.Format("Multiple default gateways are in use: {0}.", Distance.Utils.StringUtils.ToString(this.Gateways));
            }
        }
        
        public override Distance.Runtime.EventSeverity Severity {
            get {
                return Distance.Runtime.EventSeverity.Warning;
            }
        }
        
        public override string ToString() {
            return string.Format("MultipleDefaultGateways: gateways={0}", this.Gateways);
        }
        
        public override int GetHashCode() {
            return Distance.Utils.HashFunction.GetHashCode(this.Gateways);
        }
        
        public override bool Equals(object obj) {
            MultipleDefaultGateways that = obj as MultipleDefaultGateways;
            return ((that != null) 
                        && object.Equals(this.Gateways, that.Gateways));
        }
    }
    
    public class NetBtDuplicateName : Distance.Runtime.DistanceEvent {
        
        private String _HostName;
        
        private String[] _IpAddresses;
        
        [FieldName("host.name")]
        public virtual String HostName {
            get {
                return this._HostName;
            }
            set {
                this._HostName = value;
            }
        }
        
        [FieldName("ip.addresses")]
        public virtual String[] IpAddresses {
            get {
                return this._IpAddresses;
            }
            set {
                this._IpAddresses = value;
            }
        }
        
        public override string Name {
            get {
                return "NetBtDuplicateName";
            }
        }
        
        public override string Message {
            get {
                return string.Format("A computer on the network with the same name exists.");
            }
        }
        
        public override Distance.Runtime.EventSeverity Severity {
            get {
                return Distance.Runtime.EventSeverity.Error;
            }
        }
        
        public override string ToString() {
            return string.Format("NetBtDuplicateName: host.name={0} ip.addresses={1}", this.HostName, this.IpAddresses);
        }
        
        public override int GetHashCode() {
            return Distance.Utils.HashFunction.GetHashCode(this.HostName, this.IpAddresses);
        }
        
        public override bool Equals(object obj) {
            NetBtDuplicateName that = obj as NetBtDuplicateName;
            return (((that != null) 
                        && object.Equals(this.HostName, that.HostName)) 
                        && object.Equals(this.IpAddresses, that.IpAddresses));
        }
    }
}
