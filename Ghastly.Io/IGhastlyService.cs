﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghastly.Io
{
    public interface IGhastlyService
    {
        Task<IEnumerable<SceneDescription>> GetScenes();
    }
}