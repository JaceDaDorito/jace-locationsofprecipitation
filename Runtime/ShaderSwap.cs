using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LOP
{
    public static class ShaderSwap
    {
        private static Dictionary<string, Shader> cachedShaderDict = new Dictionary<string, Shader>();
        public static List<Material> MaterialsWithSwappedShaders { get; } = new List<Material>();
        static ShaderSwap()
        {
            RoR2.RoR2Application.onLoad = (Action)Delegate.Combine(RoR2Application.onLoad, new Action(() => cachedShaderDict.Clear()));
        }

        public static async void SwapShader(Material material)
        {
            string shaderName = material.shader.name.Substring("Stubbed".Length);
            string addressablePath = $"{shaderName}.shader";
            if (cachedShaderDict.ContainsKey(addressablePath))
            {
                material.shader = cachedShaderDict[addressablePath];
            }
            else
            {
                var asyncOp = Addressables.LoadAssetAsync<Shader>(addressablePath);
                var shaderTask = asyncOp.Task;
                Shader shader = await shaderTask;
                cachedShaderDict.Add(addressablePath, shader);
                material.shader = shader;
            }
            
            /*if (material.shader.name.Contains("Cloud Remap"))
            {
                var cloudMatAsyncOp = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matLightningLongBlue.mat");
                var cloudMat = await cloudMatAsyncOp.Task;
                var remapper = new RuntimeCloudMaterialMapper(material);
                material.CopyPropertiesFromMaterial(cloudMat);
                remapper.SetMaterialValues(ref material);
            }*/
            MaterialsWithSwappedShaders.Add(material);
        }
    }
}
