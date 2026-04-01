using UnityEngine;
using NodeSystem;
using NodeSystem.Nodes;
using NodeSystem.Serialization;

namespace NodeSystem.Runtime
{
    /// <summary>
    /// Runtime UI for creating and managing graphs via OnGUI.
    /// Attach this to a GameObject and configure through Inspector.
    ///
    /// NOTE: This script was intentionally placed in Scripts/Runtime (NOT Scripts/Editor).
    /// Unity's 'Editor' folder is a special folder — scripts inside it are compiled into
    /// the editor-only assembly and CANNOT be attached to GameObjects at runtime.
    /// </summary>
    public class GraphEditorUI : MonoBehaviour
    {
        [SerializeField] private NodeGraph currentGraph;
        [SerializeField] private bool executeOnStart = false;

        [Header("Graph Controls")]
        [SerializeField] private string graphName = "MyGraph";

        [Header("Node Creation")]
        [SerializeField] private NodeType nodeTypeToCreate = NodeType.CreateCube;

        [Header("Serialization")]
        [SerializeField] private string exportPath = "Assets/Data/graphs/export.json";
        [SerializeField] private string importPath = "Assets/Data/graphs/export.json";

        private enum NodeType
        {
            CreateCube,
            CreateSphere,
            CreateCylinder,
            Move,
            Rotate,
            Scale,
            SetValue,
            GetValue,
            Compare,
            Branch
        }

        private void Start()
        {
            if (executeOnStart && currentGraph != null)
            {
                ExecuteGraph();
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, Screen.height - 20));

            GUILayout.Label("=== NODE GRAPH EDITOR ===", new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold });
            GUILayout.Space(10);

            if (GUILayout.Button("Create New Graph", GUILayout.Height(30)))
            {
                CreateNewGraph();
            }

            if (currentGraph != null)
            {
                GUILayout.Label($"Current Graph: {currentGraph.graphName}", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
                GUILayout.Space(5);

                // Graph Controls
                GUILayout.Label("--- Graph Controls ---", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 12 });

                if (GUILayout.Button("Add Node", GUILayout.Height(25)))
                {
                    AddNode();
                }

                if (GUILayout.Button("Execute Graph", GUILayout.Height(25)))
                {
                    ExecuteGraph();
                }

                if (GUILayout.Button("Clear Graph", GUILayout.Height(25)))
                {
                    currentGraph.Clear();
                }

                GUILayout.Space(10);

                // Serialization
                GUILayout.Label("--- Serialization ---", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 12 });

                if (GUILayout.Button("Export to JSON", GUILayout.Height(25)))
                {
                    ExportGraph();
                }

                if (GUILayout.Button("Import from JSON", GUILayout.Height(25)))
                {
                    ImportGraph();
                }

                GUILayout.Space(10);

                // Display Graph Info
                DisplayGraphInfo();
            }

            GUILayout.EndArea();
        }

        private void CreateNewGraph()
        {
            currentGraph = ScriptableObject.CreateInstance<NodeGraph>();
            currentGraph.graphName = graphName;
            Debug.Log($"Created new graph: {graphName}");
        }

        private void AddNode()
        {
            if (currentGraph == null)
            {
                Debug.LogWarning("No graph selected. Create a new graph first.");
                return;
            }

            Node newNode = null;

            switch (nodeTypeToCreate)
            {
                case NodeType.CreateCube:
                    newNode = ScriptableObject.CreateInstance<CreateCubeNode>();
                    break;
                case NodeType.CreateSphere:
                    newNode = ScriptableObject.CreateInstance<CreateSphereNode>();
                    break;
                case NodeType.CreateCylinder:
                    newNode = ScriptableObject.CreateInstance<CreateCylinderNode>();
                    break;
                case NodeType.Move:
                    newNode = ScriptableObject.CreateInstance<MoveNode>();
                    break;
                case NodeType.Rotate:
                    newNode = ScriptableObject.CreateInstance<RotateNode>();
                    break;
                case NodeType.Scale:
                    newNode = ScriptableObject.CreateInstance<ScaleNode>();
                    break;
                case NodeType.SetValue:
                    newNode = ScriptableObject.CreateInstance<SetValueNode>();
                    break;
                case NodeType.GetValue:
                    newNode = ScriptableObject.CreateInstance<GetValueNode>();
                    break;
                case NodeType.Compare:
                    newNode = ScriptableObject.CreateInstance<CompareNode>();
                    break;
                case NodeType.Branch:
                    newNode = ScriptableObject.CreateInstance<BranchNode>();
                    break;
            }

            if (newNode != null)
            {
                var existingNodes = currentGraph.GetAllNodes();
                newNode.nodeName = $"{nodeTypeToCreate}_{existingNodes.Count}";

                // Wire the previous last node → new node (sequential flow)
                if (existingNodes.Count > 0)
                {
                    var previousNode = existingNodes[existingNodes.Count - 1];
                    currentGraph.SetNodeFlow(previousNode, newNode);

                    // SMART AUTO-WIRING: If the new node needs an Object, search backwards 
                    // through the list to find the closest node that outputs one!
                    var newInput = newNode.GetPort("Object", Port.PortType.Input);
                    if (newInput != null)
                    {
                        for (int i = existingNodes.Count - 1; i >= 0; i--)
                        {
                            var providerNode = existingNodes[i];
                            var prevOutput = providerNode.GetPort("Object", Port.PortType.Output);
                            if (prevOutput != null)
                            {
                                currentGraph.ConnectPorts(providerNode, "Object", newNode, "Object");
                                Debug.Log($"🔗 Connected {providerNode.nodeName}[Object] → {newNode.nodeName}[Object]");
                                break; // Stop searching once we find the provider!
                            }
                        }
                    }
                }

                currentGraph.AddNode(newNode);

                // First node automatically becomes the start node
                if (currentGraph.GetStartNode() == null)
                {
                    currentGraph.SetStartNode(newNode);
                    Debug.Log($"⭐ Start node set to: {newNode.nodeName}");
                }

                Debug.Log($"✅ Added node: {newNode.nodeName}");
            }
        }

        private void ExecuteGraph()
        {
            if (currentGraph == null)
            {
                Debug.LogWarning("No graph to execute");
                return;
            }

            Debug.Log("═══════════════════════════════════");
            Debug.Log("🎬 GRAPH EXECUTION STARTED");
            Debug.Log("═══════════════════════════════════");

            var executor = new GraphExecutor(currentGraph);
            executor.Execute();

            Debug.Log("═══════════════════════════════════");
            Debug.Log("✅ GRAPH EXECUTION COMPLETED");
            Debug.Log("═══════════════════════════════════");
        }

        private void ExportGraph()
        {
            if (currentGraph == null)
            {
                Debug.LogWarning("No graph to export");
                return;
            }

            try
            {
                string json = GraphSerializer.SerializeGraph(currentGraph);
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(exportPath));
                System.IO.File.WriteAllText(exportPath, json);
                Debug.Log($"✅ Graph exported to: {exportPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Export failed: {e.Message}");
            }
        }

        private void ImportGraph()
        {
            if (!System.IO.File.Exists(importPath))
            {
                Debug.LogError($"❌ File not found: {importPath}");
                return;
            }

            try
            {
                currentGraph = GraphDeserializer.LoadGraphFromFile(importPath);
                Debug.Log($"✅ Graph imported from: {importPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Import failed: {e.Message}");
            }
        }

        private void DisplayGraphInfo()
        {
            GUILayout.Label("--- Graph Info ---", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 12 });

            var nodes = currentGraph.GetAllNodes();
            GUILayout.Label($"Nodes in graph: {nodes.Count}", GUI.skin.box);

            GUILayout.BeginVertical(GUI.skin.box);
            foreach (var node in nodes)
            {
                GUILayout.Label($"  • {node.nodeName} ({node.GetType().Name})");
            }
            GUILayout.EndVertical();
        }
    }
}
