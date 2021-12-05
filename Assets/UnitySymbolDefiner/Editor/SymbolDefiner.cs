#region license

// Copyright 2021 - 2021 Arcueid Elizabeth D'athemon
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace CorePlugin.Editor.Helpers
{
    /// <summary>
    /// Class for "Scripting Define Symbols" defining from CoreManager Inspector.
    /// <seealso cref="CorePlugin.Core.CoreManager"/>
    /// </summary>
    public class SymbolDefiner
    {
        private const string Symbol = "SYMBOL_DEFINER_ASSET";
        private static readonly Dictionary<string, bool> Symbols = new Dictionary<string, bool>
                                                                   {
                                                                       { Symbol, true }
                                                                   };

        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic |
                                           BindingFlags.Static | BindingFlags.Instance |
                                           BindingFlags.DeclaredOnly;

        [DidReloadScripts]
        private static async void CollectSymbols()
        {
            var consts = await GetFields();
            SetScriptingDefine(new KeyValuePair<string, bool>(Symbol, true));
            var list = AllDefines();
            var buffer = consts.ToDictionary(value => value, value => false);
            foreach (var item in buffer.Keys) Symbols[item] = !list.Contains(item);
        }

        private static Task<IEnumerable<string>> GetFields()
        {
            return Task<IEnumerable<string>>.Factory.StartNew(() => AppDomain.CurrentDomain.GetAssemblies()
                                                                             .SelectMany(x => x.GetTypes()
                                                                                               .SelectMany(t => t.GetFields(Flags)
                                                                                                   .Where(fi => fi.IsLiteral &&
                                                                                                        !fi.IsInitOnly &&
                                                                                                        fi.FieldType ==
                                                                                                        typeof(string)))
                                                                                               .Select(info => (string)info.GetRawConstantValue())));
        }

        /// <summary>
        /// Shows buttons in Inspector.
        /// </summary>
        public void ShowSymbolsButtons()
        {
            var bufferSymbols = new Dictionary<string, bool>(Symbols);

            foreach (var symbol in from symbol in bufferSymbols
                                   let text = symbol.Value ? $"Define {symbol.Key}" : $"Undefine {symbol.Key}"
                                   where GUILayout.Button(text)
                                   select symbol)
            {
                Symbols[symbol.Key] = !symbol.Value;
                SetScriptingDefine(symbol);
            }
        }

        public void DefineSymbol(string key)
        {
            if (!Symbols.TryGetValue(key, out var value)) return;
            Symbols[key] = !value;
            SetScriptingDefine(Symbols.FirstOrDefault(x => x.Key.Equals(key)));
        }

        private static void SetScriptingDefine(KeyValuePair<string, bool> pair)
        {
            var allDefines = AllDefines();
            allDefines.RemoveAll(string.IsNullOrWhiteSpace);

            if (pair.Value)
            {
                if (!allDefines.Contains(pair.Key)) allDefines.Add(pair.Key);
            }
            else
            {
                allDefines.RemoveAll(x => x == pair.Key);
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                                                             EditorUserBuildSettings.selectedBuildTargetGroup,
                                                             string.Join(";", allDefines.ToArray()));
            AssetDatabase.Refresh();
        }

        private static List<string> AllDefines()
        {
            var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var allDefines = definesString.Split(';').ToList();
            return allDefines;
        }
    }
}
