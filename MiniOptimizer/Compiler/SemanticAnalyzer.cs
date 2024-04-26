using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using MiniOptimizer.Exceptions;
using MiniOptimizer.Metadata;
using MiniOptimizer.Utils;

namespace MiniOptimizer.Compiler
{
    public class SemanticAnalyzer
    {
        private Catalog _catalog;

        public bool Active { get; private set; }

        public SemanticAnalyzer(Catalog catalog) {
            _catalog = catalog;
            Active = true;
        }

        public void TurnOff()
        {
            Active = false;
        }

        public void TurnOn()
        {
            Active = true;
        }

        public void CheckPredicateOperands(MiniQLParser.AttributeContext a1, MiniQLParser.AttributeContext a2) 
        {
            if (a1.constant() != null && a2.constant() != null) throw new BaseException("Error 900. Predicate cannot contain two constants.");

            if (a1.constant() != null) throw new BaseException("Error 901. Left operand in the predicate must be indicator.");

            CheckAttribute(a1.GetText());

            var qualifiedName = ParseHelper.ParseQualifiedName(a1.GetText());

            ColumnType type = _catalog.GetColumnType(qualifiedName.Item1, qualifiedName.Item2);

            if (a2.constant() != null)
            {
                if (a2.constant().INTEGER_VALUE() != null && type != ColumnType.INT)
                    throw new TypeMissmatchException(qualifiedName.Item2);
                else if (a2.constant().QUOTED_STRING() != null && type != ColumnType.STRING)
                    throw new TypeMissmatchException(qualifiedName.Item2);
            }
              
        }

        public void CheckAttribute(string attribute)
        {
            Tuple<string, string> qualifiedName = ParseHelper.ParseQualifiedName(attribute);

            if (Active && qualifiedName == null)
                throw new BaseException("Column identifiers must be specified as <table>.<column>");
            else if (!_catalog.CheckIfColumnExists(qualifiedName.Item1, qualifiedName.Item2) && Active)
                throw new ColumnNotFoundException(qualifiedName.Item2);
        }

        public void CheckIfTableExists(string tableName)
        {
            if (!_catalog.CheckIfTableExists(tableName) && Active)
                throw new TableNotFoundException(tableName);
        }

        public void CheckIfColumnExists(string columnName)
        {
            if(_catalog.GetTablesByColumn(columnName).Count == 0) 
                throw new ColumnNotFoundException(columnName);
        }
    }
}
