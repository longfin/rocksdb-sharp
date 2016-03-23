﻿using RocksDbSharp;
using System;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace SimpleExampleHighLevel
{
    class Program
    {
        static void Main(string[] args)
        {
            string temp = Path.GetTempPath();
            string path = Environment.ExpandEnvironmentVariables(Path.Combine(temp, "rocksdb_simple_hl_example"));
            // the Options class contains a set of configurable DB options
            // that determines the behavior of a database
            // Why is the syntax, SetXXX(), not very C#-like? See Options for an explanation
            using (var options = new Options().SetCreateIfMissing(true))
            using (var db = RocksDb.Open(options, path))
            {
                try
                {
                    {
                        // With strings
                        string value = db.Get("key");
                        db.Put("key", "value");
                        value = db.Get("key");
                        string iWillBeNull = db.Get("non-existent-key");
                        db.Remove("key");
                    }

                    {
                        // With bytes
                        var key = Encoding.UTF8.GetBytes("key");
                        byte[] value = Encoding.UTF8.GetBytes("value");
                        db.Put(key, value);
                        value = db.Get(key);
                        byte[] iWillBeNull = db.Get(new byte[] { 0, 1, 2 });
                        db.Remove(key);

                        db.Put(key, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });
                    }

                    {
                        // With buffers
                        var key = Encoding.UTF8.GetBytes("key");
                        var buffer = new byte[100];
                        long length = db.Get(key, buffer, 0, buffer.Length);
                    }

                    {
                        // Removal of non-existent keys
                        db.Remove("I don't exist");
                    }

                    {
                        // Write batches
                        // With strings
                        using (WriteBatch batch = new WriteBatch()
                            .Put("one", "uno")
                            .Put("two", "deuce")
                            .Put("two", "doce")
                            .Put("three", "tres"))
                        {
                            db.Write(batch);
                        }

                        // With bytes
                        var utf8 = Encoding.UTF8;
                        using (WriteBatch batch = new WriteBatch()
                            .Put(utf8.GetBytes("four"), new byte[] { 4, 4, 4 } )
                            .Put(utf8.GetBytes("five"), new byte[] { 5, 5, 5 } ))
                        {
                            db.Write(batch);
                        }
                    }

                    var two = db.Get("two");
                    Debug.Assert(two == "doce");

                    {
                        // Iterators
                        using (ReadOptions readOpts = new ReadOptions().SetIterateUpperBound("t"))
                        using (var iterator = db.NewIterator(readOpts))
                        {
                            for (iterator.Seek("k"); iterator.Valid(); iterator.Next())
                            {
                                Console.WriteLine(iterator.StringKey() + " -> " + iterator.StringValue());
                            }
                        }
                    }

                }
                catch (RocksDbException)
                {

                }
            }
        }
    }
}
