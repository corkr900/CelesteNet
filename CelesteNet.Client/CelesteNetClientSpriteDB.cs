﻿using Monocle;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.CelesteNet.Client
{
    public static class CelesteNetClientSpriteDB {

        private static ConditionalWeakTable<Sprite, SpriteExt> SpriteExts = new();

        public static Dictionary<string, SpriteMeta> SpriteMetas = new() {
            { "glider", new SpriteMeta {
                ForceOutline = true
            } },

            { "pufferFish", new SpriteMeta {
                ForceOutline = true
            } }

        };

        public static void Load() {
            On.Monocle.Sprite.CloneInto += OnSpriteCloneInto;
            On.Monocle.SpriteData.Add += OnSpriteDataAdd;
        }

        public static void Unload() {
            On.Monocle.Sprite.CloneInto -= OnSpriteCloneInto;
            On.Monocle.SpriteData.Add -= OnSpriteDataAdd;
        }

        private static Sprite OnSpriteCloneInto(On.Monocle.Sprite.orig_CloneInto orig, Sprite self, Sprite clone) {
            clone.SetID(self.GetID());
            return orig(self, clone);
        }

        private static void OnSpriteDataAdd(On.Monocle.SpriteData.orig_Add orig, SpriteData self, System.Xml.XmlElement xml, string overridePath) {
            self.Sprite.SetID(xml.Name);
            orig(self, xml, overridePath);
        }

        public static void SetID(this Sprite self, string? value)
            => SpriteExts.GetOrCreateValue(self).ID = value;

        public static string? GetID(this Sprite self)
            => SpriteExts.TryGetValue(self, out SpriteExt? ext) ? ext.ID : null;

        public static SpriteMeta? GetMeta(this Sprite self) {
            string? id = self.GetID();
            if (id == null)
                return null;
            if (SpriteMetas.TryGetValue(id, out SpriteMeta? meta))
                return meta;
            return SpriteMetas[id] = new();
        }

        private class SpriteExt {
            public string? ID;
        }

        public class SpriteMeta {
            public bool ForceOutline;
        }

    }
}
