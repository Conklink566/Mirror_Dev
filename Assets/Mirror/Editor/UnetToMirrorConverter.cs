// Mirror Network Migration Tool
// Written by M. Coburn (@coburn64 on Twitter/SoftwareGuy on Github) and Lymdun.
// This file is part of Mirror Networking by Coburn64, Lymdun, vis2k and Paul (goldbug).
using UnityEditor;
using UnityEngine;
using UnetNetworkAnimator = UnityEngine.Networking.NetworkAnimator;
using UnetNetworkIdentity = UnityEngine.Networking.NetworkIdentity;
using UnetNetworkManager = UnityEngine.Networking.NetworkManager;
using UnetNetworkProximityChecker = UnityEngine.Networking.NetworkProximityChecker;
using UnetNetworkStartPosition = UnityEngine.Networking.NetworkStartPosition;
using UnetNetworkTransform = UnityEngine.Networking.NetworkTransform;
using UnetNetworkTransformChild = UnityEngine.Networking.NetworkTransformChild;

using UnetNetworkDiscovery = UnityEngine.Networking.NetworkDiscovery;

using MirrorNetworkAnimator = Mirror.NetworkAnimator;
using MirrorNetworkIdentity = Mirror.NetworkIdentity;
using MirrorNetworkManager = Mirror.NetworkManager;
using MirrorNetworkProximityChecker = Mirror.NetworkProximityChecker;
using MirrorNetworkStartPosition = Mirror.NetworkStartPosition;
using MirrorNetworkTransform = Mirror.NetworkTransform;
using MirrorNetworkTransformChild = Mirror.NetworkTransformChild;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.Generic;

namespace Mirror.MigrationUtilities {
    // Code converter
    public static class MirrorNetworkMigrator {
        // Private variables that don't need to be modified.
        static string scriptExtension = "*.cs";

        static string[] knownIncompatibleRegexes = {
            "SyncListSTRUCT",   // this probably needs improvement but i didn't want to duplicate lines of code
            @"\[Command],]*)\]",    // Commands over non-reliable channels
            @"\[ClientRpc],]*)\]",  // ClientRPCs over non-reliable channels
            @"\[TargetRpc],]*)\]",  // TargetRPCs over non-reliable channels
            @"\[SyncEvent],]*)\]"   // SyncEvents (over non-reliable channels) - seriously?
        };

        static string[] knownCompatibleReplacements = {
            "SyncListSTRUCT",   // because mirror's version is moar bettah.
            "[Command]",
            "[ClientRpc]",
            "[TargetRpc]",
            "[SyncEvent]"
        };

        static int filesModified = 0;
        static string scriptBuffer = string.Empty;
        static MatchCollection matches;

        // Logic portion begins below.

        [MenuItem("Tools/Mirror/Migrate scripts from UNET")]
        public static void Mirror_Migration_Tool() {
            // Safeguard in case a developer goofs up
            if (knownIncompatibleRegexes.Length != knownCompatibleReplacements.Length) {
                Debug.Log("[Mirror Migration Tool] BUG DETECTED: Regexes to search for DO NOT match the Regex Replacements. Cannot continue.");
                return;
            }

            // Display a welcome dialog.
            if (EditorUtility.DisplayDialog("Mirror Network Migration Tool", "Welcome to the Migration Tool for Mirror Networking. " +
                "This tool will convert your existing UNET code into the Mirror equivalent code.\n\nBefore we begin, we STRONGLY " +
                "recommend you take a full backup of your project as this tool is not perfect.\n\nWhile it does not attempt to " +
                "purposefully trash your network scripts, it could break your project. Be smart and BACKUP NOW.",
                "I'm good.", "I'll backup first.")) {

                // User accepted the risks - go ahead!
                MigrationTool_DoActualMigration();
                // Cleanup after yourself.
                MigrationTool_Cleanup();
                // Refresh the asset database, because sometimes Unity will be lazy about it.
                AssetDatabase.Refresh();
            } else {
                EditorUtility.DisplayDialog("Aborted", "You opted to abort the migration process. Please come back once you've taken a backup.", "Got it");
                return;
            }
        }

        private static void MigrationTool_DoActualMigration() {
            // Place holder for the assets folder location.
            string assetsFolder = Application.dataPath;
            // List structure for the CSharp files.
            List<string> filesToScanAndModify = new List<string>();

            // Be verbose and say what's happening.
            Debug.Log("[Mirror Migration Tool] Determined your asset folder is at: " + assetsFolder);
            Debug.Log("[Mirror Migration Tool] Scanning your C# scripts... This might take a moment.");

            // Now we scan the directory...
            try {
                DirectoryInfo dirInfo = new DirectoryInfo(assetsFolder);

                // For every entry in this structure add it to the list.
                // SearchOption.AllDirectories will traverse the directory stack
                foreach (FileInfo potentialFile in dirInfo.GetFiles(scriptExtension, SearchOption.AllDirectories)) {
                    // DEBUG ONLY. This will cause massive Unity Console Spammage!
                    // Debug.Log("[Mirror Migration Tool] DEBUG: Scanned " + potentialFile.FullName);
                    filesToScanAndModify.Add(potentialFile.FullName);
                }

                // Final chance to abort.
                if (!EditorUtility.DisplayDialog("Continue?", string.Format("We've found {0} file(s) that may need updating. Depending on your hardware and storage, " +
                    "this might take a while. Do you wish to continue the process?", filesToScanAndModify.Count), "Go ahead!", "Abort")) {
                    EditorUtility.DisplayDialog("Aborted", "You opted to abort the migration process. Please come back once you're ready to migrate.", "Got it");
                    return;
                }

                // Okay, let's do this!
                MigrationTool_FileProcessor(filesToScanAndModify);

                Debug.Log("[Mirror Migration Tool] Processed (and patched, if required) " + filesModified + " files");

                EditorUtility.DisplayDialog("Migration complete.", "Congratulations, you should now be Mirror Network ready.\n\n" +
                    "Thank you for using Mirror and Telepathy Networking Stack for Unity!\n\nPlease don't forget to drop by the GitHub " +
                    "repository to keep up to date and the Discord server if you have any problems. Have fun!", "Awesome");
                return;

            } catch (System.Exception ex) {
                EditorUtility.DisplayDialog("Oh no!", "An exception occurred. If you think this is a Mirror Networking bug, please file a bug report on the GitHub repository." +
                    "It could also be a logic bug in the Migration Tool itself. I encountered the following exception:\n\n" + ex.ToString(), "Okay");
                MigrationTool_Cleanup();
                return;
            }
        }

        private static void MigrationTool_FileProcessor(List<string> filesToProcess) {
            StreamReader sr;
            StreamWriter sw;

            foreach (string file in filesToProcess) {
                try {
                    // Open and load it into the script buffer.
                    using (sr = new StreamReader(file)) {
                        scriptBuffer = sr.ReadToEnd();
                        sr.Close();
                    }

                    // Get outta here, UnityEngine.Networking !
                    scriptBuffer = scriptBuffer.Replace("using Mirror;", "using Mirror;");

                    // Work our magic.
                    for (int i = 0; i < knownIncompatibleRegexes.Length; i++) {
                        matches = Regex.Matches(scriptBuffer, knownIncompatibleRegexes[i]);
                        if (matches.Count > 0) {
                            // It was successful - replace it.
                            scriptBuffer = Regex.Replace(scriptBuffer, knownIncompatibleRegexes[i], knownCompatibleReplacements[i]);
                        }
                    }

                    // Be extra gentle with some like NetworkSettings directives.
                    matches = Regex.Matches(scriptBuffer, @"NetworkSettings\(([^\)]*)\)");
                    // A file could have more than one NetworkSettings... better to just do the whole lot.
                    // We don't know what the developer might be doing.
                    if (matches.Count > 0) {
                        for (int i = 0; i < matches.Count; i++) {
                            Match nsm = Regex.Match(matches[i].ToString(), @"(?<=\().+?(?=\))");
                            if (nsm.Success) {
                                string[] netSettingArguments = nsm.ToString().Split(',');
                                if (netSettingArguments.Length > 1) {
                                    string patchedNetSettings = string.Empty;

                                    int a = 0;
                                    foreach (string argument in netSettingArguments) {
                                        // Increment a, because that's how many elements we've looked at.
                                        a++;

                                        // If it contains the offender, just continue, don't do anything.
                                        if (argument.Contains("channel")) continue;

                                        // If it doesn't then add it to our new string.
                                        patchedNetSettings += argument.Trim();
                                        if (a < netSettingArguments.Length) patchedNetSettings += ", ";
                                    }

                                    // a = netSettingArguments.Length; patch it up and there we go.
                                    scriptBuffer = Regex.Replace(scriptBuffer, nsm.Value, patchedNetSettings);
                                } else {
                                    // Replace it.
                                    if (netSettingArguments[0].Contains("channel")) {
                                        // Don't touch this.
                                        scriptBuffer = scriptBuffer.Replace(string.Format("[{0}]", matches[i].Value), string.Empty);
                                    }
                                    // DONE!
                                }
                            }
                        }
                    }

                    // Backup the old files for safety.
                    // The user can delete them later.
                    if (!File.Exists(file + ".bak")) File.Copy(file, file + ".bak");

                    // Now the job is done, we want to write the data out to disk... 
                    using (sw = new StreamWriter(file, false, Encoding.UTF8)) {
                        sw.WriteLine(scriptBuffer);
                        sw.Close();
                    }

                    // Increment the modified counter for statistics.
                    filesModified++;
                } catch (System.Exception e) {
                    // Kaboom, this tool ate something it shouldn't have.
                    Debug.LogError(string.Format("[Mirror Migration Tool] Encountered an exception processing {0}:\n{1}", file, e.ToString()));
                }
            }
        }

        /// <summary>
        /// Cleans up after the migration tool is completed or has failed.
        /// </summary>
        private static void MigrationTool_Cleanup() {
            scriptBuffer = string.Empty;
            matches = null;
            filesModified = 0;
        }
    }

    // Component converter
    public class UnetToMirrorConverter : EditorWindow {
        [MenuItem("Tools/Mirror/Migrate UNET components on Prefabs")]
        private static void ReplaceComponentsOnPrefabs() {
            if (EditorUtility.DisplayDialog("Prefabs Converter",
                "Are you sure you want to convert prefabs of your project from UNET to Mirror?\nNote: Depending on your project size, it could take lot of time. Please don't close Unity during the process to avoid corrupted project.",
                "Yes, farewell UNET!", "Cancel")) {
                var converter = CreateInstance<UnetToMirrorConverter>();
                int netComponentObsolete = 0;

                converter.FindAndReplaceUnetComponents(out netComponentObsolete);

                if (netComponentObsolete > 0) {
                    EditorUtility.DisplayDialog("Warning",
                        "Please check your console logs, obsolete components found.",
                        "OK");
                }
            }
        }

        [MenuItem("Tools/Mirror/Migrate UNET components on Scene")]
        private static void ReplaceComponentsOnScene() {
            if (EditorUtility.DisplayDialog("Scene GameObjects Converter",
                "Are you sure you want to convert GameObjects of your scene from UNET to Mirror?\nNote: Depending on your scene size, it could take lot of time. Please don't close Unity during the process to avoid corrupted scene.",
                "Yes, farewell UNET!", "Cancel")) {
                var converter = CreateInstance<UnetToMirrorConverter>();
                int netComponentObsolete = 0;

                converter.FindAndReplaceUnetSceneGameObject(out netComponentObsolete);

                if (netComponentObsolete > 0) {
                    EditorUtility.DisplayDialog("Warning",
                        "Please check your console logs, obsolete components found.",
                        "OK");
                }
            }
        }

        public void FindAndReplaceUnetComponents(out int netComponentObsolete) {
            int fileCounter = 0; // files on the project
            netComponentObsolete = 0; // obsolete components found (like lobby)
            int netIdComponentsCount = 0; // network identities
            int netComponentCount = 0; // networking components
            string logErrors = ""; // error message

            string[] files = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
            int gameObjectCount = files.Length;

            foreach (string file in files) {
                fileCounter++;
                EditorUtility.DisplayProgressBar("Mirror Migration Progress", string.Format("{0} of {1} files scanned...", fileCounter, gameObjectCount), fileCounter / gameObjectCount);

                string relativepath = "Assets" + file.Substring(Application.dataPath.Length);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(relativepath);

                IEnumerable<Transform> childsAndParent = prefab.GetComponentsInChildren<Transform>(true);

                foreach (Transform actualChild in childsAndParent) {
                    // replace UNET components with their mirror counterpart
                    netComponentCount += ReplaceEveryNetworkComponent(actualChild.gameObject);

                    // always replace NetworkIdentity as last element, due to dependencies
                    netIdComponentsCount += ReplaceEveryNetworkIdentity(actualChild.gameObject);

                    // check for obsolete components
                    int compObsolete = 0;
                    logErrors += CheckObsoleteComponents(actualChild.gameObject, out compObsolete);
                    netComponentObsolete += compObsolete;
                }
            }

            EditorUtility.ClearProgressBar();
            Debug.LogFormat("Searched {0} Prefabs, found {1} UNET NetworkIdentity, {2} Components and replaced them with Mirror components.\nAlso found {3} now deprecated components.", gameObjectCount, netIdComponentsCount, netComponentCount, netComponentObsolete);

            if (netComponentObsolete > 0)
                Debug.LogWarningFormat("List of now deprecated components found on your project:\n{0}", logErrors);
        }

        public void FindAndReplaceUnetSceneGameObject(out int netComponentObsolete) {
            int convertedGoCounter = 0; // counter of converted game objects
            netComponentObsolete = 0; // obsolete components found (like lobby)
            int netIdComponentsCount = 0; // network identities
            int netComponentCount = 0; // networking components
            string logErrors = ""; // error message

            // safest way to get all gameObjects on the scene instead of FindObjectOfType()
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            int gameObjectCount = allObjects.Length;

            foreach (GameObject currentGameObject in allObjects) {
                if (currentGameObject.scene.isLoaded) {
                    convertedGoCounter++;
                    EditorUtility.DisplayProgressBar("Mirror Migration Progress", string.Format("{0} of {1} game object scanned...", convertedGoCounter, gameObjectCount), convertedGoCounter / gameObjectCount);

                    IEnumerable<Transform> childsAndParent = currentGameObject.GetComponentsInChildren<Transform>(true);

                    foreach (Transform actualChild in childsAndParent) {
                        // replace UNET components with their mirror counterpart
                        netComponentCount += ReplaceEveryNetworkComponent(actualChild.gameObject);

                        // always replace NetworkIdentity as last element, due to dependencies
                        netIdComponentsCount += ReplaceEveryNetworkIdentity(actualChild.gameObject);

                        // check for obsolete components
                        int compObsolete = 0;
                        logErrors += CheckObsoleteComponents(actualChild.gameObject, out compObsolete);
                        netComponentObsolete += compObsolete;
                    }
                }
            }

            EditorUtility.ClearProgressBar();
            Debug.LogFormat("Searched {0} GameObjects, found {1} UNET NetworkIdentity, {2} Components and replaced them with Mirror components.\nAlso found {3} now deprecated components.", convertedGoCounter, netIdComponentsCount, netComponentCount, netComponentObsolete);

            if (netComponentObsolete > 0)
                Debug.LogWarningFormat("List of now deprecated components found on your project:\n {0}", logErrors);
        }

        int ReplaceEveryNetworkComponent(GameObject go) {
            int compCount = 0;

            compCount += ReplaceNetworkComponent<UnetNetworkAnimator, MirrorNetworkAnimator>(go);
            compCount += ReplaceNetworkComponent<UnetNetworkTransform, MirrorNetworkTransform>(go);
            compCount += ReplaceNetworkComponent<UnetNetworkManager, MirrorNetworkManager>(go);
            compCount += ReplaceNetworkComponent<UnetNetworkProximityChecker, MirrorNetworkProximityChecker>(go);
            compCount += ReplaceNetworkComponent<UnetNetworkStartPosition, MirrorNetworkStartPosition>(go);
            compCount += ReplaceNetworkComponent<UnetNetworkTransformChild, MirrorNetworkTransformChild>(go);

            return compCount;
        }

        int ReplaceEveryNetworkIdentity(GameObject go) {
            int niCount = 0;

            niCount += ReplaceNetworkComponent<UnetNetworkIdentity, MirrorNetworkIdentity>(go);

            return niCount;
        }

        string CheckObsoleteComponents(GameObject go, out int compObsolete) {
            string errors = "";
            compObsolete = 0;

            // TODO: others obsolete components from original UNET (and not HLAPI_CE) like lobby
            if (go.GetComponent<UnetNetworkDiscovery>()) {
                compObsolete++;
                errors += go.name + "\n";
            }

            return errors;
        }

        static int ReplaceNetworkComponent<TSource, TDestination>(GameObject prefab)
            where TSource : Component
            where TDestination : Component {

            int netComponentCount = 0;
            TSource unetNetworkComponent = prefab.GetComponent<TSource>();
            if (unetNetworkComponent != null) {
                netComponentCount++;

                // check for mirror component
                TDestination mirrorNetworkComponent = prefab.AddComponent<TDestination>();
                if (mirrorNetworkComponent == null) {
                    mirrorNetworkComponent = prefab.GetComponent<TDestination>();
                }

                // copy values
                CopyProperties(unetNetworkComponent, mirrorNetworkComponent);

                // destroy UNET component
                DestroyImmediate(unetNetworkComponent, true);
            }

            return netComponentCount;
        }

        // source: https://stackoverflow.com/questions/930433/apply-properties-values-from-one-object-to-another-of-the-same-type-automaticall
        static void CopyProperties(object source, object destination) {

            // If any this null throw an exception
            if (source == null || destination == null)
                throw new Exception("Source or/and Destination Objects are null");

            // Getting the Types of the objects
            Type typeDest = destination.GetType();
            Type typeSrc = source.GetType();

            // Iterate the Properties of the source instance and  
            // populate them from their desination counterparts  
            PropertyInfo[] srcProps = typeSrc.GetProperties();
            foreach (PropertyInfo srcProp in srcProps) {
                if (!srcProp.CanRead) {
                    continue;
                }

                PropertyInfo targetProperty = typeDest.GetProperty(srcProp.Name,
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Public |
                    BindingFlags.Instance);

                if (targetProperty == null) {
                    continue;
                }
                if (!targetProperty.CanWrite) {
                    continue;
                }
                if (targetProperty.GetSetMethod(true) != null && targetProperty.GetSetMethod(true).IsPrivate) {
                    continue;
                }
                if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0) {
                    continue;
                }
                if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType)) {
                    continue;
                }

                // Passed all tests, lets set the value
                targetProperty.SetValue(destination, srcProp.GetValue(source, null), null);
            }
        }
    }
}


