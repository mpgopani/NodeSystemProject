using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeSystem.Serialization
{
    /// <summary>
    /// Serializes a NodeGraph to JSON format.
    /// Preserves all node data, connections, and metadata.
    /// </summary>
    public static class GraphSerializer
    {
        /// <summary>
        /// Export a graph to JSON string.
        /// </summary>
        public static string SerializeGraph(NodeGraph graph)
        {
            if (graph == null)
            {
                Debug.LogError("Cannot serialize null graph");
                return "";
            }

            var graphData = new GraphSerializationData();
            graphData.graphId = graph.graphId;
            graphData.graphName = graph.graphName;

            var nodes = graph.GetAllNodes();
            foreach (var node in nodes)
            {
                var nodeData = SerializeNode(node);
                graphData.nodes.Add(nodeData);
            }

            // Serialize connections
            foreach (var node in nodes)
            {
                foreach (var outputPort in node.GetOutputPorts())
                {
                    if (outputPort.connectedPort != null)
                    {
                        var connection = new ConnectionData
                        {
                            sourceNodeId = node.nodeId,
                            sourcePortName = outputPort.portName,
                            targetNodeId = FindNodeIdWithPort(nodes, outputPort.connectedPort),
                            targetPortName = outputPort.connectedPort.portName
                        };
                        graphData.connections.Add(connection);
                    }
                }
            }

            // Serialize start node
            var startNode = graph.GetStartNode();
            if (startNode != null)
                graphData.startNodeId = startNode.nodeId;

            string json = JsonUtility.ToJson(graphData, true);
            return json;
        }

        /// <summary>
        /// Save graph to JSON file.
        /// </summary>
        public static void SaveGraphToFile(NodeGraph graph, string filePath)
        {
            string json = SerializeGraph(graph);
            System.IO.File.WriteAllText(filePath, json);
            Debug.Log($"Graph saved to {filePath}");
        }

        private static NodeSerializationData SerializeNode(Node node)
        {
            var nodeData = new NodeSerializationData
            {
                nodeId = node.nodeId,
                nodeName = node.nodeName,
                nodeType = node.GetType().FullName,
                editorPosition = new FloatVector2 { x = node.editorPosition.x, y = node.editorPosition.y },
                nextNodeId = node.nextNode != null ? node.nextNode.nodeId : ""
            };

            // Serialize public and [SerializeField] non-public fields
            var fields = node.GetType().GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance
            );

            foreach (var field in fields)
            {
                bool shouldSerialize = field.IsPublic || System.Attribute.IsDefined(field, typeof(SerializeField));
                
                // Only serialize types we know how to parse back (primitive, string, Vector2, Vector3)
                bool isSupportedType = field.FieldType.IsPrimitive || 
                                       field.FieldType == typeof(string) || 
                                       field.FieldType == typeof(Vector2) || 
                                       field.FieldType == typeof(Vector3);

                if (shouldSerialize && isSupportedType)
                {
                    var fieldData = new FieldData
                    {
                        fieldName = field.Name,
                        fieldType = field.FieldType.FullName,
                        fieldValue = field.GetValue(node)?.ToString() ?? ""
                    };
                    nodeData.fields.Add(fieldData);
                }
            }

            return nodeData;
        }

        private static string FindNodeIdWithPort(List<Node> nodes, Port port)
        {
            foreach (var node in nodes)
            {
                if (node.GetInputPorts().Contains(port) || node.GetOutputPorts().Contains(port))
                    return node.nodeId;
            }
            return "";
        }
    }

    // Serialization data classes
    [System.Serializable]
    public class GraphSerializationData
    {
        public string graphId = "";
        public string graphName = "";
        public string startNodeId = "";
        public List<NodeSerializationData> nodes = new();
        public List<ConnectionData> connections = new();
    }

    [System.Serializable]
    public class NodeSerializationData
    {
        public string nodeId = "";
        public string nodeName = "";
        public string nodeType = "";
        public string nextNodeId = "";
        public FloatVector2 editorPosition = new();
        public List<FieldData> fields = new();
    }

    [System.Serializable]
    public class FieldData
    {
        public string fieldName = "";
        public string fieldType = "";
        public string fieldValue = "";
    }

    [System.Serializable]
    public class ConnectionData
    {
        public string sourceNodeId = "";
        public string sourcePortName = "";
        public string targetNodeId = "";
        public string targetPortName = "";
    }

    [System.Serializable]
    public class FloatVector2
    {
        public float x;
        public float y;
    }
}
