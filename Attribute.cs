// Copyright (C) 2024 - Nordic Space Link
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NordicSpaceLink.IIO
{
    [DebuggerDisplay("{Name}: {Value}")]
    public abstract class Attribute
    {
        public string Name { get; }

        /// <summary>
        /// Read/write the current value of the attribute.
        /// </summary>
        public string Value { get => GetValue(); set => SetValue(value); }

        /// <summary>
        /// Read/write the current numeric value of the attribute.
        /// Reading will try to parse as much as possible of the value if the value starts with a valid number.
        /// </summary>
        public long Int64 { get => long.Parse(NumberRegex.Match(Value).ToString()); set => Value = value.ToString(); }

        /// <summary>
        /// Read/write the current floating point value of the attribute.
        /// Reading will try to parse as much as possible of the value if the value starts with a valid floating point number.
        /// </summary>
        public double Float { get => double.Parse(FloatRegex.Match(Value).ToString(), CultureInfo.InvariantCulture); set => Value = value.ToString(CultureInfo.InvariantCulture); }

        /// <summary>
        /// Read/write the current boolean value of the attribute. A non-zero integer is treated as true, while 0 is false.
        /// </summary>
        public bool Bool { get => Int64 != 0; set => Value = value ? "1" : "0"; }

        public Attribute(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Value;
        }

        protected virtual void SetValue(string value)
        {
            throw new ReadOnlyException($"Attribute {Name} is read-only.");
        }

        protected abstract string GetValue();

        private static readonly Regex FloatRegex = new(@"^[+-]?\d+(?:.\d+)?([eE][+-]?\d\d+)?", RegexOptions.Compiled);
        private static readonly Regex NumberRegex = new(@"^[+-]?\d+", RegexOptions.Compiled);
    }
}