#region DS Photosorter - MIT - (c) 2017 Thijs Elenbaas.
/*
  DS Photosorter - tool that processes photos captured with Synology DS Photo

  Permission is hereby granted, free of charge, to any person obtaining
  a copy of this software and associated documentation files (the
  "Software"), to deal in the Software without restriction, including
  without limitation the rights to use, copy, modify, merge, publish,
  distribute, sublicense, and/or sell copies of the Software, and to
  permit persons to whom the Software is furnished to do so, subject to
  the following conditions:

  The above copyright notice and this permission notice shall be
  included in all copies or substantial portions of the Software.

  Copyright 2017 - Thijs Elenbaas
*/
#endregion

using System.IO;
using Newtonsoft.Json;

namespace PhotoSorter
{
    public class ConfigurationSerializer<TConfig>
    {
        private readonly string _fileName;
        private readonly JsonSerializer _serializer;

        public ConfigurationSerializer(string fileName, TConfig configuration)
        {
            _fileName = fileName;
            _serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented                
            };
            if (!File.Exists(_fileName))
            {
                Serialize(configuration);
            }
        }

        public ConfigurationSerializer(string fileName)
        {
            _fileName = fileName;
            _serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.Indented                
            };
        }
        public void Serialize(TConfig configuration)
        {                        
            using (var streamWriter = new StreamWriter(_fileName))
            {
                _serializer.NullValueHandling = NullValueHandling.Ignore;
                _serializer.Serialize(streamWriter, configuration);
            }        
        }

        public TConfig Deserialize()
        {

            using (var streamReader = new StreamReader(_fileName))
            {
                return (TConfig)_serializer.Deserialize(streamReader, typeof(TConfig));
            } 
        }

    }
}
