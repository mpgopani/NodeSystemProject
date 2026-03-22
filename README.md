# Node System - Digital Twin Architecture

A dynamic, data-driven visual scripting engine built from scratch in Unity 6, designed specifically for industrial "Digital Twin" simulations, logical sequence generation, and mathematical validation. 

This project prioritizes high-level architectural clarity, performance stability, and modularity over visual polish, providing a robust logic framework capable of powering enterprise-level simulation environments.

## Features

- **Total Execution Decoupling**: Nodes are generated actively in memory (inheriting from `ScriptableObject`). Adding unlimited node types requires zero modifications to the core executor or UI layer.
- **Custom JSON Serialization**: Replaces standard Unity serialization limitations. Uses robust C# Reflection (`BindingFlags.NonPublic | BindingFlags.Public`) to dynamically map unexposed variables, port connections, and execution flows into transportable JSON.
- **Deterministic Flow Engine**: A custom `GraphExecutor` handles execution flow, mapping data through node ports recursively, and traversing the graph deterministically using an execution queue mechanism rather than relying on standard Unity events.
- **Dynamic Variable Injection**: Edit JSON files on the fly and import them into the active runtime. The deserializer automatically rebuilds the node sequence and uses Type matching to reinject modified parameter values straight into memory.

## Architectural Structure

The codebase is strictly separated into modular namespaces:
- `NodeSystem`: The **Core Layer**. Contains the base structures (`Node.cs`, `NodeGraph.cs`, `Port.cs`).
- `NodeSystem.Nodes`: The **Functional Layer**. Where all concrete logic nodes live (`CreateCubeNode.cs`, `MoveNode.cs`, `BranchNode.cs`).
- `NodeSystem.Serialization`: The **Compiler Layer**. Houses `GraphSerializer` and `GraphDeserializer` for saving/loading stringified states.
- `NodeSystem.Runtime`: The **Front-End Layer**. Houses `GraphEditorUI`, the only element of the project that physically acts as a Unity `MonoBehaviour`.

## How to Run & Test

1. Open the project in Unity 6000.x.x.
2. Load the main scene and ensure `GraphEditorUI` is attached to a GameObject in the hierarchy.
3. Hit **Play**.
4. Use the custom `OnGUI` menu on the left side of the screen to **Create A New Graph**.
5. Add nodes (e.g., `Create Cube` -> `Move Node`). The system will automatically sequence their `nextNode` flows and automatically plug together identical ports.
6. Click **Export to JSON** to save the active graph hierarchy.
7. Open `Assets/Data/graphs/export.json` in any text editor, modify a serialized variable (like `targetPosition` to `(5.0, 3.0, 0.0)`), and save the file.
8. Back in Unity, hit **Import from JSON** and then **Execute Graph** to see the logic reconstruct and play out instantly.

## Node Types Built-In

* **Creation Nodes**: `CreateCubeNode`, `CreateSphereNode`, `CreateCylinderNode`
* **Transform Nodes**: `MoveNode`, `RotateNode`, `ScaleNode`
* **Logic Nodes**:
  * `BranchNode`: Routes execution flow dynamically based on boolean states.
  * `CompareNode`: Evaluates mathematical float comparisons (`>`, `<`, `==`).
  * `SetValueNode` / `GetValueNode`: A shared static global Dictionary allowing cross-graph memory variables.

## 💡 Examples of How to Use Nodes via JSON

Because this system embraces decoupled data inputs, you can construct massive factory logic sequences entirely without the Unity Editor by editing the JSON configurations directly!

### 1. The `MoveNode`
**Goal:** Spawn a physical cube and move it to coordinates (10, 5, 0).
**Workflow:**
1. In the UI, add `Create Cube` -> `Move`.
2. Export to JSON. Open the file and locate the `Move_1` node block in the `nodes` array.
3. Find the `targetPosition` field inside its `fields` array and change it:
```json
{
    "fieldName": "targetPosition",
    "fieldType": "UnityEngine.Vector3",
    "fieldValue": "(10.0, 5.0, 0.0)"
}
```
4. Import the JSON and Execute. The cube will instantly spawn and jump to `X:10, Y:5, Z:0`.

### 2. The `SetValueNode` and `GetValueNode` (Global Memory)
**Goal:** Store a constant global math variable so multiple independent sequences can access it without drawing wires.
**Workflow:**
1. Create a `SetValue` node. In JSON, find its `variableName` and `floatValue` fields.
```json
{
    "fieldName": "variableName",
    "fieldType": "System.String",
    "fieldValue": "machine_speed"
},
{
    "fieldName": "floatValue",
    "fieldType": "System.Single",
    "fieldValue": "150.5"
}
```
2. Somewhere else in the graph, create a `GetValue` node. Change its `variableName` inside the JSON to exactly `"machine_speed"`. Its Output port will now mathematically push `150.5` through the wire!

### 3. The `BranchNode` (Dynamic If-Else Execution)
**Goal:** Route code execution down Path A if a condition is true, or Path B if it's false.
**Workflow:**
1. Assume node `True_Response` has ID `"ABCD-1234"` and `False_Response` has ID `"WXYZ-9876"`.
2. In the JSON generated for the `BranchNode`, modify the hidden node references to tell the system where to jump:
```json
{
    "fieldName": "ifTrueNode",
    "fieldType": "NodeSystem.Node",
    "fieldValue": "ABCD-1234"
},
{
    "fieldName": "ifFalseNode",
    "fieldType": "NodeSystem.Node",
    "fieldValue": "WXYZ-9876"
}
```
3. Based on whatever is plugged into its Input Port, the engine will dynamically switch its `nextNode` flow to one of those two IDs!

## Future Optimizations (Scale)

While current execution runs seamlessly via dynamic Memory Objects, simulating massive numbers of nodes for real-world digital factories (e.g. 100,000+ operations/frame) would require extracting the execution pipeline into Unity's **Data-Oriented Technology Stack (DOTS)**. In such an implementation, `NodeGraph` components would be utilized purely as Editor/Authoring structures that compile their data directly down into C# Job Systems and ECS Entities.
