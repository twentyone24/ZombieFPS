using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class Config : MonoBehaviour
{
    //version code
    public static string version_code = "v2.0.2";

    //crystal health
    public static int play_health = 10;

    //map account
    public static int map_account;

    //map index
    public static int map_index = 0;

    //生成敌军的数据 Generate enemy data
    public static JsonData json_data_enemy_create;

    void Awake()
    {

        //敌军生成数据
        //先从本地注册表中获取
        //如果本地没有就去Resouce里面找
        //Enemy generated data
        //Get it from the local registry first
        //If you don’t have it locally, look for it in Resouce
        if (PlayerPrefs.GetString("Config.json_data_enemy_create") == null || PlayerPrefs.GetString("Config.json_data_enemy_create") == "")
        {
            Config.json_data_enemy_create = JsonMapper.ToObject(this.read_json_file("enemy_create_data"));
        }
        else
        {
            Config.json_data_enemy_create = JsonMapper.ToObject(PlayerPrefs.GetString("Config.json_data_enemy_create"));
        }

    }

    //读取json文件的函数 Function to read json file
    public string read_json_file(string name)
    {
        string json = "";
        TextAsset text = Resources.Load<TextAsset>(name);
        json = text.text;
        if (string.IsNullOrEmpty(json)) return null;
        return json;
    }
}



//game statu
public enum Game_statu
{
    game_start, 
    gaming,    
    game_over   
}

//Enemy statu
public enum Enemy_statu
{
    running,    
    attack,     
    dead        
}

//Enemy type
public enum Enemy_type
{
    hammer,
    machete,
    ax
}

//先定义好一个委托 Define a delegate
public delegate void Handler();


