using EFT.UI.DragAndDrop;
using Newtonsoft.Json;
using SPT.Common.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace EternalCycleClient
{
    public class ClientResourceManager
    {
        public static Dictionary<string, Sprite> DecoIconDict { get; private set; } = new Dictionary<string, Sprite>();
        public static Dictionary<string, Texture2D> TargetDict { get; private set; } = new Dictionary<string, Texture2D>();
        public static void LoadResources(string url, string modPath, string localCacheRootDir)
        {
            string correctPath = Path.Combine(modPath, localCacheRootDir);
            string normalizedRootDir = correctPath.Replace('\\', '/');
            if (!normalizedRootDir.EndsWith("/")) normalizedRootDir += "/";

            if (!Directory.Exists(normalizedRootDir))
            {
                Directory.CreateDirectory(normalizedRootDir);
            }

            // 1. 扫描本地
            var request = new SyncResourceRequest();
            // 客户端搜索本地缓存时也可以扫描全部，防止玩家乱建文件夹
            var localFiles = Directory.GetFiles(normalizedRootDir);

            foreach (var filePath in localFiles)
            {
                // 【核心修改】：直接拿纯文件名当 Key
                string fileName = Path.GetFileName(filePath);

                string md5 = GetFileMD5(filePath);
                if (!string.IsNullOrEmpty(md5))
                {
                    request.ClientHashes[fileName] = md5;
                }
            }

            string jsonPayload = JsonConvert.SerializeObject(request);

            // 2. 发起网络请求
            string jsonResponse = RequestHandler.PostJson(url, jsonPayload);

            if (string.IsNullOrWhiteSpace(jsonResponse))
            {
                Console.WriteLine("[资源同步] 服务端未返回任何数据！");
                return;
            }

            var response = JsonConvert.DeserializeObject<SyncResourceResponse>(jsonResponse);
            if (response == null) return;

            // 3. 垃圾回收
            foreach (var filePath in localFiles)
            {
                // 【核心修改】：用文件名去服务端的合法名单里比对
                string fileName = Path.GetFileName(filePath);

                if (!response.ValidFiles.Contains(fileName))
                {
                    File.Delete(filePath);
                }
            }

            // 4. 增量更新写入 (扁平化写入)
            foreach (var kvp in response.FilesToUpdate)
            {
                // 这里的 kvp.Key 现在是纯文件名 (如 "rig_01.bundle")
                string fileName = kvp.Key;
                string base64Data = kvp.Value;

                // 【核心修改】：直接拼在根目录下，实现扁平化存储
                string savePath = Path.Combine(normalizedRootDir, fileName);

                byte[] fileData = Convert.FromBase64String(base64Data);
                File.WriteAllBytes(savePath, fileData);

                Console.WriteLine($"[资源同步] 下载/更新了资源: {fileName}");
            }

            Console.WriteLine($"[资源同步] 同步完成！共更新 {response.FilesToUpdate.Count} 个 文件。");
        }

        public static void LoadVoice(string url)
        {

            // 2. 发起网络请求
            string jsonResponse = RequestHandler.PostJson(url, "What is a wave without the ocean？A beginning without an end？");

            if (string.IsNullOrWhiteSpace(jsonResponse))
            {
                Console.WriteLine("[资源同步] 服务端未返回任何数据！");
                return;
            }

            var response = JsonConvert.DeserializeObject<VoiceResourceRequest>(jsonResponse);
            if (response == null) return;

            foreach (var kvp in response.VoicePath)
            {
                ResourceKeyManagerAbstractClass.Dictionary_0.TryAdd(kvp.Key, kvp.Value);

                Console.WriteLine($"[资源同步] 成果添加了声线资源: {kvp.Value}");
            }
        }

        public static void LoadRigLayout(string modPath, string folderPath)
        {
            string fullPath = Path.Combine(modPath, folderPath);
            if (!Directory.Exists(fullPath))
            {
                //实例模式真死妈了, 等我下午开完药回来我就写个Logger, 操你妈
                //Console.WriteLine(12312312);
                return;
            }

            foreach (string file in Directory.GetFiles(fullPath))
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(file);

                if (bundle == null)
                {
                    continue;
                }

                GameObject[] prefabs = bundle.LoadAllAssets<GameObject>();

                foreach (GameObject prefab in prefabs)
                {
                    var gridView = prefab.GetComponent<ContainedGridsView>();
                    if (gridView != null)
                    {
                        Console.WriteLine($"loading gridview");
                        var key = $"UI/Rig Layouts/{prefab.name}";
                        if (!CacheResourcesPopAbstractClass.Dictionary_0.ContainsKey(key))
                        {
                            Console.WriteLine($"gridview added successful: {key}");
                            //Console.WriteLine($"Test: {gridView.GridViews}");
                            CacheResourcesPopAbstractClass.Dictionary_0.Add(key, gridView);
                        }

                    }
                }
            }
        }

        public static void LoadSlotIcon(string modPath, string folderPath)
        {
            string fullPath = Path.Combine(modPath, folderPath);
            if (!Directory.Exists(fullPath))
            {
                return;
            }

            foreach (string file in Directory.GetFiles(fullPath))
            {
                string fileName = Path.GetFileName(file).Replace(".png", "").Replace(".jpg", "");
                CacheResourcesPopAbstractClass.Dictionary_0.Add($"Slots/{fileName}", fileName);
                var sprite = TextureUtils.SimpleCreateSprite(TextureUtils.LoadFromFile(file, 1, 1), 100);
                for (var i = 0; i < 20; i++)
                {
                    CacheResourcesPopAbstractClass.Dictionary_0.Add($"Slots/{fileName}_00{i}", sprite);
                }
            }
        }

        public static void LoadDecoIcon(string modPath, string folderPath)
        {
            string fullPath = Path.Combine(modPath, folderPath);
            if (!Directory.Exists(fullPath))
            {
                return;
            }

            foreach (string file in Directory.GetFiles(fullPath))
            {
                string fileName = Path.GetFileName(file).Replace(".png", "").Replace(".jpg", "");
                var sprite = TextureUtils.SimpleCreateSprite(TextureUtils.LoadFromFile(file, 1, 1), 100);
                DecoIconDict.TryAdd(fileName, sprite);

            }
        }

        public static void LoadTarget(string modPath, string folderPath)
        {
            string fullPath = Path.Combine(modPath, folderPath);
            if (!Directory.Exists(fullPath))
            {
                return;
            }

            foreach (string file in Directory.GetFiles(fullPath))
            {
                string fileName = Path.GetFileName(file).Replace(".png", "").Replace(".jpg", "");
                var texture = TextureUtils.LoadFromFile(file, 1, 1);
                TargetDict.TryAdd(fileName, texture);

            }
        }

        private static string GetFileMD5(string filePath)
        {
            if (!File.Exists(filePath)) return string.Empty;
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                var hashBytes = md5.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}