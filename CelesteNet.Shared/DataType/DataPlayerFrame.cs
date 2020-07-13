﻿using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteNet.DataTypes {
    public class DataPlayerFrame : DataType<DataPlayerFrame> {

        static DataPlayerFrame() {
            DataID = "playerFrame";
        }

        public override DataFlags DataFlags => DataFlags.Update;

        public uint UpdateID;

        public DataPlayerInfo? Player;

        public Vector2 Position;
        public Vector2 Speed;
        public Vector2 Scale;
        public Color Color;
        public Facings Facing;
        public int Depth;

        public PlayerSpriteMode SpriteMode;
        public float SpriteRate;
        public Vector2? SpriteJustify;

        public string CurrentAnimationID = "";
        public int CurrentAnimationFrame;

        public Color HairColor;
        public bool HairSimulateMotion;

        public byte HairCount;
        public Color[] HairColors = Dummy<Color>.EmptyArray;
        public string[] HairTextures = Dummy<string>.EmptyArray;

        public Entity[] Followers = Dummy<Entity>.EmptyArray;

        public Entity? Holding;

        // TODO: Get rid of this, sync particles separately!
        public bool? DashWasB;
        public Vector2 DashDir;

        public bool Dead;

        public override bool FilterHandle(DataContext ctx)
            => Player != null; // Can be RECEIVED BY CLIENT TOO EARLY because UDP is UDP.

        public override MetaType[] GenerateMeta(DataContext ctx)
            => new MetaType[] {
                new MetaPlayerUpdate(Player),
                new MetaOrderedUpdate(Player?.ID ?? uint.MaxValue, UpdateID)
            };

        public override void FixupMeta(DataContext ctx) {
            MetaPlayerUpdate playerUpd = Get<MetaPlayerUpdate>(ctx);
            MetaOrderedUpdate order = Get<MetaOrderedUpdate>(ctx);

            order.ID = playerUpd;
            UpdateID = order.UpdateID;
            Player = playerUpd;
        }

        public override void Read(DataContext ctx, BinaryReader reader) {
            Position = reader.ReadVector2();
            Speed = reader.ReadVector2();
            Scale = reader.ReadVector2Scale();
            Color = reader.ReadColor();
            Facing = reader.ReadBoolean() ? Facings.Left : Facings.Right;
            Depth = reader.ReadInt32();

            SpriteMode = (PlayerSpriteMode) reader.ReadByte();
            SpriteRate = reader.ReadSingle();
            SpriteJustify = reader.ReadBoolean() ? (Vector2?) reader.ReadVector2() : null;

            CurrentAnimationID = reader.ReadNetString();
            CurrentAnimationFrame = reader.ReadInt32();

            HairColor = reader.ReadColor();
            HairSimulateMotion = reader.ReadBoolean();

            HairCount = reader.ReadByte();
            HairColors = new Color[HairCount];
            for (int i = 0; i < HairColors.Length; i++)
                HairColors[i] = reader.ReadColor();
            HairTextures = new string[HairCount];
            for (int i = 0; i < HairColors.Length; i++) {
                HairTextures[i] = reader.ReadNetString();
                if (HairTextures[i] == "-")
                    HairTextures[i] = HairTextures[i - 1];
            }

            Followers = new Entity[reader.ReadByte()];
            for (int i = 0; i < Followers.Length; i++) {
                Entity f = new Entity();
                f.Scale = reader.ReadVector2Scale();
                f.Color = reader.ReadColor();
                f.Depth = reader.ReadInt32();
                f.SpriteRate = reader.ReadSingle();
                f.SpriteJustify = reader.ReadBoolean() ? (Vector2?) reader.ReadVector2() : null;
                f.SpriteID = reader.ReadNetString();
                if (f.SpriteID == "-") {
                    Entity p = Followers[i - 1];
                    f.SpriteID = p.SpriteID;
                    f.CurrentAnimationID = p.CurrentAnimationID;
                } else {
                    f.CurrentAnimationID = reader.ReadNetString();
                }
                f.CurrentAnimationFrame = reader.ReadInt32();
                Followers[i] = f;
            }

            if (reader.ReadBoolean())
                Holding = new Entity {
                    Position = reader.ReadVector2Scale(),
                    Scale = reader.ReadVector2(),
                    Color = reader.ReadColor(),
                    Depth = reader.ReadInt32(),
                    SpriteRate = reader.ReadSingle(),
                    SpriteJustify = reader.ReadBoolean() ? (Vector2?) reader.ReadVector2() : null,
                    SpriteID = reader.ReadNetString(),
                    CurrentAnimationID = reader.ReadNetString(),
                    CurrentAnimationFrame = reader.ReadInt32()
                };

            if (reader.ReadBoolean()) {
                DashWasB = reader.ReadBoolean();
                DashDir = reader.ReadVector2();

            } else {
                DashWasB = null;
                DashDir = default;
            }

            Dead = reader.ReadBoolean();
        }

        public override void Write(DataContext ctx, BinaryWriter writer) {
            writer.Write(Position);
            writer.Write(Speed);
            writer.Write(Scale);
            writer.Write(Color);
            writer.Write(Facing == Facings.Left);
            writer.Write(Depth);

            writer.Write((byte) SpriteMode);
            writer.Write(SpriteRate);
            if (SpriteJustify == null) {
                writer.Write(false);
            } else {
                writer.Write(true);
                writer.Write(SpriteJustify.Value);
            }
            writer.WriteNetString(CurrentAnimationID);
            writer.Write(CurrentAnimationFrame);

            writer.Write(HairColor);
            writer.Write(HairSimulateMotion);

            writer.Write(HairCount);
            if (HairCount != 0) {
                for (int i = 0; i < HairCount; i++)
                    writer.Write(HairColors[i]);
            }
            if (HairCount != 0) {
                for (int i = 0; i < HairCount; i++) {
                    if (i >= 1 && HairTextures[i] == HairTextures[i - 1])
                        writer.WriteNetString("-");
                    else
                        writer.WriteNetString(HairTextures[i]);
                }
            }

            writer.Write((byte) Followers.Length);
            if (Followers.Length != 0) {
                for (int i = 0; i < Followers.Length; i++) {
                    Entity f = Followers[i];
                    writer.Write(f.Scale);
                    writer.Write(f.Color);
                    writer.Write(f.Depth);
                    writer.Write(f.SpriteRate);
                    if (f.SpriteJustify == null) {
                        writer.Write(false);
                    } else {
                        writer.Write(true);
                        writer.Write(f.SpriteJustify.Value);
                    }
                    if (i >= 1 &&
                        f.SpriteID == Followers[i - 1].SpriteID &&
                        f.CurrentAnimationID == Followers[i - 1].CurrentAnimationID) {
                        writer.WriteNetString("-");
                    } else {
                        writer.WriteNetString(f.SpriteID);
                        writer.WriteNetString(f.CurrentAnimationID);
                    }
                    writer.Write(f.CurrentAnimationFrame);
                }
            }

            if (Holding == null) {
                writer.Write(false);

            } else {
                writer.Write(true);
                Entity h = Holding;
                writer.Write(h.Position);
                writer.Write(h.Scale);
                writer.Write(h.Color);
                writer.Write(h.Depth);
                writer.Write(h.SpriteRate);
                if (h.SpriteJustify == null) {
                    writer.Write(false);

                } else {
                    writer.Write(true);
                    writer.Write(h.SpriteJustify.Value);
                }
                writer.WriteNetString(h.SpriteID);
                writer.WriteNetString(h.CurrentAnimationID);
                writer.Write(h.CurrentAnimationFrame);
            }

            if (DashWasB == null) {
                writer.Write(false);

            } else {
                writer.Write(true);
                writer.Write(DashWasB.Value);
                writer.Write(DashDir);
            }

            writer.Write(Dead);
        }

        public class Entity {
            public Vector2 Position;
            public Vector2 Scale;
            public Color Color;
            public int Depth;
            public float SpriteRate;
            public Vector2? SpriteJustify;
            public string SpriteID = "";
            public string CurrentAnimationID = "";
            public int CurrentAnimationFrame;
        }

    }
}
