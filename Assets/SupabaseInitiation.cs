using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Supabase;
using Postgrest.Models;
using static Supabase.Realtime.PostgresChanges.PostgresChangesOptions;
using Unity.VisualScripting;


[Postgrest.Attributes.Table("text-copy-from-unity")]
class MyMessage : BaseModel
{

    [Postgrest.Attributes.Column("message")]
    public string Message { get; set; }
}


[Postgrest.Attributes.Table("rename-folder-in-app")]
class RenameMessage : BaseModel
{

    [Postgrest.Attributes.Column("folder_name")]
    public string Rename { get; set; }
}

public class SupabaseInitiation : MonoBehaviour
{

    public Supabase.Client supabase;
    public string renameName;
    void Start()
    {
        var options = new Supabase.SupabaseOptions
        {
            AutoConnectRealtime = true
        };
        supabase = new Supabase.Client("url", "key", options);
        initializeSupabase();
        Debug.Log("Supabase Initialized");
        fetchData();
        // insertData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    async void initializeSupabase()
    {
        await supabase.InitializeAsync();
        Debug.Log("Initializing Supabase");
    }

    async void insertData()
    {
        var model = new MyMessage
        {
            Message = "The Shire"
        };
        await supabase.From<MyMessage>().Insert(model);
    }

    async void fetchData()
    {

        //await supabase.From<RenameMessage>().On(ListenType.Inserts, (sender, change) =>
        //{
        //  Debug.Log(change.Payload.Data);
        //});

        var channel = supabase.Realtime.Channel("realtime", "public", "rename-folder-in-app", "folder_name");

        channel.AddPostgresChangeHandler(ListenType.Inserts, async (sender, change) =>
        {
            // whenever a new row is inserted in the table, this code will be executed
            var result = await supabase.From<RenameMessage>().Get();
            renameName = result.Models[result.Models.Count - 1].Rename;
            Debug.Log("Renamed Folder: " + renameName);
        });

        await channel.Subscribe();
    }
}
