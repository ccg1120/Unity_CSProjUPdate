using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Choe
{
    public class Parser : AssetPostprocessor
    {
        private readonly static string DefaultFolder = "Parser";
        private readonly static string RootPathDataFolder = "PathData";
        private readonly static string RootPathFileName = "PathData.txt";

        private static string mUnityDataPath = string.Empty;
        private static string mDefaulPath = string.Empty;
        private static string mRootPathDataPath = string.Empty;
        private static string mRootPathDataFilePath = string.Empty;


        private static string DefaultPath = @"..\..\";

        private static string DllType = ".dll";
        private static string EventNodeName = "PropertyGroup";
        private static string EventKeyword = "PostBuildEvent";
        private static string UnityAssetFolder = "Assets";

        private static string TargetNodeName = "ItemGroup";
        private static string TargetNodeChileName = "Compile";
        private static string CopyEventCommandtoTargetDir = "XCOPY /y \"$(TargetDir)\"";
        
        

        private static char UnityPathSplitChar = '\\';
        private static char WindowPathSplitChar = '/';

        private static RootPathContainer RootDataSet;

        public static string UnityDataPath
        {
            get
            {
                return mUnityDataPath;
            }
        }
        public static string RootPathDataPath
        {
            get
            {
                return mRootPathDataPath;
            }
        }
        public static string RootPathDataFilePath
        {
            get
            {
                return mRootPathDataFilePath;
            }
        }

        public static void NewRootDataSet()
        {
            if(RootDataSet != null)
            {
                Debug.Log("RootDataSet is Not null");
                return;
            }
            RootDataSet = new RootPathContainer();
        }
        public static bool CheckRootDataSet()
        {
            if (RootDataSet == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public static string RootDataSetToJson()
        {
            Debug.Log("RootDataSetToJson count : " + RootDataSet.RootPathList.Count);
            string json = JsonUtility.ToJson(RootDataSet);
            Debug.Log("RootDataSetToJson : Json : "+ json);
            return json;
        }
        public static void RootDataSetFromJson(string json)
        {
            RootDataSet = (RootPathContainer)JsonUtility.FromJson(json, typeof(RootPathContainer));
            Debug.Log(RootDataSet);
        }
      
        #region SetRootdata
        public static void SetUnityRootPath(int index, string path)
        {
            if(RootDataSet.RootPathList.Count < index)
            {
                Debug.LogError("SetRootPath Faile");
                return;
            }
            RootDataSet.RootPathList[index].UnityRootPath = path;
        }
        public static void SetCSProjectPath(int index, string path)
        {
            if (RootDataSet.RootPathList.Count < index)
            {
                Debug.LogError("SetRootPath Faile");
                return;
            }
            RootDataSet.RootPathList[index].CSProjectPath = path;
        }
        public static void SetCSProjectBasePath(int index)
        {
            if (RootDataSet.RootPathList.Count < index)
            {
                Debug.LogError("SetRootPath Faile");
                return;
            }
            if(!RootDataSet.RootPathList[index].CSProjectPath.Equals(string.Empty))
            {
                int pahtlenght = RootDataSet.RootPathList[index].CSProjectPath.Length;
                int filenamelenght = Path.GetFileName(RootDataSet.RootPathList[index].CSProjectPath).Length;

                string pathwithoutfilename = RootDataSet.RootPathList[index].CSProjectPath.Substring(0, pahtlenght - filenamelenght);
                RootDataSet.RootPathList[index].CSProjectRootPath = pathwithoutfilename;
            }
            
        }
        public static void SetCSBaseUnityCommonPath(int index, string path)
        {
            if (RootDataSet.RootPathList.Count < index)
            {
                Debug.LogError("SetRootPath Faile");
                return;
            }
            RootDataSet.RootPathList[index].CSBaseUnityCommonPath = path;
        }
        public static void SetDLLName(int index, string path)
        {
            if (RootDataSet.RootPathList.Count < index)
            {
                Debug.LogError("SetRootPath Faile");
                return;
            }
            RootDataSet.RootPathList[index].DLLName = path;
        }
        public static void SetDLLCopyPath(int index, string path)
        {
            if (RootDataSet.RootPathList.Count < index)
            {
                Debug.LogError("SetRootPath Faile");
                return;
            }
            RootDataSet.RootPathList[index].DLLCopyPath = path;
        }
        #endregion

        public static void AddRootPath()
        {
            if (RootDataSet == null)
            {
                Debug.LogError("RootDataSet is null you need load data");
                return;
            }
            RootPath pathdat = new RootPath();

            RootDataSet.RootPathList.Add(pathdat);
            Debug.Log("Add Data :"+ RootDataSet.RootPathList.Count);
        }

        public static RootPath GetRootPathByIndex(int index)
        {
            return RootDataSet.RootPathList[index];
        }
        public static int GetRootPathListCount()
        {
            return RootDataSet.RootPathList.Count;
        }

        static void OnPostprocessAllAssets(
            string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {

            LoadorCreate();

            CheckCSDirAndCreate();

            if (RootDataSet == null)
            {
                Debug.Log("RootDataSet is null");
                return;
            }

            Debug.Log(importedAssets.Length);
            string[] csfilenameArray = FindCSFileNameArry(importedAssets);
            if (csfilenameArray.Length == 0)
            {
                Debug.Log("CS File null");

            }
            else
            {
                for (int index = 0; index < csfilenameArray.Length; index++)
                {
                    Debug.Log(csfilenameArray[index]);
                }

                int setlenght = RootDataSet.RootPathList.Count;
                Debug.Log("RootDataSet list Count = "+ setlenght);
                for (int listindex = 0; listindex < setlenght; listindex++)
                {
                    GetCSPROJData(listindex, csfilenameArray);
                }

            }
        }

        /// <summary>
        /// 린큐 사용법으로 그냥 둠
        /// </summary>
        /// <param name="array"></param>
        private static void ImportAssetChecker(string[] array)
        {
            var directofry = array.Where(im => im.EndsWith(".cs"))
               .GroupBy(ia => Path.GetDirectoryName(ia))
               .Select(g => g.Key);
        }

        private static string[] GetAddFileArrayByCommonPath(string commonpath, string[] csfilearray)
        {
            List<string> result = new List<string>();
            string common = commonpath;
            int cslength = csfilearray.Length;
                
            for (int csindex = 0; csindex < cslength; csindex++)
            {
                if(csfilearray[csindex].Contains(common))
                {
                    Debug.Log("GetAddFileArrayByCommonPath Add File name : "+ csfilearray[csindex]);
                    result.Add(csfilearray[csindex]);
                }
                else
                {
                    Debug.Log("GetAddFileArrayByCommonPath file  : " + csfilearray[csindex]+ " ,, common : "+ commonpath);
                }
            }

            return result.ToArray();
        }
        private static void GetCSPROJData(int num, string[] csfilearray)
        {

            var csprojdata = new XmlDocument();
            //데이터 로드
            csprojdata.Load(RootDataSet.RootPathList[num].CSProjectPath);
            //네임스페이스
            string mBaseNameSpace = csprojdata.DocumentElement.NamespaceURI;
            string commonpath = RootDataSet.RootPathList[num].CSBaseUnityCommonPath;
            Debug.Log("CommonPath : " + commonpath);
            string unitycommonpath = GetUnityCommonPath(commonpath, DefaultPath);
            
            string[] addfilearray = GetAddFileArrayByCommonPath(unitycommonpath, csfilearray);

            Debug.Log("addfilearray lenght" + addfilearray.Length);
            int childcount = csprojdata.DocumentElement.ChildNodes.Count;
            XmlNodeList nodelist = csprojdata.DocumentElement.ChildNodes;

            XmlNode itemnode = null;

            for (int index = 0; index < childcount; index++)
            {
                if (nodelist[index].Name.Equals(TargetNodeName))
                {
                    if (nodelist[index].FirstChild.Name.Equals(TargetNodeChileName))
                    {
                        //컴파일이 들어가는 노드 Get
                        itemnode = nodelist[index];
                        break;
                    }
                }
            }

            XmlNode copylastnode = null;
            //공통 경로 추출용 임시 리스트
            List<string> tempattributelist = new List<string>();
            if (itemnode != null)
            {
                //마지막 노드 복사 및 제거 
                copylastnode = itemnode.LastChild.CloneNode(true);
                itemnode.RemoveChild(itemnode.LastChild);

                //Debug.Log("itemnode.Name : " + itemnode.Name);
                //Debug.Log("itemnode Value " + itemnode.Value);
                XmlNodeList nodechildlist = itemnode.ChildNodes;
                int nodechildcount = nodechildlist.Count;
                Debug.Log("Child Count : " + nodechildcount);

                for (int index = 0; index < nodechildcount; index++)
                {
                    Debug.Log("Item node name " + nodechildlist[index].Name);
                    XmlAttributeCollection attributes = nodechildlist[index].Attributes;
                    int attcount = attributes.Count;
                    for (int attindex = 0; attindex < attcount; attindex++)
                    {
                        Debug.Log("Item node name " + attributes[attindex].Value);
                        tempattributelist.Add(attributes[attindex].Value);
                    }
                }
            }
            else
            {
                Debug.Log("itemnode is null");
            }

            string windowcommonpath = ConvertWindowPathDirectorySeparatorChar(unitycommonpath);
            Debug.Log("windowcommonpath = "+ windowcommonpath );
            string includePath = DefaultPath + windowcommonpath;

            int csfilelenght = addfilearray.Length;
            for (int csindex = 0; csindex < csfilelenght; csindex++)
            {
                Debug.Log(csindex + " : GetPathWithOutCommonPath : " + GetPathWithOutCommonPath(unitycommonpath, addfilearray[csindex]));
                AddNode(itemnode, includePath, GetPathWithOutCommonPath(unitycommonpath, addfilearray[csindex]), mBaseNameSpace);
            }

            //last node add
            itemnode.AppendChild(copylastnode);

            
            csprojdata.Save(RootDataSet.RootPathList[num].CSProjectPath);
        }

        private static string CreatefolderByPath(string paht)
        {
            DirectoryInfo folderinfo = new DirectoryInfo(paht);

            if(!folderinfo.Exists)
            {
                folderinfo.Create();
            }
            return folderinfo.FullName;
        }

        private static void CheckCSDirAndCreate()
        {
            int pathlistlenght = RootDataSet.RootPathList.Count;

            for (int subindex = 0; subindex < pathlistlenght; subindex++)
            {
                Debug.Log(RootDataSet.RootPathList[subindex].UnityRootPath);
                Debug.Log(RootDataSet.RootPathList[subindex].CSProjectRootPath);
                TransferUnityToCSDirectory(RootDataSet.RootPathList[subindex].UnityRootPath, RootDataSet.RootPathList[subindex].CSProjectRootPath);
            }
        }

        private static void TransferUnityToCSDirectory(string unitypaht, string cspath)
        {
            DirectoryInfo unityrootinfo = new DirectoryInfo(unitypaht);
            DirectoryInfo cspathinfo = new DirectoryInfo(cspath);

            DirectoryInfo[] subinfos = unityrootinfo.GetDirectories();

            int subdinfolenght = subinfos.Length;

            for (int subindex = 0; subindex < subdinfolenght; subindex++)
            {
                string copydirpath = Path.Combine(cspathinfo.FullName, subinfos[subindex].Name);
                Debug.Log("Copy paht : " + copydirpath);

                string createcopypath = CreatefolderByPath(copydirpath);
                TransferUnityToCSDirectory(subinfos[subindex].FullName, createcopypath);
            }
        }
        private void TrandferUnityToCSDirectory(DirectoryInfo info)
        {

            DirectoryInfo[] subinfos = info.GetDirectories();
            int subdinfolenght = subinfos.Length;

            for (int subindex = 0; subindex < subdinfolenght; subindex++)
            {

            }
        }



        /// <summary>
        /// CS파일 이름 반환
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private static string[] FindCSFileNameArry(List<string> array)
        {
            return FindCSFileNameArry(array.ToArray());
        }
        private static string[] FindCSFileNameArry(string[] array)
        {
            List<string> result = new List<string>();
            int arrayindex = array.Length;

            for (int index = 0; index < arrayindex; index++)
            {
                if (array[index].EndsWith(".cs"))
                {
                    result.Add(array[index]);
                    Debug.Log(Path.GetDirectoryName(array[index]));
                }
            }
            return result.ToArray();
        }


        /// <summary>
        /// 공통 이용 경로 확인
        /// </summary>
        /// <param name="pathlist"></param>
        /// <returns></returns>
        private static string GetCommonPath(List<string> pathlist)
        {
            string result = string.Empty;
            if (pathlist.Count == 0)
            {
                return result;
            }
            int comoparemaxcount = int.MaxValue;
            if (pathlist.Count == 1)
            {
                string[] comparegroupbyindex = pathlist[0].Split(UnityPathSplitChar);
                comoparemaxcount = comparegroupbyindex.Length - 1;
            }
            else
            {
                for (int pathindex = 0; pathindex < pathlist.Count - 1; pathindex++)
                {
                    string[] comparegroupbyindex = pathlist[pathindex].Split(UnityPathSplitChar);
                    string[] comparegroupbyaddindex = pathlist[pathindex + 1].Split(UnityPathSplitChar);

                    int comparecount = 0;

                    if (comparegroupbyindex.Length > comparegroupbyaddindex.Length)
                    {
                        comparecount = comparegroupbyaddindex.Length;
                    }
                    else
                    {
                        comparecount = comparegroupbyindex.Length;
                    }

                    //cs 파일 제외
                    comparecount -= 1;
                    int tempcomoparemaxcount = 0;

                    for (int compareindex = 0; compareindex < comparecount; compareindex++)
                    {
                        if (comparegroupbyindex[compareindex].Equals(comparegroupbyaddindex[compareindex]))
                        {
                            tempcomoparemaxcount++;
                        }
                    }
                    if (tempcomoparemaxcount < comoparemaxcount)
                    {
                        comoparemaxcount = tempcomoparemaxcount;
                    }
                }
            }


            string[] tempresultarray = pathlist[0].Split(UnityPathSplitChar);
            for (int resultindex = 0; resultindex < comoparemaxcount; resultindex++)
            {
                result = Path.Combine(result, tempresultarray[resultindex]);
            }

            return result;
        }

        //private static string GetUnityCommonPath(string commonpath, string defaltpath)
        //{
        //    Debug.Log("GetUnityCommonPath________________");
        //    Debug.Log("commonpath : "+ commonpath);
        //    Debug.Log("defaltpath : " + defaltpath);
        //    Debug.Log("GetUnityCommonPath________________End");
        //    string result = string.Empty;
        //    int startindex = defaltpath.Length;
        //    int substringleght = commonpath.Length - defaltpath.Length;
        //    result = commonpath.Substring(startindex, substringleght);
        //    Debug.Log("GetUnityCommonPath : " + result);
        //    return result;
        //}

        private static string GetUnityCommonPath(string commonpath, string defaltpath)
        {
            Debug.Log("GetUnityCommonPath________________");
            Debug.Log("commonpath : " + commonpath);
            Debug.Log("defaltpath : " + defaltpath);
            Debug.Log("GetUnityCommonPath________________End");
            string[] foldernamearray = commonpath.Split(WindowPathSplitChar);
            int namearraylenght = foldernamearray.Length;
            
            int unityassetfolderindex = -1;
            //find asset forder index
            for (int nameindex = 0; nameindex < namearraylenght; nameindex++)
            {
                if (foldernamearray[nameindex].Equals(UnityAssetFolder))
                {
                    unityassetfolderindex = nameindex;
                    break;  
                }
            }
            string result = string.Empty;

            for (int naemindex = unityassetfolderindex; naemindex < namearraylenght; naemindex++)
            {
                //result = Path.Combine(result, foldernamearray[naemindex]);
                result = result + foldernamearray[naemindex];
                if(naemindex != namearraylenght-1)
                {
                    result += "/";
                }
            }
            Debug.Log("GetUnityCommonPath : "+ result);

            return result;
        }

        private static string ConvertWindowPathDirectorySeparatorChar(string unitypath)
        {
            return unitypath.Replace('/', '\\');
        }

        private static string GetPathWithOutCommonPath(string commonPath, string fullpath)
        {
            Debug.Log("GetPathWithOutCommonPath : commonPath : " + commonPath);
            
            int startindex = commonPath.Length +1; // 마지막 / 추가
            int resultlenght = fullpath.Length - startindex;

            string result = fullpath.Substring(startindex, resultlenght);

            string[] resultsplit = result.Split(WindowPathSplitChar);
            if (resultsplit.Length != 1)
            {
                result = string.Empty;

                for (int splitindex = 0; splitindex < resultsplit.Length - 1; splitindex++)
                {
                    result += resultsplit[splitindex] + @"\";
                }
                result += resultsplit[resultsplit.Length - 1];
            }

            return result;
        }

        private static void AddNode(XmlNode root, string commonpath, string link, string mBaseNameSpace)
        {
            XmlElement compilenode = root.OwnerDocument.CreateElement(string.Empty, "Compile", mBaseNameSpace);
            string linkpath = Path.Combine(commonpath, link);
            Debug.Log("commonpath : " + commonpath);

            Debug.Log("Link Path : " + linkpath);

            compilenode.SetAttribute("Include", linkpath);

            XmlElement linknode = compilenode.OwnerDocument.CreateElement(string.Empty, "Link", mBaseNameSpace);
            linknode.InnerText = link;

            compilenode.AppendChild(linknode);

            root.AppendChild(compilenode);
        }

        public class RootPathContainer
        {
            public List<RootPath> RootPathList = new List<RootPath>();
        }

        [System.Serializable]
        public class RootPath
        {
            public string UnityRootPath = string.Empty;
            public string CSProjectPath = string.Empty;
            public string CSProjectRootPath = string.Empty;
            public string CSBaseUnityCommonPath = string.Empty;
            public string DLLName = string.Empty;
            public string DLLCopyPath = string.Empty;
        }

        public static void LoadorCreate()
        {
            GetPath();

            if (CheckRootPathFile())
            {
                Debug.Log("Have data file");
                if(RootDataSet == null)
                {
                    ReadPathData(mRootPathDataFilePath);
                }
            }
            else
            {
                CreateRootPathData(mRootPathDataFilePath);
            }
        }

        /// <summary>
        /// 경로 획득
        /// </summary>
        private static void GetPath()
        {
            Debug.Log("GetPath");
            mUnityDataPath = Application.dataPath;
            mDefaulPath = Path.Combine(mUnityDataPath, DefaultFolder);
            mRootPathDataPath = Path.Combine(mDefaulPath, RootPathDataFolder);
            mRootPathDataFilePath = Path.Combine(mRootPathDataPath, RootPathFileName);
            Debug.Log("File path : " + mRootPathDataFilePath);
        }

        /// <summary>
        /// PathData데이터 파일 확인
        /// </summary>
        /// <returns></returns>
        private static bool CheckRootPathFile()
        {
            bool result = false;

            DirectoryInfo defaultinfo = new DirectoryInfo(mDefaulPath);
            if (!defaultinfo.Exists)
            {
                defaultinfo.Create();
                AssetDatabase.Refresh();
            }
            DirectoryInfo rootpathdatainfo = new DirectoryInfo(mRootPathDataPath);
            if (!rootpathdatainfo.Exists)
            {
                rootpathdatainfo.Create();
                AssetDatabase.Refresh();
            }
            FileInfo[] rootpathfilearray = rootpathdatainfo.GetFiles();
            int filearraylenght = rootpathfilearray.Length;
            for (int fileindex = 0; fileindex < filearraylenght; fileindex++)
            {
                if (rootpathfilearray[fileindex].Name.Equals(RootPathFileName))
                {
                    Debug.Log("Have " + RootPathFileName + " file");
                    result = true;
                }
            }
            return result;
        }



        /// <summary>
        /// 초기 빈 데이터 파일 생성 Parser.RootData 생성
        /// </summary>
        /// <param name="fullfilepath"></param>
        private static void CreateRootPathData(string fullfilepath)
        {
            NewRootDataSet();
            SaveRootPathData();
        }

        /// <summary>
        /// PathData파일 로더
        /// </summary>
        /// <param name="fullfilepaht"></param>
        private static void ReadPathData(string fullfilepaht)
        {
            string json = File.ReadAllText(fullfilepaht);
            Debug.Log(json);
            Parser.RootDataSetFromJson(json);
        }

        /// <summary>
        /// RootDataSet 저장
        /// </summary>
        /// <param name="fullfilepath"></param>
        public static void SaveRootPathData()
        {
            string json = Parser.RootDataSetToJson();
            Debug.Log("Json : " + json);
            if(mRootPathDataFilePath.Equals(string.Empty))
            {
                Debug.LogError("SaveRootPathData Erro : mRootPathDataFilePath is Empty");
                return;
            }
            File.WriteAllText(mRootPathDataFilePath, json);

            AssetDatabase.Refresh();
        }
    }


}




