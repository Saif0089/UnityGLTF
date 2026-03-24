using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityGLTF.Interactivity;
using UnityGLTF.Interactivity.Schema;
using UnityGLTF.Interactivity.Export;

namespace UnityGLTF.Trace
{
    /// <summary>
    /// Serializes VS export graph data into trace-viewer's flat GraphData JSON format.
    /// trace-viewer expects: { types, declarations, variables, events, nodes }
    ///
    /// Post-processes animation/start nodes to use trace-viewer's config format
    /// (clip name as config string) instead of KHR's value inputs (animation index).
    /// Also strips pointer/get nodes which trace-viewer doesn't support.
    /// </summary>
    public static class TraceGraphDataSerializer
    {
        // Ops that trace-viewer's GraphCompiler does NOT support
        // (pointer/* is KHR-only; not needed now that animation uses trace/playAnimation directly)
        private static readonly HashSet<string> UnsupportedOps = new HashSet<string>
        {
            "pointer/get", "pointer/set", "pointer/interpolate",
        };
        // NOTE: These are kept as a safety net. The animation exporter no longer
        // creates pointer/get nodes, but other KHR exporters might.

        public static JObject Serialize(
            GltfInteractivityNode[] nodes,
            GltfInteractivityGraph.Declaration[] declarations,
            GltfTypes.TypeMapping[] types,
            GltfInteractivityGraph.Variable[] variables,
            GltfInteractivityGraph.CustomEvent[] events)
        {
            // Build op lookup: declaration index → op string
            var opByDecl = new Dictionary<int, string>();
            for (int i = 0; i < declarations.Length; i++)
                opByDecl[i] = declarations[i].op;

            // Find indices of unsupported nodes to strip
            var unsupportedIndices = new HashSet<int>();
            for (int i = 0; i < nodes.Length; i++)
            {
                if (opByDecl.TryGetValue(nodes[i].OpDeclaration, out var op) && UnsupportedOps.Contains(op))
                    unsupportedIndices.Add(i);
            }

            // Pass 1: Build full index remapping BEFORE serializing
            // (so forward references like event→animation resolve correctly)
            var indexMap = new Dictionary<int, int>();
            int newIndex = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (unsupportedIndices.Contains(i))
                    continue;
                indexMap[i] = newIndex++;
            }

            // Pass 2: Serialize nodes using the complete index map
            var serializedNodes = new JArray();
            for (int i = 0; i < nodes.Length; i++)
            {
                if (unsupportedIndices.Contains(i))
                    continue;

                var node = nodes[i];
                var op = opByDecl.TryGetValue(node.OpDeclaration, out var o) ? o : "";

                JObject serialized = SerializeNode(node, indexMap, unsupportedIndices);

                serializedNodes.Add(serialized);
            }

            // Filter declarations to only those used by remaining nodes
            var usedDecls = new HashSet<int>();
            foreach (var n in nodes)
            {
                if (!unsupportedIndices.Contains(n.Index))
                    usedDecls.Add(n.OpDeclaration);
            }

            // Rebuild declarations with new indices
            var declRemap = new Dictionary<int, int>();
            var filteredDecls = new JArray();
            int newDeclIndex = 0;
            for (int i = 0; i < declarations.Length; i++)
            {
                if (usedDecls.Contains(i))
                {
                    declRemap[i] = newDeclIndex++;
                    filteredDecls.Add(new JObject { new JProperty("op", declarations[i].op) });
                }
            }

            // Fix declaration references in serialized nodes
            foreach (JObject nodeObj in serializedNodes)
            {
                var oldDecl = (int)nodeObj["declaration"];
                if (declRemap.ContainsKey(oldDecl))
                    nodeObj["declaration"] = declRemap[oldDecl];
            }

            var typesArray = new JArray(
                from t in types select new JObject(new JProperty("signature", t.GltfSignature)));
            var varsArray = new JArray(
                from v in variables select SerializeVariable(v));
            var eventsArray = new JArray(
                from e in events select SerializeEvent(e));

            return new JObject
            {
                new JProperty("types", typesArray),
                new JProperty("declarations", filteredDecls),
                new JProperty("variables", varsArray),
                new JProperty("events", eventsArray),
                new JProperty("nodes", serializedNodes),
            };
        }

        /// <summary>
        /// Serialize animation/start in trace-viewer format:
        /// Config: clip (string), target (string), loop (bool)
        /// Values: speed (float) only
        /// Strips: animation (int index), startTime, endTime
        /// </summary>
        private static JObject SerializeAnimationStartNode(
            GltfInteractivityNode node,
            Dictionary<int, int> indexMap,
            HashSet<int> unsupportedIndices)
        {
            // Build config from stored metadata
            var config = new JObject();
            foreach (var kvp in node.Configuration)
            {
                if (kvp.Value.Value != null)
                {
                    var configObj = new JObject();
                    GltfInteractivityNode.ValueSerializer.Serialize(kvp.Value.Value, configObj);
                    config.Add(kvp.Key, configObj);
                }
            }

            // Only keep speed from value inputs (strip animation, startTime, endTime)
            var values = new JObject();
            if (node.ValueInConnection.TryGetValue("speed", out var speedData))
            {
                values.Add("speed", SerializeValueSocket(speedData, indexMap, unsupportedIndices));
            }

            // Remap flow connections
            var flows = new JObject();
            foreach (var flow in node.FlowConnections)
            {
                if (flow.Value.Node != null && flow.Value.Node.HasValue)
                {
                    var targetIdx = flow.Value.Node.Value;
                    if (!unsupportedIndices.Contains(targetIdx) && indexMap.ContainsKey(targetIdx))
                    {
                        flows.Add(flow.Key, new JObject
                        {
                            new JProperty("node", indexMap[targetIdx]),
                            new JProperty("socket", flow.Value.Socket),
                        });
                    }
                }
            }

            var result = new JObject
            {
                new JProperty("declaration", node.OpDeclaration),
            };
            if (config.Count > 0) result.Add("configuration", config);
            if (values.Count > 0) result.Add("values", values);
            if (flows.Count > 0) result.Add("flows", flows);

            return result;
        }

        /// <summary>
        /// Default node serialization with index remapping and unsupported-node filtering.
        /// </summary>
        private static JObject SerializeNode(
            GltfInteractivityNode node,
            Dictionary<int, int> indexMap,
            HashSet<int> unsupportedIndices)
        {
            // Config
            var config = new JObject();
            foreach (var kvp in node.Configuration)
            {
                var serialized = kvp.Value.SerializeObject();
                if (serialized != null)
                    config.Add(kvp.Key, serialized);
            }

            // Values with remapped node references
            var values = new JObject();
            foreach (var kvp in node.ValueInConnection)
            {
                values.Add(kvp.Key, SerializeValueSocket(kvp.Value, indexMap, unsupportedIndices));
            }

            // Flows with remapped node references
            var flows = new JObject();
            foreach (var flow in node.FlowConnections)
            {
                if (flow.Value.Node != null && flow.Value.Node.HasValue)
                {
                    var targetIdx = flow.Value.Node.Value;
                    if (!unsupportedIndices.Contains(targetIdx) && indexMap.ContainsKey(targetIdx))
                    {
                        flows.Add(flow.Key, new JObject
                        {
                            new JProperty("node", indexMap[targetIdx]),
                            new JProperty("socket", flow.Value.Socket),
                        });
                    }
                }
            }

            var result = new JObject
            {
                new JProperty("declaration", node.OpDeclaration),
            };
            if (config.Count > 0) result.Add("configuration", config);
            if (values.Count > 0) result.Add("values", values);
            if (flows.Count > 0) result.Add("flows", flows);

            return result;
        }

        private static JObject SerializeValueSocket(
            GltfInteractivityNode.ValueSocketData data,
            Dictionary<int, int> indexMap,
            HashSet<int> unsupportedIndices)
        {
            var obj = new JObject();

            if (data.Node != null && data.Node.HasValue)
            {
                var srcIdx = data.Node.Value;
                if (!unsupportedIndices.Contains(srcIdx) && indexMap.ContainsKey(srcIdx))
                {
                    obj.Add("node", indexMap[srcIdx]);
                    obj.Add("socket", data.Socket);
                }
            }
            else if (data.Value != null)
            {
                obj.Add("type", data.Type);
                GltfInteractivityNode.ValueSerializer.Serialize(data.Value, obj);
            }

            return obj;
        }

        private static JObject SerializeVariable(GltfInteractivityGraph.Variable variable)
        {
            var jo = new JObject
            {
                new JProperty("name", variable.Id),
                new JProperty("type", variable.Type),
            };
            GltfInteractivityNode.ValueSerializer.Serialize(variable.Value, jo);
            return jo;
        }

        private static JObject SerializeEvent(GltfInteractivityGraph.CustomEvent evt)
        {
            var values = new JObject();
            foreach (var v in evt.Values)
                values.Add(v.Key, v.Value.SerializeObject());

            return new JObject
            {
                new JProperty("id", evt.Id),
                new JProperty("values", values),
            };
        }
    }
}
