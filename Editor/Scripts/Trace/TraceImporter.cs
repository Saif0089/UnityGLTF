using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;
using UnityGLTF.Trace.Components;

namespace UnityGLTF.Trace
{
    /// <summary>
    /// Imports a .trace file (ZIP with scene.json + assets/) into the Unity scene.
    /// Creates GameObjects with appropriate Trace components matching trace-viewer behaviors.
    /// </summary>
    public static class TraceImporter
    {
        [MenuItem("Assets/UnityGLTF/Import .trace", false, 37)]
        [MenuItem("GameObject/UnityGLTF/Import .trace", false, 37)]
        private static void ImportTraceMenu()
        {
            var path = EditorUtility.OpenFilePanel("Import .trace", "", "trace");
            if (string.IsNullOrEmpty(path)) return;

            Import(path);
        }

        public static GameObject Import(string tracePath)
        {
            if (!File.Exists(tracePath))
            {
                Debug.LogError($"[TraceImport] File not found: {tracePath}");
                return null;
            }

            // Extract ZIP
            var tempDir = Path.Combine(Path.GetTempPath(), "trace_import_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                ZipFile.ExtractToDirectory(tracePath, tempDir);

                // Read scene.json
                var sceneJsonPath = Path.Combine(tempDir, "scene.json");
                if (!File.Exists(sceneJsonPath))
                {
                    Debug.LogError("[TraceImport] scene.json not found in .trace archive");
                    return null;
                }

                var sceneJson = JObject.Parse(File.ReadAllText(sceneJsonPath));
                var sceneName = sceneJson["sceneName"]?.ToString() ?? Path.GetFileNameWithoutExtension(tracePath);
                var content = sceneJson["content"] as JArray;
                if (content == null || content.Count == 0)
                {
                    Debug.LogError("[TraceImport] Empty content in scene.json");
                    return null;
                }

                // Import GLB assets into project
                var assetsDir = Path.Combine(tempDir, "assets");
                var projectAssetsDir = Path.Combine("Assets", "TraceImports", sceneName);
                if (!Directory.Exists(projectAssetsDir))
                    Directory.CreateDirectory(projectAssetsDir);

                if (Directory.Exists(assetsDir))
                {
                    foreach (var file in Directory.GetFiles(assetsDir))
                    {
                        var destPath = Path.Combine(projectAssetsDir, Path.GetFileName(file));
                        File.Copy(file, destPath, true);
                    }
                    AssetDatabase.Refresh();
                }

                // Build scene objects
                var objectMap = new Dictionary<string, GameObject>();
                GameObject firstRoot = null;

                foreach (var item in content)
                {
                    var go = CreateSceneObject(item as JObject, objectMap, projectAssetsDir);
                    if (go != null && firstRoot == null)
                        firstRoot = go;
                }

                Debug.Log($"[TraceImport] Imported '{sceneName}' with {objectMap.Count} objects from {tracePath}");
                return firstRoot;
            }
            finally
            {
                // Clean up temp directory
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }

        private static GameObject CreateSceneObject(JObject data, Dictionary<string, GameObject> objectMap, string projectAssetsDir)
        {
            if (data == null) return null;

            var name = data["name"]?.ToString() ?? "Object";
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Import .trace");

            // Active state
            var active = data["active"];
            if (active != null && active.Type == JTokenType.Boolean)
                go.SetActive((bool)active);

            // Transform
            var transform = data["transform"] as JObject;
            if (transform != null)
            {
                var translation = transform["translation"] as JArray;
                if (translation != null && translation.Count >= 3)
                    go.transform.localPosition = new Vector3((float)translation[0], (float)translation[1], (float)translation[2]);

                var rotation = transform["rotation"] as JArray;
                if (rotation != null && rotation.Count >= 4)
                    go.transform.localRotation = new Quaternion((float)rotation[0], (float)rotation[1], (float)rotation[2], (float)rotation[3]);

                var scale = transform["scale"] as JArray;
                if (scale != null && scale.Count >= 3)
                    go.transform.localScale = new Vector3((float)scale[0], (float)scale[1], (float)scale[2]);

                // Parent
                var parentPath = transform["parent"]?.ToString();
                if (!string.IsNullOrEmpty(parentPath) && objectMap.ContainsKey(parentPath))
                    go.transform.SetParent(objectMap[parentPath].transform, false);
            }

            // Register in map
            var currentPath = go.transform.parent != null
                ? GetObjectPath(go.transform.parent) + "/" + name
                : name;

            // Also register just by name for simple lookups
            objectMap[name] = go;
            objectMap[currentPath] = go;

            // Load GLB model if src is specified
            var src = data["src"]?.ToString();
            if (!string.IsNullOrEmpty(src))
            {
                var glbFileName = Path.GetFileName(src.Replace("./", ""));
                var glbPath = Path.Combine(projectAssetsDir, glbFileName);
                if (File.Exists(glbPath))
                {
                    // Import the GLB as a prefab and instantiate
                    var assetPath = glbPath.Replace("\\", "/");
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (prefab != null)
                    {
                        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, go.transform);
                        if (instance != null)
                            instance.transform.localPosition = Vector3.zero;
                    }
                }
            }

            // Apply behaviors
            var behaviors = data["behaviors"] as JArray;
            if (behaviors != null)
            {
                foreach (var behavior in behaviors)
                {
                    ApplyBehavior(go, behavior as JObject, objectMap);
                }
            }

            return go;
        }

        private static void ApplyBehavior(GameObject go, JObject behavior, Dictionary<string, GameObject> objectMap)
        {
            if (behavior == null) return;
            var type = behavior["@type"]?.ToString();

            switch (type)
            {
                case "BoxCollider":
                    var bc = go.AddComponent<BoxCollider>();
                    var size = behavior["size"] as JObject;
                    if (size != null)
                        bc.size = new Vector3(
                            (float)(size["x"] ?? 1), (float)(size["y"] ?? 1), (float)(size["z"] ?? 1));
                    var center = behavior["center"] as JObject;
                    if (center != null)
                        bc.center = new Vector3(
                            (float)(center["x"] ?? 0), (float)(center["y"] ?? 0), (float)(center["z"] ?? 0));
                    break;

                case "SphereCollider":
                    var sc = go.AddComponent<SphereCollider>();
                    if (behavior["radius"] != null)
                        sc.radius = (float)behavior["radius"];
                    var sCenter = behavior["center"] as JObject;
                    if (sCenter != null)
                        sc.center = new Vector3(
                            (float)(sCenter["x"] ?? 0), (float)(sCenter["y"] ?? 0), (float)(sCenter["z"] ?? 0));
                    break;

                case "Billboard":
                    var bb = go.AddComponent<TraceBillboard>();
                    bb.mode = behavior["mode"]?.ToString() == "Y"
                        ? TraceBillboard.BillboardMode.Y
                        : TraceBillboard.BillboardMode.All;
                    break;

                case "Rotate":
                    var rot = go.AddComponent<TraceRotate>();
                    rot.axis = behavior["axis"]?.ToString() ?? "y";
                    rot.speed = (float)(behavior["speed"] ?? 30);
                    rot.speedRange = (float)(behavior["speedRange"] ?? 0);
                    break;

                case "Rotatable":
                    go.AddComponent<TraceRotatable>();
                    break;

                case "Moveable":
                    var mov = go.AddComponent<TraceMoveable>();
                    mov.axes = behavior["axes"]?.ToString() ?? "XYZ";
                    break;

                case "Pages":
                    var pages = go.AddComponent<TracePages>();
                    pages.autoplayPageDuration = (float)(behavior["autoplayPageDuration"] ?? 0);
                    pages.autoloop = (bool)(behavior["autoloop"] ?? false);
                    pages.nested = (bool)(behavior["nested"] ?? false);
                    break;

                case "Interactions":
                    var interactions = go.AddComponent<TraceInteractions>();
                    var interactionsArr = behavior["interactions"] as JArray;
                    if (interactionsArr != null)
                    {
                        foreach (var interactionToken in interactionsArr)
                        {
                            var interactionObj = interactionToken as JObject;
                            if (interactionObj == null) continue;
                            var interaction = new TraceInteractions.Interaction
                            {
                                on = interactionObj["on"]?.ToString() ?? "select",
                                distance = (float)(interactionObj["distance"] ?? 5),
                            };
                            ImportActions(interactionObj["enter"] as JArray, interaction.enter, objectMap);
                            ImportActions(interactionObj["exit"] as JArray, interaction.exit, objectMap);
                            interactions.interactions.Add(interaction);
                        }
                    }
                    break;

                case "Properties":
                    var propsComp = go.AddComponent<TraceProperties>();
                    var propsArr = behavior["properties"] as JArray;
                    if (propsArr != null)
                    {
                        foreach (var propToken in propsArr)
                        {
                            var propObj = propToken as JObject;
                            if (propObj == null) continue;
                            propsComp.properties.Add(new TraceProperties.Property
                            {
                                name = propObj["name"]?.ToString() ?? "",
                                type = propObj["type"]?.ToString() ?? "number",
                                value = propObj["value"]?.ToString() ?? "0",
                                label = propObj["label"]?.ToString() ?? "",
                                min = (float)(propObj["min"] ?? 0),
                                max = (float)(propObj["max"] ?? 1),
                                step = (float)(propObj["step"] ?? 0.01f),
                            });
                        }
                    }
                    break;

                case "Variables":
                    var varsComp = go.AddComponent<TraceVariables>();
                    var varsArr = behavior["variables"] as JArray;
                    if (varsArr != null)
                    {
                        foreach (var varToken in varsArr)
                        {
                            var varObj = varToken as JObject;
                            if (varObj == null) continue;
                            varsComp.variables.Add(new TraceVariables.Variable
                            {
                                name = varObj["name"]?.ToString() ?? "",
                                type = varObj["type"]?.ToString() ?? "number",
                                value = varObj["value"]?.ToString() ?? "0",
                            });
                        }
                    }
                    break;

                case "Renderer":
                    // Renderer properties are applied to existing renderers
                    var rendererComp = go.GetComponent<Renderer>();
                    if (rendererComp != null)
                    {
                        var castShadows = behavior["castShadows"];
                        if (castShadows != null)
                            rendererComp.shadowCastingMode = (bool)castShadows
                                ? UnityEngine.Rendering.ShadowCastingMode.On
                                : UnityEngine.Rendering.ShadowCastingMode.Off;
                    }
                    break;

                case "Animation":
                    // Animation is handled by the GLB import
                    break;

                case "Model":
                    // Model is handled by src/GLB loading
                    break;

                case "BehaviorGraph":
                    // BehaviorGraph would need VS graph import — complex, deferred
                    Debug.Log($"[TraceImport] BehaviorGraph on '{go.name}' — VS graph import not yet supported");
                    break;

                default:
                    Debug.Log($"[TraceImport] Unknown behavior type '{type}' on '{go.name}'");
                    break;
            }
        }

        private static void ImportActions(JArray actionsArr, List<TraceInteractions.Action> targetList, Dictionary<string, GameObject> objectMap)
        {
            if (actionsArr == null) return;
            foreach (var actionToken in actionsArr)
            {
                var actionObj = actionToken as JObject;
                if (actionObj == null) continue;
                var action = new TraceInteractions.Action
                {
                    type = actionObj["type"]?.ToString() ?? "setActive",
                    state = (bool)(actionObj["state"] ?? true),
                    index = (int)(actionObj["index"] ?? 0),
                    clip = actionObj["clip"]?.ToString(),
                    url = actionObj["url"]?.ToString(),
                    property = actionObj["property"]?.ToString(),
                    speed = (float)(actionObj["speed"] ?? 1),
                    volume = (float)(actionObj["volume"] ?? 1),
                    eventName = actionObj["event"]?.ToString(),
                    value = actionObj["value"]?.ToString(),
                };
                // Resolve target by name
                var targetName = actionObj["target"]?.ToString();
                if (!string.IsNullOrEmpty(targetName) && objectMap.ContainsKey(targetName))
                    action.target = objectMap[targetName];

                targetList.Add(action);
            }
        }

        private static string GetObjectPath(Transform t)
        {
            if (t.parent == null) return t.name;
            return GetObjectPath(t.parent) + "/" + t.name;
        }
    }
}
