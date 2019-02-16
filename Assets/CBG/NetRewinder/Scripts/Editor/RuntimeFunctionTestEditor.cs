using UnityEngine;
using System.Collections;
using UnityEditor;

namespace CBG {
    [CustomEditor(typeof(RuntimeFunctionTest))]
    public class RuntimeFunctionTestEditor : Editor {
        public override void OnInspectorGUI() {
            var tester = (RuntimeFunctionTest)target;
            if (GUILayout.Button("Test GetHitboxes")) {
                tester.TestRewinderGetHitboxes();
            }
            if (GUILayout.Button("Test SetHitboxes")) {
                tester.TestRewinderSetHitboxes();
            }
        }

    }
}


