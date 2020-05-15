﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace DataAccess.Data.Repository.IRepository
{
    public interface ISP_Call
    {
        T Single<T>(string procedureName, DynamicParameters param = null);

        void Execute(string procedureName, DynamicParameters param = null);

        T OneRecord<T>(string procedureName, DynamicParameters param = null);

        IEnumerable<T> List<T>(string procedureName, DynamicParameters param = null);

        Tuple<IEnumerable<T1>, IEnumerable<T2>> List<T1, T2>(string procedureName, DynamicParameters param = null);
    }
}
