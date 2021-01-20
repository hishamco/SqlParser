﻿using System.Collections.Generic;

namespace SqlParser
{
    public abstract class Statement
    {
        public Statement(string tableName)
        {
            TableName = tableName;
            Tokens = new List<Token>();
        }

        public string TableName { get; }

        public IList<Token> Tokens { get; }
    }
}
