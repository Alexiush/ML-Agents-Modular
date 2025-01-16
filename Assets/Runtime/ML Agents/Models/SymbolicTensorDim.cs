using System.Collections.Generic;
using System.Linq;

namespace ModularMLAgents.Models
{
    public abstract class SymbolicTensorDim
    {
        public abstract string Compile();
    }

    public class DefinedSymbolicTensorDim : SymbolicTensorDim
    {
        private int _dimensionSize;

        public DefinedSymbolicTensorDim(int dimensionSize)
        {
            _dimensionSize = dimensionSize;
        }

        public override string Compile()
        {
            return _dimensionSize.ToString();
        }
    }

    public class LiteralSymbolicTensorDim : SymbolicTensorDim
    {
        private string _literal;

        public LiteralSymbolicTensorDim(string literal)
        {
            _literal = literal;
        }

        public override string Compile()
        {
            return $"literals_mapping['{_literal}']";
        }
    }

    public class SumSymbolicTensorDim : SymbolicTensorDim
    {
        private List<SymbolicTensorDim> _args;

        public SumSymbolicTensorDim(List<SymbolicTensorDim> args)
        {
            _args = args;
        }

        public override string Compile()
        {
            return string.Join(" + ", _args.Select(a => a.Compile()));
        }
    }

    public class ProductSymbolicTensorDim : SymbolicTensorDim
    {
        private List<SymbolicTensorDim> _args;

        public ProductSymbolicTensorDim(List<SymbolicTensorDim> args)
        {
            _args = args;
        }

        public override string Compile()
        {
            return string.Join(" * ", _args.Select(a => a.Compile()));
        }
    }

    public class DifferenceSymbolicTensorDim : SymbolicTensorDim
    {
        private SymbolicTensorDim _first;
        private SymbolicTensorDim _second;

        public DifferenceSymbolicTensorDim(SymbolicTensorDim first, SymbolicTensorDim second)
        {
            _first = first;
            _second = second;
        }

        public override string Compile()
        {
            return $"{_first.Compile()} - {_second.Compile()}";
        }
    }

    public class DivisionSymbolicTensorDim : SymbolicTensorDim
    {
        private SymbolicTensorDim _first;
        private SymbolicTensorDim _second;

        public DivisionSymbolicTensorDim(SymbolicTensorDim first, SymbolicTensorDim second)
        {
            _first = first;
            _second = second;
        }

        public override string Compile()
        {
            return $"{_first.Compile()} / {_second.Compile()}";
        }
    }

    public class IntegerDivisionSymbolicTensorDim : SymbolicTensorDim
    {
        private SymbolicTensorDim _first;
        private SymbolicTensorDim _second;

        public IntegerDivisionSymbolicTensorDim(SymbolicTensorDim first, SymbolicTensorDim second)
        {
            _first = first;
            _second = second;
        }

        public override string Compile()
        {
            return $"{_first.Compile()} // {_second.Compile()}";
        }
    }
}
