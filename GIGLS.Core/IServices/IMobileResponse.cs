﻿using System.Collections.Generic;

namespace POST.Core.IServices
{
    public interface IMobileResponse<TResponse>
    {
        
        TResponse Object { get; set; }
        int Total { get; set; }
        Dictionary<string, IEnumerable<string>> ValidationErrors { get; set; }

    }
}