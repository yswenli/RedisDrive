using System;
using System.Collections.Generic;
using System.Text;

namespace Wenli.Drive.Redis
{
    public enum RedisConfigType
    {
        Single = 0,
        Sentinel = 1,
        Cluster = 2
    }
}
