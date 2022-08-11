﻿/*************************************************************************
 *  Copyright © 2022 Mogoson. All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  AssemblymanEditor.cs
 *  Description  :  Editor to analyse assembly.
 *------------------------------------------------------------------------
 *  Author       :  Mogoson
 *  Version      :  0.1.0
 *  Date         :  8/11/2022
 *  Description  :  Initial development version.
 *************************************************************************/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MGS.Assemblyman
{
    public sealed class AssemblymanEditor : EditorWindow
    {
        #region
        private static AssemblymanEditor instance;

        [MenuItem("Tool/Assemblyman &M")]
        private static void ShowEditor()
        {
            instance = GetWindow<AssemblymanEditor>("Assemblyman");
            instance.Show();
        }
        #endregion

        #region
        private Dictionary<string, List<string>> refAssems = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> refByAssems = new Dictionary<string, List<string>>();
        private List<string> refrefAssems = new List<string>();

        private const string SYSTEM = "System";
        private string filter = "System,Unity";
        private string keyword = string.Empty;
        private Vector2 scrollPos = Vector2.zero;
        #endregion

        #region
        private void OnEnable()
        {
            RefreshAssemblyInfo();
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawAssemblyView();
        }

        private void OnDisable()
        {
            EditorUtility.UnloadUnusedAssetsImmediate(true);
        }
        #endregion

        #region
        private void RefreshAssemblyInfo()
        {
            ClearAssemblyInfo();

            #region
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    if (CheckContainsKeyword(assembly.FullName, filter))
                    {
                        continue;
                    }
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    if (!CheckContainsKeyword(assembly.FullName, keyword))
                    {
                        continue;
                    }
                }

                refAssems.Add(assembly.FullName, new List<string>());
                var refAssemblies = assembly.GetReferencedAssemblies();
                foreach (var refAssembly in refAssemblies)
                {
                    refAssems[assembly.FullName].Add(refAssembly.FullName);
                }

                if (!assembly.FullName.Contains(SYSTEM))
                {
                    foreach (var refAssembly in refAssemblies)
                    {
                        if (refAssembly.FullName.Contains(SYSTEM))
                        {
                            continue;
                        }
                        if (!refByAssems.ContainsKey(refAssembly.FullName))
                        {
                            refByAssems.Add(refAssembly.FullName, new List<string>());
                        }
                        refByAssems[refAssembly.FullName].Add(assembly.FullName);
                    }
                }
            }
            #endregion

            #region
            foreach (var reference in refAssems)
            {
                if (refByAssems.ContainsKey(reference.Key))
                {
                    foreach (var item in reference.Value)
                    {
                        if (refByAssems[reference.Key].Contains(item))
                        {
                            if (!refrefAssems.Contains(item))
                            {
                                refrefAssems.Add(item);
                            }
                        }
                    }
                }
            }
            #endregion
        }

        private bool CheckContainsKeyword(string value, string keyword)
        {
            var keywords = keyword.Split(',');
            value = value.ToLower();
            foreach (var key in keywords)
            {
                if (value.Contains(key.ToLower().Trim()))
                {
                    return true;
                }
            }
            return false;
        }

        private void ClearAssemblyInfo()
        {
            refAssems.Clear();
            refByAssems.Clear();
            refrefAssems.Clear();
        }
        #endregion

        #region
        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("}{", GUILayout.Width(20));
            EditorGUI.BeginChangeCheck();
            filter = GUILayout.TextField(filter, GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck())
            {
                RefreshAssemblyInfo();
            }

            GUILayout.Label("Q", GUILayout.Width(15));
            EditorGUI.BeginChangeCheck();
            keyword = GUILayout.TextField(keyword, GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck())
            {
                RefreshAssemblyInfo();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawAssemblyView()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            foreach (var refAssem in refAssems)
            {
                DrawAssemblyArea(refAssem);
            }
            GUILayout.EndScrollView();
            GUILayout.Space(5);
        }

        private void DrawAssemblyArea(KeyValuePair<string, List<string>> refAssem)
        {
            DrawTextArea(refAssem.Key, Color.white);

            if (refAssem.Value.Count > 0)
            {
                DrawAssemblyArea(refAssem.Value);
            }

            if (refByAssems.ContainsKey(refAssem.Key))
            {
                if (refAssem.Value.Count > 0)
                {
                    DrawTextArea("-><-", Color.gray);
                }
                DrawAssemblyArea(refByAssems[refAssem.Key]);
            }
        }

        private void DrawAssemblyArea(IEnumerable<string> assems)
        {
            foreach (var assem in assems)
            {
                var color = Color.gray;
                if (refrefAssems.Contains(assem))
                {
                    color = Color.red;
                }
                DrawTextArea(assem, color);
            }
        }

        private void DrawTextArea(string text, Color color)
        {
            var origin = GUI.color;
            GUI.color = color;
            GUILayout.TextArea(text);
            GUI.color = origin;
        }
        #endregion
    }
}