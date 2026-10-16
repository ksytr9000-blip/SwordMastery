using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace SwordMastery;

// The assets/ui images contain only pre-rendered glyph pixels and their bounds;
// NO external font binaries are bundled. Create SpriteFont dynamically so
// both SpriteBatch and GMCM can draw the full JA/ZH text independently of game locale.
internal sealed class LocalizedFontRenderer
{
    private readonly IModHelper helper;
    private readonly IMonitor monitor;
    private SpriteFont? small;
    private SpriteFont? heading;
    private bool initialized;

    public LocalizedFontRenderer(IModHelper helper, IMonitor monitor)
    { this.helper=helper; this.monitor=monitor; }

    public SpriteFont? Small => Get(false);
    public SpriteFont? Heading => Get(true);

    private SpriteFont? Get(bool large)
    {
        if (!initialized)
        {
            initialized=true;
            try
            {
                small=Create("ui_small");
                heading=Create("ui_heading");
                monitor.Log("CJK glyph atlas loaded; independent-language UI rendering is ready.", LogLevel.Info);
            }
            catch(Exception ex)
            {
                monitor.Log($"CJK glyph atlas could not load: {ex}", LogLevel.Error);
            }
        }
        return large ? heading : small;
    }

    private SpriteFont Create(string name)
    {
        var tex=helper.ModContent.Load<Texture2D>($"assets/ui/{name}.png");
        string json=System.IO.File.ReadAllText(System.IO.Path.Combine(helper.DirectoryPath,"assets","ui",name+".json"));
        using var document=JsonDocument.Parse(json);
        var root=document.RootElement;
        var entries=root.GetProperty("glyphs").EnumerateObject().OrderBy(p=>p.Name[0]).ToArray();
        var bounds=new List<Rectangle>(); var crops=new List<Rectangle>();
        var chars=new List<char>(); var kernings=new List<Vector3>();
        foreach (var entry in entries)
        {
            int[] rect=entry.Value.EnumerateArray().Select(v=>v.GetInt32()).ToArray();
            char c=entry.Name[0];
            chars.Add(c);
            bounds.Add(new Rectangle(rect[0],rect[1],rect[2],rect[3]));
            crops.Add(new Rectangle(0,0,rect[2],rect[3]));
            kernings.Add(new Vector3(0,rect[2],0));
        }
        return new SpriteFont(tex,bounds,crops,chars,root.GetProperty("lineSpacing").GetInt32(),0,kernings,'?');
    }

}
