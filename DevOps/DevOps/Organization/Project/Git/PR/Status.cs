using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOps.Organization.Project.Git.PR
{
    internal enum Status
    {
        NotSet = 0,
        Active = 1,
        Abandoned = 2,
        Completed = 3,
        All = 4
    }
}
