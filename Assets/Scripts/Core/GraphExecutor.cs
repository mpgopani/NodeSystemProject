using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NodeSystem
{
    /// <summary>
    /// Executes a node graph at runtime.
    /// Handles traversal through nodes and data flow.
    /// </summary>
    public class GraphExecutor
    {
        private NodeGraph graph;
        private HashSet<Node> executedNodes = new();
        private Queue<Node> executionQueue = new();
        private int maxExecutionSteps = 1000; // Prevent infinite loops

        public GraphExecutor(NodeGraph nodeGraph)
        {
            graph = nodeGraph;
        }

        /// <summary>
        /// Execute the entire graph starting from the start node.
        /// </summary>
        public void Execute()
        {
            Reset();

            if (graph == null)
            {
                Debug.LogError("No graph assigned to executor");
                return;
            }

            var startNode = graph.GetStartNode();
            if (startNode == null)
            {
                Debug.LogError("Graph has no start node");
                return;
            }

            ExecuteNode(startNode);

            int steps = 0;
            while (executionQueue.Count > 0 && steps < maxExecutionSteps)
            {
                var node = executionQueue.Dequeue();
                ExecuteNode(node);
                steps++;
            }

            if (steps >= maxExecutionSteps)
                Debug.LogWarning("Graph execution exceeded maximum steps - possible infinite loop");
        }

        private void ExecuteNode(Node node)
        {
            if (node == null) return;

            // Avoid executing the same node twice
            if (executedNodes.Contains(node))
                return;

            executedNodes.Add(node);

            Debug.Log($"[Executor] Executing: {node.nodeName}");
            node.Execute();

            // Get the next node to execute
            var nextNode = node.GetNextNode();
            if (nextNode != null && !executedNodes.Contains(nextNode))
            {
                executionQueue.Enqueue(nextNode);
            }
        }

        private void Reset()
        {
            executedNodes.Clear();
            executionQueue.Clear();

            // Reset all nodes
            foreach (var node in graph.GetAllNodes())
            {
                node.OnReset();
            }
        }
    }
}
    