using MiniOptimizer.Metadata;
using MiniOptimizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Test
{
    public static class TestData
    {

        public static Dictionary<string, Table> tables = new Dictionary<string, Table>();

        public static Catalog CreateSimpleTestCase()
        {
            //Dictionary<string, Table> tables = new Dictionary<string, Table>();
            Column mbr = new Column("mbr", false);
            Table radnik = new Table("radnik");
            radnik.AddColumn(mbr);
            Table projekat = new Table("projekat");
            tables["radnik"] = radnik;
            tables["projekat"] = projekat;

            Catalog catalog = new Catalog(tables);
            return catalog;
        }

        public static Catalog TestDataFromFile(bool generate) 
        {
            string filePath1 = "radnik.txt";
            string filePath2 = "projekat.txt";
            string filePath3 = "radproj.txt";
            string filePath4 = "angazovanje.txt";

            string[] columns1 = new string[] { "mbr+", "god", "plt" };
            string[] columns2 = new string[] { "spr+", "ruk", "trajanje" };
            string[] columns3 = new string[] { "mbr+", "spr+", "brc" };
            string[] columns4 = new string[] { "mbr+", "brp" };

            if (generate)
            {
                FileGenerator.GenerateDataFile(filePath1, columns1, 10000, new int[] { 3000, 1990, 1000 }, new int[] { 10000, 2004, 5000 });
                FileGenerator.GenerateDataFile(filePath2, columns2, 50000, new int[] { 1, 1, 1 }, new int[] { 500000, 50, 12 });
                FileGenerator.GenerateDataFile(filePath3, columns3, 10000, new int[] { 1, 1, 1 }, new int[] { 10000, 500000, 12 });
                FileGenerator.GenerateDataFile(filePath4, columns4, 10000, new int[] { 1, 1 }, new int[] { 10000, 10});
            }

            Console.WriteLine("Files generated successfully.");

            Table radnik = FileGenerator.CreateTableFromFile(filePath1, true, ["mbr"]);
            Table projekat = FileGenerator.CreateTableFromFile(filePath2, true, ["spr"]);
            Table radproj = FileGenerator.CreateTableFromFile(filePath3, false, ["mbr", "spr"]);
            Table angazovanje = FileGenerator.CreateTableFromFile(filePath4, false, ["mbr"]);

            tables["radnik"] = radnik;
            tables["projekat"] = projekat;
            tables["radproj"] = radproj;
            tables["angazovanje"] = angazovanje;

            Console.WriteLine("Catalog successfully created. Table stats: ");

            radnik.PrintTableStats();
            radproj.PrintTableStats();

            Catalog catalog = new Catalog(tables);
            return catalog;
        }

        public static Catalog TestDataFromFile2()
        {
            string filePath1 = "A.txt";
            string filePath2 = "B.txt";
            string filePath3 = "C.txt";
            string filePath4 = "D.txt";

            string[] columns1 = new string[] { "a1+", "b1", "c1" };
            string[] columns2 = new string[] { "b1+", "c1", "d1" };
            string[] columns3 = new string[] { "a1+", "f1+", "e1" };
            string[] columns4 = new string[] { "a1+", "f1" };

            Table radnik = FileGenerator.CreateTableFromFile(filePath1, true, ["a1"]);
            Table projekat = FileGenerator.CreateTableFromFile(filePath2, true, ["b1"]);
            Table radproj = FileGenerator.CreateTableFromFile(filePath3, false, ["a1", "f1"]);
            Table angazovanje = FileGenerator.CreateTableFromFile(filePath4, false, ["a1"]);

            tables["A"] = radnik;
            tables["B"] = projekat;
            tables["C"] = radproj;
            tables["D"] = angazovanje;

            Console.WriteLine("Catalog successfully created. Table stats: ");

            radnik.PrintTableStats();
            radproj.PrintTableStats();

            Catalog catalog = new Catalog(tables);
            return catalog;
        }
    }
}
