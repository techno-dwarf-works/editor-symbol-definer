using UnitySymbolDefiner.Attributes;

namespace UnitySymbolDefiner.Examples
{
    public class TestExamples
    {
        [SymbolDefiner(true)] private const string SomeDefinition = "DEFINITION_WITH_AUTO_DEFINE_2";
        [SymbolDefiner(false)] private const string SomeOtherDefinition = "DEFINITION_WITHOUT_AUTO_DEFINE";
    }
}