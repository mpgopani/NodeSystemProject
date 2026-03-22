using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NodeSystem
{
    /// <summary>
    /// Container for all nodes and their connections.
    /// Represents a complete functional block configuration.
    /// </summary>
    public class NodeGraph : ScriptableObject
    {
        public string graphId = "";
        public string graphName = "";

        [SerializeField] private List<Node> nodes = new();
        [SerializeField] private Node startNode;

        protected virtual void OnEnable()
        {
            if (string.IsNullOrEmpty(graphId))
                graphId = System.Guid.NewGuid().ToString();

            if (string.IsNullOrEmpty(graphName))
                graphName = "New Node Graph";
        }

        public void AddNode(Node node)
        {
            if (node == null) return;
            if (nodes.Contains(node)) return;

            nodes.Add(node);
            node.parentGraph = this;
            // REMOVED: EditorUtility.SetDirty(this);
        }

        public void RemoveNode(Node node)
        {
            if (node == null) return;
            nodes.Remove(node);
            if (startNode == node)
                startNode = null;
            // REMOVED: EditorUtility.SetDirty(this);
        }

        public List<Node> GetAllNodes() => new List<Node>(nodes);

        public void SetStartNode(Node node)
        {
            startNode = node;
            // REMOVED: EditorUtility.SetDirty(this);
        }

        public Node GetStartNode() => startNode;

        public void Clear()
        {
            nodes.Clear();
            startNode = null;
            // REMOVED: EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Connects two node ports together.
        /// </summary>
        public void ConnectPorts(Node sourceNode, string outputPortName, Node targetNode, string inputPortName)
        {
            var sourcePort = sourceNode.GetPort(outputPortName, Port.PortType.Output);
            var targetPort = targetNode.GetPort(inputPortName, Port.PortType.Input);

            if (sourcePort != null && targetPort != null)
            {
                sourcePort.Connect(targetPort);
                // REMOVED: EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// For flow-based nodes, set the next node to execute.
        /// </summary>
        public void SetNodeFlow(Node currentNode, Node nextNode)
        {
            currentNode.nextNode = nextNode;
            // REMOVED: EditorUtility.SetDirty(this);
        }
    }
}
