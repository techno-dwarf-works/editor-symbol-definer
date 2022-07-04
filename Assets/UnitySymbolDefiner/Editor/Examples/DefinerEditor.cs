using UnityEditor;
using UnitySymbolDefiner.Attributes;

namespace UnitySymbolDefiner.Examples
{
    [CustomEditor(typeof(Definer))]
    public class DefinerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            SymbolDefiner.ShowSymbolsButtons();
        }
    }
}