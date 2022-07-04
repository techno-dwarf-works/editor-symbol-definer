using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnitySymbolDefiner.Attributes;

namespace UnitySymbolDefiner
{
    public static class SymbolDefiner
    {
        [SymbolDefiner(true)] private const string Symbol = "SYMBOL_DEFINER_ASSET";
        private const string SymbolDefinerPath = nameof(SymbolDefiner); 

        private class DefineData
        {
            public DefineData(bool autoDefine, bool isDefined)
            {
                AutoDefine = autoDefine;
                IsDefined = isDefined;
            }

            public bool AutoDefine { get; }
            public bool IsDefined { get; set; }
        }

        private static Dictionary<string, DefineData> _symbols = new Dictionary<string, DefineData>
        {
            { Symbol, new DefineData(true, false) }
        };

        private static List<string> _allDefines = new List<string>();

        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic |
                                           BindingFlags.Static | BindingFlags.Instance |
                                           BindingFlags.DeclaredOnly;

        [DidReloadScripts]
        private static async void CollectSymbols()
        {
            var consts = await GetFields();
            SetScriptingDefine(new KeyValuePair<string, DefineData>(Symbol, new DefineData(true, true)));
            _allDefines = GetAllDefines();
            _symbols = consts.ToDictionary(value => value.Key,
                value => new DefineData(value.Value, _allDefines.Contains(value.Key)));

            foreach (var symbol in _symbols)
            {
                SetScriptingDefine(symbol);
            }

            ScheduleUpdate();
        }

        private static Task<Dictionary<string, bool>> GetFields()
        {
            return Task<Dictionary<string, bool>>.Factory.StartNew(() =>
                AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()
                        .SelectMany(t => t.GetFields(Flags).Where(IsFieldValid)))
                    .ToDictionary(info => (string)info.GetRawConstantValue(),
                        info => info.GetCustomAttribute<SymbolDefinerAttribute>().AutoDefine));
        }

        private static bool IsFieldValid(FieldInfo fieldInfo)
        {
            return fieldInfo.GetCustomAttribute<SymbolDefinerAttribute>() != null && fieldInfo.IsLiteral &&
                   !fieldInfo.IsInitOnly &&
                   fieldInfo.FieldType == typeof(string);
        }

        /// <summary>
        /// Shows buttons in Inspector.
        /// </summary>
        public static void ShowSymbolsButtons()
        {
            var bufferSymbols = new Dictionary<string, DefineData>(_symbols);
            bufferSymbols.Remove(Symbol);
            foreach (var symbol in bufferSymbols.Where(x => !x.Value.AutoDefine)
                         .Select(symbol => new
                             { symbol, text = symbol.Value.IsDefined ? $"Undefine {symbol.Key}" : $"Define {symbol.Key}" })
                         .Where(t => GUILayout.Button(t.text)).Select(t => t.symbol))
            {
                if (symbol.Value.IsDefined)
                {
                    UndefineSymbol(symbol.Key);
                }
                else
                {
                    DefineSymbol(symbol.Key);
                }
            }
        }

        private static void SetSymbolValue(string key, bool value)
        {
            _symbols[key].IsDefined = value;
            SetScriptingDefine(new KeyValuePair<string, DefineData>(key, _symbols[key]));
        }

        public static void DefineSymbol(string key)
        {
            if (!_symbols.ContainsKey(key)) return;
            SetSymbolValue(key, true);

            ScheduleUpdate();
        }

        public static void UndefineSymbol(string key)
        {
            if (!_symbols.ContainsKey(key)) return;
            SetSymbolValue(key, false);

            ScheduleUpdate();
        }

        private static void SetScriptingDefine(KeyValuePair<string, DefineData> pair)
        {
            _allDefines.RemoveAll(string.IsNullOrWhiteSpace);

            if (pair.Value.IsDefined)
            {
                if (!_allDefines.Contains(pair.Key)) _allDefines.Add(pair.Key);
            }
            else
            {
                _allDefines.RemoveAll(x => x == pair.Key);
            }
        }

        private static void ScheduleUpdate()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", _allDefines.ToArray()));
            AssetDatabase.Refresh();
        }

        private static List<string> GetAllDefines()
        {
            var definesString =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var allDefines = definesString.Split(';').ToList();
            return allDefines;
        }
    }
}