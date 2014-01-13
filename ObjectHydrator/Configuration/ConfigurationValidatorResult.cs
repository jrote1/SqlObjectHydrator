using System;
using System.Collections.Generic;

namespace SqlObjectHydrator.Configuration
{
    internal class ConfigurationValidatorResult
    {
        public ConfigurationValidatorResult()
        {
            Errors = new List<string>(); 
        }

        public Boolean ConfigurationValid
        {
            get { return Errors.Count == 0; }
        }

        public List<String> Errors { get; private set; }
    }
}