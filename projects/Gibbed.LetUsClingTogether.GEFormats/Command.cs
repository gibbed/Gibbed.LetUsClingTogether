using System;

namespace Gibbed.LetUsClingTogether.GEFormats
{
    public struct Command : IEquatable<Command>, IEquatable<Operation>
    {
        public readonly Operation Operation;
        public readonly int Argument;

        public Command(Operation operation, int argument)
        {
            this.Operation = operation;
            this.Argument = argument;
        }

        public Command(Operation operation)
        {
            this.Operation = operation;
            this.Argument = default;
        }

        public Command(uint value)
        {
            this.Operation = (Operation)((value >> 24) & 0xFF);
            this.Argument = (int)((value >> 0) & 0xFFFFFF);
        }

        public override bool Equals(object obj)
        {
            return
                obj is Command command &&
                this.Equals(command) == true;
        }

        public bool Equals(Command other)
        {
            return this.Operation == other.Operation &&
                   this.Argument == other.Argument;
        }

        public static bool operator ==(Command left, Command right)
        {
            return left.Equals(right) == true;
        }

        public static bool operator !=(Command left, Command right)
        {
            return left.Equals(right) == false;
        }

        public static bool operator ==(Command left, Operation right)
        {
            return left.Operation == right;
        }

        public static bool operator !=(Command left, Operation right)
        {
            return left.Operation != right;
        }

        public bool Equals(Operation other)
        {
            return this.Operation == other;
        }

        public static bool operator ==(Operation left, Command right)
        {
            return left == right.Operation;
        }

        public static bool operator !=(Operation left, Command right)
        {
            return left != right.Operation;
        }

        public override int GetHashCode()
        {
            int hashCode = -1623617556;
            hashCode = hashCode * -1521134295 + this.Operation.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Argument.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return this.Argument != 0
                ? $"{this.Operation} {this.Argument}"
                : $"{this.Operation}";
        }
    }
}
