using UnityEditor;
using UnitySymbolDefiner.Attributes;

namespace UnitySymbolDefiner.Examples
{
    [CustomEditor(typeof(TestDefiner))]
    public class TestDefinerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            SymbolDefiner.ShowSymbolsButtons();
        }
    }
}