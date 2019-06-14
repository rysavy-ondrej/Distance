//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Distance.Diagnostics.Arp {
    using Distance.Runtime;
    using Distance.Utils;
    using System;
    
    
    public class ArpPacket : Distance.Runtime.DistanceFact {
        
        private Int32 _FrameNumber;
        
        private Double _FrameTimeRelative;
        
        private String _EthSrc;
        
        private String _EthDst;
        
        private Int32 _ArpOpcode;
        
        private String _ArpSrcHw;
        
        private String _ArpDstHw;
        
        private String _ArpSrcProtoIpv4;
        
        private String _ArpDstProtoIpv4;
        
        public static string Filter = "arp";
        
        public static string[] Fields = new string[] {
                "frame.number",
                "frame.time_relative",
                "eth.src",
                "eth.dst",
                "arp.opcode",
                "arp.src.hw",
                "arp.dst.hw",
                "arp.src.proto_ipv4",
                "arp.dst.proto_ipv4"};
        
        [FieldName("frame.number")]
        public virtual Int32 FrameNumber {
            get {
                return this._FrameNumber;
            }
            set {
                this._FrameNumber = value;
            }
        }
        
        [FieldName("frame.time_relative")]
        public virtual Double FrameTimeRelative {
            get {
                return this._FrameTimeRelative;
            }
            set {
                this._FrameTimeRelative = value;
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
        
        [FieldName("arp.opcode")]
        public virtual Int32 ArpOpcode {
            get {
                return this._ArpOpcode;
            }
            set {
                this._ArpOpcode = value;
            }
        }
        
        [FieldName("arp.src.hw")]
        public virtual String ArpSrcHw {
            get {
                return this._ArpSrcHw;
            }
            set {
                this._ArpSrcHw = value;
            }
        }
        
        [FieldName("arp.dst.hw")]
        public virtual String ArpDstHw {
            get {
                return this._ArpDstHw;
            }
            set {
                this._ArpDstHw = value;
            }
        }
        
        [FieldName("arp.src.proto_ipv4")]
        public virtual String ArpSrcProtoIpv4 {
            get {
                return this._ArpSrcProtoIpv4;
            }
            set {
                this._ArpSrcProtoIpv4 = value;
            }
        }
        
        [FieldName("arp.dst.proto_ipv4")]
        public virtual String ArpDstProtoIpv4 {
            get {
                return this._ArpDstProtoIpv4;
            }
            set {
                this._ArpDstProtoIpv4 = value;
            }
        }
        
        public override string ToString() {
            return string.Format("ArpPacket: frame.number={0} frame.time_relative={1} eth.src={2} eth.dst={3} arp.o" +
                    "pcode={4} arp.src.hw={5} arp.dst.hw={6} arp.src.proto_ipv4={7} arp.dst.proto_ipv" +
                    "4={8}", this.FrameNumber, this.FrameTimeRelative, this.EthSrc, this.EthDst, this.ArpOpcode, this.ArpSrcHw, this.ArpDstHw, this.ArpSrcProtoIpv4, this.ArpDstProtoIpv4);
        }
        
        public override int GetHashCode() {
            return Distance.Utils.HashFunction.GetHashCode(this.FrameNumber, this.FrameTimeRelative, this.EthSrc, this.EthDst, this.ArpOpcode, this.ArpSrcHw, this.ArpDstHw, this.ArpSrcProtoIpv4, this.ArpDstProtoIpv4);
        }
        
        public override bool Equals(object obj) {
            ArpPacket that = obj as ArpPacket;
            return ((((((((((that != null) 
                        && object.Equals(this.FrameNumber, that.FrameNumber)) 
                        && object.Equals(this.FrameTimeRelative, that.FrameTimeRelative)) 
                        && object.Equals(this.EthSrc, that.EthSrc)) 
                        && object.Equals(this.EthDst, that.EthDst)) 
                        && object.Equals(this.ArpOpcode, that.ArpOpcode)) 
                        && object.Equals(this.ArpSrcHw, that.ArpSrcHw)) 
                        && object.Equals(this.ArpDstHw, that.ArpDstHw)) 
                        && object.Equals(this.ArpSrcProtoIpv4, that.ArpSrcProtoIpv4)) 
                        && object.Equals(this.ArpDstProtoIpv4, that.ArpDstProtoIpv4));
        }
        
        public static ArpPacket Create(System.Func<string, string, string> mapper, string[] values) {
            ArpPacket newObj = new ArpPacket();
            newObj._FrameNumber = mapper.Invoke("frame.number", values[0]).ToInt32();
            newObj._FrameTimeRelative = mapper.Invoke("frame.time_relative", values[1]).ToDouble();
            newObj._EthSrc = mapper.Invoke("eth.src", values[2]).ToString();
            newObj._EthDst = mapper.Invoke("eth.dst", values[3]).ToString();
            newObj._ArpOpcode = mapper.Invoke("arp.opcode", values[4]).ToInt32();
            newObj._ArpSrcHw = mapper.Invoke("arp.src.hw", values[5]).ToString();
            newObj._ArpDstHw = mapper.Invoke("arp.dst.hw", values[6]).ToString();
            newObj._ArpSrcProtoIpv4 = mapper.Invoke("arp.src.proto_ipv4", values[7]).ToString();
            newObj._ArpDstProtoIpv4 = mapper.Invoke("arp.dst.proto_ipv4", values[8]).ToString();
            return newObj;
        }
    }
    
    public class ArpRequestReply : Distance.Runtime.DistanceDerived {
        
        private ArpPacket _Request;
        
        private ArpPacket _Reply;
        
        [FieldName("request")]
        public virtual ArpPacket Request {
            get {
                return this._Request;
            }
            set {
                this._Request = value;
            }
        }
        
        [FieldName("reply")]
        public virtual ArpPacket Reply {
            get {
                return this._Reply;
            }
            set {
                this._Reply = value;
            }
        }
        
        public override string ToString() {
            return string.Format("ArpRequestReply: request={0} reply={1}", this.Request, this.Reply);
        }
        
        public override int GetHashCode() {
            return Distance.Utils.HashFunction.GetHashCode(this.Request, this.Reply);
        }
        
        public override bool Equals(object obj) {
            ArpRequestReply that = obj as ArpRequestReply;
            return (((that != null) 
                        && object.Equals(this.Request, that.Request)) 
                        && object.Equals(this.Reply, that.Reply));
        }
    }
    
    public class ArpUnanswered : Distance.Runtime.DistanceDerived {
        
        private ArpPacket _Request;
        
        [FieldName("request")]
        public virtual ArpPacket Request {
            get {
                return this._Request;
            }
            set {
                this._Request = value;
            }
        }
        
        public override string ToString() {
            return string.Format("ArpUnanswered: request={0}", this.Request);
        }
        
        public override int GetHashCode() {
            return Distance.Utils.HashFunction.GetHashCode(this.Request);
        }
        
        public override bool Equals(object obj) {
            ArpUnanswered that = obj as ArpUnanswered;
            return ((that != null) 
                        && object.Equals(this.Request, that.Request));
        }
    }
    
    public class ArpAddressMapping : Distance.Runtime.DistanceDerived {
        
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
            return string.Format("ArpAddressMapping: ip.addr={0} eth.addr={1}", this.IpAddr, this.EthAddr);
        }
        
        public override int GetHashCode() {
            return Distance.Utils.HashFunction.GetHashCode(this.IpAddr, this.EthAddr);
        }
        
        public override bool Equals(object obj) {
            ArpAddressMapping that = obj as ArpAddressMapping;
            return (((that != null) 
                        && object.Equals(this.IpAddr, that.IpAddr)) 
                        && object.Equals(this.EthAddr, that.EthAddr));
        }
    }
}
