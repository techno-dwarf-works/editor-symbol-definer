# Symbol Definer

This package allowing you quick enable or disable Scripting Define Symbols in Player Settings.
Mark any constant field with `SymbolDefiner` attribute and this constant field will be used as Scripting Define Symbol.

Usages:
```c#
///This constant field will be automaticaly added to 
///Scripting Define Symbols on scripts reload
[SymbolDefiner(autoDefine: true)] 
private const string Symbol = "SYMBOL_DEFINER_ASSET";

///This constant field will be marked for Definer to 
///display buttons to define or undefine symbols
[SymbolDefiner(autoDefine: false)] 
private const string Symbol1 = "SOME_SYMBOL_1";

///OR
[SymbolDefiner(] private const string Symbol2 = "SOME_SYMBOL_2";
```
To use `Definer` just add it to any object on scene.

##TODOs:
1. Add possibility to undefine any Scripting Define Symbols;
2. Add tracking in case constant field value changed;
3. Add menu or window instead `Definer`
