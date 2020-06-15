using System;
using System.Collections.Generic;
using System.Text;

namespace BookStoreAggregateLoader
{
    public interface IResult
    {
    }

    public class Success : IResult
    {

    }

    public class Error : IResult
    {

    }
}
