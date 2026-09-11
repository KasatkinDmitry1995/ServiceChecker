using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceChecker
{
    public enum CheckStatusCodes
    {
        OK,
        ERROR,
        TIMEOUT,
        UNEXPECTED_STATUS,
        CANCELED
    }
}
