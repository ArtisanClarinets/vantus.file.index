using Vantus.Core.Services;
using Xunit;
using System.Text.Json;
using Directory = System.IO.Directory;
using Path = System.IO.Path;

namespace Vantus.Tests;

public class SettingsSchemaTests
{
    [Fact]
    public void CanLoadSchema()
    {
        var json = """
        {
          "categories": [ { "id": "cat1", "title": "Cat 1", "page_ids": ["p1"] } ],
          "pages": [ { "id": "p1", "title": "Page 1", "intro": "Intro" } ],
          "settings": [
            { "setting_id": "s1", "page": "p1", "label": "S1", "control_type": "toggle", "defaults": { "personal": true } }
          ]
        }
        """;

        var schema = new SettingsSchema(json);
        
        Assert.Single(schema.GetCategories());
        Assert.NotNull(schema.GetPage("p1"));
        Assert.Single(schema.GetSettingsForPage("p1"));
        
        var def = schema.GetDefault("s1", "personal");
        Assert.NotNull(def);
        // JsonElement handling
        if (def is JsonElement je) Assert.True(je.GetBoolean());
        else Assert.True((bool)def);
    }
}

public class SettingsStoreTests
{
    [Fact]
    public async Task CanSaveAndLoadValues()
    {
        var schemaJson = """
        {
          "settings": [
            { "setting_id": "s1", "page": "p1", "label": "S1", "control_type": "toggle", "defaults": { "personal": true } }
          ]
        }
        """;
        var schema = new SettingsSchema(schemaJson);
        var tempPath = Path.Combine(Path.GetTempPath(), "VantusTests_" + Guid.NewGuid());
        Directory.CreateDirectory(tempPath);
        
        try
        {
            var policyEngine = new PolicyEngine(tempPath);
            var store = new SettingsStore(tempPath, schema, policyEngine);
            store.SetUserValue("s1", false);
            await store.SaveAsync(); 
            
            var store2 = new SettingsStore(tempPath, schema, policyEngine);
            await store2.InitializeAsync();
            
            var val = store2.GetUserValue("s1");
            Assert.NotNull(val);
            if (val is JsonElement je) Assert.False(je.GetBoolean());
            else Assert.False((bool)val);
        }
        finally
        {
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        }
    }
}
