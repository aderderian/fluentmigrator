#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion


using System;

using FluentMigrator.Infrastructure;

namespace FluentMigrator
{
    /// <summary>
    /// The base migration class for custom SQL queries and data updates/deletions
    /// </summary>
    public abstract class DynamicMigration : Migration, IDynamicMigration
    {
        private readonly object _mutex = new object();

        /// <summary>
        /// 
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }

        public Action UpMethod { get; set; } 

        public Action DownMethod { get; set; }

        public string GetName()
        {
            return string.Format("{0}: {1}", Version, GetType().Name);
        }

        public virtual void GetDynamicUpExpressions(IMigrationContext context, Action upMethod)
        {
            lock (_mutex)
            {
#pragma warning disable 618
                _context = context;
#pragma warning restore 618
#pragma warning disable 612
                ApplicationContext = context.ApplicationContext;
#pragma warning restore 612
                ConnectionString = context.Connection;
                upMethod();
#pragma warning disable 618
                _context = null;
#pragma warning restore 618
            }
        }

        public virtual void GetDynamicDownExpressions(IMigrationContext context, Action downMethod)
        {
            lock (_mutex)
            {
#pragma warning disable 618
                _context = context;
#pragma warning restore 618
#pragma warning disable 612
                ApplicationContext = context.ApplicationContext;
#pragma warning restore 612
                ConnectionString = context.Connection;
                downMethod();
#pragma warning disable 618
                _context = null;
#pragma warning restore 618
            }
        }
    }
}
