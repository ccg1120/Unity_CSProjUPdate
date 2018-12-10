using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;


namespace Choe
{
    public class PathSettingEditor : EditorWindow {

        static void ShowWindow()
        {
            PathSettingEditor window = (PathSettingEditor)GetWindow<PathSettingEditor>("Path Setting");
            window.Show();

        }

        [MenuItem("Tool/CSProj Setting")]
        public static void ShowMenu()
        {
            ShowWindow();
        }

        private void OnEnable()
        {
            Debug.Log("Window OnEnable");
            Parser.LoadorCreate();
        }

   
        private void OnDisable()
        {
            Debug.Log("Window OnDisable");
        }

        private void OnGUI()
        {
            GUILayout.Label("TESTTEST");

            RootPathDataSettingUIMenu();

            if (GUILayout.Button("데이터 추가"))
            {
                Parser.AddRootPath();
            }

            if (GUILayout.Button("저장"))
            {
                Parser.SaveRootPathData();
            }
        }

        private  void RootPathDataSettingUIMenu()
        {
            if(!Parser.CheckRootDataSet())
            {
                return;
            }
            int rootpathlistlenght = Parser.GetRootPathListCount();

            for (int listindex = 0; listindex < rootpathlistlenght; listindex++)
            {
                RootPathDataSettingUIMenuByIndex(listindex);
            }
        }
        private void RootPathDataSettingUIMenuByIndex(int index)
        {
            GUILayout.BeginVertical("HelpBox");
        
            GUILayout.BeginHorizontal();
            GUILayout.Label(" Unity CSProject Path : " + Parser.GetRootPathByIndex(index).CSProjectPath);
            if (GUILayout.Button("Open"))
            {
                string CSProjectPath = EditorUtility.OpenFilePanel("CSProjectPath", Parser.UnityDataPath, "csproj");
                Parser.SetCSProjectPath(index, CSProjectPath);
                Parser.SetCSProjectBasePath(index);
            }
            GUILayout.EndHorizontal();
            //GUILayout.BeginHorizontal();
            GUILayout.Label(" Unity CSProject Base Path : " + Parser.GetRootPathByIndex(index).CSProjectRootPath);
            //if (GUILayout.Button("Open"))
            //{
            //    string CSProjectBasePath = EditorUtility.OpenFolderPanel("CSProjectBasePath", mUnityDataPath, "");
            //    Parser.GetRootPathByIndex(index).CSProjectBasePath = CSProjectBasePath;
            //}
            //GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(" Unity CSBase Unity CommonPath : " + Parser.GetRootPathByIndex(index).CSBaseUnityCommonPath);
            if (GUILayout.Button("Open"))
            {
                string CSBaseUnityCommonPath = EditorUtility.OpenFolderPanel("CSBaseUnityCommonPath", Parser.UnityDataPath, "");
                Parser.GetRootPathByIndex(index).CSBaseUnityCommonPath = CSBaseUnityCommonPath;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(" DLL Name : ");
            string inputdllname = GUILayout.TextField(Parser.GetRootPathByIndex(index).DLLName);

            if (!inputdllname.Equals(Parser.GetRootPathByIndex(index).DLLName))
            {
                Parser.GetRootPathByIndex(index).DLLName = inputdllname;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(" DLLCopyPath : " + Parser.GetRootPathByIndex(index).DLLCopyPath);
            if (GUILayout.Button("Open"))
            {
                string DLLCopyPath = EditorUtility.OpenFolderPanel("CSBaseUnityCommonPath", Parser.UnityDataPath, "");
                Parser.GetRootPathByIndex(index).DLLCopyPath = DLLCopyPath;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
      
    }
}