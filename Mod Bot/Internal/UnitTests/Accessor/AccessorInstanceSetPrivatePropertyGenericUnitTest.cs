﻿using ModLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace InternalModBot.UnitTests
{
    public class AccessorInstanceSetPrivatePropertyGenericUnitTest : UnitTest
    {
        AccessorTestFields _accessorTestFieldsInstance;

        public override string CommandActivator => "AccessorInstanceSetPrivatePropertyGeneric";

        public override bool IsExpectedResult(object[] result)
        {
            if (_accessorTestFieldsInstance.FloatingPointValueProperty != 2f)
            {
                debug.Log(CommandActivator + ": Expected 2.0, Got: " + _accessorTestFieldsInstance.FloatingPointValueProperty);
                return false;
            }

            if (_accessorTestFieldsInstance.StringValueProperty != "TestValue2")
            {
                debug.Log(CommandActivator + ": Expected \"TestValue2\", Got: \"" + _accessorTestFieldsInstance.StringValueProperty + "\"");
                return false;
            }

            return true;
        }

        public override object[] RunTest()
        {
            Accessor accessor = new Accessor(typeof(AccessorTestFields), _accessorTestFieldsInstance);

            accessor.SetPrivateProperty("FloatingPointValueProperty", 2f);
            accessor.SetPrivateProperty("StringValueProperty", "TestValue2");

            return null;
        }

        public override void SetupUnitTest()
        {
            _accessorTestFieldsInstance = new AccessorTestFields(1f, "TestValue1");
        }

        public override void Cleanup()
        {
            _accessorTestFieldsInstance = null;
        }

        private class AccessorTestFields
        {
            float _floatingPointValue;

            string _stringValue;

            public float FloatingPointValueProperty
            {
                get => _floatingPointValue;
                private set => _floatingPointValue = value;
            }

            public string StringValueProperty
            {
                get => _stringValue;
                private set => _stringValue = value;
            }

            public AccessorTestFields(float floatingPointValue, string stringValue)
            {
                _floatingPointValue = floatingPointValue;
                _stringValue = stringValue;
            }
        }
    }
}
