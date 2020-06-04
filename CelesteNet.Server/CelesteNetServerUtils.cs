﻿using Celeste.Mod.CelesteNet.DataTypes;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteNet.Server {
    public static class CelesteNetServerUtils {

        public static object ToFrontendChat(this DataChat msg)
            => new {
                ID = msg.ID,
                PlayerID = msg.Player.ID,
                Color = msg.Color.ToHex(),
                Text = msg.ToString()
            };

    }
}
