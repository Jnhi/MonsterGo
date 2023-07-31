using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.GUI;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class CreateAddressable
{
    /// <summary>
    /// 本地AB包路径
    /// </summary>
    public static string LOCALROOT = Application.dataPath + "/AddressableAssets/Local";
    /// <summary>
    /// 远程AB包路径
    /// </summary>
    public static string REMOTEROOT = Application.dataPath + "/AddressableAssets/Remote";

    public static string settings_asset = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
    static AddressableAssetSettings setting { 
        get{
            return getSettingsObject(settings_asset); 
        } 
    }

    static AddressableAssetSettings getSettingsObject(string settingsAsset) {
        // This step is optional, you can also use the default settings:
        //settings = AddressableAssetSettingsDefaultObject.Settings;
        AddressableAssetSettings setting = AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsAsset) as AddressableAssetSettings;
        if (setting == null)
        {
            Debug.LogError($"{settingsAsset} couldn't be found or isn't " + $"a settings object.");
        }
        return setting;
    }


    [MenuItem("工具/产品资源分组", false, 0)]
    internal static void UpdateProductGroups()
    {
        DirectoryInfo localDirInfo = new DirectoryInfo(LOCALROOT);
        var s = localDirInfo.GetDirectories();
        // 遍历本地AB目录下所有的子目录
        foreach (DirectoryInfo childDirInfo in localDirInfo.GetDirectories())
        {
            CreateGroup(childDirInfo,"Local_");
        }

        DirectoryInfo remoteDirInfo = new DirectoryInfo(REMOTEROOT);
        // 遍历远程AB目录下所有的子目录
        foreach (DirectoryInfo childDirInfo in remoteDirInfo.GetDirectories())
        {
            CreateGroup(childDirInfo,"Remote_");
        }
        markStatus(1);
    }

    [MenuItem("工具/打包", false, 0)]
    internal static void Pack()
    {
        UpdateProductGroups();
        AddressableAssetSettings.BuildPlayerContent();
    }
    private static AddressableAssetGroup FindGroup(string groupName)
    {
        for (int i = 0; i < setting.groups.Count; ++i)
        {
            if (groupName == setting.groups[i].Name) 
            { 
                return setting.groups[i]; 
            }
        }
        return null;
    }

    /// <summary>
    /// 创建分组
    /// </summary>
    /// <param name="dirInfo"></param>
    private static void CreateGroup(DirectoryInfo dirInfo,String prefix)
    {
        string groupName = prefix + dirInfo.Name;
        AddressableAssetGroup group = FindGroup(groupName);
        if (group == null)
        {
            //创建Group
            group = setting.CreateGroup(groupName, false, false, true, new List<AddressableAssetGroupSchema>{setting.DefaultGroup.Schemas[0], setting.DefaultGroup.Schemas[1]});
        }
        // 将指定目录下的文件和子目录添加到分组当中
        AddDirectoryToGroup(group, dirInfo);
    }

    /// <summary>
    /// 将资源塞入组
    /// </summary>
    /// <param name="group"></param>
    /// <param name="dirInfo"></param>
    private static void AddDirectoryToGroup(AddressableAssetGroup group, DirectoryInfo dirInfo)
    {
        foreach (FileInfo fileInfo in dirInfo.GetFiles("*.*"))
        {
            // 跳过.meta文件
            if (fileInfo.Extension == ".meta")
                continue;

            // 获得一个相对Assets的路径，但是要包含Assets，如Assets/Res/Cube/Sphere1.prefab
            string fullname = fileInfo.FullName.Replace("\\", "/");
            string path = fullname.Substring(fullname.IndexOf("Assets"));
            //要打包资源的路径转成guid
            string guid = AssetDatabase.AssetPathToGUID(path);
            // 创建资源
            AddressableAssetEntry entry = setting.CreateOrMoveEntry(guid, group, false, true);//要打包的资产条目   会将要打包的路径移动到group节点下
            //entry.SetLabel("Label", true, false, true);//第一个参数是创建这个标签  第二个是 是否开启标签 
            string address = path.Substring(path.IndexOf("/") + 1);
            address = address.Substring(address.IndexOf("/") + 1);
            address = address.Substring(address.IndexOf("/") + 1);
            if (fileInfo.Extension == ".js")
                entry.SetLabel("JS",true);

            entry.SetAddress(address);
        }
        // 遍历子目录
        foreach (DirectoryInfo childDirInfo in dirInfo.GetDirectories())
        {
            AddDirectoryToGroup(group, childDirInfo);
        }
    }

    /// <summary>
    /// 标记为资源分组
    /// 0 小包，所有资源存放资源服务器
    /// 1 分包 ，Local资源存本地，Remoted资源存资源服务器
    /// 2 整包，所有资源存本地
    /// </summary>
    private static void markStatus(int status)
    {
        List<AddressableAssetGroup> deleteList = new List<AddressableAssetGroup>();
        for (int i = 0; i < setting.groups.Count; i++)
        {
            var group = setting.groups[i];
            if (group.name != "Default Local Group" && group.name != "Built In Data")
            {
                if (group.entries.Count <= 0)
                {
                    ///删除没有资源的分组
                    deleteList.Add(group);
                }
                else
                {
                    foreach (var schema in group.Schemas)
                    {
                        if (schema is UnityEditor.AddressableAssets.Settings.GroupSchemas
                                .BundledAssetGroupSchema)
                        {
                            bool bundleCrc = true;
                            string buildPath = AddressableAssetSettings.kLocalBuildPath;
                            string loadPath = AddressableAssetSettings.kLocalLoadPath;
                            // bool isIncludeBuild = true;
                            if (group.name.Contains("Local_"))
                            {
                                // isIncludeBuild = true;
                                bundleCrc = status == 0;
                                buildPath = status == 0 ? AddressableAssetSettings.kRemoteBuildPath : AddressableAssetSettings.kLocalBuildPath;
                                loadPath = status == 0 ? AddressableAssetSettings.kRemoteLoadPath : AddressableAssetSettings.kLocalLoadPath;
                            }
                            else if (group.name.Contains("Remote_"))
                            {
                                bundleCrc = !(status == 2);
                                // isIncludeBuild = false;
                                buildPath = status == 2 ? AddressableAssetSettings.kLocalBuildPath : AddressableAssetSettings.kRemoteBuildPath;
                                loadPath = status == 2 ? AddressableAssetSettings.kLocalLoadPath : AddressableAssetSettings.kRemoteLoadPath;
                            }
                            else if (group.name.Contains("UpdateGroup_"))
                            {
                                // isIncludeBuild = false;
                                bundleCrc = true;
                                buildPath = AddressableAssetSettings.kRemoteBuildPath;
                                loadPath = AddressableAssetSettings.kRemoteLoadPath;
                            }
                            var bundledAssetGroupSchema = (schema as UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema);
                            bundledAssetGroupSchema.BuildPath.SetVariableByName(group.Settings, buildPath);
                            bundledAssetGroupSchema.LoadPath.SetVariableByName(group.Settings, loadPath);

                            bundledAssetGroupSchema.UseAssetBundleCrc = bundleCrc;
                            bundledAssetGroupSchema.BundleNaming = UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema.BundleNamingStyle.NoHash;
                            bundledAssetGroupSchema.BundleMode = UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema.BundlePackingMode.PackTogether;
                            // bundledAssetGroupSchema.IncludeInBuild = isIncludeBuild;
                        }
                        else if (schema is UnityEditor.AddressableAssets.Settings.GroupSchemas.ContentUpdateGroupSchema)
                        {
                            var updateGroupSchema = (schema as UnityEditor.AddressableAssets.Settings.GroupSchemas.ContentUpdateGroupSchema);

                            if (group.name.Contains("Local_"))
                            {
                                updateGroupSchema.StaticContent = !(status == 0);
                            }
                            else if (group.name.Contains("Remote_"))
                            {
                                updateGroupSchema.StaticContent = (status == 2);
                            }
                            else if (group.name.Contains("UpdateGroup_"))
                            {
                                updateGroupSchema.StaticContent = false;
                            }

                        }
                    }
                }
            }
        }
        for (int i = 0; i < deleteList.Count; i++)
        {
            setting.RemoveGroup(deleteList[i]);
        }
    }

}
