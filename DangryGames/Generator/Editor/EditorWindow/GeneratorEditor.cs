using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.IO;
using UnityEditor.UIElements;
using Unity.VisualScripting;

namespace DangryGames.Generator
{
    public class GeneratorEditor : EditorWindow
    {
        VisualElement _container;
        TextField _txtScriptName;
        TextField _pathName;
        TextField _folderName;
        TextField _soFilename;
        TextField _soMenuname;
        Toggle _toggleNewFolder;
        TextField _nameSpace;
        Toggle _toggleNameSpace;
        ObjectField _baseClassName;
        ObjectField _addToObject;
        
        Toggle _baseClassConstructor;
        bool _needConstructor;
        Button _classBtn;
        Button _monoBtn;
        Button _derivedBtn;
        Button _interfaceBtn;
        Button _soBtn;
        Label _warningLbl;
        string filePathName;

        Color showColour = Color.grey;
        Color unselectColour = Color.black;

        VisualElement _soContainer;
        VisualElement _derivedContainer;

        Button _testBtn;
        string _currentSelectedBtn = "";

        private Button[] _buttonArray = new Button[5];
        

        public const string _assetPath = "Assets/DangryGames/Generator/Editor/EditorWindow/";

        [MenuItem("DangryGames/Script Generator")]
        public static void ShowWindow()
        {
            //Opens an editor window with a name of the class
            GeneratorEditor window = GetWindow<GeneratorEditor>();
            //Changes the name of the editor window to the string
            window.titleContent = new GUIContent("Script Generator");
            //sets the min size of the editor window
            window.minSize = new Vector2(600, 600);
            
        }


        private void CreateGUI()
        {
            _container = rootVisualElement;
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_assetPath  + "GeneratorEditor.uxml");
            _container.Add(visualTree.Instantiate());

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(_assetPath + "GeneratorEditor.uss");
            _container.styleSheets.Add(styleSheet);

            //Gets the first element of type TextField called txtScriptName
            _txtScriptName = _container.Q<TextField>("txtScriptName");

            //Gets the first element of type TextField called pathName
            _pathName = _container.Q<TextField>("pathName");

            _folderName = _container.Q<TextField>("folderName");
            if (_folderName == null)
            {
                Debug.Log("Failed to get Folder");
            }

            _toggleNameSpace = _container.Q<Toggle>("toggleNameSpace");
            _toggleNameSpace.RegisterValueChangedCallback(ShowNamespaceField);


            _nameSpace = _container.Q<TextField>("nameSpace");

            _testBtn = _container.Q<Button>("testBtn");
            _testBtn.clicked += CreateTestFolder;

            _monoBtn = _container.Q<Button>("monoBtn");
            _monoBtn.clicked += _baseBtn_clicked;

            _classBtn = _container.Q<Button>("classBtn");
            _classBtn.clicked += OnScriptButtonClick;

            _derivedBtn = _container.Q<Button>("derivedBtn");
            _derivedBtn.clicked += _derivedBtn_clicked;

            _interfaceBtn = _container.Q<Button>("interfaceBtn");
            _interfaceBtn.clicked += _interfaceBtn_clicked;

            _soBtn = _container.Q<Button>("soBtn");
            _soBtn.clicked += _soBtn_clicked;

            _buttonArray[0] = _monoBtn;
            _buttonArray[1] = _classBtn;
            _buttonArray[2] = _derivedBtn;
            _buttonArray[3] = _interfaceBtn;
            _buttonArray[4] = _soBtn;

            _soContainer = _container.Q<VisualElement>("SOContainer");

            _soFilename = _container.Q<TextField>("soFilename");
            _soMenuname = _container.Q<TextField>("soMenuname");

            _derivedContainer = _container.Q<VisualElement>("DerivedContainer");

            _baseClassName = _container.Q<ObjectField>("childName");
            _baseClassName.objectType = typeof(MonoScript);

            _baseClassConstructor = _container.Q<Toggle>("ctor");
            _baseClassConstructor.RegisterValueChangedCallback(NeedConstructor);

            _warningLbl = _container.Q<Label>("warningLbl");

            _addToObject = _container.Q<ObjectField>("addToObject");
            _addToObject.objectType = typeof(GameObject);
        }

        private void LoopThroughButtons(string currentBtn, int indexInArray)
        {
            if (_currentSelectedBtn != _buttonArray[indexInArray].name)
            {
                _currentSelectedBtn = _buttonArray[indexInArray].name;
                for(int i = 0; i < _buttonArray.Length; i++)
                {
                    if (_buttonArray[i].name == _currentSelectedBtn)
                    {
                        _buttonArray[i].style.backgroundColor = showColour;
                    }
                    else
                    {
                        _buttonArray[i].style.backgroundColor = unselectColour;
                    }
                    _warningLbl.AddToClassList("txtHidden");
                    _warningLbl.RemoveFromClassList("txtShow");
                    switch (indexInArray)
                    {
                        case 0:
                            _soContainer.style.display = DisplayStyle.None;
                            _derivedContainer.style.display = DisplayStyle.None;
                            break;
                        case 1:
                            _soContainer.style.display = DisplayStyle.None;
                            _derivedContainer.style.display = DisplayStyle.None;
                            break;
                        case 2:
                            _derivedContainer.style.display = DisplayStyle.Flex;
                            _soContainer.style.display = DisplayStyle.None;
                            break;
                        case 3:
                            _soContainer.style.display = DisplayStyle.None;
                            _derivedContainer.style.display = DisplayStyle.None;
                            break;
                        case 4:
                            _soContainer.style.display = DisplayStyle.Flex;
                            _derivedContainer.style.display = DisplayStyle.None;
                            break;
                    }

                }
            }else if (_currentSelectedBtn == currentBtn)
            {
                _currentSelectedBtn = "";
                _buttonArray[indexInArray].style.backgroundColor = unselectColour;
                switch (indexInArray)
                {
                    case 2:
                        _derivedContainer.AddToClassList("txtHidden");
                        _derivedContainer.RemoveFromClassList("txtShow");
                        _derivedContainer.style.display = DisplayStyle.None;
                        break;
                    case 4:
                        _soContainer.style.display = DisplayStyle.None;
                        break;
                }
            }
        }

        private void _soBtn_clicked()
        {
            LoopThroughButtons("soBtn", 4);
        }

        private void _interfaceBtn_clicked()
        {
            LoopThroughButtons("interfaceBtn", 3);
        }

        private void _derivedBtn_clicked()
        {
            LoopThroughButtons("derivedBtn", 2);
        }

        private void _baseBtn_clicked()
        {
            LoopThroughButtons("monoBtn", 0);
        }

        private void OnScriptButtonClick()
        {
            LoopThroughButtons("classBtn", 1);
        }

        private void CreateTestFolder()
        {
            if(_currentSelectedBtn == "")
            {
                _warningLbl.AddToClassList("txtShow");
                _warningLbl.RemoveFromClassList("txtHidden");
                _warningLbl.text = "Need to select a script format";
                return;
            }
            else
            {

                if (_currentSelectedBtn == "derivedBtn")
                {
                    if (_baseClassName.value == null)
                    {
                        _warningLbl.AddToClassList("txtShow");
                        _warningLbl.RemoveFromClassList("txtHidden");
                        _warningLbl.text = "Need to select a Parent class";
                        return;
                    }
                }

                if (_txtScriptName.text == "NewScript")
                {
                    _warningLbl.AddToClassList("txtShow");
                    _warningLbl.RemoveFromClassList("txtHidden");
                    _warningLbl.text = "Need to add a script name";
                    return;
                }

                //if (!_pathName.value.EndsWith("/"))
                //{
                //    //Adds the / to the end of the pathname as it needs it
                //    _pathName.value += "/";
                //}

                //string newFolderpath = "" + _pathName.value + _folderName.value + "/";
                string newFolderpath = "" + _pathName.value + "/";
                if (AssetDatabase.IsValidFolder(newFolderpath))
                {
                    _warningLbl.AddToClassList("txtShow");
                    _warningLbl.RemoveFromClassList("txtHidden");
                    _warningLbl.text = "Folder exists, creating file inside folder";
                }
                else
                {
                    //Creates a new folder
                    Directory.CreateDirectory(newFolderpath);
                    //Refreshes the editor window to show the new folder
                    AssetDatabase.Refresh();
                }
                CreateBasicScript(newFolderpath);
            }

            
        }

        public void NoNameMono(string filePathName)
        {
            if (File.Exists(filePathName))
            {
                Debug.Log("File already exists");
            }
            FileStream file = File.Create(filePathName);
            file.Close();
            using (StreamWriter outfile =
                     new StreamWriter(filePathName))
            {
                outfile.WriteLine("using UnityEngine;\n" +
                    "using System.Collections;\n" +
                    "using System.Collections.Generic;\n" +
                    "\n" +
                    "public class " + _txtScriptName.value + " : MonoBehaviour\n" +
                    "{\n" +
                    "    private void Start()\n" +
                    "    {\n" +
                    "\n" +
                    "    }\n" +
                    "\n" +
                    "    private void Update()\n" +
                    "    {\n" +
                    "    \n" +
                    "    }\n" +
                    "}\n");
            }//File written
        }

        public void NoNameClass(string filePathName)
        {
            if (File.Exists(filePathName))
            {
                Debug.Log("File already exists");
            }
            FileStream file = File.Create(filePathName);
            file.Close();
            using (StreamWriter outfile =
                     new StreamWriter(filePathName))
            {
                outfile.WriteLine("using UnityEngine;\n" +
                    "using System.Collections;\n" +
                    "using System.Collections.Generic;\n" +
                    "\n" +
                    "public class " + _txtScriptName.value +"\n" +
                    "{\n" +
                    "\n" +
                    "}");
            }//File written
        }

        public void NoNameInterface(string filePathName)
        {
            if (File.Exists(filePathName))
            {
                Debug.Log("File already exists");
            }
            FileStream file = File.Create(filePathName);
            file.Close();
            using (StreamWriter outfile =
                     new StreamWriter(filePathName))
            {
                outfile.WriteLine(
                    "using UnityEngine;\n" +
                    "using System.Collections;\n" +
                    "\n" +
                    "public interface " + _txtScriptName.value + "\n" +
                    "{\n" +
                    "\n" +
                    "}");
            }//File written
        }

        public void NoNameSO(string filePathName)
        {
            if (File.Exists(filePathName))
            {
                Debug.Log("File already exists");
            }
            FileStream file = File.Create(filePathName);
            file.Close();
            using (StreamWriter outfile =
                     new StreamWriter(filePathName))
            {
                outfile.WriteLine("using UnityEngine;\n" +
                    "\n" +
                    "[CreateAssetMenu(fileName = \"" + _soFilename.value + "\", menuName = \"" + _soMenuname.value + "\", order =1)]\n" +
                    "public class " + _txtScriptName.value + ": ScriptableObject\n" +
                    "{\n" +
                    "\n" +
                    "}");
            }//File written
        }

        public void NoNameDerived(string filePathName)
        {
            if (_needConstructor)
            {
                if (File.Exists(filePathName))
                {
                    Debug.Log("File already exists");
                }
                if (_baseClassName.value.name == "")
                {
                    _warningLbl.AddToClassList("txtShow");
                    _warningLbl.RemoveFromClassList("txtHidden");
                    _warningLbl.text = "Need to add in a bass class";
                    return;
                }
                FileStream file = File.Create(filePathName);
                file.Close();
                using (StreamWriter outfile =
                         new StreamWriter(filePathName))
                {
                    outfile.WriteLine("using UnityEngine;\n" +
                        "using System.Collections;\n" +
                        "using System.Collections.Generic;\n" +
                        "\n" +
                        "public class " + _txtScriptName.value + " : " + _baseClassName.value.name + "\n" +
                        "{\n" +
                        "    public " + _txtScriptName.value + "()\n" +
                        "    {\n" +
                        "\n" +
                        "    }\n" +
                        "}");
                }//File written
            }
            else
            {
                if (File.Exists(filePathName))
                {
                    Debug.Log("File already exists");
                }
                if (_baseClassName.value.name == "")
                {
                    _warningLbl.AddToClassList("txtShow");
                    _warningLbl.RemoveFromClassList("txtHidden");
                    _warningLbl.text = "Need to add in a bass class";
                    return;
                }
                FileStream file = File.Create(filePathName);
                file.Close();
                using (StreamWriter outfile =
                         new StreamWriter(filePathName))
                {
                    outfile.WriteLine("using UnityEngine;\n" +
                        "using System.Collections;\n" +
                        "using System.Collections.Generic;\n" +
                        "\n" +
                        "public class " + _txtScriptName.value +" : " + _baseClassName.value.name + "\n" +
                        "{\n" +
                        "\n" +
                        "}");
                }//File written
            }
        }

        public void NamespaceMono(string filePathName)
        {
            FileStream file = File.Create(filePathName);
            file.Close();
            using (StreamWriter outfile =
                     new StreamWriter(filePathName))
            {
                outfile.WriteLine("using UnityEngine;\n" +
                    "using System.Collections;\n" +
                    "\n" +
                    "namespace " + _nameSpace.value + "\n" +
                    "\n" +
                    "{\n" +
                    "    public class " + _txtScriptName.value + " : MonoBehaviour\n" +
                    "    {\n" +
                    "        private void Start()\n" +
                    "        {\n" +
                    "\n" +
                    "        }\n" +
                    "\n" +
                    "        private void Update()\n" +
                    "        {\n" +
                    "\n" +
                    "        }\n" +
                    "    }\n" +
                    "}");
            }//File written
        }

        public void NamespaceClass(string filePathName)
        {
            FileStream file = File.Create(filePathName);
            file.Close();
            using (StreamWriter outfile =
                     new StreamWriter(filePathName))
            {
                outfile.WriteLine("using UnityEngine;\n" +
                    "using System.Collections;\n" +
                    "using System.Collections.Generic;\n" +
                    "\n" +
                    "namespace " + _nameSpace.value + "\n" +
                    "{\n" +
                    "    public class " + _txtScriptName.value + "\n" +
                    "    {\n" +
                    "\n" +
                    "    }\n" +
                    "}");
            }//File written
        }

        public void NamespaceInterface(string filePathName)
        {
            FileStream file = File.Create(filePathName);
            file.Close();
            using (StreamWriter outfile =
                     new StreamWriter(filePathName))
            {
                outfile.WriteLine("using UnityEngine;\n" +
                    "using System.Collections;\n" +
                    "using System.Collections.Generic;\n" +
                    "\n" +
                    "namespace " + _nameSpace.value + "\n" +
                    "{\n" +
                    "    public interface " + _txtScriptName.value + "\n" +
                    "    {\n" +
                    "\n" +
                    "    }\n" +
                    "}");
            }//File written
        }

        public void NamespaceSO(string filePathName)
        {
            if (File.Exists(filePathName))
            {
                Debug.Log("File already exists");
            }
            FileStream file = File.Create(filePathName);
            file.Close();
            using (StreamWriter outfile =
                     new StreamWriter(filePathName))
            {
                outfile.WriteLine("using UnityEngine;\n" +
                    "using System.Collections;\n" +
                    "using System.Collections.Generic;\n" +
                    "\n" +
                    "namespace " + _nameSpace.value + "\n" +
                    "{\n" +
                    "[CreateAssetMenu(fileName = \"" + _soFilename.value + "\", menuName = \"" + _soMenuname.value + "\", order =1)]\n" +
                    "    public class " + _txtScriptName.value + ": ScriptableObject\n" +
                    "    {\n" +
                    "\n" +
                    "    }\n" +
                    "}");
            }//File written
        }

        public void NamespaceDerived(string filePathName)
        {
            if (_needConstructor)
            {
                FileStream file = File.Create(filePathName);
                file.Close();
                using (StreamWriter outfile =
                         new StreamWriter(filePathName))
                {
                    outfile.WriteLine("using UnityEngine;\n" +
                        "using System.Collections;\n" +
                        "using System.Collections.Generic;\n" +
                        "\n" +
                        "namespace " + _nameSpace.value + "\n" +
                        "{\n" +
                        "    public class " + _txtScriptName.value + " : " + _baseClassName.value.name + "\n" +
                        "    {\n" +
                        "        public " + _txtScriptName.value + "()\n" +
                        "        {\n" +
                        "\n" +
                        "        }\n" +
                        "    }\n" +
                        "}");
                }//File written
            }
            else
            {
                FileStream file = File.Create(filePathName);
                file.Close();
                using (StreamWriter outfile =
                         new StreamWriter(filePathName))
                {
                    outfile.WriteLine("using UnityEngine;\n" +
                        "using System.Collections;\n" +
                        "using System.Collections.Generic;\n" +
                        "\n" +
                        "namespace " + _nameSpace.value + "\n" +
                        "{\n" +
                        "    public class " + _txtScriptName.value + " : " + _baseClassName.value.name + "\n" +
                        "    {\n" +
                        "\n" +
                        "    }\n" +
                        "}");
                }//File written
            }
        }



        public void CreateBasicScript(string _path)
        {
            filePathName = (_path + _txtScriptName.value + ".cs");
            Debug.Log(filePathName);
            if (File.Exists(filePathName))
            {
                _warningLbl.AddToClassList("txtShow");
                _warningLbl.RemoveFromClassList("txtHidden");
                _warningLbl.text = "File already exists";
                return;
            }


            if (_nameSpace.value == "")
            {
                if (_currentSelectedBtn == "monoBtn")
                {
                    NoNameMono(filePathName);
                }
                else if (_currentSelectedBtn == "classBtn")
                {
                    NoNameClass(filePathName);
                }
                else if (_currentSelectedBtn == "interfaceBtn")
                {
                    NoNameInterface(filePathName);
                }
                else if (_currentSelectedBtn == "soBtn")
                {
                    NoNameSO(filePathName);
                }
                else if (_currentSelectedBtn == "derivedBtn")
                {
                    NoNameDerived(filePathName);
                }
            }
            else if (_nameSpace.value != "")
            {
                if (_currentSelectedBtn == "monoBtn")
                {
                    NamespaceMono(filePathName);
                }
                else if (_currentSelectedBtn == "classBtn")
                {
                    NamespaceClass(filePathName);
                }
                else if (_currentSelectedBtn == "interfaceBtn")
                {
                    NamespaceInterface(filePathName);
                }
                else if (_currentSelectedBtn == "soBtn")
                {
                    NamespaceSO(filePathName);
                }
                else if (_currentSelectedBtn == "derivedBtn")
                {
                    NamespaceDerived(filePathName);
                }
            }
            AssetDatabase.Refresh();
            AddComponent(_txtScriptName.value);
        }

        public void AddComponent(string componentName)
        {
            GameObject go = (GameObject)_addToObject.value;
            MonoScript newlyCreatedScript = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(filePathName + componentName))as MonoScript;
            //var runtimeType = assembly.GetType("RuntimeCompiled");
            Debug.Log($"created script??? : {newlyCreatedScript}");
            var typytype = newlyCreatedScript.GetType();
            Debug.Log($"Type = {typytype}");
        }

        private void ShowNamespaceField(ChangeEvent<bool> changeEvent)
        {
            if(_nameSpace == null)
            {
                Debug.Log("Namespace field is null");
            }

            if (changeEvent.newValue == true)
            {
                _nameSpace.AddToClassList("txtShow");
                _nameSpace.RemoveFromClassList("txtHidden");
                
            }
            else if (changeEvent.newValue == false)
            {
                _nameSpace.value = "";
                _nameSpace.AddToClassList("txtHidden");
                _nameSpace.RemoveFromClassList("txtShow");
                
            }

        }

        private void NeedConstructor(ChangeEvent<bool> changeEvent)
        {
            if(changeEvent.newValue == true)
            {
                _needConstructor = true;
            }else if (changeEvent.newValue == false)
            {
                _needConstructor = false;
            }
        }
    }
}



    