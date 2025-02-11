﻿#region Licence
/* The MIT License (MIT)
Copyright © 2014 Francesco Pighi <francesco.pighi@gmail.com>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the “Software”), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. */

#endregion

namespace Paramore.Brighter.Outbox.Sqlite
{
    public class SqliteOutboxBuilder
    {
        const string OutboxDdl = @"CREATE TABLE {0} (
                                          [MessageId] uniqueidentifier NOT NULL
                                        , [Topic] nvarchar(255) NULL
                                        , [MessageType] nvarchar(32) NULL
                                        , [Timestamp] datetime NULL
                                        , [CorrelationId] UNIQUEIDENTIFIER NULL
                                        , [ReplyTo] NVARCHAR(255) NULL
                                        , [ContentType] NVARCHAR(128) NULL  
                                        , [Dispatched] datetime NULL
                                        , [HeaderBag] ntext NULL
                                        , [Body] ntext NULL
                                        , CONSTRAINT[PK_MessageId] PRIMARY KEY([MessageId])
                                        );";

        public static string GetDDL(string tableName)
        {
            return string.Format(OutboxDdl, tableName);
        }
        
        public static string GetExists(string tableName)
        {
            return string.Format($"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';");
        }
    }
}
