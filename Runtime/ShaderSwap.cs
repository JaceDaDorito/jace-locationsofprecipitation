using RoR2;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LOP
{
    public static class ShaderSwap
    {
        public static List<Material> MaterialsWithSwappedShaders { get; } = new List<Material>();

        public static async Task ConvertShader(Material material)
        {
            if (!material.shader.name.StartsWith("Stubbed"))
            {
                LOPLog.Warning($"The material {material} has a shader which's name doesnt start with \"Stubbed\". Skipping material.");

                return;
            }

            try
            {
                var shaderName = material.shader.name.Substring(7);
                var addressablePath = $"{shaderName}.shader";
                var asyncOp = Addressables.LoadAssetAsync<Shader>(addressablePath);
                var shaderTask = asyncOp.Task;
                var shader = await shaderTask;
                material.shader = shader;

                MaterialsWithSwappedShaders.Add(material);
            }
            catch (Exception ex)
            {
                LOPLog.Error($"Failed to swap shader of material {material}: {ex}");
            }
        }
    }
}
