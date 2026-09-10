using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceChecker
{
    enum ExitCodes
    {
        Success = 0,
        GeneralError = 1,
        UsageError = 2,
        ValidationError = 3,
        FileNotFound = 4,
        Interrupted = 130
    }
}
