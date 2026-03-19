using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityGLTF.Trace
{
    /// <summary>
    /// Adds "Export .trace" menu items to UnityGLTF menus.
    /// Exports selected GameObjects as a .trace file (ZIP with scene.json + GLB assets)
    /// compatible with trace-viewer / sandbox.trace3d.app.
    /// </summary>
    public static class TraceExportMenu
    {
        private const string MenuPrefix = "Assets/UnityGLTF/";
        private const string MenuPrefixGameObject = "GameObject/UnityGLTF/";
        private const string ExportTrace = "Export .trace";
        private const int Priority = 36;

        [MenuItem(MenuPrefix + ExportTrace, true, Priority)]
        [MenuItem(MenuPrefixGameObject + ExportTrace, true, Priority)]
        private static bool ExportTraceValidate()
        {
            return Selection.activeGameObject != null;
        }

        [MenuItem(MenuPrefix + ExportTrace, false, Priority)]
        [MenuItem(MenuPrefixGameObject + ExportTrace, false, Priority)]
        private static void ExportTraceSelected(MenuCommand command)
        {
            if (command.context && Selection.objects.Length > 1 && command.context != Selection.objects[0])
                return;

            var selectedObjects = Selection.gameObjects;
            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                Debug.LogError("[TraceExport] No GameObjects selected");
                return;
            }

            var rootTransforms = selectedObjects.Select(go => go.transform).ToArray();
            var sceneName = selectedObjects.Length == 1
                ? selectedObjects[0].name
                : UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            var settings = GLTFSettings.GetOrCreateSettings();
            var path = EditorUtility.SaveFilePanel(
                "Export .trace",
                settings.SaveFolderPath,
                sceneName + ".trace",
                "trace");

            if (string.IsNullOrEmpty(path))
                return;

            settings.SaveFolderPath = Path.GetDirectoryName(path);

            TraceSceneExporter.Export(rootTransforms, path, sceneName);

            EditorUtility.RevealInFinder(path);
        }
    }
}
