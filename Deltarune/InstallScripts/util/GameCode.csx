using UndertaleModLib.Util; // TextureWorker
using System.Text; // StringBuilder
using System.Xml; // XmlDocument
using System.Threading.Tasks;
using System.Text.RegularExpressions; // Regex
using System.Linq;
using System.IO;
using ImageMagick;

void ImportAllCode(string codeFolder) {
    UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data);
    foreach (string file in Directory.GetFiles(codeFolder))
    {
        if (!file.EndsWith(".gml")) continue;

        string code = File.ReadAllText(file);
        string codeName = Path.GetFileNameWithoutExtension(file);
        importGroup.QueueReplace(codeName, code);
    }
    importGroup.Import();
}

// Replaces a variable setting in the code with a new value.
// If the variable is not found, it will be added at the end of the code.
string UpdateVariableSetting(string code, string varName, string newValue)
{
    var pattern = $@"({varName}\s*=\s*"")[^""]*("")";
	if (!Regex.IsMatch(code, pattern)) {
		// If the variable is not found, we can just add it at the end of the code.
		return code + Environment.NewLine + $"{varName} = \"{newValue}\";" + Environment.NewLine;
	}
    var replacement = $"$1{newValue}$2";
    return Regex.Replace(code, pattern, replacement);
}

GlobalDecompileContext g_GlobalDecompileContext = new(Data);

void UpdateCodeVariable(UndertaleModLib.Compiler.CodeImportGroup importGroup, string codeName, string varName, string newValue)
{
	var decompiledCode = GetDecompiledText(codeName, g_GlobalDecompileContext);
	string updatedCode = UpdateVariableSetting(decompiledCode, varName, newValue);
	importGroup.QueueReplace(codeName, updatedCode);
}

// Finds the first setting of the given variable in the code.
string? FindCodeVariableValue(string codeName, string varName)
{
    var decompiledCode = GetDecompiledText(codeName, g_GlobalDecompileContext);
    var pattern = $@"({varName})\s*=\s*""([^""]*)""";
    Match m = Regex.Match(decompiledCode, pattern);
	if (!m.Success) {
        return null;
	}
    return m.Groups[2]?.Value;
}

void UpdateKeyboardPuzzleCode()
{
	UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data);
	// obj_ch2_keyboardpuzzle_tile
	UpdateCodeVariable(importGroup, "gml_Object_obj_ch2_keyboardpuzzle_tile_PreCreate_0", "myString", "M");
	
	// room_dw_cyber_keyboard_puzzle_1
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_1_1_PreCreate", "myString", "E");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_1_2_PreCreate", "myString", "L");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_1_3_PreCreate", "myString", "A");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_1_4_PreCreate", "myString", "A");

	// room_dw_cyber_keyboard_puzzle_2
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_1_PreCreate", "myString", "A");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_2_PreCreate", "myString", "N");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_3_PreCreate", "myString", "A");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_4_PreCreate", "myString", "N");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_5_PreCreate", "myString", "C");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_6_PreCreate", "myString", "S");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_7_PreCreate", "myString", "O");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_8_PreCreate", "myString", "E");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_9_PreCreate", "myString", "M");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_10_PreCreate", "myString", "T");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_11_PreCreate", "myString", "C");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_12_PreCreate", "myString", "S");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_13_PreCreate", "myString", "O");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_14_PreCreate", "myString", "T");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_15_PreCreate", "myString", "D");
	UpdateCodeVariable(importGroup, "gml_RoomCC_room_dw_cyber_keyboard_puzzle_2_16_PreCreate", "myString", "I");
	
	importGroup.Import();
}

void UpdateWaterCoolerCode()
{
	string codeFile = "gml_Object_obj_watercooler_enemy_Step_0";
	UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data);

	// Watercooler "b[aeiou]b[il]e"
	// Format string is B~1b~2e
	// Replace with Bl~1b~2 for "bl[aeiou]b(bi)?
	// Only need to replace choose statements in code, rest can be done in strings
	importGroup.QueueFindReplace(codeFile, """choose("i", "l")""", """choose("", "bi")""");
	importGroup.Import();
}