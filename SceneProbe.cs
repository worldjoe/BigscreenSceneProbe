using System;
using System.IO;
using System.Text;
using Il2CppBigscreen.Monitors;
using Il2CppBigscreen.UI;
using Il2CppTMPro;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.Events;

[assembly: MelonInfo(typeof(BigscreenSceneProbe.SceneProbe), "Bigscreen Scene Probe", "0.1.0", "Copilot")]
[assembly: MelonGame("Bigscreen Inc.", "Bigscreen")]

namespace BigscreenSceneProbe
{
    public sealed class SceneProbe : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Press F8 in a room to write a scene inventory to UserData.");
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                WriteInventory();
                WriteMonitorInventory();
            }
        }

        private void WriteMonitorInventory()
        {
            try
            {
                var pages = UnityEngine.Object.FindObjectsOfType<MonitorAdjustmentPage>();
                foreach (MonitorAdjustmentPage page in pages)
                {
                    if (!page.isActiveAndEnabled)
                        continue;

                    var output = new StringBuilder();
                    output.AppendLine("Bigscreen Scene Probe: monitor adjustment page and monitor views");
                    output.AppendLine("Object paths and labels may contain user-identifying information.");
                    output.AppendLine($"PAGE index={page.CurrentScreenIndex} path={GetPath(page.transform)}");
                    output.AppendLine($"PERSONAL path={PathOf(page.showPersonalScreenBtn)}");
                    output.AppendLine($"CYCLER path={PathOf(page.cycler3d)}");

                    foreach (Transform item in page.GetComponentsInChildren<Transform>(true))
                    {
                        GameObject obj = item.gameObject;
                        var button = obj.GetComponent<BigUIButton>();
                        var cycler = obj.GetComponent<BigUISelectionCycler>();
                        var text = obj.GetComponent<TextMeshPro>();
                        var collider = obj.GetComponent<BoxCollider>();
                        output.AppendLine($"UI path={GetPath(item)} activeSelf={obj.activeSelf} activeInHierarchy={obj.activeInHierarchy} localPosition={item.localPosition} localRotation={item.localRotation.eulerAngles} localScale={item.localScale} button={button != null} cycler={cycler != null} box={(collider != null ? $"center:{collider.center} size:{collider.size}" : "<none>")} text={(text != null ? text.text : "<none>")}");
                        if (button != null)
                            WriteEvent(output, "BUTTON_ON_POKED", button.onPoked);
                        if (cycler != null)
                        {
                            output.AppendLine($"CYCLERDETAIL path={GetPath(item)} button={PathOf(cycler.button)} current={cycler.CurrentValue} index={cycler.textValueIndex}");
                            WriteEvent(output, "CYCLER_ON_CHANGED", cycler.onSelectorChanged);
                            if (cycler.textValuePairs != null)
                            {
                                foreach (var pair in cycler.textValuePairs)
                                    if (pair != null)
                                        output.AppendLine($"  OPTION text={pair.text} secondary={pair.secondaryText} value={pair.value}");
                            }
                        }
                    }

                    foreach (MonitorViewController view in UnityEngine.Object.FindObjectsOfType<MonitorViewController>())
                    {
                        var model = view.MonitorModel;
                        output.AppendLine($"MONITOR path={GetPath(view.transform)} index={view.MultiMonitorIndex} owned={view.ClientOwnsMonitor} local={(model != null && model.IsLocalMonitor)} format={(model != null ? model.Format.ToString() : "<none>")} renderer={PathOf(view.renderer)}");
                        if (view.renderer != null && view.renderer.displays != null)
                        {
                            foreach (MonitorDisplay display in view.renderer.displays)
                            {
                                if (display == null || display.renderer == null)
                                    continue;
                                Material material = display.renderer.sharedMaterial;
                                output.AppendLine($"  DISPLAY path={GetPath(display.transform)} enabled={display.renderer.enabled} tiling={display.tiling} material={(material != null ? material.name : "<none>")} materialId={(material != null ? material.GetInstanceID() : 0)}");
                                if (display.gameObject.activeInHierarchy && material != null && material.shader != null)
                                    WriteMaterial(output, material);
                            }
                        }
                    }

                    string path = Path.Combine(MelonEnvironment.UserDataDirectory, $"BigscreenMonitorProbe-{page.CurrentScreenIndex + 1}.txt");
                    File.WriteAllText(path, output.ToString());
                    Transform clone = page.showPersonalScreenBtn.transform.parent.Find("BigscreenMulti3dSelector");
                    BigUISelectionCycler selected = clone != null && clone.gameObject.activeInHierarchy
                        ? clone.GetComponent<BigUISelectionCycler>() : page.cycler3d;
                    if (selected != null && (selected.CurrentValue == "Regular" || selected.CurrentValue == "SBS" || selected.CurrentValue == "OverUnder"))
                        File.WriteAllText(Path.Combine(MelonEnvironment.UserDataDirectory, $"BigscreenMonitorProbe-{page.CurrentScreenIndex + 1}-{selected.CurrentValue}.txt"), output.ToString());
                    LoggerInstance.Msg($"Monitor inventory written to {path}");
                    return;
                }
                LoggerInstance.Msg("No active monitor adjustment page; monitor inventory was not written.");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"Monitor inventory failed: {ex}");
            }
        }

        private static void WriteMaterial(StringBuilder output, Material material)
        {
            Shader shader = material.shader;
            output.AppendLine($"    SHADER name={shader.name}");
            foreach (string keyword in material.shaderKeywords)
                output.AppendLine($"    KEYWORD {keyword}");
            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                string property = shader.GetPropertyName(i);
                var type = shader.GetPropertyType(i);
                string value;
                switch (type)
                {
                    case UnityEngine.Rendering.ShaderPropertyType.Float:
                    case UnityEngine.Rendering.ShaderPropertyType.Range:
                        value = $"material={material.GetFloat(property)} global={Shader.GetGlobalFloat(property)}";
                        break;
                    case UnityEngine.Rendering.ShaderPropertyType.Color:
                    case UnityEngine.Rendering.ShaderPropertyType.Vector:
                        value = $"material={material.GetVector(property)} global={Shader.GetGlobalVector(property)}";
                        break;
                    case UnityEngine.Rendering.ShaderPropertyType.Texture:
                        value = $"scale={material.GetTextureScale(property)} offset={material.GetTextureOffset(property)}";
                        break;
                    default:
                        value = "<unsupported>";
                        break;
                }
                output.AppendLine($"    PROPERTY name={property} type={type} {value}");
            }
            output.AppendLine($"    GLOBAL _StereoMode={Shader.GetGlobalFloat("_StereoMode")} _EyeIndex={Shader.GetGlobalFloat("_EyeIndex")} _Use3D={Shader.GetGlobalFloat("_Use3D")}");
        }

        private static void WriteEvent(StringBuilder output, string name, UnityEventBase ev)
        {
            if (ev == null)
            {
                output.AppendLine($"  {name} <none>");
                return;
            }
            for (int i = 0; i < ev.GetPersistentEventCount(); i++)
            {
                UnityEngine.Object target = ev.GetPersistentTarget(i);
                output.AppendLine($"  {name} target={(target != null ? target.name : "<none>")} method={ev.GetPersistentMethodName(i)}");
            }
        }

        private static string PathOf(Component? component)
        {
            return component != null ? GetPath(component.transform) : "<none>";
        }

        private void WriteInventory()
        {
            try
            {
                var output = new StringBuilder();
                output.AppendLine("Bigscreen Scene Probe: active cameras and renderers");
                output.AppendLine("Object paths may include user-identifying names.");
                foreach (Camera camera in UnityEngine.Object.FindObjectsOfType<Camera>())
                {
                    output.AppendLine($"CAMERA path={GetPath(camera.transform)} enabled={camera.enabled} clear={camera.clearFlags} mask={camera.cullingMask} depth={camera.depth} position={camera.transform.position} rotation={camera.transform.rotation.eulerAngles} fov={camera.fieldOfView} aspect={camera.aspect} orthographic={camera.orthographic} target={(camera.targetTexture != null ? camera.targetTexture.name : "<display>")} rect={camera.rect}");
                }

                foreach (Il2Cpp.StreamerCamera stream in UnityEngine.Object.FindObjectsOfType<Il2Cpp.StreamerCamera>())
                {
                    Camera view = stream.viewCamera;
                    Transform follow = stream.followTarget;
                    Transform look = stream.lookAtTarget;
                    output.AppendLine($"STREAMER path={GetPath(stream.transform)} view={(view != null ? GetPath(view.transform) : "<none>")} follow={(follow != null ? GetPath(follow) : "<none>")} look={(look != null ? GetPath(look) : "<none>")} angle={stream.currentAngle} visual={(stream.cameraVisual != null ? GetPath(stream.cameraVisual.transform) : "<none>")}");
                }

                foreach (Renderer renderer in UnityEngine.Object.FindObjectsOfType<Renderer>())
                {
                    GameObject obj = renderer.gameObject;
                    output.AppendLine($"RENDERER path={GetPath(obj.transform)} type={renderer.GetType().Name} enabled={renderer.enabled} layer={obj.layer}");
                }

                string path = Path.Combine(MelonEnvironment.UserDataDirectory, "BigscreenSceneProbe.txt");
                File.WriteAllText(path, output.ToString());
                LoggerInstance.Msg($"Scene inventory written to {path}");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"Scene inventory failed: {ex}");
            }
        }

        private static string GetPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }
    }
}
