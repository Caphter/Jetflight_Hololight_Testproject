namespace ExtendedColliders3DEditor {
    using UnityEngine;
    using UnityEditor;

    public class ExtendedColliders3DVersionChanges : EditorWindow {

        //Variables.
        Vector2 scrollPosition = Vector2.zero;
        GUIStyle _headerLabel = null;
        GUIStyle headerLabel {
            get {
                if (_headerLabel == null) {
                    _headerLabel = new GUIStyle(EditorStyles.boldLabel);
                    _headerLabel.alignment = TextAnchor.MiddleCenter;
                    _headerLabel.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                }
                return _headerLabel;
            }
        }
        GUIStyle _subHeaderLabel = null;
        GUIStyle subHeaderLabel {
            get {
                if (_subHeaderLabel == null) {
                    _subHeaderLabel = new GUIStyle(EditorStyles.boldLabel);
                    _subHeaderLabel.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                }
                return _subHeaderLabel;
            }
        }
        GUIStyle _boldWrappedLabel = null;
        GUIStyle boldWrappedLabel {
            get {
                if (_boldWrappedLabel == null) {
                    _boldWrappedLabel = new GUIStyle(EditorStyles.boldLabel);
                    _boldWrappedLabel.wordWrap = true;
                }
                return _boldWrappedLabel;
            }
        }
        GUIStyle _wrappedLabel = null;
        GUIStyle wrappedLabel {
            get {
                if (_wrappedLabel == null) {
                    _wrappedLabel = new GUIStyle(EditorStyles.label);
                    _wrappedLabel.padding = new RectOffset(25, 0, 0, 0);
                    _wrappedLabel.wordWrap = true;
                }
                return _wrappedLabel;
            }
        }
        GUIStyle _bulletPointAlignedAtTop = null;
        GUIStyle bulletPointAlignedAtTop {
            get {
                if (_bulletPointAlignedAtTop == null) {
                    _bulletPointAlignedAtTop = new GUIStyle(EditorStyles.label);
                    _bulletPointAlignedAtTop.alignment = TextAnchor.UpperCenter;
                }
                return _bulletPointAlignedAtTop;
            }
        }

        //Draw the GUI.
        void OnGUI() {

            //Display the version change text.
            EditorGUILayout.LabelField("Extended Colliders 3D Version Changes", headerLabel);
            EditorGUILayout.GetControlRect();
            EditorGUILayout.LabelField("If you have any comments or suggestions as to how we could improve Extended Colliders 3D, or if you want to report a " +
                    "bug in the software, feel free to e-mail us on info@battenbergsoftware.com and we'll get back to you. Thanks!", boldWrappedLabel);
            EditorGUILayout.GetControlRect();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.LabelField("Version 1.0.8", subHeaderLabel);
            EditorGUILayout.LabelField("Added geosphere colliders, which are spheres made from triangulated icosahedrons and have a custom level of detail.",
                    wrappedLabel);
            addBulletPoint();
            EditorGUILayout.LabelField("Added half tube colliders.", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.LabelField("Added torus (doughnut-shaped) colliders.", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.LabelField("When a new collider type is selected, the default properties for it are set on the component.", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.GetControlRect();
            EditorGUILayout.LabelField("Version 1.0.7", subHeaderLabel);
            EditorGUILayout.LabelField("Added support for tube colliders, which can optionally be capped on the top and/or bottom.", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.LabelField("Added support for half-sphere colliders, which can optionally be capped on the bottom.", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.LabelField("Gizmos of colliders can now be shown as wireframe meshes, solid meshes or both.", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.LabelField("Added assembly definition files for build and editor scripts.", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.LabelField("Added indentation in the inspector for Extended Colliders 3D components to improve layout.", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.GetControlRect();
            EditorGUILayout.LabelField("Version 1.0.6", subHeaderLabel);
            EditorGUILayout.LabelField("Added new collider types: Octahedron, Dodecahedron and Icosahedron.", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.LabelField("Added cooking options as a new property on the Extended Colliders 3D component which is copied across to the mesh " +
                    "collider that is created at runtime.", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.GetControlRect();
            EditorGUILayout.LabelField("Version 1.0.5", subHeaderLabel);
            EditorGUILayout.LabelField("Added gizmos for positioning, rotating and scaling Extended Colliders 3D components in the scene view. The gizmo to " +
                    "be used can be selected in the inspector, and will affect the centre, rotation and scale in the general settings of the collider (not " +
                    "the overall transform of the game object).", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.GetControlRect();
            EditorGUILayout.LabelField("Version 1.0.4", subHeaderLabel);
            EditorGUILayout.LabelField("Extended Colliders 3D components can now be automatically sized to fit the associated mesh filter, if there is one. " +
                    "This occurs automatically when the Extended Colliders 3D component is first added and can be performed manually by clicking a new " +
                    "\"Auto-size to Mesh\" button in the general collider settings.", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.GetControlRect();
            EditorGUILayout.LabelField("Version 1.0.3", subHeaderLabel);
            EditorGUILayout.LabelField(
                    "Fixed memory leaks when drawing gizmos for Extended Colliders 3D components and when unloading a scene that contained one.", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.LabelField("Added scripts to \"ExtendedColliders3D\" namespace.", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.LabelField("Added this version changes window.", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.GetControlRect();
            EditorGUILayout.LabelField("Version 1.0.2", subHeaderLabel);
            EditorGUILayout.LabelField("Added support for sphere colliders, the detail of which can be customised. The main advantage of these over Unity's " +
                    "built-in sphere collider is that they can be scaled in a non-uniform way.", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.GetControlRect();
            EditorGUILayout.LabelField("Version 1.0.1", subHeaderLabel);
            EditorGUILayout.LabelField("Fixed an issue whereby the position, rotation and scale of the colliders in the scene view was incorrect if the " +
                    "Extended Colliders 3D component was on a game object that was more than one level down in the hierarchy. The parent, grandparent, etc. " +
                    "transforms weren't being applied properly.", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.GetControlRect();
            EditorGUILayout.LabelField("Version 1.0.0", subHeaderLabel);
            EditorGUILayout.LabelField("Initial release.", wrappedLabel);
            addBulletPoint();
            EditorGUILayout.EndScrollView();
        }

        //Adds a bullet point before the label that has just been added.
        void addBulletPoint() {
            Rect rect = GUILayoutUtility.GetLastRect();
            EditorGUI.LabelField(new Rect(17, rect.yMin - 1, 12, rect.height), "•", bulletPointAlignedAtTop);
        }
    }
}