using UnityEngine;
using UnityEditor;
using SilverBuild;
using System.Xml;

public class EditorPlatformCheckBuildPhase : BuildPhase {
	
	public override string Name {
		get {
			return "Prevent building if editor mode does not match target";
		}
	}
	
	public override string Category {
		get {
			return "Safety";
		}
	}
	
	public override BuildPhaseStatus Status {
		get {
			BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
			BuildTargetGroup	activeMode = BuildUtility.GroupFromTarget(activeTarget),
								targetMode = BuildUtility.GroupFromTarget(Target.TargetPlatform);
			if (activeMode != targetMode) {
				return BuildPhaseStatus.Error;
			} else {
				return BuildPhaseStatus.Normal;
			}
		}
	}
	
	public override string StatusTooltip {
		get {
			if (Status == BuildPhaseStatus.Error) {
				return "Current editor mode does not match target";
			} else {
				return "";
			}
		}
	}
	
	public override string ActiveDescription {
		get {
			return "Checking editor mode...";
		}
	}
	
	public override int DisplayOrder {
		get {
			return 1000;
		}
	}
	
	public override bool HasPreprocess {
		get {
			return false;
		}
	}
	
	public override bool HasPostprocess {
		get {
			return false;
		}
	}
	
	public override bool SupportsTarget(BuildTarget target) {
		return true;
	}
	
	protected override void FromXml(XmlNode node) {
		m_enabled = true;
	}
	
	public override void OnGUI() {
		GUILayout.BeginHorizontal();
			GUILayout.Label("Prevent accidental editor mode switches");
			GUILayout.FlexibleSpace();
			GUILayout.Label("Current Mode: " + CurrentEditorMode);
			GUILayout.Space(15f);
			if (Status == BuildPhaseStatus.Error) {
				if (GUILayout.Button("Switch to " + TargetEditorMode, GUILayout.Height(20f))) {
					if (
						EditorUtility.DisplayDialog(
							"Switch editor from " + CurrentEditorMode + " to " + TargetEditorMode + "?",
							"Depending on the size of your project, this might take a while.",
							"Switch Mode",
							"Cancel"
						)
					) {
						EditorUserBuildSettings.SwitchActiveBuildTarget(Target.TargetPlatform);
					}
				}
			} else {
				GUILayout.Label("", GUILayout.Height(20f));
			}
			if (Enabled) {
				StatusLight();
			} else {
				DisabledStatusLight();
			}
		GUILayout.EndHorizontal();
	}
	
	protected BuildTargetGroup CurrentEditorMode {
		get {
			return BuildUtility.GroupFromTarget(EditorUserBuildSettings.activeBuildTarget);
		}
	}
	
	protected BuildTargetGroup TargetEditorMode {
		get {
			return BuildUtility.GroupFromTarget(Target.TargetPlatform);
		}
	}
	
}