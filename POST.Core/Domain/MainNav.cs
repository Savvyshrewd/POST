﻿using POST.Core;
using POST.Core.Domain;
using System.Collections.Generic;

namespace POST.CORE.Domain
{
    public class MainNav : BaseDomain, IAuditable
    {
        public MainNav()
        {
            SubNavs = new HashSet<SubNav>();
        }
        public int MainNavId { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string Param { get; set; }
        public string Position { get; set; }

        public virtual ICollection<SubNav> SubNavs { get; set; }
    }
}
