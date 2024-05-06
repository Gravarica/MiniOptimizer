using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using MiniOptimizer.Exceptions;
using MiniOptimizer.Metadata;
using MiniOptimizer.Utils;

namespace MiniOptimizer.Compiler
{
    public class SemanticAnalyzer
    {
        private Catalog _catalog;
        private List<string> _tablesSelected;

        public bool Active { get; private set; }

        public SemanticAnalyzer(Catalog catalog, bool active = true) {
            _catalog = catalog;
            Active = active;
            _tablesSelected = new List<string>();
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
            CheckPredicateSyntax(a1.constant(), a2.constant());

            CheckAttribute(a1.GetText());

            var qualifiedName = ParseHelper.ParseQualifiedName(a1.GetText());
            CheckPredicateTables(qualifiedName.Item1);

            MiniQLDataType leftOperandType = _catalog.GetMiniQLDataType(qualifiedName.Item1, qualifiedName.Item2);

            if (a2.constant() != null)
            {
                MiniQLDataType constantType = ParseHelper.ConvertToMiniQLDataType(a2.constant().Start.Type);
                CheckPredicateTypes(constantType, leftOperandType, qualifiedName.Item2);
            } else
            {
                // If it goes here, it's a join predicate. Check if tables are specified in relation list
                var qualifiedNameRightOperand = ParseHelper.ParseQualifiedName(a2.GetText());
                CheckPredicateTables(qualifiedNameRightOperand.Item1);
                var rightOperandType = _catalog.GetMiniQLDataType(qualifiedNameRightOperand.Item1, qualifiedNameRightOperand.Item2);
                CheckPredicateTypes(leftOperandType, rightOperandType, qualifiedName.Item2);
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

            _tablesSelected.Add(tableName);
        }

        public void CheckIfColumnExists(string columnName)
        {
            if(_catalog.GetTablesByColumn(columnName).Count == 0) 
                throw new ColumnNotFoundException(columnName);
        }

        private bool CheckIfTableIsMentioned(string tableName)
        {
            return _tablesSelected.Contains(tableName);
        }

        private void CheckPredicateSyntax(MiniQLParser.ConstantContext c1, MiniQLParser.ConstantContext c2)
        {
            if (c1 != null && c2 != null) throw new BaseException("Error 900. Predicate cannot contain two constants.");

            if (c1 != null) throw new BaseException("Error 901. Left operand in the predicate must be indicator.");
        }

        private void CheckPredicateTypes(MiniQLDataType constantType, MiniQLDataType columnType, string columnName)
        {
            if (constantType != columnType)
                throw new TypeMissmatchException(columnName);
        }

        private void CheckPredicateTables(string tableName)
        {
            if (!CheckIfTableIsMentioned(tableName))
                throw new UnknownReferenceToTable(tableName);
        }
    }
}
