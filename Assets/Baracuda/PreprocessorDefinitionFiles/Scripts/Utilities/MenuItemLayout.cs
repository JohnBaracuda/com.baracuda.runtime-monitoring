using UnityEditor;

namespace Baracuda.PreprocessorDefinitionFiles.Utilities
{
    internal static class MenuItemLayout
    {
        [MenuItem("Tools/Documentation/Preprocessor-Symbol-Definition-File", priority = int.MaxValue)]
        [MenuItem("Tools/Preprocessor-Symbol-Definition-File/Documentation", priority = 2365)]
        private static void OpenDefaultOnlineDocumentation()
        {
            Documentation.OpenOnlineDocumentation();
        }

        [MenuItem("Tools/Preprocessor-Symbol-Definition-File/Settings", priority = 2360)]
        public static void SelectPreprocessorSymbolDefinitionSettings()
        {
            PreprocessorSymbolDefinitionSettings.Select();
        }
    }
}
