using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NodeSystem.Serialization
{
    /// <summary>
    /// Deserializes a JSON string back into a NodeGraph.
    /// Reconstructs all nodes, their data, and connections.
    /// </summary>
    public static class GraphDeserializer
    {
        public static NodeGraph DeserializeGraph(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("JSON string is empty");
                return null;
            }

            GraphSerializationData data = JsonUtility.FromJson<GraphSerializationData>(json);
            if (data == null)
            {
                Debug.LogError("Failed to deserialize JSON");
                return null;
            }

            return ReconstructGraph(data);
        }

        public static NodeGraph LoadGraphFromFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError($"File not found: {filePath}");
                return null;
            }

            string json = System.IO.File.ReadAllText(filePath);
            return DeserializeGraph(json);
        }

        private static NodeGraph ReconstructGraph(GraphSerializationData data)
        {
            // Create new graph
            NodeGraph graph = ScriptableObject.CreateInstance<NodeGraph>();
            graph.graphId = data.graphId;
            graph.graphName = data.graphName;

            // Dictionary to map node IDs to reconstructed nodes
            Dictionary<string, Node> nodeMap = new();

            // Reconstruct all nodes
            foreach (var nodeData in data.nodes)
            {
                var node = ReconstructNode(nodeData);
                if (node != null)
                {
                    graph.AddNode(node);
                    nodeMap[nodeData.nodeId] = node;
                }
            }

            // Reconstruct connections
            foreach (var connection in data.connections)
            {
                if (nodeMap.TryGetValue(connection.sourceNodeId, out var sourceNode) &&
                    nodeMap.TryGetValue(connection.targetNodeId, out var targetNode))
                {
                    graph.ConnectPorts(sourceNode, connection.sourcePortName,
                                      targetNode, connection.targetPortName);
                }
            }

            // Restore nextNode flow
            foreach (var nodeData in data.nodes)
            {
                if (!string.IsNullOrEmpty(nodeData.nextNodeId) && 
                    nodeMap.TryGetValue(nodeData.nodeId, out var sourceNode) &&
                    nodeMap.TryGetValue(nodeData.nextNodeId, out var targetNode))
                {
                    sourceNode.nextNode = targetNode;
                }
            }

            // Set start node
            if (!string.IsNullOrEmpty(data.startNodeId) && nodeMap.TryGetValue(data.startNodeId, out var startNode))
            {
                graph.SetStartNode(startNode);
            }

            Debug.Log($"Graph '{graph.graphName}' loaded with {graph.GetAllNodes().Count} nodes");
            return graph;
        }

        private static Node ReconstructNode(NodeSerializationData nodeData)
        {
            // Find the node type
            System.Type nodeType = System.Type.GetType(nodeData.nodeType);
            if (nodeType == null)
            {
                Debug.LogError($"Cannot find node type: {nodeData.nodeType}");
                return null;
            }

            // Create instance
            Node node = ScriptableObject.CreateInstance(nodeType) as Node;
            if (node == null)
            {
                Debug.LogError($"Failed to create node of type: {nodeData.nodeType}");
                return null;
            }

            node.nodeId = nodeData.nodeId;
            node.nodeName = nodeData.nodeName;
            node.editorPosition = new Vector2(nodeData.editorPosition.x, nodeData.editorPosition.y);

            // Restore field values
            var fields = nodeType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var fieldData = nodeData.fields.FirstOrDefault(f => f.fieldName == field.Name);
                if (fieldData != null)
                {
                    try
                    {
                        object value = ParseValue(fieldData.fieldValue, field.FieldType);
                        field.SetValue(node, value);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Failed to restore field {field.Name}: {e.Message}");
                    }
                }
            }

            return node;
        }

        private static object ParseValue(string value, System.Type targetType)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            try
            {
                if (targetType == typeof(int))
                    return int.Parse(value);
                else if (targetType == typeof(float))
                    return float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
                else if (targetType == typeof(bool))
                    return bool.Parse(value);
                else if (targetType == typeof(string))
                    return value;
                else if (targetType.IsEnum)
                    return System.Enum.Parse(targetType, value);
                else if (targetType == typeof(Vector3))
                {
                    // Input format: "(1.5, 2.5, 3.5)"
                    string cleanValue = value.Trim('(', ')').Trim();

                    // Split by comma - returns string[]
                    string[] parts = cleanValue.Split(new char[] { ',' });

                    // Check we have exactly 3 parts
                    if (parts.Length >= 3)
                    {
                        // Parse each individual element
                        float x = float.Parse(parts[0].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        float y = float.Parse(parts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        float z = float.Parse(parts[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        return new Vector3(x, y, z);
                    }

                    Debug.LogWarning($"Invalid Vector3 format: {value}");
                    return Vector3.zero;
                }
                else if (targetType == typeof(Vector2))
                {
                    string cleanValue = value.Trim('(', ')').Trim();
                    string[] parts = cleanValue.Split(new char[] { ',' });
                    if (parts.Length >= 2)
                    {
                        float x = float.Parse(parts[0].Trim());
                        float y = float.Parse(parts[1].Trim());
                        return new Vector2(x, y);
                    }
                    Debug.LogWarning($"Invalid Vector2 format: {value}");
                    return Vector2.zero;
                }
                else
                {
                    return value;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"ParseValue error - Type: {targetType.Name}, Value: {value}, Error: {e.Message}");
                return null;
            }
        }

    }
}
