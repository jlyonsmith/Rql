using System;
using Rql;
using Rql.MongoDB;
using System.Reflection;
using System.IO;
using MongoDB.Bson;

namespace Rql.Tools
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            if (args.Length == 1 && (args[0] == "-?" || args[0] == "--help" || args[0] == "-h"))
            {
                Console.WriteLine("usage: {0} <id>", Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));
                Console.WriteLine();
                Console.WriteLine("Give the id in RQL or MongoDB format, e.g. $1234 or ObjectId(\"51d1e6baec98e811b7ee9d20\").  The program will show the other.");
            }

            string id = args[0];

            if (id.StartsWith("$"))
            {
                RqlId rqlId; 

                if (RqlId.TryParse(id, out rqlId))
                {
                    Console.WriteLine("ObjectId(\"{0}\")", rqlId.ToObjectId().ToString());
                }
                else
                    Console.WriteLine("error: '{0}' is an invalid RQL id", id);
            }
            else 
            {
                int objectIdLen = 24 + 2 + 2 + 8;

                if (id.StartsWith("ObjectId") && id.Length != objectIdLen)
                {
                    Console.WriteLine("error: '{0}' must be exactly {1} characters long", objectIdLen);
                }
                else
                {
                    id = id.Substring(10, 24);
                }

                ObjectId objId;

                if (ObjectId.TryParse(id, out objId))
                {
                    Console.WriteLine(objId.ToRqlId().ToString());
                }
                else
                    Console.WriteLine("error: '{0}' is an invalid MongoDB ObjectId", id);
            }
        }
    }
}
