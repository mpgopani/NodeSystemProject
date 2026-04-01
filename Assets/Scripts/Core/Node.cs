using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NodeSystem
{
    /// <summary>
    /// Base class for all functional blocks in the node system.
    /// All nodes must inherit from this and implement Execute().
    /// </summary>
    public abstract class Node : ScriptableObject
    {
        public string nodeId = "";
        public string nodeName = "";
        public Vector2 editorPosition = Vector2.zero;

        [SerializeField] protected List<Port> inputPorts = new();
        [SerializeField] protected List<Port> outputPorts = new();

        // Stores execution state
        [SerializeField] protected bool hasExecuted = false;

        // Stores the next node to execute (for flow control)
        [NonSerialized] public Node nextNode;

        // Reference to parent graph for accessing other nodes
        [NonSerialized] public NodeGraph parentGraph;

        protected virtual void OnEnable()
        {
            if (string.IsNullOrEmpty(nodeId))
                nodeId = System.Guid.NewGuid().ToString();

            if (string.IsNullOrEmpty(nodeName))
                nodeName = GetType().Name;
        }

        /// <summary>
        /// Override this to implement node logic.
        /// Called when the graph executor reaches this node.
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Gets the next node to execute in the flow.
        /// Default: check output ports for flow connections.
        /// </summary>
        public virtual Node GetNextNode()
        {
            return nextNode;
        }

        /// <summary>
        /// Gets a value from an input port by looking at the connected output port.
        /// </summary>
        public object GetInputValue(string portName)
        {
            var inputPort = inputPorts.FirstOrDefault(p => p.portName == portName);
            if (inputPort == null || inputPort.connections.Count == 0)
                return null;

            // Get the value from the connected output port's parent node
            var connectedOut = inputPort.connections[0];
            var sourceNode = GetNodeFromPort(connectedOut);
            if (sourceNode == null) return null;

            return sourceNode.GetOutputValue(connectedOut.portName);
        }

        /// <summary>
        /// Sets a value for an output port.
        /// Override in subclasses that produce output.
        /// </summary>
        public virtual object GetOutputValue(string portName)
        {
            return null;
        }

        protected Port AddInputPort(string name, System.Type dataType)
        {
            var port = new Port(name, Port.PortType.Input, dataType);
            inputPorts.Add(port);
            return port;
        }

        protected Port AddOutputPort(string name, System.Type dataType)
        {
            var port = new Port(name, Port.PortType.Output, dataType);
            outputPorts.Add(port);
            return port;
        }

        public Port GetPort(string portName, Port.PortType portType)
        {
            var ports = portType == Port.PortType.Input ? inputPorts : outputPorts;
            return ports.FirstOrDefault(p => p.portName == portName);
        }

        private Node GetNodeFromPort(Port port)
        {
            if (parentGraph == null) return null;

            foreach (var node in parentGraph.GetAllNodes())
            {
                if (node.outputPorts.Contains(port))
                    return node;
            }
            return null;
        }

        public List<Port> GetInputPorts() => new List<Port>(inputPorts);
        public List<Port> GetOutputPorts() => new List<Port>(outputPorts);

        public virtual void OnReset()
        {
            hasExecuted = false;
        }
    }
}
