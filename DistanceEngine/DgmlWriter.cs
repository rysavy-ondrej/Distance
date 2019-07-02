using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using NRules.Diagnostics;

namespace NRules.Diagnostics
{
    public class GraphWriter
    {
        private readonly Dictionary<NodeInfo, int> _idMap;
        private readonly SessionSnapshot _snapshot;
        private readonly XNamespace _namespace = XNamespace.Get("http://www.gexf.net/1.2draft");

        public GraphWriter(SessionSnapshot snapshot)
        {
            _snapshot = snapshot;

            _idMap = snapshot.Nodes
                .Select((x, i) => new {Node = x, Index = i})
                .ToDictionary(x => x.Node, x => x.Index);
        }

        public void WriteTo(XmlWriter writer)
        {
            var document = new XDocument(new XDeclaration("1.0", "utf-8", null));

            var root = new XElement(Name("gexf"));
            var meta = new XElement(Name("meta"), new XAttribute("lastmodifieddate", DateTime.Now.ToShortDateString()),
                new XElement(Name("creator"), "DistanceEngine"), new XElement(Name("description"),"Rete Network"));
            var graph = new XElement(Name("graph"), new XAttribute("mode", "static"), new XAttribute("defaultedgetype", "directed"));
            var nodes = new XElement(Name("nodes"));
            var edges = new XElement(Name("edges"));
            WriteNodes(nodes);
            WriteLinks(edges);
            graph.Add(nodes);
            graph.Add(edges);
            root.Add(graph);
            document.Add(root);
            document.WriteTo(writer);
        }

        private void WriteNodes(XElement nodes)
        {
            foreach (NodeInfo nodeInfo in _snapshot.Nodes)
            {
                var labelComponents = new[] {nodeInfo.NodeType.ToString(), nodeInfo.Details}
                    .Union(nodeInfo.Conditions)
                    .Union(nodeInfo.Expressions)
                    .Where(x => !string.IsNullOrEmpty(x));
                string label = string.Join("\n", labelComponents);
                var node = new XElement(Name("node"),
                                        new XAttribute("id", Id(nodeInfo)),
                                        new XAttribute("label",label));
                nodes.Add(node);

                for (int i = 0; i < nodeInfo.Items.Length; i++)
                {
                    var itemNode = new XElement(Name("node"),
                        new XAttribute("id", SubNodeId(nodeInfo, i)),
                        new XAttribute("data", nodeInfo.Items[i]));
                    nodes.Add(itemNode);
                }
            }
        }

        private void WriteLinks(XElement links)
        {
            foreach (var linkInfo in _snapshot.Links)
            {
                var link = new XElement(Name("edge"),
                                        new XAttribute("source", Id(linkInfo.Source)),
                                        new XAttribute("target", Id(linkInfo.Target)));
                links.Add(link);
            }
            foreach (var nodeInfo in _snapshot.Nodes)
            {
                for (int i = 0; i < nodeInfo.Items.Length; i++)
                {
                    var link = new XElement(Name("edge"),
                                            new XAttribute("source", Id(nodeInfo)),
                                            new XAttribute("target", SubNodeId(nodeInfo, i)));
                    links.Add(link);
                }
            }
        }

        private void WriteCategories(XElement categories)
        {
            categories.Add(Category(NodeType.Root, "Black"));
            categories.Add(Category(NodeType.Type, "Orange"));
            categories.Add(Category(NodeType.Selection, "Blue"));
            categories.Add(Category(NodeType.AlphaMemory, "Red"));
            categories.Add(Category(NodeType.Dummy, "Silver"));
            categories.Add(Category(NodeType.Join, "Blue"));
            categories.Add(Category(NodeType.Not, "Brown"));
            categories.Add(Category(NodeType.Exists, "Brown"));
            categories.Add(Category(NodeType.Aggregate, "Brown"));
            categories.Add(Category(NodeType.BetaMemory, "Green"));
            categories.Add(Category(NodeType.Adapter, "Silver"));
            categories.Add(Category(NodeType.Binding, "LightBlue"));
            categories.Add(Category(NodeType.Terminal, "Silver"));
            categories.Add(Category(NodeType.Rule, "Purple"));
        }

        private XElement Category(NodeType category, string background)
        {
            return new XElement(Name("Category"),
                                new XAttribute("Id", category.ToString()),
                                new XAttribute("Label", category.ToString()),
                                new XAttribute("Background", background));
        }

        private XName Name(string name)
        {
            return _namespace + name;
        }

        private int Id(NodeInfo nodeInfo)
        {
            return _idMap[nodeInfo];
        }

        private string SubNodeId(NodeInfo nodeInfo, int itemIndex)
        {
            return $"{Id(nodeInfo)}_{itemIndex}";
        }
    }
}