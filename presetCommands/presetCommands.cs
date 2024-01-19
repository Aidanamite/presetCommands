using UnityEngine;

public class presetCommands : Mod
{
    static HarmonyLib.Traverse consoleSender = HarmonyLib.Traverse.Create(RConsole.instance);
    static bool saving = false;
    static string globalFile = System.IO.Path.Combine(SaveAndLoad.WorldPath, "presetCommands.json");
    static string fileName
    {
        get
        {
            string time = "";
            if (saving)
                time = System.DateTime.Now.ToString(SaveAndLoad.dateTimeFormattingSaveFile);
            else
                time = new System.DateTime(SaveAndLoad.WorldToLoad.lastPlayedDateTicks).ToString(SaveAndLoad.dateTimeFormattingSaveFile);
            string path = System.IO.Path.Combine(SaveAndLoad.WorldPath, SaveAndLoad.WorldToLoad.name, time);
            if (System.IO.Directory.Exists(path))
                return System.IO.Path.Combine(path, "presetCommands.json");
            return System.IO.Path.Combine(SaveAndLoad.WorldPath, SaveAndLoad.WorldToLoad.name, time + SaveAndLoad.latestStringNameEnding, "presetCommands.json");
        }
    }
    static JSONObject worldData = JSONObject.Create();
    static JSONObject globalData = getGlobalSaveJson();

    public void Start()
    {
        Debug.Log("Mod presetCommands has been loaded!");
    }

    public void OnModUnload()
    {
        Debug.Log("Mod presetCommands has been unloaded!");
    }

    [ConsoleCommand(name: "addcommand", docs: "Adds a new command to world que")]
    public static string MyCommand(string[] args)
    {
        if (args.Length == 0)
            return "Must be followed by a command";
        string str = args[0];
        for (int i = 1; i < args.Length; i++)
            str += " " + args[i];
        addCommandToData(ref worldData, str);
        return "Command added";
    }

    [ConsoleCommand(name: "removecommand", docs: "Removes a command from the world que")]
    public static string MyCommand2(string[] args)
    {
        if (args.Length == 0)
            return "Not enough arguments";
        if (args.Length > 1)
            return "Too many arguments";
        int index = 0;
        try
        {
            index = int.Parse(args[0]);
        }
        catch
        {
            return "Failed to parse \"" + args[0] + "\" as a number";
        }
        try
        {
            removeCommandFromData(ref worldData, index - 1);
            return "Removed command from world que";
        }
        catch
        {
            return "There is not that many commands in the que";
        }
    }

    [ConsoleCommand(name: "listcommands", docs: "Lists commands in the world que")]
    public static string MyCommand3(string[] args)
    {
        if (worldData.IsNull || worldData.Count == 0)
            return "No commands in que";
        for (int i = 1; i <= worldData.keys.Count; i++)
            Debug.Log("Slot " + i.ToString() + ": " + worldData.keys[i - 1]);
        return "End----";
    }

    override public void WorldEvent_WorldSaved()
    {
        saving = true;
        saveJson(worldData);
        saving = false;
    }

    public override void WorldEvent_WorldLoaded()
    {
        if (globalData.IsNull || globalData.Count == 0)
        {
            Debug.Log("[presetCommands]: No commands found in global settings");
        }
        else
        {
            Debug.Log("[presetCommands]: Running preset global commands");
            foreach (string cmd in globalData.keys)
                runCommand(cmd);
        }
        worldData = getSaveJson();
        if (worldData.IsNull || worldData.Count == 0)
        {
            Debug.Log("[presetCommands]: No commands found for this save");
        }
        else
        {
            Debug.Log("[presetCommands]: Running preset commands");
            foreach (string cmd in worldData.keys)
                runCommand(cmd);
        }
    }

    [ConsoleCommand(name: "addglobalcommand", docs: "Adds a new command to global que")]
    public static string MyCommand4(string[] args)
    {
        if (args.Length == 0)
            return "Must be followed by a command";
        string str = args[0];
        for (int i = 1; i < args.Length; i++)
            str += " " + args[i];
        addCommandToData(ref globalData, str);
        saveGlobalJson(globalData);
        return "Command added";
    }

    [ConsoleCommand(name: "removeglobalcommand", docs: "Removes a command from the global que")]
    public static string MyCommand5(string[] args)
    {
        if (args.Length == 0)
            return "Not enough arguments";
        if (args.Length > 1)
            return "Too many arguments";
        int index = 0;
        try
        {
            index = int.Parse(args[0]);
        }
        catch
        {
            return "Failed to parse \"" + args[0] + "\" as a number";
        }
        try
        {
            removeCommandFromData(ref globalData, index - 1);
            saveGlobalJson(globalData);
            return "Removed command from global que";
        }
        catch
        {
            return "There is not that many commands in the que";
        }
    }

    [ConsoleCommand(name: "listglobalcommands", docs: "Lists commands in the global que")]
    public static string MyCommand6(string[] args)
    {
        if (globalData.IsNull || globalData.Count == 0)
            return "No commands in que";
        for (int i = 1; i <= globalData.Count; i++)
            Debug.Log("Slot " + i.ToString() + ": " + globalData.keys[i - 1]);
        return "End----";
    }

    static public void addCommandToData(ref JSONObject data,string cmd)
    {
        data.AddField(cmd, 0);
    }
    static public void removeCommandFromData(ref JSONObject data, int ind)
    {
        data.RemoveField(data.keys[ind]);
    }

    private static JSONObject getSaveJson()
    {
        JSONObject data;
        try
        {
            data = new JSONObject(System.IO.File.ReadAllText(fileName));
        }
        catch
        {
            data = JSONObject.Create();
            saveJson(data);
        }
        return data;
    }

    private static void saveJson(JSONObject data)
    {
        try
        {
            System.IO.File.WriteAllText(fileName, data.ToString());
        }
        catch (System.Exception err)
        {
            Debug.Log("An error occured while trying to save presetCommand settings: " + err.Message);
        }
    }

    private static JSONObject getGlobalSaveJson()
    {
        JSONObject data;
        try
        {
            data = new JSONObject(System.IO.File.ReadAllText(globalFile));
        }
        catch
        {
            data = JSONObject.Create();
            saveGlobalJson(data);
        }
        return data;
    }

    private static void saveGlobalJson(JSONObject data)
    {
        try
        {
            System.IO.File.WriteAllText(globalFile, data.ToString());
        }
        catch (System.Exception err)
        {
            Debug.Log("An error occured while trying to save global presetCommand settings: " + err.Message);
        }
    }

    public static void runCommand(string cmd)
    {
        if (cmd != "")
            consoleSender.Method("SilentlyRunCommand", new System.Type[] { typeof(string) }, new object[] { cmd }).GetValue();
    }
}