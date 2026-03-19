using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityGLTF.Interactivity;
using UnityGLTF.Interactivity.Export;
using UnityGLTF.Interactivity.Schema;
using UnityGLTF.Interactivity.VisualScripting;
using UnityGLTF.Plugins;
using UnityGLTF.Trace.Components;
using UnityEditor;
#if HAVE_VISUAL_SCRIPTING
using Unity.VisualScripting;
using UnityGLTF.Interactivity.VisualScripting.Export;
#endif

namespace UnityGLTF.Trace
{
    /// <summary>
    /// Exports a Unity scene as a .trace file (ZIP containing scene.json + assets/).
    /// The .trace format is compatible with trace-viewer / sandbox.trace3d.app.
    ///
    /// Graph data goes in scene.json as BehaviorGraph behaviors (not in GLB's KHR_interactivity).
    /// The GLB is exported clean (no KHR_interactivity) so Babylon's native parser won't choke.
    /// </summary>
    public class TraceSceneExporter
    {
        public static void Export(Transform[] rootTransforms, string outputPath, string sceneName)
        {
            // Step 1: Export GLB WITHOUT KHR_interactivity
            var settings = GLTFSettings.GetOrCreateSettings();

            // Temporarily disable the KHR_interactivity export plugin
            GLTFExportPlugin interactivityPlugin = null;
            bool wasEnabled = false;
            if (settings.ExportPlugins != null)
            {
                foreach (var p in settings.ExportPlugins)
                {
                    if (p != null && p.DisplayName != null && p.DisplayName.Contains("KHR_interactivity"))
                    {
                        interactivityPlugin = p;
                        wasEnabled = p.Enabled;
                        p.Enabled = false;
                        break;
                    }
                }
            }

            byte[] glbBytes;
            GLTFSceneExporter glbExporter;

            // Save original transforms, then reset roots to identity for GLB export.
            // This ensures the GLB root is at origin — scene.json carries the real transform.
            // Without this, the position gets applied twice (GLB + scene.json).
            var savedTransforms = new List<(Vector3 pos, Quaternion rot, Vector3 scl)>();
            foreach (var root in rootTransforms)
            {
                savedTransforms.Add((root.localPosition, root.localRotation, root.localScale));
                root.localPosition = Vector3.zero;
                root.localRotation = Quaternion.identity;
                root.localScale = Vector3.one;
            }

            try
            {
                var exportOptions = new ExportContext(settings)
                {
                    TexturePathRetriever = texture => AssetDatabase.GetAssetPath(texture)
                };
                glbExporter = new GLTFSceneExporter(rootTransforms, exportOptions);
                glbBytes = glbExporter.SaveGLBToByteArray(sceneName);
            }
            finally
            {
                // Restore original transforms
                for (int i = 0; i < rootTransforms.Length; i++)
                {
                    rootTransforms[i].localPosition = savedTransforms[i].pos;
                    rootTransforms[i].localRotation = savedTransforms[i].rot;
                    rootTransforms[i].localScale = savedTransforms[i].scl;
                }
                if (interactivityPlugin != null)
                    interactivityPlugin.Enabled = wasEnabled;
            }

            // Step 2: Export VS graphs as BehaviorGraph data
            JObject graphData = null;
#if HAVE_VISUAL_SCRIPTING
            graphData = ExportAllVSGraphs(rootTransforms, glbExporter);
#endif

            // Step 3: Build scene.json
            var sceneJson = BuildSceneJson(rootTransforms, sceneName, glbExporter, graphData);

            // Step 3: Bundle into ZIP
            var glbFileName = SanitizeFileName(sceneName) + ".glb";
            using (var fileStream = new FileStream(outputPath, FileMode.Create))
            using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Create))
            {
                var sceneEntry = zip.CreateEntry("scene.json", System.IO.Compression.CompressionLevel.Optimal);
                using (var writer = new StreamWriter(sceneEntry.Open()))
                {
                    writer.Write(sceneJson.ToString(Formatting.Indented));
                }

                var glbEntry = zip.CreateEntry("assets/" + glbFileName, System.IO.Compression.CompressionLevel.Optimal);
                using (var glbStream = glbEntry.Open())
                {
                    glbStream.Write(glbBytes, 0, glbBytes.Length);
                }
            }

            Debug.Log($"[TraceExport] Exported .trace to {outputPath} ({glbBytes.Length / 1024}KB GLB)");
        }

        private static JObject BuildSceneJson(Transform[] rootTransforms, string sceneName, GLTFSceneExporter glbExporter, JObject graphData)
        {
            var content = new JArray();
            var glbFileName = SanitizeFileName(sceneName) + ".glb";
            var src = "./assets/" + glbFileName;

            foreach (var root in rootTransforms)
            {
                CollectSceneObjects(root, null, content, src, glbExporter, graphData);
            }

            return new JObject
            {
                new JProperty("sceneName", sceneName),
                new JProperty("version", "2.0.0"),
                new JProperty("content", content),
            };
        }

        private static void CollectSceneObjects(Transform transform, string parentPath, JArray content, string src, GLTFSceneExporter glbExporter, JObject graphData)
        {
            var go = transform.gameObject;
            var name = go.name;

            var obj = new JObject
            {
                new JProperty("name", name),
                new JProperty("active", go.activeSelf),
            };

            if (parentPath == null)
                obj.Add("src", src);

            // Transform — uses real local transform values.
            // The GLB is exported with root at identity (reset before export),
            // so scene.json is the sole source of truth for positioning.
            var t = transform;
            var transformObj = new JObject
            {
                new JProperty("translation", new JArray(t.localPosition.x, t.localPosition.y, t.localPosition.z)),
                new JProperty("rotation", new JArray(t.localRotation.x, t.localRotation.y, t.localRotation.z, t.localRotation.w)),
                new JProperty("scale", new JArray(t.localScale.x, t.localScale.y, t.localScale.z)),
            };

            if (parentPath != null)
                transformObj.Add("parent", parentPath);

            obj.Add("transform", transformObj);

            // Behaviors
            var behaviors = SerializeBehaviors(go, graphData);
            if (behaviors.Count > 0)
                obj.Add("behaviors", behaviors);

            content.Add(obj);

            // Recurse children
            var currentPath = parentPath != null ? parentPath + "/" + name : name;
            for (int i = 0; i < transform.childCount; i++)
                CollectSceneObjects(transform.GetChild(i), currentPath, content, src, glbExporter, graphData);
        }

        private static JArray SerializeBehaviors(GameObject go, JObject graphData)
        {
            var behaviors = new JArray();

            // Model
            if (go.GetComponentInChildren<MeshFilter>() != null || go.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                behaviors.Add(new JObject { new JProperty("@type", "Model") });

            // Animation
            var animation = go.GetComponent<Animation>();
            var animator = go.GetComponent<Animator>();
            if (animation != null || animator != null)
            {
                var animBehavior = new JObject { new JProperty("@type", "Animation") };
                if (animation != null && animation.clip != null)
                {
                    animBehavior.Add("clip", animation.clip.name);
                    animBehavior.Add("wrapMode", animation.wrapMode == WrapMode.Loop ? "Loop" : "Once");
                    animBehavior.Add("speed", 1);
                    animBehavior.Add("playAutomatically", animation.playAutomatically);
                }
                else if (animator != null)
                {
                    // Animator: default to not auto-playing (user controls via Interactions/BehaviorGraph)
                    animBehavior.Add("playAutomatically", false);
                }
                else
                {
                    animBehavior.Add("playAutomatically", false);
                }
                behaviors.Add(animBehavior);
            }

            // Video
            var videoPlayer = go.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer != null)
            {
                behaviors.Add(new JObject
                {
                    new JProperty("@type", "Video"),
                    new JProperty("playOnStart", videoPlayer.playOnAwake),
                    new JProperty("loop", videoPlayer.isLooping),
                });
            }

            // Renderer
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                behaviors.Add(new JObject
                {
                    new JProperty("@type", "Renderer"),
                    new JProperty("castShadows", renderer.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off),
                });
            }

            // BoxCollider
            var boxCollider = go.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                behaviors.Add(new JObject
                {
                    new JProperty("@type", "BoxCollider"),
                    new JProperty("size", new JObject
                    {
                        new JProperty("x", boxCollider.size.x),
                        new JProperty("y", boxCollider.size.y),
                        new JProperty("z", boxCollider.size.z),
                    }),
                    new JProperty("center", new JObject
                    {
                        new JProperty("x", boxCollider.center.x),
                        new JProperty("y", boxCollider.center.y),
                        new JProperty("z", boxCollider.center.z),
                    }),
                });
            }

            // SphereCollider
            var sphereCollider = go.GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                behaviors.Add(new JObject
                {
                    new JProperty("@type", "SphereCollider"),
                    new JProperty("radius", sphereCollider.radius),
                    new JProperty("center", new JObject
                    {
                        new JProperty("x", sphereCollider.center.x),
                        new JProperty("y", sphereCollider.center.y),
                        new JProperty("z", sphereCollider.center.z),
                    }),
                });
            }

            // TraceInteractions component
            var interactions = go.GetComponent<TraceInteractions>();
            if (interactions != null)
            {
                behaviors.Add(interactions.SerializeToBehavior());
            }

            // TraceProperties component
            var props = go.GetComponent<TraceProperties>();
            if (props != null)
            {
                behaviors.Add(props.SerializeToBehavior());
            }

            // TraceVariables component
            var vars = go.GetComponent<TraceVariables>();
            if (vars != null)
            {
                behaviors.Add(vars.SerializeToBehavior());
            }

            // TraceBillboard component
            var billboard = go.GetComponent<TraceBillboard>();
            if (billboard != null)
            {
                behaviors.Add(new JObject
                {
                    new JProperty("@type", "Billboard"),
                    new JProperty("mode", billboard.mode.ToString()),
                });
            }

            // TraceRotate component
            var rotate = go.GetComponent<TraceRotate>();
            if (rotate != null)
            {
                behaviors.Add(new JObject
                {
                    new JProperty("@type", "Rotate"),
                    new JProperty("axis", rotate.axis),
                    new JProperty("speed", rotate.speed),
                    new JProperty("speedRange", rotate.speedRange),
                });
            }

            // TraceRotatable component (marker)
            if (go.GetComponent<TraceRotatable>() != null)
            {
                behaviors.Add(new JObject { new JProperty("@type", "Rotatable") });
            }

            // TraceMoveable component
            var moveable = go.GetComponent<TraceMoveable>();
            if (moveable != null)
            {
                behaviors.Add(new JObject
                {
                    new JProperty("@type", "Moveable"),
                    new JProperty("axes", moveable.axes),
                });
            }

            // TracePages component
            var pages = go.GetComponent<TracePages>();
            if (pages != null)
            {
                behaviors.Add(new JObject
                {
                    new JProperty("@type", "Pages"),
                    new JProperty("autoplayPageDuration", pages.autoplayPageDuration),
                    new JProperty("autoloop", pages.autoloop),
                    new JProperty("nested", pages.nested),
                });
            }

            // ParticleSystem
            var ps = go.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                var emission = ps.emission;
                var shape = ps.shape;
                var psBehavior = new JObject
                {
                    new JProperty("@type", "ParticleSystem"),
                    new JProperty("emissionRate", emission.rateOverTime.constant),
                    new JProperty("maxParticles", main.maxParticles),
                    new JProperty("lifetime", new JObject
                    {
                        new JProperty("min", main.startLifetime.constantMin),
                        new JProperty("max", main.startLifetime.constantMax),
                    }),
                    new JProperty("startSpeed", new JObject
                    {
                        new JProperty("min", main.startSpeed.constantMin),
                        new JProperty("max", main.startSpeed.constantMax),
                    }),
                    new JProperty("gravityModifier", main.gravityModifier.constant),
                    new JProperty("startColor", UnityEngine.ColorUtility.ToHtmlStringRGBA(main.startColor.color)),
                    new JProperty("particleScale", main.startSize.constant),
                };
                behaviors.Add(psBehavior);
            }

#if HAVE_VISUAL_SCRIPTING
            // BehaviorGraph from Visual Scripting
            var scriptMachine = go.GetComponent<ScriptMachine>();
            if (scriptMachine != null && scriptMachine.graph != null && graphData != null)
            {
                behaviors.Add(new JObject
                {
                    new JProperty("@type", "BehaviorGraph"),
                    new JProperty("graph", graphData.DeepClone()),
                });
            }
#endif

            return behaviors;
        }

#if HAVE_VISUAL_SCRIPTING
        /// <summary>
        /// Run the VS export pipeline on all ScriptMachines to produce
        /// trace-viewer compatible GraphData JSON.
        /// </summary>
        private static JObject ExportAllVSGraphs(Transform[] rootTransforms, GLTFSceneExporter glbExporter)
        {
            try
            {
                var vsContext = new VisualScriptingExportContext();
                vsContext.cleanUpAndOptimizeExportedGraph = true;

                var gltfRoot = glbExporter.GetRoot();
                vsContext.AfterSceneExport(glbExporter, gltfRoot);

                if (vsContext.Nodes == null || vsContext.Nodes.Count == 0)
                    return null;

                return TraceGraphDataSerializer.Serialize(
                    vsContext.Nodes.ToArray(),
                    vsContext.opDeclarations.ToArray(),
                    GltfTypes.TypesMapping,
                    vsContext.variables.ToArray(),
                    vsContext.customEvents.ToArray());
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[TraceExport] Failed to export VS graphs: {e.Message}\n{e.StackTrace}");
                return null;
            }
        }
#endif

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
