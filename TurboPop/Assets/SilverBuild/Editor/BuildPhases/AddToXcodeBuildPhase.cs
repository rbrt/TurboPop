using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml;
using SilverBuild;
using SilverBuild.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

/** Injects files into the Xcode project that results from an iOS-targeted build. */
public class AddToXcodeBuildPhase : BuildPhase {
	//Helper struct
	protected struct XcodePath{
		public string path;
		public bool isFrameworkLink;
	}

	protected const string kProjectPath = "Unity-iPhone.xcodeproj/project.pbxproj"; //Relative to build directory
	protected const string kAutomaticReferenceCountingFlag = "-fobjc-arc";
	//Files with these extensions will be added to the Sources Compilation Xcode build phase
	protected static string[] kSourceFileExtensions = new string[]{".c", ".cpp", ".m", ".mm", ".dll", ".dll.s"}; 
	//Files and directories with these names will be ignored
	protected static string[] kIgnoredFiles = new string[]{".DS_Store", ".svn"}; 
	//Files and directories ending with these names will be ignored
	protected static string[] kIgnoredSuffixes = new string[]{".meta"};
	//Files with these extensions will not be added to any Xcode build phase.
	protected static string[] kHeaderFileExtensions = new string[]{".h"};
	//Files with these extensions will be added to the frameworks Xcode build phase
	protected static string[] kArchiveFileExtensions = new string[]{".a"};
	//"Directories" with these extensions will be added as a file and not explored recursively
	protected static string[] kBundleFileExtensions = new string[]{".bundle"};
	//Files with these names will be read to determine framework requirements.
	//These files are also not included in the project.
	protected static string[] kFrameworkDefinitionFiles = new string[]{"frameworks.cfg"};
	//These directories will be copied using cp -a in order to preserve symlinks
	protected static string[] kFrameworkExtensions = new string[]{".framework"};
	
	protected XcodePath[] m_paths = new XcodePath[0]; //Array of paths to be added to the project
	protected string m_errorMessage = "";
	protected string m_sdkPath;

	public override string Name {
		get {
			return "Add Files To Xcode";
		}
	}

	public override string ActiveDescription {
		get {
			return "Adding files to Xcode";
		}
	}
	
	public override string Category{
		get {return "Plugins";}
	}

	public override bool HasPreprocess{
		get{return false;}
	}

	public override bool HasPostprocess{
		get{return true;}
	}

	public override int PostprocessOrder {
		get {
			return -1000;
		}
	}
	
	public override BuildPhaseStatus Status {
		get {
			if (PathsValid) {
				return BuildPhaseStatus.Normal;
			} else {
				return BuildPhaseStatus.Error;
			}
		}
	}
	
	public override string StatusTooltip {
		get {
			return m_errorMessage;
		}
	}
	
	/* Check that paths exist */	
	public bool PathsValid{
		get {
			if (m_pathsValid == null) {
				CheckPathsValid();
			}
			return (bool)m_pathsValid;
		}
	}
	protected bool? m_pathsValid;
	
	/** Map of Xcode unique IDs to our internal representations of them */
	protected Dictionary<string, XcodeObject> UniqueIds{
		get; set;
	}
	
	protected HashSet<string> RequiredFrameworks{
		get { 
			if (m_requiredFrameworks == null){
				m_requiredFrameworks = new HashSet<string>();
			}
			return m_requiredFrameworks;
		}
	}
	private HashSet<string> m_requiredFrameworks;
	
	protected bool EnableBfgAutomaticReferenceCounting{
		get{
			return m_enableBFGARC;
		}
		set{
			if(m_enableBFGARC != value){
				m_enableBFGARC = value;
				Dirty = true;
			}
		}		
	}
	protected bool m_enableBFGARC;
	
	protected bool Expanded{get; set;} //Expand path list
	protected XcodeObject ObjectRoot{get; set;} //Root object of all Xcode objects we'll be dealing with
	protected XcodeObject RootGroup{get; set;}  //Object representing the root group
	protected XcodeObject ProjectObject{get; set;} //Object representing the project itself
	//Objects representing various build phases
	protected XcodeObject MainTargetFrameworkPhase{get; set;}
	protected XcodeObject MainTargetResourcePhase{get; set;}
	protected XcodeObject MainTargetSourcePhase{get; set;}
	protected string BuildPath{get; set;} //Full path of the build directory
	protected string m_uidPrefix = "0000000000000001"; //16 digit prefix concat 8 digit counter = 24 digit UID
	protected uint m_uidOffset = 0;

	public bool IsIgnored(string fileName){
		return Array.Exists(kIgnoredFiles, ignored => ignored == fileName)
			|| Array.Exists(kIgnoredSuffixes, suffix => fileName.EndsWith(suffix))
			|| IsFrameworkDefinition(fileName);
	}
	
	public bool IsSource(string fileName){
		return Array.Exists(kSourceFileExtensions, extension => fileName.EndsWith(extension));
	}
	
	public bool IsHeader(string fileName){
		return Array.Exists(kHeaderFileExtensions, extension => fileName.EndsWith(extension));
	}
	
	public bool IsArchive(string fileName){
		return Array.Exists(kArchiveFileExtensions, extension => fileName.EndsWith(extension));
	}
	
	public bool IsBundle(string fileName){
		return Array.Exists(kBundleFileExtensions, extension => fileName.EndsWith(extension));
	}
	
	public bool IsFrameworkDefinition(string fileName){
		return Array.Exists(kFrameworkDefinitionFiles, defFileName => defFileName == fileName);
	}
	
	public bool IsFrameworkDirectory(string directoryName){
		return Array.Exists(kFrameworkExtensions, extension => directoryName.EndsWith(extension));
	}
	
	//Whether file should have Automatic Reference Counting flag enabled
	public bool UsesAutomaticReferenceCounting(string filePath){
		return filePath.Contains("/bfg_source/") 
		&& !filePath.Contains("/bfg_source/json-framework/")
		&& !filePath.EndsWith("NSData+Base64.m")
		&& !filePath.EndsWith("RegexKitLite.m");
	}

	public override bool SupportsTarget(BuildTarget target) {
		return	target == BuildTarget.iOS;
	}
	
	public override void Refresh(){
		CheckPathsValid();
		FindRequiredFrameworks();
	}

	public override bool PostprocessBuild(BuildTarget target, string buildPath, string dataPath) {
		if (!PathsValid) {
			Debug.LogError("Paths not valid: " + m_errorMessage);
			return false;
		}
		BuildPath = Path.GetFullPath(buildPath);		
		try{
			UniqueIds = new Dictionary<string, XcodeObject>();
			
			//Load pbxproj file
			Console.WriteLine("Loading .xcodeproj at " + buildPath + "/" + kProjectPath);
			StreamReader reader = new StreamReader(buildPath + "/" + kProjectPath);
			reader.ReadLine(); //Eat explicit encoding header
			XcodeObject proj = new XcodeObject(reader, UniqueIds);
			reader.Close();
			
			//Setup important properties
			FindRequiredFrameworks();
			ObjectRoot = proj.children["objects"];			
			ObjectRoot.DefinePreExistingObjects();
			ProjectObject = ObjectRoot.children[proj.literals["rootObject"]];
			RootGroup = ObjectRoot.children[ProjectObject.literals["mainGroup"]];
			List<XcodeObject> mainTargetBuildPhases = ProjectObject.ParseIDList("targets")[0].ParseIDList("buildPhases");
			MainTargetFrameworkPhase = mainTargetBuildPhases.Find(delegate(XcodeObject obj){return obj.literals["isa"] == "PBXFrameworksBuildPhase";});
			MainTargetResourcePhase = mainTargetBuildPhases.Find(delegate(XcodeObject obj){return obj.literals["isa"] == "PBXResourcesBuildPhase";});
			MainTargetSourcePhase = mainTargetBuildPhases.Find(delegate(XcodeObject obj){return obj.literals["isa"] == "PBXSourcesBuildPhase";});
			
			//Remove secondary targets (currently only the non-functional simulator target)
			string[] buildTargetIDs = XcodeObject.SplitPCSVList(ProjectObject.literals["targets"]);
			ProjectObject.literals.Remove("targets");
			ProjectObject.AddLiteralListElement("targets", buildTargetIDs[0]);
			
			//Add required frameworks
			Console.WriteLine("Adding required frameworks");
			foreach(string frameworkPath in RequiredFrameworks){
				Console.WriteLine(">\t" + frameworkPath);
				AddSDKFramework(frameworkPath);
			}
						
			//Add files
			Console.WriteLine("Adding specified directories and frameworks");
			for(int i = 0; i < m_paths.Length; i++){
				if (!m_paths[i].isFrameworkLink) {
					Console.WriteLine("DIR>\t"+m_paths[i].path);
					AddDirectoryRecursively(m_paths[i].path, buildPath, RootGroup);
				}
				else if (!RequiredFrameworks.Contains(m_paths[i].path)) {
					Console.WriteLine("FWK>\t"+m_paths[i].path);
					AddSDKFramework(m_paths[i].path);
				}
			}
			
			//Save modified file
			Console.WriteLine("Saving .xcodeproj file");
			StreamWriter writer = new StreamWriter(buildPath + "/" + kProjectPath); //Overwrite .pbxproj file
			writer.Write("// !$*UTF8*$!\n"); //Explicit format header
			proj.Save(writer, 0);
			writer.Close();
		}
		catch(Exception e){
			Debug.LogError(e.ToString());
			return false;
		}
		return true;
	}
	
	public override void OnGUI() {
		GUILayout.BeginHorizontal();
			Enabled = GUILayout.Toggle(Enabled, Name);
			GUILayout.FlexibleSpace();
			EnableBfgAutomaticReferenceCounting = GUILayout.Toggle(EnableBfgAutomaticReferenceCounting, new GUIContent("BFG ARC", "Enable Automatic Reference Counting for BFGLib"));
			GUILayout.FlexibleSpace();
			if(Expanded){
				//List currently included items	
				GUILayout.BeginVertical();
				for(int i = 0; i < m_paths.Length; i++){
					GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();	
						GUILayout.Label(m_paths[i].path);
						if(m_paths[i].isFrameworkLink){
							GUILayout.Label("[F]");
						}
						if(GUILayout.Button("-", GUILayout.Width(30))){
							m_paths = ArrayExtensions.RemoveAt(m_paths, i);
							Dirty = true;
						}
					GUILayout.EndHorizontal();					
				}
				//list of automatically included frameworks
				if(RequiredFrameworks.Count > 0){
					string frameworkList = "";
					int addedFrameworksCount = 0;
					foreach(string framework in RequiredFrameworks){
						if(!Array.Exists(m_paths, xpath => xpath.isFrameworkLink && xpath.path == framework)){
							frameworkList += framework + "\n";
							addedFrameworksCount ++;
						}
					}
					frameworkList = frameworkList.Trim();
					
					if(addedFrameworksCount > 0){
						GUILayout.BeginHorizontal();
							GUI.enabled = false;
							GUILayout.FlexibleSpace();
							GUILayout.Label(new GUIContent("Plus " + addedFrameworksCount + " required frameworks & libs", frameworkList), EditorStyles.miniLabel);
							GUI.enabled = true;
						GUILayout.EndHorizontal();
					}
				}
				//Add-new-item buttons
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if(GUILayout.Button("Add Dir")){
					string dataPathParent = Directory.GetParent(Application.dataPath).FullName;
					string newPath = EditorUtility.OpenFolderPanel("Select Directory To Add", dataPathParent, "");
					if(newPath != ""){
						//Check new path is valid
						if(newPath.StartsWith(dataPathParent)){							
							XcodePath xpath = new XcodePath();
							//Change to relative path
							xpath.path = newPath.Substring(dataPathParent.Length + 1); // +1 chomps leading slash
							xpath.isFrameworkLink = false;
							//Insert it
							m_paths = ArrayExtensions.InsertAt(m_paths, xpath, m_paths.Length);
							Dirty = true;
						}
						else{
							Debug.LogError("Could not add folder: Directory must be inside the project folder.");
						}
					}
				}
				if(GUILayout.Button("Add Framework")){
					if(m_sdkPath == null){
						m_sdkPath = EditorUtility.OpenFolderPanel("Select SDK directory", "/Developer/Platforms/iPhoneOS.platform/Developer/SDKs", "");
					}
					string newPath = EditorUtility.OpenFolderPanel("Select File to Add", m_sdkPath + "/System/Library/Frameworks/", "");
					if(newPath != ""){
						//Check new path is valid
						if(newPath.StartsWith(m_sdkPath)){
							XcodePath xpath = new XcodePath();
							//Change to relative path
							xpath.path = newPath.Substring(m_sdkPath.Length + 1); // +1 chomps leading slash
							xpath.isFrameworkLink = true;
							//Insert it
							m_paths = ArrayExtensions.InsertAt(m_paths, xpath, m_paths.Length);
							Dirty = true;
						}
						else{
							Debug.LogError("Could not add file: File must be inside the SDK folder.");
						}
					}
				}
				if(GUILayout.Button("Add Library")){
					if(m_sdkPath == null){
						m_sdkPath = EditorUtility.OpenFolderPanel("Select SDK directory", "/Developer/Platforms/iPhoneOS.platform/Developer/SDKs", "");
					}
					string newPath = EditorUtility.OpenFilePanel("Select File to Add", m_sdkPath, "");
					if(newPath != ""){
						//Check new path is valid
						if(newPath.StartsWith(m_sdkPath)){
							//If selected file was an alias the file panel will dereference it without telling us. Try to undo the dereference here.
							FileInfo fileInfo = new FileInfo(newPath);
							if(fileInfo.Name.IndexOf(".") != fileInfo.Name.LastIndexOf(".")){
								FileInfo baseFileInfo = new FileInfo(Path.Combine(fileInfo.DirectoryName, fileInfo.Name.Substring(0, fileInfo.Name.IndexOf(".")) + fileInfo.Name.Substring(fileInfo.Name.LastIndexOf("."))));
								if(baseFileInfo.Exists){
									newPath = baseFileInfo.FullName;
								}
							}								
							XcodePath xpath = new XcodePath();
							//Change to relative path
							xpath.path = newPath.Substring(m_sdkPath.Length + 1); // +1 chomps leading slash
							xpath.isFrameworkLink = true;
							//Insert it
							m_paths = ArrayExtensions.InsertAt(m_paths, xpath, m_paths.Length);
							Dirty = true;
						}
						else{
							Debug.LogError("Could not add file: File must be inside the SDK folder.");
						}
					}
				}
				if(GUILayout.Button(">.<")){
					Expanded = false;
				}
				GUILayout.EndHorizontal();
				GUILayout.EndVertical();
			} //End if Expanded			
			else{
				GUILayout.Label(m_paths.Length + " files");
				if(GUILayout.Button("...")){
					Expanded = true;
				}
			}
			if (Enabled) {
				StatusLight();
			} else {
				DisabledStatusLight();
			}
		GUILayout.EndHorizontal();
	}
	
	protected override void ToXml(XmlNode root) {
		root.AddBoolElement("Enabled", Enabled);
		root.AddIntElement("Count", m_paths.Length);
		for(int i = 0; i < m_paths.Length; i++){
			root.AddStringElement("Path" + i, m_paths[i].path);
			root.AddBoolElement("IsFrame"+i, m_paths[i].isFrameworkLink);
		}
		root.AddBoolElement("BfgArc", m_enableBFGARC);
	}
	
	protected override void FromXml(XmlNode node) {
		m_enabled = node["Enabled"].GetInnerBool();
		int count = node["Count"].GetInnerInt();
		m_paths = new XcodePath[count];
		for(int i = 0; i < count; i++){
			m_paths[i].path = node["Path"+i].InnerText;
			m_paths[i].isFrameworkLink = node["IsFrame"+i].GetInnerBool();
		}
		if(node["BfgArc"] != null){
			m_enableBFGARC = node["BfgArc"].GetInnerBool();
		}
		else{
			m_enableBFGARC = true;
		}
	}
	
	protected void CheckPathsValid(){
		for(int i = 0; i < m_paths.Length; i++){
			if(!m_paths[i].isFrameworkLink){
				DirectoryInfo di = new DirectoryInfo(m_paths[i].path);
				//Check directory exists
				if(!di.Exists){
					m_errorMessage = "Directory '" + m_paths[i].path + "' doesn't exist";
					m_pathsValid = false;
					return;
				}
			}				
		}
		m_errorMessage = "";
		m_pathsValid = true;
	}
	
	//Provides unique file IDs and ensures the ObjectRoot knows about them. Don't have more than 2^32 files.
	protected string GetNewUniqueId(XcodeObject requester){
		//Find a non-taken UID
		string uid;
		do{
			uid = m_uidPrefix + m_uidOffset.ToString("X8"); //Hex encode
			m_uidOffset++;
		} while(UniqueIds.ContainsKey(uid));
		
		//Assign it
		UniqueIds[uid] = requester;
		ObjectRoot.children[uid] = requester;
		return uid;
	}
	
	//Fetches a group object by name
	protected XcodeObject GetGroup(string groupName){
		return RootGroup.GetGroup(groupName);
	}
	
	//frameworkFileName -- filepath relative to SDKs directory (e.g. "System/Library/Frameworks/CoreTelephony.framework")
	//framework is put under the "Frameworks" group and added to the main target
	protected void AddSDKFramework(string frameworkFilename){
		//Make the file reference object
		XcodeObject frameworkObject = new XcodeObject(UniqueIds);
		frameworkObject.literals["isa"] = "PBXFileReference";
		if(frameworkFilename.EndsWith(".dylib")){
			frameworkObject.literals["lastKnownFileType"] = "\"compiled.mach-o.dylib\"";
		}
		else{
			frameworkObject.literals["lastKnownFileType"] = "wrapper.framework";
		}
		frameworkObject.literals["name"] = "\""+Path.GetFileName(frameworkFilename)+"\"";
		frameworkObject.literals["path"] = "\""+frameworkFilename+"\"";
		frameworkObject.literals["sourceTree"] = "SDKROOT";
		string frameworkUId = GetNewUniqueId(frameworkObject);
		//Make appear in frameworks group
		GetGroup("Frameworks").AddLiteralListElement("children", frameworkUId);
		//Make build reference
		XcodeObject buildFile = new XcodeObject(UniqueIds);
		buildFile.literals["isa"] = "PBXBuildFile";
		buildFile.literals["fileRef"] = frameworkUId;
		string buildFileUId = GetNewUniqueId(buildFile);
		//Add build reference to build phase
		MainTargetFrameworkPhase.AddLiteralListElement("files", buildFileUId);
	}
	
	//Copes a directory into the XCode project as a framework -- the directory is referenced directly
	//as a framework, its contents are not directly referenced.
	protected void AddFrameworkDirectory(DirectoryInfo srcDir, string destDir, XcodeObject parentGroup){
		//Create XCode file ref
		XcodeObject fileRef = new XcodeObject(UniqueIds);
		fileRef.literals["isa"] = "PBXFileReference";
		fileRef.literals["name"] = "\""+Path.GetFileName(srcDir.Name)+"\"";
		fileRef.literals["path"] = "\""+srcDir.Name+"\"";
		fileRef.literals["sourceTree"] = "\"<group>\"";
		fileRef.literals["lastKnownFileType"] = "wrapper.framework";
		//Add to group
		string fileRefId = GetNewUniqueId(fileRef);
		parentGroup.AddLiteralListElement("children", fileRefId);
		//Create Xcode build file ref
		XcodeObject buildFile = new XcodeObject(UniqueIds);
		buildFile.literals["isa"] = "PBXBuildFile";
		buildFile.literals["fileRef"] = fileRefId;
		string buildFileId = GetNewUniqueId(buildFile);
		//Add build reference to resources phase
		MainTargetFrameworkPhase.AddLiteralListElement("files", buildFileId);		
		//Copy framework contents
		DirectoryInfo frameworkDir = new DirectoryInfo(Path.Combine(destDir, srcDir.Name));
		ExternalProcesses.RunProcess("cp", "-a \"" + srcDir.FullName + "\" \"" + frameworkDir.FullName+ "\"");
		//Add path of framework's containing directory to search paths
		AddFrameworkSearchPath(frameworkDir.Parent.FullName);
	}
	
	//Copies a directory and its contents into the XCode project recursively and adds it to the main target.
	protected void AddDirectoryRecursively(string sourceDir, string destDir, XcodeObject parentGroup){
		DirectoryInfo sourceDirInfo = new DirectoryInfo(sourceDir);
		//Create files
		foreach(FileInfo file in sourceDirInfo.GetFiles()){
			if(IsIgnored(file.Name)){
				continue;
			}		
			else{	
				AddFile(file, destDir, parentGroup);
			}
		}
		//Recurse
		foreach(DirectoryInfo subDir in sourceDirInfo.GetDirectories()){
			if(IsIgnored(subDir.Name)){
				// ...
			}
			else if(IsBundle(subDir.Name)){
				AddBundle(subDir, destDir, parentGroup);
			}
			else if(IsFrameworkDirectory(subDir.Name)){
				AddFrameworkDirectory(subDir, destDir, parentGroup);
			}
			else{
				XcodeObject groupObj = null;
				DirectoryInfo destSubDir = new DirectoryInfo(Path.Combine(destDir, subDir.Name));
				//Set up the dest directory
				if(!destSubDir.Exists || parentGroup.GetGroup(destSubDir.Name) == null){
					destSubDir.Create();
					//Make containing Xcode group
					groupObj = new XcodeObject(UniqueIds);
					groupObj.literals["isa"] = "PBXGroup";
					groupObj.literals["sourceTree"] = "\"<group>\"";
					groupObj.literals["children"] = "()";
					groupObj.literals["path"] = "\""+destSubDir.Name+"\"";
					string groupObjId = GetNewUniqueId(groupObj);
					parentGroup.AddLiteralListElement("children", groupObjId);
				}
				else{
					groupObj = parentGroup.GetGroup(destSubDir.Name);
				}
				AddDirectoryRecursively(subDir.FullName, Path.Combine(destDir, subDir.Name), groupObj);
			}
		}
	}
	
	//Adds a directory to the library search paths for the project
	protected void AddLibrarySearchPath(string directory){
		directory = Path.GetFullPath(directory).Substring(BuildPath.Length);
		List<XcodeObject> buildConfigs = UniqueIds[ProjectObject.literals["buildConfigurationList"]].ParseIDList("buildConfigurations");
		foreach(XcodeObject obj in buildConfigs){
			obj.children["buildSettings"].AddLiteralListElement("LIBRARY_SEARCH_PATHS", "\"\\\"$(SRCROOT)" + directory + "\\\"\""); //So XCode interprets as quoted
		}
	}
	
	//Adds a directory to the framework search paths for the project
	protected void AddFrameworkSearchPath(string directory){
		directory = Path.GetFullPath(directory).Substring(BuildPath.Length);
		List<XcodeObject> buildConfigs = UniqueIds[ProjectObject.literals["buildConfigurationList"]].ParseIDList("buildConfigurations");
		foreach(XcodeObject obj in buildConfigs){
			obj.children["buildSettings"].AddLiteralListElement("FRAMEWORK_SEARCH_PATHS", "\"\\\"$(SRCROOT)" + directory + "\\\"\""); //So XCode interprets as quoted
		}
	}
	
	protected void AddFile(FileInfo file, string destDir, XcodeObject parentGroup){
		string filePath = Path.Combine(destDir, file.Name);
		if(!File.Exists(filePath)){
			//Create XCode file ref
			XcodeObject fileRef = new XcodeObject(UniqueIds);
			fileRef.literals["isa"] = "PBXFileReference";
			fileRef.literals["name"] = "\""+file.Name+"\"";
			fileRef.literals["path"] = "\""+file.Name+"\"";
			fileRef.literals["sourceTree"] = "\"<group>\"";
			//Add to group
			string fileRefId = GetNewUniqueId(fileRef);
			parentGroup.AddLiteralListElement("children", fileRefId);
			//Create Xcode build file ref
			XcodeObject buildFile = new XcodeObject(UniqueIds);
			buildFile.literals["isa"] = "PBXBuildFile";
			buildFile.literals["fileRef"] = fileRefId;
			string buildFileId = GetNewUniqueId(buildFile);
			//Add build reference to appropriate build phase
			if(IsSource(file.Name)){
				MainTargetSourcePhase.AddLiteralListElement("files", buildFileId);
			}
			else if(IsArchive(file.Name)){
				MainTargetFrameworkPhase.AddLiteralListElement("files", buildFileId);
				AddLibrarySearchPath(destDir);
			}
			else if(!IsHeader(file.Name)){
				MainTargetResourcePhase.AddLiteralListElement("files", buildFileId);
			}
			if(EnableBfgAutomaticReferenceCounting && UsesAutomaticReferenceCounting(file.FullName)){
				var settingsObj = new XcodeObject(UniqueIds);
				buildFile.children["settings"] = settingsObj;
				settingsObj.literals["COMPILER_FLAGS"] = kAutomaticReferenceCountingFlag;
			}
		}
		else{
			Console.WriteLine("Overwrote file " + filePath);
		}
		//Copy disk file
		file.CopyTo(filePath, true);
	}
	
	//Adds a directory to the XCode project as a bundle -- the directory is referenced directly as a file, its contents are copied unreferenced
	protected void AddBundle(DirectoryInfo srcDir, string destDir, XcodeObject parentGroup){		
		//Create XCode file ref
		XcodeObject fileRef = new XcodeObject(UniqueIds);
		fileRef.literals["isa"] = "PBXFileReference";
		fileRef.literals["name"] = "\""+srcDir.Name+"\"";
		fileRef.literals["path"] = "\""+srcDir.Name+"\"";
		fileRef.literals["sourceTree"] = "\"<group>\"";
		//Add to group
		string fileRefId = GetNewUniqueId(fileRef);
		parentGroup.AddLiteralListElement("children", fileRefId);
		//Create Xcode build file ref
		XcodeObject buildFile = new XcodeObject(UniqueIds);
		buildFile.literals["isa"] = "PBXBuildFile";
		buildFile.literals["fileRef"] = fileRefId;
		string buildFileId = GetNewUniqueId(buildFile);
		//Add build reference to resources phase
		MainTargetResourcePhase.AddLiteralListElement("files", buildFileId);
		DirectoryInfo bundleDir = new DirectoryInfo(Path.Combine(destDir, srcDir.Name));
		AddBundleContentsRecursively(srcDir, bundleDir);
	}
	
	//Copies the directory and all its contents recursively into the xcode project but does not add references
	protected void AddBundleContentsRecursively(DirectoryInfo srcDir, DirectoryInfo destDir){
		destDir.Create();
		foreach(FileInfo fi in srcDir.GetFiles()){
			if(!IsIgnored(fi.Name)){
				fi.CopyTo(Path.Combine(destDir.FullName, fi.Name));
			}			
		}
		foreach(DirectoryInfo di in srcDir.GetDirectories()){
			if(!IsIgnored(di.Name)){
				AddBundleContentsRecursively(di, new DirectoryInfo(Path.Combine(destDir.FullName, di.Name)));
			}			
		}
	}
	
	protected void FindRequiredFrameworks(){
		RequiredFrameworks.Clear();
		foreach(XcodePath xpath in m_paths){
			if(!xpath.isFrameworkLink && Directory.Exists(xpath.path)){
				FindRequiredFrameworksRecursively(new DirectoryInfo(xpath.path));
			}
		}
	}
	
	protected void FindRequiredFrameworksRecursively(DirectoryInfo dir){
		foreach(FileInfo fi in dir.GetFiles()){
			if(IsFrameworkDefinition(fi.Name)){
				ReadFrameworkDefinition(fi.FullName);
			}
		}
		foreach(DirectoryInfo subDir in dir.GetDirectories()){
			FindRequiredFrameworksRecursively(subDir);
		}
	}
	
	protected void ReadFrameworkDefinition(string filePath){
		StreamReader reader = new StreamReader(filePath);
		while(!reader.EndOfStream){
			string line = reader.ReadLine();
			if(line.StartsWith("//")){				
				continue; // line is a comment
			}
			else{
				RequiredFrameworks.Add(line);
			}			
		}
		reader.Close();
	}
}

public class XcodeObject {
	
	public Dictionary<string, string> literals = new Dictionary<string, string>(); // Key -> Value
	public Dictionary<string, XcodeObject> children = new Dictionary<string, XcodeObject>(); //Key -> Object
	
	protected Dictionary<string, XcodeObject> m_uniqueIds; //Shared object! Don't do anything too crazy
	
	//Basic constructor
	public XcodeObject(Dictionary <string, XcodeObject> uids){
		m_uniqueIds = uids;
	}
	
	//Construct an XcodeObject and fill it with data from the given reader
	public XcodeObject(StreamReader reader, Dictionary <string, XcodeObject> uids){
		m_uniqueIds = uids;
		
		StringBuilder sb = new StringBuilder();
		//Object definitions are expected to start with an opening brace, eat it
		if(reader.Read() != '{'){
			Debug.LogError("Object didn't start with opening brace");
		}

		bool quoted = false;	//True if Currently inside quotes, ignore special character meanings
		bool escaped = false;	//True if Next character has special meaning ignored
		bool commented = false;	//True if Inside a block comment, ignore everything except end-comment
		bool isLiteralValue = true; //Value is a literal (as opposed to a container of objects)
		string key = ""; //Last key read
		int readChar; //Char read from the stream
		//Now start parsing for real
		while(reader.Peek() != -1){
			//If currently in a comment
			if(commented){
				//Check if we drop out of comment
				readChar = reader.Read();
				if(readChar == '*' && reader.Peek() == '/'){
					reader.Read(); //Eat the end-of-comment '/'
					commented = false;
				}
				continue; //Ignore read character
			}
			//Check to see if we should start reading child object instead
			if(!quoted && reader.Peek() == '{'){
				isLiteralValue = false;
				children[key] = new XcodeObject(reader, m_uniqueIds); //Read child data
			}
			//Read next character
			readChar = reader.Read();
			// = : Key finished
			if(!quoted && readChar == '='){
				key = sb.ToString().Trim();
				sb.Length = 0; //Clear contents of stringbuilder
			}
			// ; : Key-value pair finished
			else if(!quoted && readChar == ';'){				
				if(isLiteralValue){
					string value = sb.ToString().Trim();
					literals[key] = value;					
				}
				sb.Length = 0; //Clear contents of stringbuilder
				isLiteralValue = true;				
			}
			// } : Object finished
			else if(!quoted && readChar == '}'){
				break;
			}
			// / : Might be the start of a comment
			else if(!quoted && readChar == '/'){
				//Comment starts
				if(reader.Peek() == '*'){
					reader.Read(); //Eat the *
					commented = true;
				}
				//Comment didn't start, add the '/'
				else{
					sb.Append((char)readChar);
				}
			}
			//Ignore whitespace outside quotes
			else if(!quoted && char.IsWhiteSpace((char)readChar)){
				// Do nothing
			}
			// " : Toggle quoted / not
			else if(!escaped && readChar == '"'){
				quoted = !quoted;
				sb.Append((char)readChar);
				escaped = false;
			}
			// \ : "Escape" next char
			else if(!escaped && readChar == '\\'){
				sb.Append((char)readChar);
				escaped = true;
			}
			//Just another character
			else{
				sb.Append((char)readChar);
				escaped = false;
			}			
		}
	}
	
	//Write out representation to given writer at given indent level
	//(The indenting doesn't matter to Xcode)
	public void Save(StreamWriter writer, int indentLevel){		
		writer.Write("{\n"); //Beginning-of-object opening brace, indenting assumed to already be correct
		//Write literals in key = value format
		foreach(string s in literals.Keys){			
			if(literals[s].StartsWith("(")){
				//Write each element of a PCSV list on its own line
				WriteIndentedString(s + " = ", writer, indentLevel+1);
				WriteIndentedList(literals[s], writer, indentLevel+1);
			}
			else{
				WriteIndentedString(s + " = " + literals[s] + ";\n", writer, indentLevel+1);
			}
		}
		//Write object links, also in key = value format
		foreach(string s in children.Keys){
			WriteIndentedString(s + " = ", writer, indentLevel+1);
			children[s].Save(writer, indentLevel + 1);
			writer.Write(";\n");
		}
		WriteIndentedString("}", writer, indentLevel); //Closing brace
	}
	
	/** Adds the element 'value' into the PCSV list 'key',
		creating the list if it doesn't already exist. */
	public void AddLiteralListElement(string key, string value){
		if(literals.ContainsKey(key)){
			string curVal = literals[key];
			literals[key] = curVal.Substring(0, curVal.Length - 1) + value + ",)";
		}
		else{
			literals[key] = "("+value+",)";
		}
	}
	
	//Searches the group hierarchy for the object with the given name
	//Only call this on objects which define groups (isa = PBXGroup)
	public XcodeObject GetGroup(string groupName){
		if(groupName == ""){
			return this;
		}
		int slashIndex = groupName.IndexOf("/");
		string beforeSlash = slashIndex >= 0 ? groupName.Substring(0, slashIndex) : groupName;
		string afterSlash = slashIndex >= 0 ? groupName.Substring(slashIndex + 1) : "";
		List<XcodeObject> contents = ParseIDList("children");
		foreach(XcodeObject obj in contents){
			if(obj.literals["isa"] == "PBXGroup"){
				if(obj.literals.ContainsKey("name") && obj.literals["name"] == beforeSlash || obj.literals.ContainsKey("path") && obj.literals["path"] == beforeSlash){
					return obj.GetGroup(afterSlash);
				}
			}
		}
		return null;
	}
	
	//Links up UIDs which are already in use to their objects
	//Only call this on the ObjectRoot
	public void DefinePreExistingObjects(){
		foreach(string id in children.Keys){
			m_uniqueIds[id] = children[id];
		}
	}
	
	//Returns a list of XCodeObjects corresponding to the values (which must by UIDs) of the given key.
	public List<XcodeObject> ParseIDList(string key){
		List<XcodeObject> result = new List<XcodeObject>();
		string[] ids = SplitPCSVList(literals[key]);
		for(int i = 0; i < ids.Length; i++){
			result.Add(m_uniqueIds[ids[i]]);
		}
		return result;
	}
	
	//Save helper function, writes a string at a given indentation level
	protected void WriteIndentedString(string s, StreamWriter writer, int indentLevel){
		for(int i = 0; i < indentLevel; i++){
			writer.Write("\t");
		}
		writer.Write(s);
	}
	
	//Save helper function, writes a PCSV list at a given indentation level
	protected void WriteIndentedList(string listString, StreamWriter writer, int indentLevel){
		string[] entries = SplitPCSVList(listString);
		writer.Write("(\n");
		for(int i = 0; i < entries.Length; i++){
			WriteIndentedString(entries[i] + ",\n", writer, indentLevel+1);
		}
		WriteIndentedString(");\n", writer, indentLevel+1);
	}
	
	/** Takes a parenthesized comma-separated value (PCSV) list and returns the
		elements of the list */
	public static string[] SplitPCSVList(string listString){
		listString = listString.Substring(1, listString.Length-2); //Trim parentheses
		StringBuilder sb = new StringBuilder();
		StringReader reader = new StringReader(listString);
		List<string> result = new List<string>();
		bool escaped = false;
		bool quoted = false;
		int readChar;
		while((readChar = reader.Read()) != -1){
			if((char)readChar == '\"'){
				if(escaped){
					escaped = false;
				}
				else{
					quoted = !quoted;
				}
			}
			else if(!escaped && (char)readChar == '\\'){
				escaped = true;
			}
			else{
				escaped = false;
			}
			
			if(!quoted && (char)readChar == ','){
				result.Add(sb.ToString());
				sb.Length = 0; //Clear stringbuilder contents
			}
			else{
				sb.Append((char)readChar);
			}
		}
		return result.ToArray();
	}
}